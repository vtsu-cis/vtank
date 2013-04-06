/*!
	\file   ctf.hpp
	\brief  Capture the Flag helper class.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef CTF_HPP
#define CTF_HPP

#include <utility.hpp>
#include <gamehandler.hpp>

#define RED_FLAG_EVENT_ID 4
#define BLUE_FLAG_EVENT_ID 5
#define FLAG_RADIUS 30.0f
#define FLAG_SPAWN_RADIUS 40.0f
#define DESPAWN_TIME 5000 /* 5 seconds. */

//! Sets the flag state for each flag.
namespace CTF {
	enum Flag_State
	{
		DESPAWNED = 0,
		STATIONARY,
		HELD
	};
}

//! CTF_Helper is a Capture the Flag game mode helper class.
/*! It essentially runs a game of Capture the Flag. While CTF_Helper is not
	a singleton, an instance should only be used once per game simulation.
*/
class CTF_Helper : public Game_Handler
{
private:
	Map *map;
	int holder_red_ID;
	int holder_blue_ID;
	int score_red;
	int score_blue;
	CTF::Flag_State red_flag_state;
	CTF::Flag_State blue_flag_state;
	VTankObject::Point red_spawn_position;
	VTankObject::Point blue_spawn_position;
	VTankObject::Point red_flag_position;
	VTankObject::Point blue_flag_position;
	bool red_flag_at_home;
	bool blue_flag_at_home;
	double next_spawn;
	
	//! Look at the current map and obtain the spawn points for red and blue.
	/*!
		\param red [out] Red position.
		\param blue [out] Blue position.
	*/
	void get_flag_spawn_points(VTankObject::Point &red, VTankObject::Point &blue)
	{
		VTANK_ASSERT(map != NULL);

		bool red_found = false;
		bool blue_found = false;
		for (int y = 0; y < map->get_height(); ++y) {
			for (int x = 0; x < map->get_width(); ++x) {
				const Tile tile = map->get_tile(x, y);
				if (tile.event_id == RED_FLAG_EVENT_ID) {
					// Ensure there is only one spawn point for red flags.
					VTANK_ASSERT(!red_found);
					VTankObject::Point pos;
					pos.x = (x * TILE_SIZE) + (TILE_SIZE / 2);
					pos.y = -((y * TILE_SIZE) + (TILE_SIZE / 2));
					red = pos;
					red_found = true;
				}
				else if (tile.event_id == BLUE_FLAG_EVENT_ID) {
					// Ensure there is only one spawn point for blue flags.
					VTANK_ASSERT(!blue_found);
					VTankObject::Point pos;
					pos.x = (x * TILE_SIZE) + (TILE_SIZE / 2);
					pos.y = -((y * TILE_SIZE) + (TILE_SIZE / 2));
					blue = pos;
					blue_found = true;
				}

				if (red_found && blue_found) {
					goto done;
				}
			}
		}

done:
		// It's a bug if CTF_Helper is created on a non-Capture the Flag map, so this
		// check should ensure we catch the bug.
		VTANK_ASSERT(red_found && blue_found);
	}
	
	//! Attempt to find a tank by it's ID in a given tank list.
	/*!
		\param tank_list List of tanks to search from.
		\param tank_id ID of the tank to search for.
		\param found [out] True if found; false if not.
		\return Index in the tank list of the tank. Ignore this value if 'found' is false.
	*/
	const tank_array::size_type find_tank_by_id(const tank_array &tank_list, int tank_id, bool &found)
	{
		tank_array::size_type i;
		for (i = 0; i < tank_list.size(); ++i) {
			if (tank_list[i]->get_id() == tank_id) {
				return i;
			}
		}

		found = false;
		return 0;
	}
	
	//! Gets the team which the given tank is NOT on.
	/*!
		\param tank Tank to get the opposite team of.
		\return The alliance which the given tank is not a part of.
	*/
	const GameSession::Alliance get_opposite_team(const tank_ptr &tank)
	{
		if (tank->get_team() == GameSession::RED) {
			return GameSession::BLUE;
		}

		return GameSession::RED;
	}

	//! Process the flag in a way compatible with either red or blue flags.
	/*!
		TODO: Clean this function up a bit?
		This function processes data regarding the flag. The caller should gather a list of
		red tanks and blue tanks. If the red flag should be processed, the 'team_tanks' parameter
		should be the red tanks, and the 'opponent_tanks' parameter should be the blue tanks. The
		opposite is true if the flag is blue.
		\param all_tanks Full list of tanks.
		\param team_tanks Red tanks if the flag is red, blue tanks otherwise.
		\param opponent_tanks Red tanks if the flag is blue, blue tanks otherwise.
		\param state State of whichever flag we're processing.
		\param flag_position Position of whichever flag we're processing.
		\param flag_spawn_position Position of the flag's spawn point.
		\param opponent_flag_spawn_position Position of the opponent's flag spawn point.
		\param holder_id ID of the person holding the flag, if any.
		\param at_home True if the flag is at it's spawn point; false otherwise.
		\param score The team's score.
		\param flag_color Color of the team's flag.
	*/
	void process_flag(const tank_array &all_tanks,
		const tank_array &team_tanks, const tank_array &opponent_tanks, 
		CTF::Flag_State &state, VTankObject::Point &flag_position, 
		const VTankObject::Point &team_flag_spawn_position, 
		const VTankObject::Point &opponent_flag_spawn_position,
		int &holder_id, bool &at_home, int &score, const GameSession::Alliance &flag_color)
	{
		if (state == CTF::DESPAWNED) {
			VTANK_ASSERT(holder_id == -1);

			const double current_time = get_current_time();
			if (current_time >= next_spawn) {
				// Respawn the flags.
				red_flag_state = CTF::STATIONARY;
				blue_flag_state = CTF::STATIONARY;
				red_flag_at_home = true;
				blue_flag_at_home = true;
				red_flag_position = red_spawn_position;
				blue_flag_position = blue_spawn_position;

				Notifier::blanket_notify_flag_spawned(
					all_tanks, red_flag_position, GameSession::RED);
				Notifier::blanket_notify_flag_spawned(
					all_tanks, blue_flag_position, GameSession::BLUE);

				Logger::log(Logger::LOG_LEVEL_DEBUG, "[CTF] Both flags have respawned.");
			}
		}
		else if (state == CTF::STATIONARY) {
			VTANK_ASSERT(holder_id == -1);

			// Do collision checks against opponent tanks.
			bool picked_up = false;
			const float relevant_radius = (at_home) ? FLAG_SPAWN_RADIUS : FLAG_RADIUS;
			for (tank_array::size_type i = 0; i < opponent_tanks.size(); ++i) {
				const tank_ptr tank = opponent_tanks[i];
				if (!tank->is_alive()) {
					continue;
				}

				if (Utility::circle_collision(tank->get_position(), TANK_SPHERE_RADIUS,
						flag_position, relevant_radius)) {
					// An opponent picked up the flag.
					state = CTF::HELD;
					holder_id = tank->get_id();
					flag_position = tank->get_position();
					picked_up = true;
					at_home = false;
					
					std::ostringstream formatter;
					formatter << "[CTF] " << tank->get_name() << " picked up the flag.";
					Logger::log(Logger::LOG_LEVEL_DEBUG, formatter.str());

					Notifier::blanket_notify_flag_picked_up(all_tanks, tank->get_id(),
						flag_color);
				}
			}

			if (!picked_up && !at_home) {
				// Check if a team tank has collected the flag in order to return it to base.
				tank_array::size_type i;
				for (i = 0; i < team_tanks.size(); ++i) {
					const tank_ptr tank = team_tanks[i];
					if (!tank->is_alive()) {
						continue;
					}

					if (Utility::circle_collision(tank->get_position(), TANK_SPHERE_RADIUS,
							flag_position, FLAG_RADIUS)) {
						// A team member returned the flag.
						state = CTF::STATIONARY;
						holder_id = -1;
						flag_position = team_flag_spawn_position;
						at_home = true;

						// TODO: Credit tank for flag return.

						std::ostringstream formatter;
						formatter << "[CTF] " << tank->get_name() << " returned the flag.";
						Logger::log(Logger::LOG_LEVEL_DEBUG, formatter.str());
						
						Notifier::blanket_notify_flag_returned(all_tanks, tank->get_id(),
							flag_color);
					}
				}
			}
		}
		else if (state == CTF::HELD) {
			VTANK_ASSERT(holder_id >= 0);
			VTANK_ASSERT(!at_home);
			
			bool found;
			const tank_array::size_type index = find_tank_by_id(opponent_tanks, holder_id, found);
			if (!found) {
				// Tank doesn't exist, so drop the flag.
				holder_id = -1;
				state = CTF::STATIONARY;

				Logger::log(Logger::LOG_LEVEL_DEBUG, "[CTF] Flag dropped by leaving player.");
				
				// Because the player left while holding the flag, we use 'spawn' instead of 'dropped'.
				Notifier::blanket_notify_flag_spawned(all_tanks, flag_position, flag_color);
			}
			else {
				const tank_ptr tank = opponent_tanks[index];
				
				// Update flag's position.
				flag_position = tank->get_position();
				
				// If the tank isn't alive, drop the flag.
				if (!tank->is_alive()) {
					holder_id = -1;
					state = CTF::STATIONARY;

					std::ostringstream formatter;
					formatter << "[CTF] " << tank->get_name() << " dropped the flag.";
					Logger::log(Logger::LOG_LEVEL_DEBUG, formatter.str());

					Notifier::blanket_notify_flag_dropped(all_tanks, tank->get_id(),
						flag_position, flag_color);
				}
				else {
					// Check if tank captured the flag.
					if (!blue_flag_at_home && !red_flag_at_home) {
						// The flag is out on both teams, which means neither can capture yet.
						return;
					}

					if (Utility::circle_collision(flag_position, FLAG_RADIUS, 
							opponent_flag_spawn_position, FLAG_SPAWN_RADIUS)) {
						// Successful flag capture.
						++score;

						red_flag_state = CTF::DESPAWNED;
						blue_flag_state = CTF::DESPAWNED;
						holder_red_ID = -1;
						holder_blue_ID = -1;

						next_spawn = get_current_time() + DESPAWN_TIME;

						std::ostringstream formatter;
						formatter << "[CTF] " << tank->get_name() << " captured the flag.";
						Logger::log(Logger::LOG_LEVEL_DEBUG, formatter.str());
						
						PointManager::add_objective_captured(tank->get_id());
						Notifier::blanket_notify_flag_captured(all_tanks, tank->get_id(),
							flag_color);
					}
				}
			}
		}
	}

public:
	//! Does basic initializations and notifies all players of a flag spawning.
	/*! The game may be played immediately after constructing the object.
		\param current_map Map that's currently being played on. May not be null.
		\param tanks List of all tanks currently in-game. This is used to tell the players
			that flags have been spawned.
	*/
	CTF_Helper(Map *current_map, const tank_array &tanks)
		: map(current_map), holder_red_ID(-1), holder_blue_ID(-1), score_red(0), score_blue(0),
		red_flag_at_home(true), blue_flag_at_home(true)
	{
		VTANK_ASSERT(map != NULL);

		get_flag_spawn_points(red_spawn_position, blue_spawn_position);
		red_flag_position = red_spawn_position;
		blue_flag_position = blue_spawn_position;
		red_flag_state = CTF::STATIONARY;
		blue_flag_state = CTF::STATIONARY;

		//Notifier::blanket_notify_flag_spawned(tanks, red_flag_position, GameSession::RED);
		//Notifier::blanket_notify_flag_spawned(tanks, blue_flag_position, GameSession::BLUE);

		std::ostringstream formatter;
		formatter << "[CTF] Red flag spawned at (" 
			<< red_flag_position.x << ", " << red_flag_position.y << ");\n"
			<< "[CTF] Blue flag spawned at (" 
			<< blue_flag_position.x << ", " << blue_flag_position.y << ")";
		Logger::log(Logger::LOG_LEVEL_DEBUG, formatter.str());
	}
	
	//! Cleans up used resources.
   ~CTF_Helper()
    {
    }
	
    //! Get the position of the red flag.
    /*!
		\return Point containing the red flag's position.
	*/
    const VTankObject::Point get_red_flag_position() const
	{
		return red_flag_position;
	}

	//! Get the position of the blue flag.
    /*!
		\return Point containing the blue flag's position.
	*/
    const VTankObject::Point get_blue_flag_position() const
	{
		return blue_flag_position;
	}

	//! Gets the current state of the red flag.
	/*!
		\return CTF::Flag_State enumeration indicating what the flag is currently doing.
	*/
	const CTF::Flag_State get_red_flag_state() const
	{
		return red_flag_state;
	}

	//! Gets the current state of the blue flag.
	/*!
		\return CTF::Flag_State enumeration indicating what the flag is currently doing.
	*/
	const CTF::Flag_State get_blue_flag_state() const
	{
		return blue_flag_state;
	}

	//! Get the ID of the person holding the red flag.
	/*!
		\return ID of the tank holding the red flag, or -1 if nobody is holding it.
	*/
	const int get_red_holder_id() const
	{
		return holder_red_ID;
	}
	
	//! Get the ID of the person holding the blue flag.
	/*!
		\return ID of the tank holding the blue flag, or -1 if nobody is holding it.
	*/
	const int get_blue_holder_id() const
	{
		return holder_blue_ID;
	}

	//! Get the score of team Red.
	/*!
		\return Score of the red team.
	*/
	const int get_red_score() const
	{
		return score_red;
	}

	//! Get the score of team blue.
	/*!
		\return Score of the blue team.
	*/
	const int get_blue_score() const
	{
		return score_blue;
	}
	
	//! Gets whether CTF has custom spawn points.
	/*!
		\return CTF does not use custom spawn points, so this method
		always returns false.
	*/
	bool has_custom_spawn_points()
	{
		return false;
	}

	//! Spawns a player at a position.
	/*!
		Note that this is not implemented in CTF, and is only here for
		bug-catching purposes.
	*/
	void spawn(const tank_ptr &)
	{
		// This is a bug if this is called.
		VTANK_ASSERT(false);
	}

	//! Gets the team who is currently winning.
	/*!
		\return GameSession::RED if team Red is winning, GameSession::BLUE if
		team Blue is winning, or GameSession::NONE if it's a tie.
	*/
	const GameSession::Alliance get_winning_team() const
	{
		if (score_red == score_blue)
		{
			return GameSession::NONE;
		}

		return score_red > score_blue ? GameSession::RED : GameSession::BLUE;
	}
	
	//! Send the status of the current CTF game to the given tank.
	/*!
		\param tank Tank to send the status to.
	*/
	void send_status_to(const tank_ptr &tank)
	{
		Notifier::notify_flag_spawned(tank, red_flag_position, GameSession::RED);
		Notifier::notify_flag_spawned(tank, blue_flag_position, GameSession::BLUE);

		if (holder_red_ID >= 0) {
			Notifier::notify_flag_picked_up(tank, holder_red_ID, GameSession::RED);
		}

		if (holder_blue_ID >= 0) {
			Notifier::notify_flag_picked_up(tank, holder_blue_ID, GameSession::BLUE);
		}
	}

    //! Check for collisions between flags and tanks and do other maintenance.
    /*! If a tank runs over a flag on the ground of the opposite team, that tank picks
		up the flag. If a tank runs over a flag on the ground of his own team, and the
		flag is not already at it's initial spawn point, the flag is returned back to
		it's base.

		If a tank was holding the flag and is now dead, the tank drops the flag where he
		was last alive.
	*/
    void update(const tank_array &tanks)
	{
		try {
			tank_array red_tanks;
			tank_array blue_tanks;
			tank_array::size_type i;
			for (i = 0; i < tanks.size(); ++i) {
				try {
					const tank_ptr tank = tanks[i];
					if (tank->get_team() == GameSession::RED) {
						red_tanks.push_back(tank);
					}
					else if (tank->get_team() == GameSession::BLUE) {
						blue_tanks.push_back(tank);
					}
					else {
						// It should never get here.
						VTANK_ASSERT(false);
					}
				}
				catch (const TankNotExistException &) {
					// Nothing to do but ignore it...
				}
			}

			// First process the red flag...
			process_flag(tanks, red_tanks, blue_tanks, red_flag_state, red_flag_position, 
				red_spawn_position, blue_spawn_position, holder_red_ID, 
				red_flag_at_home, score_red, GameSession::RED);

			// ... then the blue flag.
			process_flag(tanks, blue_tanks, red_tanks, blue_flag_state, blue_flag_position, 
				blue_spawn_position, red_spawn_position, holder_blue_ID, 
				blue_flag_at_home, score_blue, GameSession::BLUE);
		}
		HANDLE_UNCAUGHT_EXCEPTIONS
	}
};

#endif
