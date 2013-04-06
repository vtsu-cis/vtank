/*!
    \file   projectilemanager.cpp
    \brief  Implements the Projectile_Manager class.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#include <master.hpp>
#include <nodemanager.hpp>
#include <projectilemanager.hpp>
#include <logger.hpp>
#include <vtassert.hpp>
#include <mapmanager.hpp>
#include <utility.hpp>
#include <notifier.hpp>
#include <playermanager.hpp>
#include <asynctemplate.hpp>
#include <gamemanager.hpp>

namespace {
	int projectile_count = 0;

	bool compare(double x1, double x2, double incr)
	{
        return abs(x2 - x1) <= incr;
	}

	float calculate_aoe_damage(const float raw_damage, const float decay, const float r, const float d)
	{
		const float ratio = d / r;
		const float damage_ratio = raw_damage * ratio;
		const float damage = raw_damage - damage_ratio + damage_ratio * decay;
		return std::max(damage, 0.0f);
	}

	//! Handle AOE weapon damage. This method assumes a projectile has had impact.
	void handle_aoe_weapon(const tank_ptr &owner, const projectile_ptr &projectile,
		const damageable_map &object_list = damageable_map())
	{
		using Utility::Circle;

		const Projectile projectile_data = projectile->type.projectile;
		const Circle splash_area(projectile_data.aoe_radius, projectile->position);

		const int node = Players::get_node_manager()->get_node_at(projectile->position);
		const tank_array players = Players::get_node_manager()->get_relevant_players(node);
		
		// Detect if players are present in the splash radius.
		for (tank_array::size_type i = 0; i < players.size(); ++i) {
			const tank_ptr player = players[i];
			if (!player->is_alive() || (player->get_team() != GameSession::NONE &&
					player->get_team() == owner->get_team()) || 
					player->get_id() == owner->get_id()) {
				continue;
			}
			const Circle player_circle(player->get_radius(), player->get_position());
			
			if (splash_area.intersects(player_circle)) {
				int final_damage = 0;
				const VTankObject::Point pos = player->get_position();
				if (splash_area.position.x == pos.x && splash_area.position.y == pos.y) {
					// The points are exactly the same: Full area damage.
					final_damage = Utility::round(projectile->damage / player->get_armor_factor());
				
					player->inflict_damage(final_damage, projectile->id, projectile_data.id, owner->get_id());
				}
				else {
					// Calculate the damage dealt based on the distance to the target.
					const float distance = static_cast<float>(sqrt(
						pow(splash_area.position.y - pos.y, 2) + 
						pow(splash_area.position.x - pos.x, 2)));
					
					const float damage = calculate_aoe_damage(projectile->damage, projectile_data.aoe_decay,
						projectile_data.aoe_radius, distance);
					final_damage = Utility::round(damage / player->get_armor_factor());
					
					player->inflict_damage(final_damage, projectile->id, projectile_data.id, owner->get_id());
				}

				Notifier::blanket_notify_player_damaged(player->get_id(), projectile->id,
					owner->get_id(), final_damage, !player->is_alive());
			}
		}
		
		// Detect if objects are present in the splash radius.
		damageable_map::const_iterator i;
		for (i = object_list.begin(); i != object_list.end(); ++i) {
			Damageable_Object *object = i->second;
			if (!object->is_alive() || (object->get_team() != GameSession::NONE &&
				object->get_team() == owner->get_team())) {
				continue;
			}

			const Circle object_circle(object->get_radius(), object->get_position());
			if (splash_area.intersects(object_circle)) {
				const float distance = static_cast<float>(sqrt(
					pow(splash_area.position.y - object->get_position().y, 2) + 
					pow(splash_area.position.x - object->get_position().x, 2)));
				
				float damage = calculate_aoe_damage(projectile->damage, projectile_data.aoe_decay,
					projectile_data.aoe_radius, distance);
				const int final_damage = Utility::round(damage / object->get_armor_factor());
				
				object->inflict_damage(final_damage, projectile->id, projectile_data.id, owner->get_id());
			}
		}
	}

	/*!
		Inflict damage to a player.
		\param victim Person getting hit.
		\param projectile Projectile hitting the player.
		\param owner Person who fired the projectile.
	*/
	void inflict_damage(const tank_ptr &victim, const projectile_ptr &projectile, const tank_ptr &owner,
		const std::map<int, Damageable_Object *> &object_list = std::map<int, Damageable_Object *>())
	{
		VTANK_ASSERT(victim->is_alive());

		const Projectile projectile_data = projectile->type.projectile;
		if (projectile_data.aoe_radius > 0.0f) {
			//projectile->position = victim->get_position();
			handle_aoe_weapon(owner, projectile, object_list);
		}
		else {
			const int damage = Utility::round(projectile->damage / victim->get_armor_factor());
			victim->inflict_damage(damage, projectile->id, projectile_data.id, owner->get_id());

			const bool killing_blow = !victim->is_alive();
			if (killing_blow) {
				Logger::debug("%s killed %s for %d damage.",
					owner->get_name().c_str(), victim->get_name().c_str(), damage);
			}

			Notifier::blanket_notify_player_damaged(victim->get_id(), projectile->id, 
				owner->get_id(), damage, killing_blow);
		}
	}
	
	/*!
		Inflict damage to a game object.
		\param object Object getting hit.
		\param projectile Projectile hitting the player.
		\param owner Person who fired the projectile.
	*/
	void inflict_damage(Damageable_Object *object, const projectile_ptr &projectile, const tank_ptr &owner,
		const std::map<int, Damageable_Object *> &object_list = std::map<int, Damageable_Object *>())
	{
		VTANK_ASSERT(object->is_alive());

		const Projectile projectile_data = projectile->type.projectile;
		if (projectile_data.aoe_radius > 0.0f) {
			projectile->position = object->get_position();
			handle_aoe_weapon(owner, projectile, object_list);
		}
		else {
			const int damage = Utility::round(projectile->damage / object->get_armor_factor());
			object->inflict_damage(damage, projectile->id, projectile_data.id, owner->get_id());

			const bool killing_blow = !object->is_alive();
			if (killing_blow) {
				Logger::debug("%s destroyed object #%d for %d damage.",
					owner->get_name().c_str(), object->get_id(), damage);
			}
		}
	}
	
	void handle_instant_weapon(const tank_ptr &owner, const projectile_ptr &projectile, 
        const damageable_map &objects)
	{
        const Weapon type = projectile->type;
		const double MAX_RANGE = type.projectile.range;
		const Map * current_map = MapManager::get_current_map();

		// Check for error in the angle; make corrections.
		const double TWO_PI = PI * 2;
		double final_angle = projectile->angle;

        Utility::Line path;
		path.x1 = projectile->position.x;
		path.y1 = projectile->position.y;
		path.x2 = projectile->position.x + cos(final_angle) * MAX_RANGE;
		path.y2 = projectile->position.y + sin(final_angle) * MAX_RANGE;

		// Calculate where the weapon's range ends.
		// TODO: Better way to do this?
		const double y_inc = 12;
		const double x_inc = 12;
		int last_tile_x = -1;
		int last_tile_y = -1;
		const int max_increments = 15;
        double y = path.y1, x = path.x1;
        while (true) {
            if (compare(x, path.x2, x_inc) || compare(y, path.y2, y_inc)) {
                break;
            }
            
            x += cos(final_angle) * x_inc;
            y += sin(final_angle) * y_inc;
			
			const int tile_x = static_cast<int>(floor(x / TILE_SIZE));
			const int tile_y = static_cast<int>(floor(-y / TILE_SIZE));
			if (tile_x == last_tile_x && tile_y == last_tile_y) {
				continue;
			}
			last_tile_x = tile_x;
			last_tile_y = tile_y;

			if (tile_x < 0 || tile_y < 0 || tile_x >= current_map->get_width() || 
					tile_y >= current_map->get_height()) {
                // The max range will fall off the map.
				path.x2 = x;
				path.y2 = y;
				break;
			}

			const Tile tile = current_map->get_tile(tile_x, tile_y);
			if (!tile.passable) {
				// Laser hit a wall: done calculation.
				// Increment slightly to prevent weird effects such as lasers not fully hitting a wall.
				path.x2 = x += cos(final_angle) * x_inc;
				path.y2 = y += sin(final_angle) * y_inc;
				break;
			}
		}

		projectile->target.x = path.x2;
		projectile->target.y = path.y2;
        
		// We now know exactly where the weapon begins and ends. Find out who it hits.
		const tank_array all_tanks = Players::tanks.get_tank_list();
		tank_array hit_tanks;
		damageable_list hit_objects;
		const double TANK_RADIUS = TANK_SPHERE_RADIUS + 15.0;
		for (tank_array::const_iterator i = all_tanks.begin(); i != all_tanks.end(); ++i) {
			const tank_ptr tank = *i;
			const VTankObject::Point tank_position = tank->get_position();

			if ((tank->get_team() == owner->get_team() && tank->get_team() != GameSession::NONE) || 
					!tank->is_alive() || tank->get_id() == owner->get_id()) {
				continue;
			}

			if (Utility::line_circle_collision(tank_position.x, tank_position.y, TANK_RADIUS,
					path.x1, path.y1, path.x2, path.y2)) {
				// It hit a player.
				hit_tanks.push_back(tank);
			}
		}
		
		damageable_map::const_iterator j = objects.begin();
		for (; j != objects.end(); ++j) {
			Damageable_Object *object = j->second;
			if (!object->is_alive() || (object->get_team() == owner->get_team() 
					&& object->get_team() != GameSession::NONE)) {
				continue;
			}
			const VTankObject::Point pos = object->get_position();

			if (Utility::line_circle_collision(pos.x, pos.y, object->get_radius(),
					path.x1, path.y1, path.x2, path.y2)) {
				// It hit the object.
				hit_objects.push_back(object);
			}
		}
		
		// Now find the person who is closest to the (x1, y1) position of the line.
		// This person, and this person only, was affected by the laser.
		if (hit_tanks.size() == 0 && hit_objects.size() == 0) {
			// Nobody was hit.
			// TODO: This should not distribute the message like this.
			VTankObject::Point end_point;
			end_point.x = path.x2;
			end_point.y = path.y2;
			
			Notifier::blanket_notify_create_projectile(owner->get_id(), projectile->id,
				type.projectile.id, end_point);

			return;
		}
		
		tank_ptr *hit_tank = NULL;
		Damageable_Object *hit_object = NULL;
		bool tank_is_closer = true;
		if (hit_tanks.size() == 1 && hit_objects.size() == 0) {
			// One person was hit -- no further calculations needed.
			hit_tank = &hit_tanks[0];
		}
		else if (hit_tanks.size() == 0 && hit_objects.size() == 1) {
			// Only one object was hit.
			hit_object = hit_objects[0];
			tank_is_closer = false;
		}
		else {
			// Find the closest person or object hit.
			double max_distance = 99999;
			for (tank_array::iterator i = hit_tanks.begin(); i != hit_tanks.end(); ++i) {
				tank_ptr tank = *i;
				const VTankObject::Point p = tank->get_position();
				const double distance = sqrt(pow(path.y1 - p.y, 2) + pow(path.x1 - p.x, 2));
				if (distance < max_distance) {
					max_distance = distance;
					hit_tank = &tank;
				}
			}
			
			damageable_list::iterator i;
			for (i = hit_objects.begin(); i != hit_objects.end(); ++i) {
				Damageable_Object *object = *i;
				const VTankObject::Point p = object->get_position();
				const double distance = sqrt(pow(path.y1 - p.y, 2) + pow(path.x1 - p.x, 2));

				if (distance < max_distance) {
					max_distance = distance;
					hit_object = object;
					tank_is_closer = false;
				}
			}
		}

		VTANK_ASSERT(hit_tank != NULL || hit_object != NULL);
		
		if (tank_is_closer) {
			inflict_damage((*hit_tank), projectile, owner);
		}
		else {
			inflict_damage(hit_object, projectile, owner);
		}
	}
}

Projectile_Manager::Projectile_Manager()
{
}

Projectile_Manager::~Projectile_Manager()
{
}

int Projectile_Manager::generate_projectile_id()
{
    Logger::Stack_Logger stack("Projectile_Manager::generate_projectile_id()", false);

    for (std::map<int, Active_Projectile>::size_type i = 0; i < projectiles.size() + 10; i++) {
        // Check if anyone holds the ID.
        const std::map<int, projectile_ptr>::iterator it =
            projectiles.find(static_cast<int>(i));
        if (it != projectiles.end()) {
            continue;
        }

        return i;
    }

    // It should never reach here.
    VTANK_ASSERT(false);
    return -1; // Makes a warning go away.
}

bool Projectile_Manager::do_remove(const int &id)
{
    const std::map<int, projectile_ptr>::iterator i = projectiles.find(id);
    if (i == projectiles.end()) {
        std::ostringstream formatter;
        formatter << "Couldn't find projectile " << id << " to remove it.";

        Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());

        return false;
    }

    (void)projectiles.erase(i);
	--projectile_count;
	if (projectile_count <= 0) {
		/* TODO: There seems to be a case where the projectiles mapping is not properly
			dealt with. This is a temporary work-around. Delete projectiles reliably. */
		projectiles.clear();
		projectile_count = 0;
	}

    return true;
}

int Projectile_Manager::add(const int &owner, const double &angle,
                            const VTankObject::Point &position,
							const VTankObject::Point &target, const Weapon &type, 
							VTankObject::Point &new_target)
{
    Logger::Stack_Logger stack("Projectile_Manager::add()", false);
    boost::lock_guard<boost::mutex> guard(mutex);
	const int id = generate_projectile_id();
    const projectile_ptr projectile = projectile_ptr(
        new Active_Projectile(id, owner, angle, position, target, type));
    do_initial_calculations(projectile);

    projectiles[id] = projectile;
	++projectile_count;

	new_target = projectile->target;

    if (type.projectile.is_instantaneous) {
        // Instant projectiles are handled internally, differently.
        return -1;
    }

    return id;
}

projectile_ptr Projectile_Manager::get(const int &id)
{
    Logger::Stack_Logger stack("Projectile_Manager::get()", false);
    boost::lock_guard<boost::mutex> guard(mutex);

    const std::map<int, projectile_ptr>::const_iterator i = projectiles.find(id);
    if (i == projectiles.end()) {
        throw std::runtime_error("Error: No such projectile exists.");
    }

    return i->second;
}

void Projectile_Manager::reset()
{
    boost::lock_guard<boost::mutex> guard(mutex);

    projectiles.clear();
    damageable_objects.clear();
}

bool Projectile_Manager::remove(const int &id)
{
	boost::lock_guard<boost::mutex> guard(mutex);

	return do_remove(id);
}

void Projectile_Manager::process(NodeManager &node_manager, const double &delta_time)
{
    Logger::Stack_Logger stack("Projectile_Manager::process()", false);
    boost::lock_guard<boost::mutex> guard(mutex);

    const long new_delta = static_cast<long>(delta_time * 1000.0);
    std::vector<int> to_remove;

    std::map<int, projectile_ptr>::iterator i;
    for (i = projectiles.begin(); i != projectiles.end(); i++) {
		if (!do_projectile_calculations(i->second, delta_time)) {
			to_remove.push_back(i->second->id);

			continue;
		}

        i->second->millisecondsAlive += new_delta;
        if (i->second->expired()) {
            to_remove.push_back(i->second->id);

            continue;
        }

        node_manager.process_projectile(i->second);

		if (i->second->type.launch_angle > 0.0f) {
			continue;
		}

		const Map * current_map = MapManager::get_current_map();
		if (Utility::wall_collision(i->second, current_map)) {
			// Projectile hit a wall.
			/*std::ostringstream formatter;
			formatter << "Projectile #" << i->first << " hit a wall.";

			Logger::log(Logger::LOG_LEVEL_DEBUG, formatter.str());*/
			
			if (i->second->type.projectile.aoe_radius > 0) {
				// The projectile has area of effect damage.
				try {
					handle_aoe_weapon(Players::tanks.get(i->second->owner), i->second);
				}
				catch (const TankNotExistException &) {}
			}

			to_remove.push_back(i->first);
		}
        else if (perform_collision_check(node_manager, i->second)) {
            to_remove.push_back(i->first);
        }
    }

    // Remove projectiles that have expired.
    while (to_remove.size() > 0) {
        do_remove(to_remove[0]);
        to_remove.erase(to_remove.begin());
    }

	environment.update(&damageable_objects);
}

bool Projectile_Manager::perform_collision_check(
    NodeManager &nodes, const projectile_ptr projectile)
{
    Logger::Stack_Logger stack("perform_collision_check()", false);
	
	const tank_ptr owner_tank = Players::get_player(projectile->owner);
	EnvironmentProperty *env = projectile->type.projectile.environment_property;

	// First check if any players have been hit.
    const tank_array players = nodes.get_relevant_players(projectile->node_id);
    for (tank_array::size_type i = 0; i < players.size(); i++) {
        const tank_ptr player = players[i];
        if (!player->is_alive() || player->get_id() == projectile->owner
                || player->is_allied(projectile->owner)) {
            continue;
        }

        if (Utility::projectile_collision(projectile, player)) {
			inflict_damage(player, projectile, owner_tank, damageable_objects);
			if (env != NULL && env->spawn_on_player_hit) {
				const int id = environment.spawn(env, owner_tank->get_team(),
					projectile->position, owner_tank->get_id());
				
				if (id >= 0) {
					Notifier::blanket_notify_spawn_env_effect(id, env->id,
						owner_tank->get_id(), projectile->position);
				}
			}

            return true;
        }
    }

	// Now check if any damageable objects have been hit.
	// TODO: This is currently a O(n^2) operation (considering all projectiles).
	//       This can be reduced if we use the node manager.
	damageable_map::iterator i;
	for (i = damageable_objects.begin(); i != damageable_objects.end(); ++i) {
		Damageable_Object *object = i->second;
		if (!object->is_alive() || (object->get_team() == owner_tank->get_team() &&
				object->get_team() != GameSession::NONE)) {
			// Not able to be hit by this projectile.
			continue;
		}

		if (Utility::projectile_collision(projectile, object->get_position(), object->get_radius())) {
			inflict_damage(object, projectile, owner_tank, damageable_objects);
			if (env != NULL && env->spawn_on_wall_hit) {
				const int id = environment.spawn(env, owner_tank->get_team(),
					projectile->position, owner_tank->get_id());
				
				if (id >= 0) {
					Notifier::blanket_notify_spawn_env_effect(id, env->id,
						owner_tank->get_id(), projectile->position);
				}
			}

			return true;
		}
	}

    return false;
}

projectile_array Projectile_Manager::get_projectiles()
{
	boost::lock_guard<boost::mutex> guard(mutex);

	projectile_array projectile_list;
	std::map<int, projectile_ptr>::iterator i;
    for (i = projectiles.begin(); i != projectiles.end(); i++) {
		projectile_list.push_back(i->second);
	}

	return projectile_list;
}

bool Projectile_Manager::do_projectile_calculations(
    const projectile_ptr &projectile, double delta_time)
{
    const Weapon weapon_data = projectile->type;
    const Projectile projectile_data = weapon_data.projectile;
	EnvironmentProperty *env = projectile_data.environment_property;
    
    if (projectile_data.is_instantaneous) {
        // Immediately take care of this calculation.
        try {
		    const tank_ptr owner_tank = Players::tanks.get(projectile->owner);

		    handle_instant_weapon(owner_tank, projectile, damageable_objects);
	    }
	    catch (const TankNotExistException &) {
	    }

		return false;
    }
	else if (weapon_data.launch_angle > 0.0f) {
		// The weapon is angled.
		// Find the current (x, y, z) position of the projectile.
		const double time_elapsed = static_cast<double>(projectile->millisecondsAlive) / 1000.0;
		const Vector3 tip = projectile->tip;
		const Vector3 velocity = projectile->velocity_component;
		const double x = projectile->origin.x + tip.x + (velocity.x * time_elapsed);
		const double y = projectile->origin.y + tip.y + (velocity.y * time_elapsed);
		const double z = tip.z + (velocity.z * time_elapsed) + (0.5 * GRAVITY * time_elapsed * time_elapsed);
		projectile->position.x = x;
		projectile->position.y = y;
		
		if (z <= 0.0) {
			// The projectile has hit the ground.
			try {
				const tank_ptr owner = Players::tanks.get(projectile->owner);
				handle_aoe_weapon(owner, projectile, damageable_objects);

				if (env != NULL && env->spawn_on_wall_hit) {
					const int id = environment.spawn(env, owner->get_team(),
						projectile->position, owner->get_id());
					
					if (id >= 0) {
						Notifier::blanket_notify_spawn_env_effect(id, env->id,
							owner->get_id(), projectile->position);
					}
				}
			}
			catch (const TankNotExistException &) {}

			return false;
		}
		
		// Check to see if it hit a wall.
		const Map *current_map = MapManager::get_current_map();
		const int tile_x = Utility::round(x / TILE_SIZE);
		const int tile_y = Utility::round(-y / TILE_SIZE);
		
		if (tile_x >= 0 && tile_x < current_map->get_width() && 
				tile_y >= 0 && tile_y < current_map->get_height()) {
			const Tile tile = MapManager::get_current_map()->get_tile(tile_x, tile_y);
			if (tile.height > 0) {
				const double tile_height = tile.height * TILE_SIZE;
				if (z <= tile_height) {
					// It has collided with the tile that it's on.
					if (z < TILE_SIZE) {
						// Do AOE damage if it's near the floor.
						try {
							const tank_ptr owner = Players::tanks.get(projectile->owner);
							handle_aoe_weapon(owner, projectile, damageable_objects);
						}
						catch (const TankNotExistException &) {}

						return false;
					}
					else {
						// It does not do AOE damage, but it still hit the wall.
						return false;
					}
				}
			}
		}
	}
    else {
        if (projectile_data.initial_velocity != projectile_data.terminal_velocity) {
            // The projectile accelerates.
            if (abs(projectile_data.terminal_velocity - projectile->velocity) > 0.001) {
                // Projectile has not reached terminal velocity.
                const double acceleration = projectile_data.acceleration;
				
                // dv = a * dt
                projectile->velocity += acceleration * delta_time;
                if (projectile->velocity > projectile_data.terminal_velocity) {
                    projectile->velocity = projectile_data.terminal_velocity;
                }
            }
        }
        
        const double angle = projectile->angle;
        projectile->position.x += cos(angle) * (projectile->velocity * delta_time);
        projectile->position.y += sin(angle) * (projectile->velocity * delta_time);
    }

	return true;
}

void Projectile_Manager::do_initial_calculations(const projectile_ptr &projectile)
{
    const Weapon weapon_data = projectile->type;
    const Projectile projectile_data = weapon_data.projectile;
    try {
        const tank_ptr owner = Players::tanks.get(projectile->owner);
        
		// Find the maximum point where the projectile could land (for cone calculations).
		const VTankObject::Point target = projectile->target;
		VTankObject::Point max_point;
		max_point.x = target.x + cos(projectile->angle) * projectile_data.range;
		max_point.y = target.y + sin(projectile->angle) * projectile_data.range;
		
        if (projectile_data.cone_radius > 0 && !projectile_data.cone_damage_full_area) {
            // The projectile fires with some variance.
			const float cone_radius = RADIANS_F(projectile_data.cone_radius);
            const double variance = random_next_f(0.0f, cone_radius * 2.0f) - cone_radius;
			const double new_angle = projectile->angle + variance;
			
			VTankObject::Point new_target;
			new_target.x = projectile->position.x + projectile_data.range * cos(new_angle);
			new_target.y = projectile->position.y + projectile_data.range * sin(new_angle);
			//new_target.x = max_point.x + cos(new_angle) * projectile_data.cone_radius;
			//new_target.y = max_point.y + sin(new_angle) * projectile_data.cone_radius;
			
			projectile->angle = new_angle;
			projectile->target = new_target;
        }

		if (projectile_data.range_variation > 0) {
			const int new_range = random_next(projectile_data.range,
				projectile_data.range + projectile_data.range_variation);
			const double difference = new_range - projectile_data.range;
			projectile->target.x = projectile->target.x + cos(projectile->angle) * difference;
			projectile->target.y = projectile->target.y + sin(projectile->angle) * difference;
		}

        const float damage_factor = owner->get_damage_factor();
	    const int min_damage = projectile->type.projectile.minimum_damage;
	    const int max_damage = projectile->type.projectile.maximum_damage;
	    float actual_damage = static_cast<float>(random_next(min_damage, max_damage));
    	
	    try {
		    VTANK_ASSERT(actual_damage >= min_damage && actual_damage <= max_damage);
	    }
	    catch (const std::logic_error &ex) {
		    Logger::debug("Logic error: %s", ex.what());
		    actual_damage = static_cast<float>(max_damage);
	    }

	    float raw_damage = actual_damage + (actual_damage * damage_factor);
        const charge_ptr charge = owner->get_charge_timer();
	    if (weapon_data.max_charge_time_seconds > 0 && charge->is_charging) {
		    charge->advance();
		    raw_damage = static_cast<float>(
				charge->modify_damage(static_cast<int>(raw_damage), 
					weapon_data.linear_factor, weapon_data.exponent));
		    charge->stop_charging();
	    }

        projectile->damage = raw_damage;

		if (weapon_data.launch_angle > 0.0f) {
			// The weapon fires at an angle.
			const float DEFAULT_CANNON_LENGTH = 60.0f;
			
			float distance = static_cast<float>(sqrt(
				pow(projectile->target.y - projectile->origin.y, 2) + 
				pow(projectile->target.x - projectile->origin.x, 2)));
			const float max_distance = static_cast<float>(projectile_data.range);
			if (distance > max_distance)
				distance = max_distance;
			
			const float tilt_angle = weapon_data.launch_angle;
			const float swivel_angle = static_cast<float>(projectile->angle);

			float projection, tipX, tipY, tipZ;
			projection = DEFAULT_CANNON_LENGTH * cos(tilt_angle);
			tipX = -projection * cos(swivel_angle);
			tipY = -projection * sin(swivel_angle);
			tipZ = abs(DEFAULT_CANNON_LENGTH * sin(swivel_angle));
			
			projectile->tip = Vector3(tipX, tipY, tipZ);
			
			// TODO: Work-around. Figure out missing velocity component.
			float offset = 1.1f;
			if (weapon_data.launch_angle > RADIANS_F(45.0f))
				offset = 1.6f;

			const float muzzle_velocity = sqrt(-GRAVITY * distance * offset);
			Vector3 component_velocity;
			component_velocity.x = (muzzle_velocity) * cos(tilt_angle) * cos(swivel_angle);
			component_velocity.y = (muzzle_velocity) * cos(tilt_angle) * sin(swivel_angle);
			component_velocity.z = muzzle_velocity * sin(tilt_angle);

			projectile->velocity_component = component_velocity;
		}
		
		
    }
    catch (const TankNotExistException &) {
    }
}

void Projectile_Manager::add_damageable_object(Damageable_Object *object)
{
	boost::lock_guard<boost::mutex> guard(mutex);
	
	VTANK_ASSERT(object->get_id() >= 0);
	
	damageable_objects[object->get_id()] = object;
}

bool Projectile_Manager::remove_damageable_object(int id)
{
	boost::lock_guard<boost::mutex> guard(mutex);

	VTANK_ASSERT(id >= 0);

	std::map<int, Damageable_Object *>::iterator result = damageable_objects.find(id);
	if (result == damageable_objects.end()) {
		return false;
	}

	damageable_objects.erase(result);

	return true;
}
