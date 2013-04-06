/*!
    \file loginsessionfactory.cpp
    \brief Implementation of the LoginSessionFactory class.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#include <master.hpp>
#include <loginsessionfactory.hpp>
#include <playermanager.hpp>
#include <server.hpp>
#include <logger.hpp>
#include <gamemanager.hpp>
#include <tank.hpp>
#include <mapmanager.hpp>

LoginSessionFactory::LoginSessionFactory()
{
}

LoginSessionFactory::~LoginSessionFactory()
{
}

GameSession::GameInfoPrx LoginSessionFactory::JoinServer(const std::string& key, 
    const GameSession::ClockSynchronizerPrx& clock, 
    const GameSession::ClientEventCallbackPrx& callback, const Ice::Current& c)
{
    try {
        // Calling is_rotating() will block until it's actually not rotating.
        while (MapManager::is_rotating())
			boost::this_thread::sleep(boost::posix_time::milliseconds(5));

        GameSession::ClockSynchronizerPrx new_clock = 
            GameSession::ClockSynchronizerPrx::uncheckedCast(
            clock->ice_timeout(5000));
        GameSession::ClientEventCallbackPrx new_callback = 
            GameSession::ClientEventCallbackPrx::uncheckedCast(
            callback->ice_oneway());
        
        // Verify the session key.
        // This call can throw PermissionDeniedException -- which is returned to the client.
        GameSession::Tank tank = Players::get_pending(key)->tank;
        tank.id = Players::generate_unique_temp_id(); // TODO: Eventually replaced by game simulation.
        tank.alive = true;
		tank.attributes.health = DEFAULT_MAX_HEALTH;
        
        Players::remove_pending(key);

        // Ice's garbage collector will take care of deallocating the player object.
        const player_ptr player(new PlayerInfo(new_callback, new_clock));
        const tank_ptr player_tank(new Tank(
            tank, player, Players::tanks.get_next_team_assignment()));

		Logger::debug("Player #%d (%s) joined the game.", tank.id, tank.attributes.name.c_str());

		Players::generate_spawn_position(player_tank);
		Players::add_player(player_tank);
        Players::nodes.process_position(player_tank);

        GameSession::GameInfoPtr player_servant = new Player(player_tank->get_id());
        Ice::ObjectPrx ice_object = c.adapter->addWithUUID(player_servant);
        player_tank->set_ice_id(ice_object->ice_getIdentity());

        player_tank->do_clock_sync();

        return GameSession::GameInfoPrx::uncheckedCast(ice_object);
    }
    catch (const Exceptions::VTankException &ex) {
        // Catch the exception for monitoring purposes.
        std::ostringstream formatter;
        formatter << "VTankException thrown from LoginSessionFactory::JoinServer(): "
            << ex.reason;
        Logger::log(Logger::LOG_LEVEL_INFO, formatter.str());

        // Escalate the exception.
        throw ex;
    }
    catch (const Ice::Exception &ex) {
        // Catch the exception for monitoring purposes.
        std::ostringstream formatter;
        formatter << "Ice::Exception thrown from LoginSessionFactory::JoinServer(): "
            << ex.what();
        Logger::log(Logger::LOG_LEVEL_INFO, formatter.str());

        // Escalate the exception.
        throw ex;
    }
	HANDLE_UNCAUGHT_EXCEPTIONS
}

Glacier2::SessionPrx LoginSessionFactory::create(const std::string&, 
    const Glacier2::SessionControlPrx&, const Ice::Current&)
{
    // Right now we'll indiscrimately return a session to the login session factory.
    // In the future we could remove the middle man and just use this.
    return Glacier2::SessionPrx::uncheckedCast(Server::server.communicator()->
        propertyToProxy("GameSessionFactoryProxy"));
}
