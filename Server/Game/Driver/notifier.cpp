/*!
    \file   notifier.cpp
    \brief  Implements functions that help notify players of events.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#include <master.hpp>
#include <notifier.hpp>
#include <playermanager.hpp>
#include <gamemanager.hpp>
#include <logger.hpp>
#include <macros.hpp>
#include <asynctemplate.hpp>

namespace Notifier {
    void handle_player_exception(const int id, const Ice::Exception &ex) 
    {
        std::ostringstream formatter;
        formatter << "Player #" << id << " was disconnected during an asynchronous "
            "message notification. Kicking the user off. Exception: " << ex.what();

        Logger::log(Logger::LOG_LEVEL_INFO, formatter.str());

        (void)Players::remove_player(id);
    }
    
    void blanket_notify_player_damaged(const int owner_id, const int projectile_id, 
        const int fired_by_id, const int damage_taken, const bool killing_blow)
    {
        const tank_array tanks = Players::tanks.get_tank_list();
        for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
            try {
                tank->get_player_info()->get_callback()->PlayerDamaged_async(
                    new PlayerAsyncCallback<
                        GameSession::AMI_ClientEventCallback_PlayerDamaged>(
                            tank->get_id(), handle_player_exception),
                    owner_id, projectile_id, fired_by_id, damage_taken, killing_blow);
            }
            catch (const Ice::Exception &e) {
                std::ostringstream formatter;
                formatter << "Exception thrown while notifying " 
                    << tank->get_name() << " of a player getting damaged. Removing him. "
                    "Exception details: " << e.what();

                Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
            }
            HANDLE_UNCAUGHT_EXCEPTIONS
        }
    }

    void blanket_notify_player_respawn(const int who, const VTankObject::Point &position)
    {
        const tank_array tanks = Players::tanks.get_tank_list();
        for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
            try {
                tank->get_player_info()->get_callback()->PlayerRespawned_async(
                    new PlayerAsyncCallback<
                        GameSession::AMI_ClientEventCallback_PlayerRespawned>(
                            tank->get_id(), handle_player_exception), who, position);
            }
            catch (const Ice::Exception &e) {
                std::ostringstream formatter;
                formatter << "Exception thrown while notifying " 
                    << tank->get_name() << " of a player respawning. Removing him. "
                    "Exception details: " << e.what();

                Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
            }
            HANDLE_UNCAUGHT_EXCEPTIONS
        }
    }

    void blanket_notify_player_left(const int id)
    {
        const tank_array tanks = Players::tanks.get_tank_list();
        for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
            if (tank->get_id() != id) {
                try {
                    tank->get_player_info()->get_callback()->PlayerLeft_async(
                        new PlayerAsyncCallback<
                            GameSession::AMI_ClientEventCallback_PlayerLeft>(
                                tank->get_id(), handle_player_exception), id);
                }
                catch (const Ice::Exception &e) {
                    std::ostringstream formatter;
                    formatter << "Exception thrown while notifying " 
                        << tank->get_name() << " of a player leaving. Removing him. "
                        "Exception details: " << e.what();

                    Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
                }
                HANDLE_UNCAUGHT_EXCEPTIONS
            }
        }
    }

    void blanket_notify_player_joined(const tank_ptr new_tank)
    {
        const tank_array tanks = Players::tanks.get_tank_list();
        for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
            if (tank->get_id() != new_tank->get_id()) {
                try {
                    tank->get_player_info()->get_callback()->PlayerJoined_async(
                        new PlayerAsyncCallback<
                            GameSession::AMI_ClientEventCallback_PlayerJoined>(
                                tank->get_id(), handle_player_exception), 
                                new_tank->get_tank_object());
                }
                catch (const Ice::Exception &e) {
                    std::ostringstream formatter;
                    formatter << "Exception thrown while notifying " 
                        << tank->get_name() << " of a player joining. Removing him. "
                        "Exception details: " << e.what();

                    Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
                }
                HANDLE_UNCAUGHT_EXCEPTIONS
            }
        }
    }

    void blanket_notify_rotate_map()
    {
        const tank_array tanks = Players::tanks.get_tank_list();
        for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
            try {
                tank->get_player_info()->get_callback()->RotateMap_async(
                    new PlayerAsyncCallback<
                        GameSession::AMI_ClientEventCallback_RotateMap>(
                            tank->get_id(), handle_player_exception));
            }
            catch (const Ice::Exception &e) {
                std::ostringstream formatter;
                formatter << "Exception thrown while notifying " 
                    << tank->get_name() << " of a map rotation. Removing him. "
                    "Exception details: " << e.what();

                Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
            }
            HANDLE_UNCAUGHT_EXCEPTIONS
        }
    }
    
    void blanket_notify_chat_message(const std::string &message, 
        const VTankObject::VTankColor &color)
    {
        const tank_array tanks = Players::tanks.get_tank_list();
        for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
            try {
                tank->get_player_info()->get_callback()->ChatMessage_async(
                    new PlayerAsyncCallback<
                        GameSession::AMI_ClientEventCallback_ChatMessage>(
                            tank->get_id(), handle_player_exception), message, color);
            }
            catch (const Ice::Exception &e) {
                std::ostringstream formatter;
                formatter << "Exception thrown while notifying " 
                    << tank->get_name() << " of a chat message. Removing him. "
                    "Exception details: " << e.what();

                Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
            }
            HANDLE_UNCAUGHT_EXCEPTIONS
        }
    }

    void notify_chat_message(const tank_array &tanks, const std::string &message, 
        const VTankObject::VTankColor &color)
    {
        for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
            try {
                tank->get_player_info()->get_callback()->ChatMessage_async(
                    new PlayerAsyncCallback<
                        GameSession::AMI_ClientEventCallback_ChatMessage>(
                            tank->get_id(), handle_player_exception), message, color);
            }
            catch (const Ice::Exception &e) {
                std::ostringstream formatter;
                formatter << "Exception thrown while notifying " 
                    << tank->get_name() << " of a chat message. Removing him. "
                    "Exception details: " << e.what();

                Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
            }
            HANDLE_UNCAUGHT_EXCEPTIONS
        }
    }
	
	void blanket_notify_create_projectile(const int owner_id, const int projectile_id,
			const int projectile_type_id, const VTankObject::Point &end_point) {
		const tank_array tanks = Players::tanks.get_tank_list();
        for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const std::string name = tanks[i]->get_name();
            try {
		        tanks[i]->get_player_info()->get_callback()->CreateProjectile_async(
                    new VoidAsyncCallback<
                        GameSession::AMI_ClientEventCallback_CreateProjectile>(),
                    owner_id, projectile_id, projectile_type_id, end_point);
            }
            catch (const Ice::Exception &e) {
                std::ostringstream formatter;
                formatter << name << " threw an exception while processing "
                    "creation of projectiles: " << e.what();
                Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
                
                (void)Players::remove_player(tanks[i]->get_id());
            }
	    }
	}

	void blanket_notify_utility_spawn(const tank_array &tanks, int utilityID,
		const VTankObject::Utility &util, const VTankObject::Point &position)
	{
		for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
            try {
                tank->get_player_info()->get_callback()->SpawnUtility_async(
                    new PlayerAsyncCallback<
						GameSession::AMI_ClientEventCallback_SpawnUtility>(
                            tank->get_id(), handle_player_exception), utilityID, util, position);
            }
            catch (const Ice::Exception &e) {
                std::ostringstream formatter;
                formatter << "Exception thrown while notifying " 
                    << tank->get_name() << " of util spawn. Removing him. "
                    "Exception details: " << e.what();

                Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
            }
            HANDLE_UNCAUGHT_EXCEPTIONS
        }
	}

	void blanket_notify_apply_utility(const tank_array &tanks, int tankID, int utilityID,
		const VTankObject::Utility &util)
	{
		for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
            try {
                tank->get_player_info()->get_callback()->ApplyUtility_async(
                    new PlayerAsyncCallback<
						GameSession::AMI_ClientEventCallback_ApplyUtility>(
                            tank->get_id(), handle_player_exception), utilityID, util, tankID);
            }
            catch (const Ice::Exception &e) {
                std::ostringstream formatter;
                formatter << "Exception thrown while notifying " 
                    << tank->get_name() << " of util apply. Removing him. "
                    "Exception details: " << e.what();

                Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
            }
            HANDLE_UNCAUGHT_EXCEPTIONS
        }
	}

	void notify_utility_spawn(const tank_ptr &tank, int utilityID,
		const VTankObject::Utility &util, const VTankObject::Point &position)
	{
        try {
            tank->get_player_info()->get_callback()->SpawnUtility_async(
                new PlayerAsyncCallback<
					GameSession::AMI_ClientEventCallback_SpawnUtility>(
                        tank->get_id(), handle_player_exception), utilityID, util, position);
        }
        catch (const Ice::Exception &e) {
            std::ostringstream formatter;
            formatter << "Exception thrown while notifying " 
                << tank->get_name() << " of util spawn. Removing him. "
                "Exception details: " << e.what();

            Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
        }
        HANDLE_UNCAUGHT_EXCEPTIONS
	}

	void notify_flag_spawned(const tank_ptr &tank, const VTankObject::Point &position,
		const GameSession::Alliance &flagColor)
	{
		try {
			tank->get_player_info()->get_callback()->FlagSpawned_async(
				new PlayerAsyncCallback<
					GameSession::AMI_ClientEventCallback_FlagSpawned>(
						tank->get_id(), handle_player_exception), position, flagColor);
		}
		catch (const Ice::Exception &e) {
			std::ostringstream formatter;
			formatter << "Exception thrown while notifying " 
				<< tank->get_name() << " of a flag being spawned. Removing him. "
				"Exception details: " << e.what();

			Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
		}
		HANDLE_UNCAUGHT_EXCEPTIONS
	}

	void notify_flag_picked_up(const tank_ptr &tank, int pickedUpId,
		const GameSession::Alliance &flagColor)
	{
		try {
			tank->get_player_info()->get_callback()->FlagPickedUp_async(
				new PlayerAsyncCallback<
					GameSession::AMI_ClientEventCallback_FlagPickedUp>(
						tank->get_id(), handle_player_exception), pickedUpId, flagColor);
		}
		catch (const Ice::Exception &e) {
			std::ostringstream formatter;
			formatter << "Exception thrown while notifying " 
				<< tank->get_name() << " of a flag being spawned. Removing him. "
				"Exception details: " << e.what();

			Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
		}
		HANDLE_UNCAUGHT_EXCEPTIONS
	}

	void blanket_notify_flag_dropped(const tank_array &tanks, int droppedBy,
		const VTankObject::Point &position, const GameSession::Alliance &flagColor)
	{
		for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
			try {
				tank->get_player_info()->get_callback()->FlagDropped_async(
					new PlayerAsyncCallback<
						GameSession::AMI_ClientEventCallback_FlagDropped>(
							tank->get_id(), handle_player_exception), droppedBy, position, flagColor);
			}
			catch (const Ice::Exception &e) {
				std::ostringstream formatter;
				formatter << "Exception thrown while notifying " 
					<< tank->get_name() << " of flag dropping. Removing him. "
					"Exception details: " << e.what();

				Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
			}
			HANDLE_UNCAUGHT_EXCEPTIONS
		}
	}

	void blanket_notify_flag_returned(const tank_array &tanks, int returnedById, 
		const GameSession::Alliance &flagColor)
	{
		for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
			try {
				tank->get_player_info()->get_callback()->FlagReturned_async(
					new PlayerAsyncCallback<
						GameSession::AMI_ClientEventCallback_FlagReturned>(
							tank->get_id(), handle_player_exception), returnedById, flagColor);
			}
			catch (const Ice::Exception &e) {
				std::ostringstream formatter;
				formatter << "Exception thrown while notifying " 
					<< tank->get_name() << " of flag returning. Removing him. "
					"Exception details: " << e.what();

				Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
			}
			HANDLE_UNCAUGHT_EXCEPTIONS
		}
	}

	void blanket_notify_flag_picked_up(const tank_array &tanks, int pickedUpById,
		const GameSession::Alliance &flagColor)
	{
		for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
			try {
				tank->get_player_info()->get_callback()->FlagPickedUp_async(
					new PlayerAsyncCallback<
						GameSession::AMI_ClientEventCallback_FlagPickedUp>(
							tank->get_id(), handle_player_exception), pickedUpById, flagColor);
			}
			catch (const Ice::Exception &e) {
				std::ostringstream formatter;
				formatter << "Exception thrown while notifying " 
					<< tank->get_name() << " of a flag being picked up. Removing him. "
					"Exception details: " << e.what();

				Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
			}
			HANDLE_UNCAUGHT_EXCEPTIONS
		}
	}

	void blanket_notify_flag_captured(const tank_array &tanks, int capturedById,
		const GameSession::Alliance &flagColor)
	{
		for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
			try {
				tank->get_player_info()->get_callback()->FlagCaptured_async(
					new PlayerAsyncCallback<
						GameSession::AMI_ClientEventCallback_FlagCaptured>(
							tank->get_id(), handle_player_exception), capturedById, flagColor);
			}
			catch (const Ice::Exception &e) {
				std::ostringstream formatter;
				formatter << "Exception thrown while notifying " 
					<< tank->get_name() << " of a flag being captured. Removing him. "
					"Exception details: " << e.what();

				Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
			}
			HANDLE_UNCAUGHT_EXCEPTIONS
		}
	}

	void blanket_notify_flag_spawned(const tank_array &tanks, 
		const VTankObject::Point &position, const GameSession::Alliance &flagColor)
	{
		for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
			try {
				tank->get_player_info()->get_callback()->FlagSpawned_async(
					new PlayerAsyncCallback<
						GameSession::AMI_ClientEventCallback_FlagSpawned>(
							tank->get_id(), handle_player_exception), position, flagColor);
			}
			catch (const Ice::Exception &e) {
				std::ostringstream formatter;
				formatter << "Exception thrown while notifying " 
					<< tank->get_name() << " of a flag being spawned. Removing him. "
					"Exception details: " << e.what();

				Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
			}
			HANDLE_UNCAUGHT_EXCEPTIONS
		}
	}

	void blanket_notify_flag_despawned(const tank_array &tanks, const GameSession::Alliance &flagColor)
	{
		for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
			try {
				tank->get_player_info()->get_callback()->FlagDespawned_async(
					new PlayerAsyncCallback<
						GameSession::AMI_ClientEventCallback_FlagDespawned>(
							tank->get_id(), handle_player_exception), flagColor);
			}
			catch (const Ice::Exception &e) {
				std::ostringstream formatter;
				formatter << "Exception thrown while notifying " 
					<< tank->get_name() << " of a flag being despawned. Removing him. "
					"Exception details: " << e.what();

				Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
			}
			HANDLE_UNCAUGHT_EXCEPTIONS
		}
	}

	void blanket_notify_base_captured(const tank_array &tanks,
		const GameSession::Alliance &old_base_color, const GameSession::Alliance &new_base_color,
		int base_id, int capturer_id)
	{
		for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
			try {
				tank->get_player_info()->get_callback()->BaseCaptured_async(
					new PlayerAsyncCallback<
						GameSession::AMI_ClientEventCallback_BaseCaptured>(
							tank->get_id(), handle_player_exception),
					old_base_color, new_base_color, base_id, capturer_id);
			}
			catch (const Ice::Exception &e) {
				std::ostringstream formatter;
				formatter << "Exception thrown while notifying " 
					<< tank->get_name() << " of BaseCaptured. Removing him. "
					"Exception details: " << e.what();

				Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
			}
			HANDLE_UNCAUGHT_EXCEPTIONS
		}
	}

	void blanket_notify_set_base_status(const tank_array &tanks,
		const GameSession::Alliance &base_color, const int base_id, const int health)
	{
		for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
			try {
				tank->get_player_info()->get_callback()->SetBaseHealth_async(
					new PlayerAsyncCallback<
						GameSession::AMI_ClientEventCallback_SetBaseHealth>(
							tank->get_id(), handle_player_exception),
					base_color, base_id, health);
			}
			catch (const Ice::Exception &e) {
				std::ostringstream formatter;
				formatter << "Exception thrown while notifying " 
					<< tank->get_name() << " of SetBaseStatus. Removing him. "
					"Exception details: " << e.what();

				Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
			}
			HANDLE_UNCAUGHT_EXCEPTIONS
		}
	}

	void notify_set_base_status(const tank_ptr &tank,
		const GameSession::Alliance &base_color, const int base_id, const int health)
	{
		try {
			tank->get_player_info()->get_callback()->SetBaseHealth_async(
				new PlayerAsyncCallback<
					GameSession::AMI_ClientEventCallback_SetBaseHealth>(
						tank->get_id(), handle_player_exception),
				base_color, base_id, health);
		}
		catch (const Ice::Exception &e) {
			std::ostringstream formatter;
			formatter << "Exception thrown while notifying " 
				<< tank->get_name() << " of SetBaseHealth. Removing him. "
				"Exception details: " << e.what();

			Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
		}
		HANDLE_UNCAUGHT_EXCEPTIONS
	}

	void blanket_notify_damage_base(const tank_array &tanks,
		const GameSession::Alliance &base_color, int base_id, int damage, int projectile_id, 
		int player_id, bool is_destroyed)
	{
		for (tank_array::size_type i = 0; i < tanks.size(); i++) {
            const tank_ptr tank = tanks[i];
			try {
				tank->get_player_info()->get_callback()->DamageBase_async(
					new PlayerAsyncCallback<
						GameSession::AMI_ClientEventCallback_DamageBase>(
							tank->get_id(), handle_player_exception),
					base_color, base_id, damage, projectile_id, player_id, is_destroyed);
			}
			catch (const Ice::Exception &e) {
				std::ostringstream formatter;
				formatter << "Exception thrown while notifying " 
					<< tank->get_name() << " of DamageBase. Removing him. "
					"Exception details: " << e.what();

				Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
			}
			HANDLE_UNCAUGHT_EXCEPTIONS
		}
	}

	void notify_reset_position(const tank_ptr &player, const VTankObject::Point &pos)
	{
		try {
			player->get_player_info()->get_callback()->ResetPosition_async(
				new PlayerAsyncCallback<
					GameSession::AMI_ClientEventCallback_ResetPosition>(
						player->get_id(), handle_player_exception), pos);
		}
		catch (const Ice::Exception &e) {
			std::ostringstream formatter;
			formatter << "Exception thrown while notifying " 
				<< player->get_name() << " of ResetPosition. Removing him. "
				"Exception details: " << e.what();

			Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());
		}
		HANDLE_UNCAUGHT_EXCEPTIONS
	}

	void blanket_notify_player_moved(const int who_moved, const VTankObject::Point &pos, 
		const VTankObject::Direction &direction)
	{
		const tank_array tanks = Players::tanks.get_tank_list();
        for (tank_array::size_type i = 0; i < tanks.size(); i++) {
			const tank_ptr tank = tanks[i];
            try {
                if (who_moved != tank->get_id()) {
			        tank->get_player_info()->get_callback()->PlayerMove_async(
                        new VoidAsyncCallback<
                            GameSession::AMI_ClientEventCallback_PlayerMove>(),
                        who_moved, pos, direction);
                }
            }
            catch (const Ice::Exception &e) {
                std::ostringstream formatter;
                formatter << tank->get_name() 
                    << " threw an exception while processing PlayerMove: " << e.what();
                Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());

                (void)Players::remove_player(tank->get_id());
            }
	    }
	}

	void blanket_notify_end_round(const GameSession::Alliance &winner)
	{
		const tank_array tanks = Players::tanks.get_tank_list();
        for (tank_array::size_type i = 0; i < tanks.size(); i++) {
			const tank_ptr tank = tanks[i];
            try {
		        tank->get_player_info()->get_callback()->EndRound_async(
                    new VoidAsyncCallback<
                        GameSession::AMI_ClientEventCallback_EndRound>(), winner);
            }
            catch (const Ice::Exception &e) {
                std::ostringstream formatter;
                formatter << tank->get_name() 
                    << " threw an exception while processing EndRound: " << e.what();
                Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());

                (void)Players::remove_player(tank->get_id());
            }
	    }
	}

	void blanket_notify_spawn_env_effect(int env_id, int type_id, int owner_id,
		const VTankObject::Point &position)
	{
		const tank_array tanks = Players::tanks.get_tank_list();
		for (tank_array::size_type i = 0; i < tanks.size(); ++i) {
			const tank_ptr tank = tanks[i];
            try {
		        tank->get_player_info()->get_callback()->SpawnEnvironmentEffect_async(
                    new VoidAsyncCallback<
                        GameSession::AMI_ClientEventCallback_SpawnEnvironmentEffect>(),
							env_id, type_id, owner_id, position);
            }
            catch (const Ice::Exception &e) {
                std::ostringstream formatter;
                formatter << tank->get_name() 
                    << " threw an exception while processing SpawnEnvEffect: " << e.what();
                Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());

                (void)Players::remove_player(tank->get_id());
            }
		}
	}

	void blanket_notify_create_projectiles(const GameSession::ProjectileDamageList &list)
	{
		const tank_array tanks = Players::tanks.get_tank_list();
		for (tank_array::size_type i = 0; i < tanks.size(); ++i) {
			const tank_ptr tank = tanks[i];
            try {
		        tank->get_player_info()->get_callback()->CreateProjectiles_async(
                    new VoidAsyncCallback<
                        GameSession::AMI_ClientEventCallback_CreateProjectiles>(), list);
            }
            catch (const Ice::Exception &e) {
                std::ostringstream formatter;
                formatter << tank->get_name() 
                    << " threw an exception while processing CreateProjectiles: " << e.what();
                Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());

                (void)Players::remove_player(tank->get_id());
            }
		}
	}

	void blanket_notify_damage_base_by_env(const GameSession::Alliance &team,
		const int base_id, const int env_id, const int damage, const bool killing_blow)
	{
		const tank_array tanks = Players::tanks.get_tank_list();
		for (tank_array::size_type i = 0; i < tanks.size(); ++i) {
			const tank_ptr tank = tanks[i];
            try {
		        tank->get_player_info()->get_callback()->DamageBaseByEnvironment_async(
                    new VoidAsyncCallback<
                        GameSession::AMI_ClientEventCallback_DamageBaseByEnvironment>(),
						team, base_id, env_id, damage, killing_blow);
            }
            catch (const Ice::Exception &e) {
                std::ostringstream formatter;
                formatter << tank->get_name() 
                    << " threw an exception while processing DamageBaseByEnv: " << e.what();
                Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());

                (void)Players::remove_player(tank->get_id());
            }
		}
	}

	void blanket_notify_damage_player_by_env(const int victim_id,
		const int env_id, const int damage, const bool killing_blow)
	{
		const tank_array tanks = Players::tanks.get_tank_list();
		for (tank_array::size_type i = 0; i < tanks.size(); ++i) {
			const tank_ptr tank = tanks[i];
            try {
		        tank->get_player_info()->get_callback()->PlayerDamagedByEnvironment_async(
                    new VoidAsyncCallback<
                        GameSession::AMI_ClientEventCallback_PlayerDamagedByEnvironment>(),
						victim_id, env_id, damage, killing_blow);
            }
            catch (const Ice::Exception &e) {
                std::ostringstream formatter;
                formatter << tank->get_name() 
                    << " threw an exception while processing DamagePlayerByEnv: " << e.what();
                Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());

                (void)Players::remove_player(tank->get_id());
            }
		}
	}
}
