/*!
	\file environmentmanager.hpp
	\brief Blueprint for the manager of environmental effects.
	\author Copyright (C) 2010 by Vermont Technical College
*/
#ifndef ENVIRONMENTMANAGER_HPP
#define ENVIRONMENTMANAGER_HPP

#define ALLOW_OVERLAP_BY_DEFAULT false

#include <envproperty.hpp>
#include <tank.hpp>

//! Manages in-game environmental effects.
class Environment_Manager
{
private:
	bool allow_overlap;
	std::map<int, environment_effect_ptr> effects;
	
	//! Generates a unique ID number for an environment effect.
	int generate_unique_id() const;
	
	//! Inflicts damage to a player.
	void inflict_damage(const tank_ptr &tank, 
		const environment_effect_ptr &env, int damage);

	//! Inflicts damage to an object.
	void inflict_damage(Damageable_Object *object,
		const environment_effect_ptr &env, int damage);

public:
	Environment_Manager(const bool allow_overlapping_effects = ALLOW_OVERLAP_BY_DEFAULT);
	~Environment_Manager();

	//! Spawn an environmental effect at a given point.
	int spawn(EnvironmentProperty *prop, const GameSession::Alliance &team,
		const VTankObject::Point &position, const int owner_id);
	
	//! Update the environment manager.
	void update(damageable_map *object_list);
	
	//! Remove an environmental effect from the game.
	bool remove(const int id);

	//! Gets the size of this container.
	int size() const;
	
	//! Clear all elements from this manager.
	void clear();
	
	//! Set whether the environment manager should allow overlapping effects.
	void should_allow_overlap(bool value) { allow_overlap = value; }
	
	//! Check whether the environment manager should allow overlapping effects.
	const bool allows_overlap() const { return allow_overlap; }
};

#endif
