/*!
    \file   tankmanager.cpp
    \brief  Implements the TankManager class.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#include <master.hpp>
#include <tankmanager.hpp>
#include <logger.hpp>
#include <mapmanager.hpp>

TankManager::TankManager()
{
}

TankManager::~TankManager()
{
}

//! Add a tank to the manager.
void TankManager::add(const tank_ptr tank)
{
    boost::unique_lock<boost::shared_mutex> guard(mutex);

    const std::map<int, tank_ptr>::iterator i = tanks.find(tank->get_id());
    if (i != tanks.end()) {
        std::ostringstream formatter;
        formatter << "TankManager::add(" << tank->get_id() << "): "
            "The ID already exists. The tank will be replaced.";
        Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
    }

    tanks[tank->get_id()] = tank;
}

//! Retrieve a tank from the manager.
const tank_ptr TankManager::get(const int id)
{
    boost::shared_lock<boost::shared_mutex> guard(mutex);

    const std::map<int, tank_ptr>::iterator i = tanks.find(id);
    if (i == tanks.end()) {
        throw TankNotExistException(id);
    }

    return tanks[id];
}

//! Remove a tank from the manager.
bool TankManager::remove(const int id)
{
    boost::unique_lock<boost::shared_mutex> guard(mutex);

    bool successfully_removed = true;

    const std::map<int, tank_ptr>::iterator i = tanks.find(id);
    if (i == tanks.end()) {
        std::ostringstream formatter;
        formatter << "TankManager::remove(" << id << "): Tried to remove, but the "
            "tank wasn't found.";

        Logger::log(Logger::LOG_LEVEL_INFO, formatter.str());
        
        successfully_removed = false;
    }
    else {
        (void)tanks.erase(i);
    }
    
    return successfully_removed;
}

const tank_array TankManager::inner_get_tank_list()
{
    if (tanks.empty()) {
        return tank_array();
    }

    tank_array temp_tanks;
    std::map<int, tank_ptr>::iterator i;
    for (i = tanks.begin(); i != tanks.end(); i++) {
        temp_tanks.push_back(i->second);
    }

    return temp_tanks;
}

const tank_array TankManager::get_tank_list()
{
    boost::shared_lock<boost::shared_mutex> guard(mutex);

    return inner_get_tank_list();
}

const int TankManager::size()
{
    boost::shared_lock<boost::shared_mutex> guard(mutex);

    return tanks.size();
}

void TankManager::organize_teams()
{
    tank_array tank_list = get_tank_list();
    if (tank_list.empty()) {
        // Nothing to do.
        return;
    }

    const VTankObject::GameMode mode = MapManager::get_current_mode();
    
    boost::unique_lock<boost::shared_mutex> guard(mutex);

	if (mode != VTankObject::DEATHMATCH) {
        // TODO: Distribute teams based on skill.
		int team_size_red = 0;
		int team_size_blue = 0;
        for (tank_array::size_type i = 0; i < tank_list.size(); i++) {
			const int assignment = rand() % 2;
			GameSession::Alliance team;
			if (team_size_red == team_size_blue)
				 team = (assignment == 0 ? GameSession::RED : GameSession::BLUE);
			else if (team_size_red > team_size_blue)
				team = GameSession::BLUE;
			else
				team = GameSession::RED;

			if (team == GameSession::RED)
				++team_size_red;
			else
				++team_size_blue;
			
            tank_list[i]->set_team(team);
        }
    }
    else {
        // Worst case, assume deathmatch.
        for (tank_array::size_type i = 0; i < tank_list.size(); i++) {
            tank_list[i]->set_team(GameSession::NONE);
        }
    }
}

GameSession::Alliance TankManager::get_next_team_assignment()
{
    const VTankObject::GameMode mode = MapManager::get_current_mode();
    if (mode == VTankObject::DEATHMATCH) {
        return GameSession::NONE;
    }

    boost::shared_lock<boost::shared_mutex> guard(mutex);
    int red_count = 0;
    int blue_count = 0;
    const tank_array list = inner_get_tank_list();
    const int size  = static_cast<int>(list.size());
    for (int i = 0; i < size; ++i) {
        if (list[i]->get_team() == GameSession::RED)
            red_count++;
        else
            blue_count++;
    }

    if (tanks.size() > 0 && red_count == 0 && blue_count == 0) {
        // This particular game mode doesn't support teams.
        return GameSession::NONE;
    }

    if (red_count > blue_count) {
        // Blue needs help.
        return GameSession::BLUE;
    }
    else if (blue_count > red_count) {
        // Red needs help.
        return GameSession::RED;
    }
    
    // Both teams match; add randomly.
    return (rand() % 2 ? GameSession::RED : GameSession::BLUE);
}
