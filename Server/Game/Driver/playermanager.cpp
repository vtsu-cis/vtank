/*!
    \file playermanager.cpp
    \brief Manage clients who are playing in a thread-safe, static manner.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#include <master.hpp>
#include <server.hpp>
#include <playermanager.hpp>
#include <gamemanager.hpp>
#include <vtassert.hpp>
#include <logger.hpp>
#include <notifier.hpp>
#include <pointmanager.hpp>

namespace Players
{
	//! How long does it take to timeout the client? (Milliseconds)
    double timeout = IceUtil::Time::milliSeconds(TIMEOUT).toMilliSecondsDouble();

	//! Maximum amount of players allowed to join.
	Ice::Int player_limit = DEFAULT_PLAYER_LIMIT;

    // Mutual exclusion object for thread safety.
    boost::recursive_mutex mutex;

    // Mutual exclusion object specifically for the list of pending players.
    boost::mutex pending_mutex;

    // Occasionally some tasks may need to be spawned to deal with players.
    boost::threadpool::pool task_pool(PLAYER_THREADS);

    // List of people waiting to join.
	std::map<std::string, pending_ptr> pending_list = std::map<std::string, pending_ptr>();

    // Tank list.
    TankManager tanks;
    
    /*!
        Kick off players who are idle.
    */
    bool manage_players()
    {
        try {
            if (Server::server.communicator()->isShutdown()) {
                return false;
            }
        }
        catch (const Ice::Exception &) {
            return false;
        }

        const double now = IceUtil::Time::now().toMilliSecondsDouble();
        const tank_array tank_list = tanks.get_tank_list();
        for (tank_array::size_type i = 0; i < tank_list.size(); i++)
        {
            const tank_ptr tank = tank_list[i];

            const double last_action = tank->get_player_info()->get_last_action_time();
            const double last_sync = tank->get_player_info()->get_last_sync_time();
            double elapsed = now - last_action;
            if (elapsed > timeout) {
                std::ostringstream formatter;
                formatter << "Removing player " << tank->get_name() 
                    << " because he hasn't replied in a while.";
                Logger::log(Logger::LOG_LEVEL_INFO, formatter.str());

                // User took too long to send another message: Kick the user off.
                if (!remove_player(tank->get_id())) {
                    Logger::log(Logger::LOG_LEVEL_WARNING,
                        "Warning: Tried to remove player from player list, but couldn't.");
                }

                continue;
            }
            
            elapsed = now - last_sync;
            if (elapsed > CLOCK_SYNC_INTERVAL) {
                // Time for another time sync.
                tank->do_clock_sync();
            }
        }

        return true;
    }

    int generate_unique_temp_id()
    {
        Logger::Stack_Logger stack("generate_unique_temp_id()", false);
        boost::lock_guard<boost::recursive_mutex> guard(mutex);

        const tank_array tank_list = tanks.get_tank_list();
        for (tank_array::size_type i = 0; i < tank_list.size() + 20; i++) {
            bool found = false;

            for (tank_array::size_type j = 0; j < tank_list.size(); j++) {
                // Check if anyone holds the ID.
                if (tank_list[j]->get_id() == i) {
                    found = true;

                    break;
                }
            }

            if (!found)
                return i;
        }

        // It should never reach here.
        VTANK_ASSERT(false);
        return -1; // Makes a warning go away.
    }

    void add_pending(const std::string& key, const GameSession::Tank tank)
    {
		boost::lock_guard<boost::mutex> guard(pending_mutex);
        const std::map<std::string, pending_ptr>::iterator i = pending_list.find(key);
		if (i != pending_list.end()) {
			// Warn that the player exists.
            Logger::log(Logger::LOG_LEVEL_WARNING, 
                "add_pending(): Key already exists. Old value was overwritten.");
		}
        
        const IceUtil::Int64 start_time = IceUtil::Time::now().toSeconds();
		pending_list[key] = pending_ptr(new PendingTank(tank, start_time));
    }

    pending_ptr get_pending(const std::string& key)
    {
        boost::lock_guard<boost::mutex> guard(pending_mutex);
        const std::map<std::string, pending_ptr>::iterator i = pending_list.find(key);
        if (i == pending_list.end()) {
            std::ostringstream formatter;
            formatter << "get_pending(): Key doesn't exist: " << key;
            Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());

            throw Exceptions::PermissionDeniedException("That session key is invalid.");
        }

        return pending_list[key];
    }

    bool remove_pending(const std::string& key)
    {
		boost::lock_guard<boost::mutex> guard(pending_mutex);

		const std::map<std::string, pending_ptr>::iterator it = pending_list.find(key);
		if (it == pending_list.end()) {
			// Key did not exist.
			return false;
		}

        std::ostringstream formatter;
        formatter << "Removed pending key: " << key;

        Logger::log(Logger::LOG_LEVEL_INFO, formatter.str());
        
        (void)pending_list.erase(key);

		return true;
    }

    void add_player(const tank_ptr player)
    {
        Logger::Stack_Logger stack("add_player()");
        boost::lock_guard<boost::recursive_mutex> guard(mutex);
        
        // First check if the client exists already.
        const tank_array tank_list = tanks.get_tank_list();
        for (tank_array::size_type i = 0; i < tank_list.size(); i++) {
            if (tank_list[i]->get_name() == player->get_name()) {
                remove_player(tank_list[i]->get_id());

                std::ostringstream formatter;
                formatter << "Player " << player->get_name() << " had already existed, "
                    << "so he was removed." << std::endl;

                Logger::log(Logger::LOG_LEVEL_INFO, formatter.str());

                break;
            }
        }
		
        // Tell everyone that the player joined.
        Notifier::blanket_notify_player_joined(player);

        // Now add it locally.
        tanks.add(player);

        PointManager::add_player(player->get_id());
	}

    const tank_ptr get_player(const int& id)
    {
        return tanks.get(id);
    }

    bool remove_player(const int& id)
    {
        Logger::Stack_Logger stack("remove_player()");

        try {
            const tank_ptr tank = tanks.get(id);
            
            boost::lock_guard<boost::recursive_mutex> guard(mutex);

            std::ostringstream formatter;
            formatter << "Removing player " << tank->get_name()
                << " from the player list and from the adapter.";

	        Logger::log(Logger::LOG_LEVEL_INFO, formatter.str());
            
            Players::nodes.unregister_player(id, tank->get_node_id());
            if (!tanks.remove(id)) {
                formatter.clear();
                formatter << "Couldn't find player #" << id << ", " << tank->get_name() 
                    << " to remove him.";

                Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
            }
            
	        (void)Server::server.get_adapter()->remove(tank->get_ice_id());
            
            Notifier::blanket_notify_player_left(id);
            
            Server::mtg_service.get_proxy()->PlayerLeft(tank->get_name());
        }
        catch (const TankNotExistException &) {
            std::ostringstream formatter;
            formatter << "Couldn't find player #" << id << " to remove him.";

            Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());

            return false;
        }
        HANDLE_UNCAUGHT_EXCEPTIONS

        return true;
    }

    int get_player_id_by_name(const std::string& username)
    {
		Logger::Stack_Logger stack("get_player_id_by_name()", false);

        const tank_array tank_list = tanks.get_tank_list();
        for (tank_array::size_type i = 0; i < tank_list.size(); i++) {
            if (tank_list[i]->get_name() == username) {
                return tank_list[i]->get_id();
            }
        }

        return -1;
    }

    GameSession::PlayerList get_player_list()
    {
		Logger::Stack_Logger stack("get_player_list()", false);

        GameSession::PlayerList list;

        const tank_array tank_list = tanks.get_tank_list();
        for (tank_array::size_type i = 0; i < tank_list.size(); i++) {
            list.push_back(tank_list[i]->get_tank_object());
        }

        return list;
    }
}
