/*!
	\file   ctb.hpp
	\brief  Game handler for Capture the Base.
	\author (C) Copyright 2010 by Vermont Technical College
*/
#ifndef CTB_HPP
#define CTB_HPP

#include <gamehandler.hpp>

#define BASE_BLUE_1 8
#define BASE_BLUE_2 9
#define BASE_BLUE_3 10
#define BASE_RED_1  13
#define BASE_RED_2  12
#define BASE_RED_3  11
#define TEAM_RED	GameSession::RED
#define TEAM_BLUE	GameSession::BLUE
#define BASE_RADIUS 35.0f
#define TEAM_RED_ID 0
#define TEAM_BLUE_ID 1

#define DEFAULT_BASE_HEALTH 600
#define DEFAULT_REGEN_TIME 15000 /* 15 seconds */

const static int BASE_TYPES[] = {
	BASE_BLUE_1, BASE_BLUE_2, BASE_BLUE_3,
	BASE_RED_1,  BASE_RED_2,  BASE_RED_3
};
const static int NUM_BASES = 6;

//! Capture the Base handler.
class CTB_Helper : public Game_Handler
{
private:
	//! Tracks several variables relating to a Base.
    struct Base : Damageable_Object
	{
    private:
		int game_id;
		int base_id;
		int health;
		VTankObject::Point position;
		GameSession::Alliance team;
		
    public:
		bool regenerating;
		double regenerates_fully_at;
		
		Base() : base_id(-1), health(DEFAULT_BASE_HEALTH), position(), team(GameSession::NONE),
			   regenerating(false), regenerates_fully_at(-1), game_id(-1) {}

        Base(int id, int base_health, const VTankObject::Point &base_position, 
            const GameSession::Alliance &base_team)
            : base_id(id), game_id(id), health(base_health), position(base_position), team(base_team),
			  regenerating(false), regenerates_fully_at(-1)
        {
        }

        int get_base_id() const
            { return base_id; }

		int get_id() const
			{ return game_id; }

		void set_id(const int new_id)
			{ game_id = new_id; }

        int get_health() const 
            { return health; }

        void set_health(const int new_health)
            { health = new_health; }
        
        void inflict_damage(const int damage, const int projectile_id, 
			const int projectile_type_id, const int owner)
        {
			const Projectile type = Players::get_weapon_data()->get_projectile(projectile_type_id);
			const int final_damage = Utility::round(static_cast<float>(damage) * type.object_damage_factor);

            health -= final_damage;
            if (health < 0)
                health = 0;
			
			const bool killing_blow = health == 0;
			if (killing_blow) {
				Logger::debug("[CTB] Base #%d was killed by %d damage!",
					base_id, final_damage);
			}

			const tank_array tanks = Players::tanks.get_tank_list();
			// TODO: Temp work around for lasers. Do it a different way later.
			if (type.is_instantaneous) {
				Notifier::blanket_notify_create_projectile(owner, projectile_id,
					projectile_type_id, position);
			}
			Notifier::blanket_notify_damage_base(tanks,
				team, base_id + 8, final_damage, projectile_id, owner, killing_blow);
        }

		void inflict_environment_damage(const int damage, const int env_id,
			const int env_type_id, const int owner)
		{
			health -= damage;
			if (health < 0)
				health = 0;

			const bool killing_blow = health == 0;
			if (killing_blow) {
				Logger::debug("[CTB] Base #%d was killed by %d damage from an environment effect!",
					base_id, damage);
			}

			Notifier::blanket_notify_damage_base_by_env(team, base_id + 8,
				env_id, damage, killing_blow);
		}

        bool is_alive() const
            { return health > 0; }

        VTankObject::Point get_position() const 
            { return position; }

        void set_position(const VTankObject::Point &position)
            { this->position = position; }

        float get_radius() const 
            { return BASE_RADIUS; }

        GameSession::Alliance get_team() const 
            { return team; }

        void set_team(const GameSession::Alliance &team)
            { this->team = team; }

        float get_armor_factor() const 
            { return 1.0f; }
	};

	int red_score;
	int blue_score;
	int red_spawn_base_id;
	int blue_spawn_base_id;
	int red_spawn_index;
	int blue_spawn_index;
	int *contended_bases;
	std::vector<Base> bases;
	std::map<int, std::vector<VTankObject::Point>> base_spawn_points;
	bool game_done;
	GameSession::Alliance last_winner;

	std::vector<VTankObject::Point> generate_base_spawn_points(const Map *map,
		int current_x, int current_y, const VTankObject::Point &position)
	{
		std::vector<VTankObject::Point> result;

		for (int y = current_y - 3; y < current_y + 3 && y < map->get_height(); ++y) {
			if (y < 0)
				continue;

			for (int x = current_x - 3; x < current_x + 3 && x < map->get_width(); ++x) {
				if (x < 0 || (x == current_x && y == current_y))
					continue;

				const Tile tile = map->get_tile(x, y);
				if (tile.passable) {
					VTankObject::Point spawn_position;
					spawn_position.x = (x * TILE_SIZE) + (TILE_SIZE / 2);
					spawn_position.y = -((y * TILE_SIZE) + (TILE_SIZE / 2));
					result.push_back(spawn_position);
				}
			}
		}

		VTANK_ASSERT(result.size() > 0);

		return result;
	}

	//! Get the next spawn point for a given player.
	const VTankObject::Point get_next_spawn(const tank_ptr &tank)
	{
		const GameSession::Alliance team = tank->get_team();
		VTankObject::Point spawn_position;
		std::vector<VTankObject::Point> spawn_points;
		
		if (team == TEAM_RED) {
			spawn_points   = base_spawn_points[red_spawn_base_id];
			spawn_position = spawn_points[red_spawn_index];
			++red_spawn_index;
			if (red_spawn_index >= static_cast<int>(spawn_points.size())) {
				red_spawn_index = 0;
			}
		}
		else {
			spawn_points   = base_spawn_points[blue_spawn_base_id];
			spawn_position = spawn_points[blue_spawn_index];
			++blue_spawn_index;
			if (blue_spawn_index >= static_cast<int>(spawn_points.size())) {
				blue_spawn_index = 0;
			}
		}

		return spawn_position;
	}
	
	//! Start a base's regeneration countdown.
	void start_regenerating(Base &base)
	{
		base.regenerating = true;
		base.regenerates_fully_at = get_current_time() + DEFAULT_REGEN_TIME;
	}
	
	//! Stop a base's regeneration countdown.
	void stop_regenerating(Base &base)
	{
		base.regenerating = false;
		base.regenerates_fully_at = -1;
	}

	//! Generate spawn points specifically for this game mode.
	void generate_spawn_points(const Map *map)
	{
		bases.resize(NUM_BASES);
		
		int base_count = 0;
		const int width = map->get_width();
		const int height = map->get_height();
		for (int y = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x) {
				const Tile tile = map->get_tile(x, y);
				if (Utility::contains(BASE_TYPES, NUM_BASES, tile.event_id)) {
					const int ID = tile.event_id - 8; // Compute offset to 0-based array.
					VTankObject::Point position;
					position.x = (x * TILE_SIZE) + (TILE_SIZE / 2);
					position.y = -((y * TILE_SIZE) + (TILE_SIZE / 2));
					
					base_spawn_points[ID] = generate_base_spawn_points(map, x, y, position);
					
					Base base(ID, DEFAULT_BASE_HEALTH, position,
                        (ID > BASE_BLUE_3 - 8) ? GameSession::RED : GameSession::BLUE);
					bases[ID] = base;
					++base_count;
					
					Logger::debug("[CTB] Base #%d at (%f, %f) for team %s.", ID, position.x, position.y,
						(base.get_team() == GameSession::RED ? "red" : "blue"));
				}
			}
		}
		
		//VTANK_ASSERT(base_count == NUM_BASES);

		red_spawn_index  = 0;
		blue_spawn_index = 0;
	}

	//! Change a base's affiliation and notify players.
	void capture_base(int base_id, const GameSession::Alliance &team, const tank_ptr &captured_by)
	{
		Projectile_Manager *projectiles = Players::get_projectile_manager();
		Base base = bases[base_id];
		const GameSession::Alliance old_team = base.get_team();
		base.set_team(team);
		base.set_health(DEFAULT_BASE_HEALTH);

		projectiles->remove_damageable_object(red_spawn_base_id);
		projectiles->remove_damageable_object(blue_spawn_base_id);

		if (team == TEAM_RED) {
			--red_spawn_base_id;
			--blue_spawn_base_id;
		}
		else {
			++red_spawn_base_id;
			++blue_spawn_base_id;
		}

		// Restore health to all other towers.
		for (std::vector<Base>::size_type i = 0; i < bases.size(); ++i) {
			Base a_base = bases[i];
			a_base.set_health(DEFAULT_BASE_HEALTH);
			stop_regenerating(a_base);
			bases[i] = a_base;
		}

		contended_bases[TEAM_RED_ID]  = red_spawn_base_id;
		contended_bases[TEAM_BLUE_ID] = blue_spawn_base_id;

		red_spawn_index = 0;
		blue_spawn_index = 0;
		
		VTANK_ASSERT(red_spawn_base_id != blue_spawn_base_id);
		VTANK_ASSERT(contended_bases[TEAM_RED_ID] != contended_bases[TEAM_BLUE_ID]);
		
		if (blue_spawn_base_id < 0) {
			// Team blue lost.
			++red_score;
			Logger::debug("[CTB] Team blue has lost all bases!");
			game_done = true;
			last_winner = TEAM_RED;
		}
		else if (red_spawn_base_id >= NUM_BASES) {
			// Team red lost.
			++blue_score;
			Logger::debug("[CTB] Team red has lost all bases!");
			game_done = true;
			last_winner = TEAM_BLUE;
		}

		if (game_done) {
			// Credit the winning team.
			const tank_array tanks = Players::tanks.get_tank_list();
			for (tank_array::size_type i = 0; i < tanks.size(); ++i) {
				const tank_ptr tank = tanks[i];
				if (tank->get_team() == last_winner) {
					PointManager::add_objective_completed(tank->get_id());
				}
			}
		}
		else {
			projectiles->add_damageable_object(&bases[red_spawn_base_id]);
			projectiles->add_damageable_object(&bases[blue_spawn_base_id]);
		}

		Notifier::blanket_notify_base_captured(Players::tanks.get_tank_list(),
			old_team, team, base_id + 8, captured_by->get_id());

		bases[base_id] = base;
	}

	//! Do collision and event checks against all players.
	void do_player_checks(const tank_array &tanks)
	{
		// Check to see if a tank will capture a base.
		const tank_array::size_type tank_size = tanks.size();
		const std::vector<Base>::size_type base_size = bases.size();

		for (std::vector<Base>::size_type i = 0; i < base_size; ++i) {
			Base base = bases[i];
			if (base.get_health() > 0) {
				// Only worry about bases with no health.
				continue;
			}

			for (tank_array::size_type j = 0; j < tank_size; ++j) {
				const tank_ptr tank = tanks[j];
				const VTankObject::Point tank_position = tank->get_position();

				if (base.get_team() == tank->get_team()) {
					// Tank can't take over it's own base, so continue.
					continue;
				}

				// At this point: base has no health, now we check if opponent tank is near the base.
				if (Utility::circle_collision(tank_position, TANK_SPHERE_RADIUS, base.get_position(), BASE_RADIUS + 5.0f)) {
					// Opponent tank has captured the base.
					Logger::debug("[CTB] Base #%d captured by %s!",
						base.get_base_id(), tank->get_name().c_str());
					capture_base(base.get_base_id(), tank->get_team(), tank);
					break;
				}
			}
		}
	}

	//! Check the bases to see if they need to be managed in some way.
	void do_base_checks()
	{
		for (std::vector<Base>::size_type i = 0; i < bases.size(); ++i) {
			Base base = bases[i];
			if (base.get_health() == 0) {
				if (!base.regenerating) {
					start_regenerating(base);
				}
				else {
					// If the base hasn't been captured in DEFAULT_REGEN_TIME seconds,
					// the base is restored to it's full health.
					if (get_current_time() >= base.regenerates_fully_at) {
						stop_regenerating(base);
						base.set_health(DEFAULT_BASE_HEALTH);
						
						VTANK_ASSERT(base.get_base_id() + 8 >= 8);
						Notifier::blanket_notify_set_base_status(
							Players::tanks.get_tank_list(), base.get_team(), base.get_base_id() + 8, base.get_health());
					}
				}
			}
			else if (base.regenerating && base.get_health() > 0) {
				stop_regenerating(base);
			}

			bases[i] = base;
		}
	}
	
	//! Start the game over.
	void reset_game(const tank_array &tank_list)
	{
		Projectile_Manager *projectiles = Players::get_projectile_manager();
		Notifier::blanket_notify_end_round(last_winner);

		red_spawn_base_id = BASE_RED_3 - 8;
		blue_spawn_base_id = BASE_BLUE_3 - 8;
		red_spawn_index = 0;
		blue_spawn_index = 0;
		contended_bases[TEAM_RED_ID] = red_spawn_base_id;
		contended_bases[TEAM_BLUE_ID] = blue_spawn_base_id;
		for (int i = 0; i < NUM_BASES; ++i) {
			Base base = bases[i];
			base.set_team(i > 2 ? TEAM_RED : TEAM_BLUE);
			base.set_health(DEFAULT_BASE_HEALTH);
			bases[i] = base;

			if (i == red_spawn_base_id || i == blue_spawn_base_id) {
				projectiles->add_damageable_object(&bases[i]);
			}
		}

		// For each player, reset their position.
		for (tank_array::size_type i = 0; i < tank_list.size(); ++i) { 
			const tank_ptr tank = tank_list[i];
			if (tank->is_alive()) {
				const VTankObject::Point new_pos = get_next_spawn(tank);
				tank->set_position(new_pos);
				Notifier::notify_reset_position(tank, new_pos);
				Notifier::blanket_notify_player_moved(tank->get_id(), new_pos, VTankObject::NONE);
			}
		}
	}

public:
	//! Constructs the CTB helper class.
	CTB_Helper(const Map *current_map)
		: red_score(0), blue_score(0), red_spawn_base_id(BASE_RED_3 - 8), 
		blue_spawn_base_id(BASE_BLUE_3 - 8), game_done(false)
	{
		generate_spawn_points(current_map);
		contended_bases = new int[2];
		contended_bases[TEAM_RED_ID]  = red_spawn_base_id;
		contended_bases[TEAM_BLUE_ID] = blue_spawn_base_id;
		
		Projectile_Manager *projectiles = Players::get_projectile_manager();
		projectiles->add_damageable_object(&bases[red_spawn_base_id]);
		projectiles->add_damageable_object(&bases[blue_spawn_base_id]);
	}

   ~CTB_Helper()
    {
		delete [] contended_bases;
	}
    
	//! Gets the current score for the red team.
	const int get_red_score() const
	{
		return red_score;
	}

	//! Gets the current score for the blue team.
	const int get_blue_score() const
	{
		return blue_score;
	}

	//! Checks whether CTB uses custom spawn points (it does).
	bool has_custom_spawn_points()
	{
		return true;
	}
	
	//! Spawns a player based on his team and progress in-game at a certain position.
	void spawn(const tank_ptr &player)
	{
		const VTankObject::Point spawn_position = get_next_spawn(player);
		std::ostringstream formatter;
		formatter << "[CTB] Spawned " << player->get_name() << " at ("
			<< spawn_position.x << ", " << spawn_position.y << ")";
		Logger::log(Logger::LOG_LEVEL_DEBUG, formatter.str());
		player->set_position(spawn_position);
	}

	//! Gets the team currently winning.
	const GameSession::Alliance get_winning_team() const
	{
		if (red_score == blue_score) {
			return GameSession::NONE;
		}

		return red_score > blue_score ? GameSession::RED : GameSession::BLUE;
	}

	//! Send the game status to a new tank so that they understand what's going on.
	void send_status_to(const tank_ptr &tank)
	{
		for (std::vector<Base>::size_type i = 0; i < bases.size(); ++i) {
			const Base base = bases[i];
			Notifier::notify_set_base_status(tank, base.get_team(), base.get_base_id() + 8, base.get_health());
		}
	}

	//! Update the status of the game.
	void update(const tank_array &tank_list)
	{
		do_player_checks(tank_list);
		do_base_checks();
		
		if (game_done) {
			reset_game(tank_list);
			game_done = false;
		}
	}
};
#endif
