/*!
	\file mtgcallback.cpp
	\brief 
	\author (C) Copyright 2009 by Vermont Technical College
*/
#include <master.hpp>
#include <playermanager.hpp>
#include <mtgcallback.hpp>
#include <server.hpp>
#include <mapmanager.hpp>
#include <logger.hpp>
#include <gamemanager.hpp>

MTGCallback::MTGCallback()
{
}

MTGCallback::~MTGCallback()
{
}

void MTGCallback::destroy(const Ice::Current&)
{
    Logger::log(Logger::LOG_LEVEL_ERROR, "The server destroyed the session.");

    Server::server.stop();
    (void)Server::mtg_service.shutdown();
}

void MTGCallback::KeepAlive(const Ice::Current&)
{
}

void MTGCallback::AddPlayer(const std::string& key, const std::string& username, 
                            Ice::Int level, const VTankObject::TankAttributes& tank, 
                            const Ice::Current&)
{
    std::ostringstream formatter;
    formatter << "Adding player " << tank.name << " (key=" << key << ")";

    Logger::log(Logger::LOG_LEVEL_INFO, formatter.str());

    // tank_ptr is deallocated by boost.
    GameSession::Tank game_tank;
    game_tank.attributes = tank;
    game_tank.angle = 0;

    Players::add_pending(key, game_tank);
}

void MTGCallback::RemovePlayer(const std::string& username, const Ice::Current&)
{
    std::ostringstream formatter;
    formatter << "The server is removing " << username << ".";

    Logger::log(Logger::LOG_LEVEL_INFO, formatter.str());
    
    const int id = Players::get_player_id_by_name(username);
    if (id >= 0) {
        Players::remove_player(id);
    }
}

void MTGCallback::ForceMaxPlayerLimit(Ice::Int limit, const Ice::Current&)
{
    std::ostringstream formatter;
    formatter << "The server insists on a " << limit << " player limit.";
    
    Logger::log(Logger::LOG_LEVEL_INFO, formatter.str());

    Players::player_limit = limit;
}

void MTGCallback::UpdateMapList(const Ice::StringSeq& mapList, const Ice::Current&)
{
    std::ostringstream formatter;
    formatter << "The server updated our copy of the map list (" 
        << mapList.size() << " maps).";

    Logger::log(Logger::LOG_LEVEL_INFO, formatter.str());

    MapManager::map_list = mapList;
}

Ice::Int MTGCallback::GetPlayerLimit(const ::Ice::Current&)
{
    return Players::player_limit;
}

void MTGCallback::UpdateUtilities(const VTankObject::UtilityList &list, const Ice::Current &)
{
	Players::update_utility_list(list);
}
