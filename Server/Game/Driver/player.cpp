/*
    \file   player.cpp
    \brief  Implements the GameSession Ice servant.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#include <master.hpp>
#include <player.hpp>
#include <playermanager.hpp>
#include <mapmanager.hpp>
#include <gamemanager.hpp>
#include <logger.hpp>
#include <server.hpp>
#include <notifier.hpp>
#include <pointmanager.hpp>

Player::Player(const int player_id) : GameSession::GameInfo()
{
    id = player_id;
}

Player::~Player()
{
}

// Ice methods:
void Player::destroy(const Ice::Current &)
{
    Logger::Stack_Logger stack("destroy()", false);

    try {
        (void)Players::remove_player(id);
    }
    catch (...) {
        Logger::log(Logger::LOG_LEVEL_ERROR, "Player::destroy threw uncaught exception.");
    }
}

void Player::Ready(const Ice::Current&)
{
	try {
		while (MapManager::is_rotating()) {
			boost::this_thread::sleep(boost::posix_time::milliseconds(10));
		}

		const tank_ptr tank = Players::tanks.get(id);
		tank->get_player_info()->refresh_timeout();
		tank->set_ready(true);

		Players::send_status_to(tank);

		// TODO: Don't do this.
		std::vector<ActiveUtility> utils = Players::get_active_utilities();
		std::vector<ActiveUtility>::iterator i = utils.begin();
		for (; i != utils.end(); ++i) {
			Notifier::notify_utility_spawn(tank, i->id, i->util, i->pos);
		}
	}
	catch (const TankNotExistException &) {
		// Nothing to do but ignore it.
	}
	HANDLE_UNCAUGHT_EXCEPTIONS
}

GameSession::PlayerList Player::GetPlayerList(const Ice::Current&)
{
    const tank_ptr tank = Players::tanks.get(id);
    tank->get_player_info()->refresh_timeout();

	while (MapManager::is_rotating()) {
		boost::this_thread::sleep(boost::posix_time::milliseconds(10));
	}

    return Players::get_player_list();
}

std::string Player::GetCurrentMapName(const Ice::Current&)
{
    const tank_ptr tank = Players::tanks.get(id);
    tank->get_player_info()->refresh_timeout();

	while (MapManager::is_rotating()) {
		boost::this_thread::sleep(boost::posix_time::milliseconds(10));
	}

    return MapManager::get_current_map_filename();
}

Ice::Double Player::GetTimeLeft(const Ice::Current&)
{
	while (MapManager::is_rotating()) {
		boost::this_thread::sleep(boost::posix_time::milliseconds(10));
	}
    
    try {
        const tank_ptr tank = Players::tanks.get(id);
        tank->get_player_info()->refresh_timeout();

        return Players::get_time_left();
    }
	catch (const TankNotExistException &) {
		
	}
    HANDLE_UNCAUGHT_EXCEPTIONS;

    return 0;
}

VTankObject::GameMode Player::GetGameMode(const Ice::Current&)
{
    return MapManager::get_current_mode();
}

VTankObject::StatisticsList Player::GetScoreboard(const Ice::Current&)
{
    while (MapManager::is_rotating()) {
		boost::this_thread::sleep(boost::posix_time::milliseconds(10));
	}
    
    return PointManager::compile();
}

GameSession::ScoreboardTotals Player::GetTeamTotals(const Ice::Current&)
{
	// TODO: This doesn't do what it's supposed to yet.
	GameSession::ScoreboardTotals totals;
	totals.completedRed = 0;
	totals.completedBlue = 0;
	totals.capturesRed = Players::get_red_score();
	totals.capturesBlue = Players::get_blue_score();
	totals.killsBlue = 0;
	totals.killsRed = 0;
	
	return totals;
}

void Player::KeepAlive(const Ice::Current&)
{
    try {
        const tank_ptr tank = Players::tanks.get(id);
        tank->get_player_info()->refresh_timeout();
    }
    HANDLE_UNCAUGHT_EXCEPTIONS
}

void Player::Move(Ice::Long timestamp, const VTankObject::Point& position, 
				  const VTankObject::Direction direction, const Ice::Current&)
{
    try {
        const tank_ptr tank = Players::tanks.get(id);
        tank->get_player_info()->refresh_timeout();

        if (MapManager::is_rotating()) {
            return;
        }

        Players::move(id, timestamp, direction, position);
    }
    HANDLE_UNCAUGHT_EXCEPTIONS;
}

void Player::Rotate(Ice::Long timestamp, Ice::Double angle, VTankObject::Direction direction, 
    const Ice::Current&)
{
    try {
        const tank_ptr tank = Players::tanks.get(id);
        tank->get_player_info()->refresh_timeout();
        
        if (MapManager::is_rotating()) {
            return;
        }
        
        Players::rotate(id, timestamp, angle, direction);
    }
    HANDLE_UNCAUGHT_EXCEPTIONS;
}

void Player::SpinTurret(Ice::Long timestamp, Ice::Double angle, VTankObject::Direction direction, 
    const Ice::Current&)
{
    try {
        const tank_ptr tank = Players::tanks.get(id);
        tank->get_player_info()->refresh_timeout();

        if (MapManager::is_rotating()) {
            return;
        }

        Players::spin_turret(id, timestamp, angle, direction);
    }
    HANDLE_UNCAUGHT_EXCEPTIONS;
}

void Player::Fire(Ice::Long timestamp, const VTankObject::Point &point, const Ice::Current&)
{
    try {
        const tank_ptr tank = Players::tanks.get(id);
        tank->get_player_info()->refresh_timeout();
        
        if (MapManager::is_rotating()) {
            return;
        }
		
		// TODO: Ask if the tank "can charge".
		/*charge_ptr charger = tank->get_charge_timer();
		if (charger->is_charging) {
			// TODO: Do something.
		}
		else {*/
			Players::fire(id, timestamp, point);
		//}
    }
    HANDLE_UNCAUGHT_EXCEPTIONS;
}

void Player::SendMessage(const std::string& message, const Ice::Current&)
{
    //TODO: We'll process commands in a more complex way in the future.
    // For now, just distribute what they typed.
    VTankObject::VTankColor message_color;
    message_color.red   = 125;
    message_color.green = 125;
    message_color.blue  = 255;
    
    try {
        const tank_ptr tank = Players::tanks.get(id);
        tank->get_player_info()->refresh_timeout();
        
        if (message == "/nodes") {
            const tank_array tanks = Players::tanks.get_tank_list();
            for (std::vector<tank_ptr>::size_type i = 0; i < tanks.size(); i++) {
                std::stringstream formatter;
                formatter << tanks[i]->get_name() << ": " << tanks[i]->get_node_id();

                tank->get_player_info()->get_callback()->ChatMessage( 
                    formatter.str(), message_color);
            }
        }
        else if (message == "/positions" || message == "/pos") {
            const tank_array tanks = Players::tanks.get_tank_list();
            for (std::vector<tank_ptr>::size_type i = 0; i < tanks.size(); i++) {
                std::stringstream formatter;
                formatter << tanks[i]->get_name() << ": (" 
                    << tanks[i]->get_position().x << ", " << tanks[i]->get_position().y << ")";

                tank->get_player_info()->get_callback()->ChatMessage( 
                    formatter.str(), message_color);
            }
        }
		else if (message == "/forcerotate" || message == "/rotate") {
			Players::force_timer_zero();
		}
        else {
            Notifier::blanket_notify_chat_message(
                tank->get_name() + ": " + message, message_color);
        }
    }
    HANDLE_UNCAUGHT_EXCEPTIONS;
}

void Player::StartCharging(const Ice::Current &)
{
	try {
		const tank_ptr tank = Players::tanks.get(id);
		tank->get_player_info()->refresh_timeout();
		if (tank->get_weapon().max_charge_time_seconds == 0) {
			// Weapon cannot charge: ignore packet.
			return;
		}

		if (MapManager::is_rotating()) {
			return;
		}

		const charge_ptr charge = tank->get_charge_timer();
		charge->start_charging();
	}
	catch (const TankNotExistException &) {
		// Ignore.
	}
	HANDLE_UNCAUGHT_EXCEPTIONS
}
