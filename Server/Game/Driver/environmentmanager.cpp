/*!
	\file environmentmanager.cpp
	\brief Implementation of the manager of environmental effects.
	\author Copyright (C) 2010 by Vermont Technical College
*/
#include <master.hpp>
#include <environmentmanager.hpp>
#include <nodemanager.hpp>
#include <gamemanager.hpp>
#include <utility.hpp>
#include <notifier.hpp>
#include <logger.hpp>

Environment_Manager::Environment_Manager(const bool overlap)
	: allow_overlap(overlap)
{
}

Environment_Manager::~Environment_Manager()
{
}

int Environment_Manager::generate_unique_id() const
{
	const int size = static_cast<int>(effects.size()) + 20;
	std::map<int, environment_effect_ptr>::const_iterator i;
	for (int id = 0; id < size; ++id) {
		bool not_found = true;
		
		for (i = effects.begin(); i != effects.end(); ++i) {
			const environment_effect_ptr env = i->second;
			if (id == env->get_id()) {
				not_found = false;
				break;
			}
		}
		
		if (not_found)
			return id;
	}

	VTANK_ASSERT(false); // it should never reach here.
	return -1;
}

void Environment_Manager::inflict_damage(const tank_ptr &tank, 
	const environment_effect_ptr &env, int damage)
{
	tank->inflict_environment_damage(damage, env->get_id(),
		env->get_property()->id, env->get_owner_id());

	Notifier::blanket_notify_damage_player_by_env(tank->get_id(),
		env->get_id(), damage, !tank->is_alive());
}

void Environment_Manager::inflict_damage(Damageable_Object *object,
	const environment_effect_ptr &env, int damage)
{
	object->inflict_environment_damage(damage, env->get_id(),
		env->get_property()->id, env->get_owner_id());
}

void Environment_Manager::update(damageable_map *object_list)
{
	NodeManager *nodes = Players::get_node_manager();
	
	std::vector<int> remove_list;
	std::map<int, environment_effect_ptr>::iterator i;
	for (i = effects.begin(); i != effects.end(); ++i) {
		const environment_effect_ptr effect = i->second;
		const int node_id = nodes->get_node_at(effect->get_position());

		if (!effect->interval_reached()) {
			// Not ready to deal damage yet.
			continue;
		}

		const int damage = effect->get_damage();
		
		// Check to see if effect damages players.
		const tank_array tank_list = nodes->get_relevant_players(node_id);
		for (tank_array::size_type i = 0; i < tank_list.size(); ++i) {
			const tank_ptr tank = tank_list[i];
			if (!tank->is_alive() || (effect->get_team() == tank->get_team() && 
					tank->get_team() != GameSession::NONE) ||
					effect->get_owner_id() == tank->get_id()) {
				continue;
			}

			// Eligible for collision check.
			if (Utility::circle_collision(tank->get_position(), tank->get_radius(),
					effect->get_position(), effect->get_radius())) {
				// Collision hit.
				Logger::debug("[ENV] #%d hit %s for %d damage!",
					effect->get_id(), tank->get_name().c_str(), damage);
				inflict_damage(tank, effect, damage);
			}
		}
		
		// Check to see if effect damages objects.
		damageable_map::const_iterator i;
		for (i = object_list->begin(); i != object_list->end(); ++i) {
			Damageable_Object *object = i->second;
			/*const int object_node = nodes->get_node_at(object->get_position());
			if (nodes->is_near(node_id, object_node)) {
				// They are not near each other.
				continue;
			}*/

			if (!object->is_alive() || (object->get_team() != GameSession::NONE &&
					object->get_team() == effect->get_team())) {
				// Not eligible for collision.
				continue;
			}

			if (Utility::circle_collision(object->get_position(), object->get_radius(),
					effect->get_position(), effect->get_radius())) {
				Logger::debug("[ENV] #%d hit object #%d for %d damage!",
					effect->get_id(), object->get_id(), damage);
				inflict_damage(object, effect, damage);
			}
		}

		if (effect->has_expired()) {
			Logger::debug("[ENV] #%d expired.", effect->get_id());
			remove_list.push_back(effect->get_id());
		}
	}

	while (remove_list.size() > 0) {
		remove(remove_list[0]);
		remove_list.erase(remove_list.begin());
	}
}

int Environment_Manager::spawn(EnvironmentProperty *prop, const GameSession::Alliance &team,
							   const VTankObject::Point &position, const int owner_id)
{
	const int new_id = generate_unique_id();
	const environment_effect_ptr env = environment_effect_ptr(
		new Active_Environment_Effect(new_id, prop, team, position, owner_id));

	effects[new_id] = env;
	Logger::debug("[ENV] #%d spawned at (%d, %d)", new_id,
		(int)position.x, (int)position.y);

	return new_id;
}

bool Environment_Manager::remove(const int id)
{
	std::map<int, environment_effect_ptr>::iterator i = effects.find(id);
	if (i == effects.end()) {
		// Did not find ID to remove.
		return false;
	}

	(void)effects.erase(i);
	return true;
}

int Environment_Manager::size() const
{
	return static_cast<int>(effects.size());
}
