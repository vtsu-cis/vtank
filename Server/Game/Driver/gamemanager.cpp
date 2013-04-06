/*!
    \file gamemanager.cpp
    \brief Implements functions that perform game management and calculations.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#include <master.hpp>
#include <playermanager.hpp>
#include <gamemanager.hpp>
#include <logger.hpp>
#include <player.hpp>
#include <utility.hpp>
#include <notifier.hpp>
#include <timer.hpp>
#include <asynctemplate.hpp>
#include <tankmanager.hpp>
#include <server.hpp>
#include <mapmanager.hpp>
#include <pointmanager.hpp>
#include <ctf.hpp>
#include <ctb.hpp>
#include <weaponsettings.hpp>

namespace Players
{
    NodeManager nodes;
	Weapon_Settings weapon_data;
	Game_Handler *game_handler = NULL;

    /*!
        The Gamespace namespace holds functions and members that only the
        GameManager should access.
    */
    namespace Gamespace
    {
        Projectile_Manager projectiles;
		UtilityManager utility_manager;
        GameTimer timer;
		std::vector<ActiveUtility> active_utils;

        //! Threadpool for player tasks. This is where most threads will go.
        boost::threadpool::pool player_pool(GAME_THREADS);

		Game_Handler *create_game_handler()
		{
			const VTankObject::GameMode mode = MapManager::get_current_mode();
			if (mode == VTankObject::CAPTURETHEFLAG) {
				return new CTF_Helper(const_cast<Map *>(MapManager::get_current_map()), tank_array());
			}
			else if (mode == VTankObject::CAPTURETHEBASE) {
				return new CTB_Helper(const_cast<Map *>(MapManager::get_current_map()));
			}

			return NULL;
		}

        /*!
            Advance a player's position based on how long they have been moving.
            It's known how long they have been moving by the timestamp given by
            the client. This function does not check for cheating.
            \param point Reference to a VTankObject::Point object, which holds the new values.
            \param angle Angle that the tank is facing.
            \param direction Direction that the tank is moving towards.
            \param speed Already-calculated velocity of the tank.
            \param delta Time factor for speed.
        */
        void advance_position(VTankObject::Point &point, double &angle, 
            const VTankObject::Direction direction, const double &speed, 
            const double &delta)
        {
			const double calc_speed = speed * delta;
            switch (direction) {
            case VTankObject::FORWARD:
                point.x += (cos(angle) * calc_speed);
				point.y += (sin(angle) * calc_speed);
                break;

            case VTankObject::REVERSE:
				point.x -= (cos(angle) * calc_speed);
				point.y -= (sin(angle) * calc_speed);
                break;

            case VTankObject::LEFT:
				angle += calc_speed;
                break;

            case VTankObject::RIGHT:
				angle -= calc_speed;
                break;

            case VTankObject::NONE:
                break;
            }
        }
    
        /*!
            Perform checks on the tank's attempted movement to see if that move is legal.
            \param tank Tank to test.
            \param position Position that the tank wants to move to.
            \return True if the move was legal, false otherwise.
        */
        bool legal_move(const tank_ptr tank, const VTankObject::Point &position)
        {
            const VTankObject::Point old_position = tank->get_position();

            const double distance = 
                sqrt(pow(position.x - old_position.x, 2) + 
                     pow(position.x - old_position.x, 2));

            return distance <= MAX_LEGAL_DISTANCE;
        }

        /*
            The following functions are meant to be run as a threadpool scheduled task.
            Documentation is available in the gamemanager.hpp file.
        */
        
        //! Process a movement sent by the client.
        void task_process_movement(const int& id, const Ice::Long& timestamp, 
            const VTankObject::Direction direction, VTankObject::Point position)
        {
            Logger::Stack_Logger stack("task_process_movement()", false);

            try {
                tank_ptr tank = Players::tanks.get(id);
                if (!tank->is_alive()) {
                    // Can't process the tank if he's not alive.
                    return;
                }

                tank->set_movement_direction(direction);

                // Based on the offset of the player's clock, change the timestamp.
                const Ice::Long new_timestamp = tank->transform_time(timestamp);
                const double delta = 
                    (IceUtil::Time::now().toMilliSeconds() - new_timestamp) / 1000.0;

                //float new_velocity = DEFAULT_VELOCITY;
                //calculate_velocity(tank->get_speed_factor(), new_velocity);
                
                double tank_angle = tank->get_angle();
                advance_position(position, tank_angle, direction, tank->get_velocity(), delta);

                /*if (!legal_move(tank, position)) {
                    // Discard the move and notify player of that.
                    tank->get_player_info()->get_callback()->ResetPosition_async(
                        new VoidAsyncCallback<
                            GameSession::AMI_ClientEventCallback_ResetPosition>(),
                        tank->get_position());

                    return;
                }*/

                tank->set_position(position);

                nodes.process_position(tank);

			    // TODO: This should not distribute the message here.
                const tank_array tanks = Players::tanks.get_tank_list();
                for (tank_array::size_type i = 0; i < tanks.size(); i++) {
                    try {
                        if (id != tanks[i]->get_id()) {
					        tanks[i]->get_player_info()->get_callback()->PlayerMove_async(
                                new VoidAsyncCallback<
                                    GameSession::AMI_ClientEventCallback_PlayerMove>(),
                                id, position, direction);
                        }
                    }
                    catch (const Ice::Exception &e) {
                        std::ostringstream formatter;
                        formatter << tanks[i]->get_name() 
                            << " threw an exception while processing rotation: " << e.what();
                        Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());

                        (void)Players::remove_player(tanks[i]->get_id());
                    }
			    }
            }
            catch (const TankNotExistException &) {
                // Can't do anything: Tank doesn't exist.
            }
        }
        
        //! Process a movement sent by the client.
        void task_process_rotation(const int& id, const Ice::Long& timestamp, 
            const Ice::Double& angle, const VTankObject::Direction direction)
        {
            Logger::Stack_Logger stack("task_process_rotation()", false);
            
            try {
                tank_ptr tank = Players::tanks.get(id);
                if (!tank->is_alive()) {
                    // Can't process the tank if he's not alive.
                    return;
                }

                tank->set_rotation_direction(direction);
                
                // Based on the offset of the player's clock, change the timestamp.
                const Ice::Long new_timestamp = tank->transform_time(timestamp);
                
                double new_angle = angle;
                VTankObject::Point position = tank->get_position();
                advance_position(position, new_angle, direction, 
                    tank->get_angular_velocity(), timer.get_delta_time());

                tank->set_angle(new_angle);

			    // TODO: This should not distribute the message immediately.
                const tank_array tanks = Players::tanks.get_tank_list();
                for (tank_array::size_type i = 0; i < tanks.size(); i++) {
                    try {
                        if (id != tanks[i]->get_id()) {
					        tanks[i]->get_player_info()->get_callback()->PlayerRotate_async(
                                new VoidAsyncCallback<
                                    GameSession::AMI_ClientEventCallback_PlayerRotate>(),
                                id, new_angle, direction);
                        }
                    }
                    catch (const Ice::Exception &e) {
                        std::ostringstream formatter;
                        formatter << tanks[i]->get_name() << " threw an exception while processing "
                            "rotation: " << e.what();
                        Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());

                        (void)Players::remove_player(tanks[i]->get_id());
                    }
			    }
            }
            catch (const TankNotExistException &) {
                // Can't do anything: Tank doesn't exist.
            }
        }
        
        //! Process a projectile fired by a client.
        void task_process_fire(const int &id, const Ice::Long &timestamp, 
            const VTankObject::Point &point)
        {
            Logger::Stack_Logger stack("task_process_fire()", false);
            
            try {
                tank_ptr tank = Players::tanks.get(id);
                if (!tank->is_alive()) {
                    // Can't process the tank if he's not alive.
                    return;
                }

                // Calculate the angle of the projectile.
                VTankObject::Point position = tank->get_position();
                const double angle = atan2(point.y - position.y, point.x - position.x);

                // Based on the offset of the player's clock, change the timestamp.
                //const Ice::Long new_timestamp = player->transform_time(timestamp);
                //const double calc_x = cos(angle) * PROJECTILE_SPAWN_OFFSET;
                //const double calc_y = sin(angle) * PROJECTILE_SPAWN_OFFSET;
                //const double x = position.x + calc_x;
                //const double y = position.y + calc_y;
                
                position.x = (position.x + (cos(angle) * PROJECTILE_SPAWN_OFFSET));
                position.y = (position.y + (sin(angle) * PROJECTILE_SPAWN_OFFSET));

                const Weapon weapon = tank->get_weapon();

				if (weapon.projectiles_per_shot == 1) {
					// Only one projectile is fired.
					VTankObject::Point target;
					const int projectile_id = projectiles.add(
						tank->get_id(), angle, position, point, weapon, target);
					if (projectile_id < 0) {
						return;
					}

					Notifier::blanket_notify_create_projectile(
						id, projectile_id, weapon.projectile.id, target);
				}
				else {
					// Several projectiles are fired.
					GameSession::ProjectileDamageList projectile_list;
					for (int i = 0; i < weapon.projectiles_per_shot; ++i) {
						VTankObject::Point target;
						const int projectile_id = projectiles.add(
							tank->get_id(), angle, position, point, weapon, target);
						if (projectile_id < 0) {
							continue;
						}
						
						GameSession::ProjectileDamageInfo projectile;
						projectile.ownerId = tank->get_id();
						projectile.projectileId = projectile_id;
						projectile.projectileTypeId = weapon.projectile.id;
						projectile.spawnTimeMilliseconds = static_cast<Ice::Long>(
							weapon.interval_between_projectile_seconds * 1000.0f * i);
						projectile.target = target;

						projectile_list.push_back(projectile);
					}
					
					Notifier::blanket_notify_create_projectiles(projectile_list);
				}

				
            }
            catch (const TankNotExistException &) {
                // Can't do anything: tank doesn't exist.
            }
        }
		
		//! Generate a unique utility ID number.
		int generate_utility_id()
		{
			int utilityID = -1;
			bool uniqueFound = false;
			while (!uniqueFound) {
				uniqueFound = true;
				++utilityID;
				std::vector<ActiveUtility>::iterator i = active_utils.begin();
				for (; i != active_utils.end(); ++i) {
					if (i->id == utilityID) {
						uniqueFound = false;
						break;
					}
				}
			}

			return utilityID;
		}

		//! Handle utility spawning and the like.
		void handle_utility_spawning()
		{
			// Handle spawning of utilities.
			const std::vector<ActiveUtility>::size_type arbitrary_maximum = 7;
			if (active_utils.size() < arbitrary_maximum && utility_manager.is_ready()) {
				// Next utility ready to spawn.
				VTankObject::Utility util;
				VTankObject::Point pos;
				std::vector<VTankObject::Point> blacklist;
				const int blacklist_size = static_cast<int>(active_utils.size());
				for (int i = 0; i < blacklist_size; ++i) {
					blacklist.push_back(active_utils[i].pos);
				}

				utility_manager.get_next_spawn(util, pos, blacklist);

				ActiveUtility powerup = ActiveUtility(generate_utility_id(), util, pos);
				active_utils.push_back(powerup);

				Notifier::blanket_notify_utility_spawn(Players::tanks.get_tank_list(),
					powerup.id, util, pos);
			}
		}

		//! Check collision between tanks and utilities.
		void handle_utility_collision(const tank_array &tanks)
		{
			if (tanks.empty() || active_utils.empty()) {
				// Nothing to do.
				return;
			}

			std::vector<ActiveUtility>::iterator i = active_utils.begin();
			const int num_tanks = static_cast<int>(tanks.size());
			
			std::vector<int> to_remove;
			
			for (; i != active_utils.end(); ++i) {
				const ActiveUtility current_util = *i;
				const Utility::Rectangle rect(current_util.pos.x, current_util.pos.y, TILE_SIZE, TILE_SIZE);
				
				for (int j = 0; j < num_tanks; ++j) {
					// TODO: Loop through only nearby tanks a la NodeManager.
					try {
						const tank_ptr tank = tanks[j];
						if (!tank->is_alive()) {
							// Dead players cannot receive buffs.
							continue;
						}

						const VTankObject::Point position = tank->get_position();
						
						if (Utility::circle_to_rectangle_collision(position, TANK_SPHERE_RADIUS, rect)) {
							// A collision exists between player and tile which has utility.
							std::ostringstream formatter;
							formatter << "Utility " << current_util.util.model << " applied to " 
								<< tank->get_name();
							Logger::log(Logger::LOG_LEVEL_DEBUG, formatter.str());

							tank->apply_utility(current_util.util);
							Notifier::blanket_notify_apply_utility(tanks, tank->get_id(), 
								current_util.id, current_util.util);
							to_remove.push_back(i->id);
							break;
						}
					}
					catch (const TankNotExistException &) {
					}
				}
			}
			
			// Removed expired utilities.
			std::vector<int>::iterator j = to_remove.begin();
			for (; j != to_remove.end(); ++j) {
				for (i = active_utils.begin(); i != active_utils.end(); ++i) {
					if (i->id == *j) {
						active_utils.erase(i);
						break;
					}
				}
			}
		}
		
        //! Process each player.
        void process(const tank_ptr tank)
        {
            if (tank->is_alive()) {
				tank->check_utility();

                VTankObject::Point position = tank->get_position();
                double angle = tank->get_angle();
                const double delta = timer.get_delta_time();

                if (tank->get_movement_direction() != VTankObject::NONE) {
                    // Tank is moving.
                    advance_position(position, angle, tank->get_movement_direction(), 
                        tank->get_velocity(), delta);

                    tank->set_position(position);

                    nodes.process_position(tank);
                }

                if (tank->get_rotation_direction() != VTankObject::NONE) {
                    // Tank is rotating.
                    advance_position(position, angle, tank->get_rotation_direction(),
                        tank->get_angular_velocity(), delta);

                    tank->set_angle(angle);
                }
            }
            else {
                const long respawns_at = tank->get_respawn_time();
                const long now = static_cast<long>(IceUtil::Time::now().toMilliSeconds());

                if (now >= respawns_at) {
                    // Respawn.
					generate_spawn_position(tank);
                    nodes.process_position(tank);
                    tank->respawn();

                    Notifier::blanket_notify_player_respawn(
                        tank->get_id(), tank->get_position());
                }
            }
        }

        //! Advance the frame by one and perform new calculations.
        bool process_frame_task()
        {
            srand(static_cast<unsigned>(IceUtil::Time::now().toMilliSeconds()));

            try {
                if (Server::server.communicator()->isShutdown()) {
                    // Stop looping: The server has shut down.
                    Logger::log(Logger::LOG_LEVEL_INFO, 
                        "Communicator shut down -- stopping frame processor.");

                    return false;
                }

                timer.advance();

				projectiles.process(nodes, timer.get_delta_time());

				handle_utility_spawning();

                const tank_array tanks = Players::tanks.get_tank_list();
				// Do custom game mode updates if necessary.
				if (game_handler != NULL) {
					game_handler->update(tanks);
				}

                for (tank_array::size_type i = 0; i < tanks.size(); i++) {
                    try {
                        process(tanks[i]);
                    }
                    catch (const TankNotExistException &) {
                        // Do nothing. The tank has been removed.
                    }
                    catch (const Ice::Exception &) {
                        std::ostringstream formatter;
                        formatter << tanks[i]->get_name() << " disconnected during process(). "
                            "Removing the player.";

                        Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());

                        (void)Players::remove_player(tanks[i]->get_id());
                    }
                    HANDLE_UNCAUGHT_EXCEPTIONS
                }

				handle_utility_collision(tanks);

                if (timer.get_time() <= 0) {
					if (game_handler != NULL) {
						// Credit the winners with 
						const GameSession::Alliance winning_team = game_handler->get_winning_team();
						if (winning_team != GameSession::NONE) {
							for (tank_array::size_type i = 0; i < tanks.size(); ++i) {
								try {
									const tank_ptr tank = tanks[i];
									if (tank->get_team() == winning_team) {
										PointManager::add_objective_completed(tank->get_id());
									}
								}
								catch (const TankNotExistException &) {
									// Nothing to do but ignore it...
								}
							}
						}

						delete game_handler;
						game_handler = NULL;
					}

                    MapManager::set_rotating(true);
                    MapManager::rotate();

					utility_manager.update_map(MapManager::current_map);
					active_utils.clear();
					projectiles.reset();
                    Players::tanks.organize_teams();
					
					game_handler = create_game_handler();

					VTankObject::StatisticsList stats = PointManager::compile_and_calculate();
					if (stats.size() > 0) {
						try {
#ifdef DEBUG
							const VTankObject::StatisticsList::size_type size = stats.size();
							std::ostringstream formatter;
							formatter << "Sending statistics for ";
							for (VTankObject::StatisticsList::size_type i = 0; i < size; ++i) {
								formatter << stats[i].tankName;
								if (i + 1 < size) {
									formatter << ", ";
								}
							}
							formatter << ".";
							Logger::log(Logger::LOG_LEVEL_DEBUG, formatter.str());
#endif
							Server::mtg_service.get_proxy()->SendStatistics(stats);
						}
						catch (const Ice::Exception &ex) {
							std::ostringstream formatter;
							formatter << "Ice threw an exception at SendStatistics: " << ex.what();

							Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());
						}
					}
                    PointManager::reset();

                    // Generate a new position for each player.
                    const tank_array tanks = Players::tanks.get_tank_list();
                    for (tank_array::size_type i = 0; i < tanks.size(); i++) {
                        const tank_ptr tank = tanks[i];
						tank->set_ready(false);
                        tank->do_clock_sync();

                        tank->set_angle(0);
                        tank->set_health(DEFAULT_MAX_HEALTH);
                        tank->set_alive(true);
                        tank->set_movement_direction(VTankObject::NONE);
                        tank->set_rotation_direction(VTankObject::NONE);

						if (game_handler != NULL && game_handler->has_custom_spawn_points()) {
							game_handler->spawn(tank);
						}
						else {
							MapManager::generate_spawn_position(tank);
						}

                        nodes.process_position(tank);

                        PointManager::add_player(tank->get_id());
                    }

                    timer.reset();
                    MapManager::set_rotating(false);
                    
                    Notifier::blanket_notify_rotate_map();
                }
            }
			catch (const std::logic_error &ex) {
				std::ostringstream formatter;
				formatter << "Logic error: " << ex.what();
				Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());
			}
            catch (const Ice::Exception &) {
                Logger::log(Logger::LOG_LEVEL_ERROR, 
                    "Unexpected Ice exception in process_frame_task(), shutting down.");

                return false;
            }
            HANDLE_UNCAUGHT_EXCEPTIONS
            
            return true;
        }
    } // Gamespace

	Projectile_Manager *get_projectile_manager()
	{
		return &Gamespace::projectiles;
	}

	NodeManager *get_node_manager()
	{
		return &nodes;
	}

	void generate_spawn_position(const tank_ptr &player)
	{
		if (game_handler != NULL && game_handler->has_custom_spawn_points()) {
			game_handler->spawn(player);
		}
		else {
			MapManager::generate_spawn_position(player);
		}
	}

	std::vector<ActiveUtility> get_active_utilities()
	{
		return Gamespace::active_utils;
	}

	void send_status_to(const tank_ptr &tank)
	{
		if (game_handler != NULL) {
			game_handler->send_status_to(tank);
		}
	}

	Game_Handler *get_game_handler()
	{
		return game_handler;
	}

	int get_red_score()
	{
		if (game_handler == NULL) {
			return 0;
		}

		return game_handler->get_red_score();
	}

	int get_blue_score()
	{
		if (game_handler == NULL) {
			return 0;
		}

		return game_handler->get_blue_score();
	}

    void start_game()
    {
		Gamespace::utility_manager.update_map(MapManager::current_map);
		
		game_handler = Gamespace::create_game_handler();

		try {
			weapon_data.load();
		}
		catch (const std::exception &ex) {
			std::ostringstream formatter;
			formatter << "Cannot load weapon data: " << ex.what();
			Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());
		}

        // Process a new frame every so often.
        Gamespace::player_pool.schedule(boost::threadpool::looped_task_func(
			&Gamespace::process_frame_task, FRAME_PROCESS_INTERVAL));
    }

    void wait_for_tasks()
    {
        //Gamespace::player_pool.wait(1000);
    }

    double get_time_left()
    {
        return Gamespace::timer.get_time();
    }
    
    void move(const int& id, const Ice::Long& timestamp, 
        const VTankObject::Direction direction, const VTankObject::Point& position)
    {
        // Valid values are: FORWARD, REVERSE, STOP.
        if (direction == VTankObject::LEFT || direction == VTankObject::RIGHT) {
            throw Exceptions::BadInformationException("Invalid direction!");
        }

        Gamespace::player_pool.schedule(boost::bind<void>(Gamespace::task_process_movement, 
            id, timestamp, direction, position));
    }

    void rotate(const int& id, const Ice::Long& timestamp, const Ice::Double& angle, 
        const VTankObject::Direction direction)
    {
        // Valid values are: LEFT, RIGHT, STOP.
        if (direction == VTankObject::FORWARD || direction == VTankObject::REVERSE) {
            throw Exceptions::BadInformationException("Invalid direction!");
        }

        Gamespace::player_pool.schedule(boost::bind<void>(Gamespace::task_process_rotation, 
            id, timestamp, angle, direction));
    }

    void spin_turret(const int &id, const Ice::Long &timestamp, const Ice::Double &angle,
        const VTankObject::Direction direction)
    {
        // Valid values are: LEFT, RIGHT, STOP.
        if (direction == VTankObject::FORWARD || direction == VTankObject::REVERSE) {
            throw Exceptions::BadInformationException("Invalid direction!");
        }
    }

    void fire(const int &id, const Ice::Long &timestamp, const VTankObject::Point &point)
    {
        Gamespace::player_pool.schedule(boost::bind<void>(Gamespace::task_process_fire, 
            id, timestamp, point));
    }

	void update_utility_list(const VTankObject::UtilityList &list)
	{
		Gamespace::utility_manager.update_utility_list(list);
	}

	Weapon_Settings *get_weapon_data()
	{
		return &weapon_data;
	}

	void force_timer_zero()
	{
		Gamespace::timer.force_timer_to_zero();
	}
}
