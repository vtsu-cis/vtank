/*!
	\file envproperty.hpp
	\brief Represents an active environment property.
	\author Copyright (C) 2010 by Vermont Technical College
*/
#ifndef ENVPROPERTY_HPP
#define ENVPROPERTY_HPP

#include <weapon.hpp>
#include <vtassert.hpp>
using VTankObject::Point;

//! Tracks a single environmental effect.
/*!
	Stores properties of an active (i.e. currently on the map) environmental effect.
*/
class Active_Environment_Effect
{
private:
	int id;
	int owner_id;
	EnvironmentProperty *env;
	GameSession::Alliance alliance;
	Point pos;
	double expire_period;
	double next_damage_period;
	bool interval_flag;

public:
	Active_Environment_Effect(const int ID, EnvironmentProperty *environment_prop, 
		const GameSession::Alliance &team, const Point &position, const int owner_ID)
		: id(ID), alliance(team), pos(position), interval_flag(false), owner_id(owner_ID)
	{
		VTANK_ASSERT(env != NULL);
		env = environment_prop;

		const double current_time = get_current_time();
		expire_period = current_time + env->duration_seconds * 1000.0f;
		next_damage_period = current_time + env->interval_seconds * 1000.0f;
	}

	/*!
		Gets the ID of this environmental effect.
		\return Unique ID number.
	*/
	const int get_id() const
	{
		return id;
	}
	
	/*!
		Get the ID of the person who fired the projectile.
		\return ID of the tank owner.
	*/
	const int get_owner_id() const
	{
		return owner_id;
	}

	/*!
		Gets the team of the player who created the environment effect, if any.
		\return GameSession::Alliance enumeration representing the environment effect's
		assocation to a team, if any. GameSession::NONE indicates no team.
	*/
	const GameSession::Alliance get_team() const
	{
		return alliance;
	}
	
	/*!
		Get information about the properties of this environmental effect.
		\return Pointer to the internal EnvironmentProperty object.
	*/
	const EnvironmentProperty *get_property() const
	{
		return env;
	}

	/*!
		Gets the radius of this environmental effect.
		\return Radius of this effect.
	*/
	const float get_radius() const
	{
		return env->aoe_radius;
	}
	
	/*!
		Gets the position of the environmental property.
		\return This environment's position.
	*/
	const Point get_position() const
	{
		return pos;
	}
	
	/*!
		Check if the damage interval has been reached.
		\return True if the damage interval was reached, allowing 'get_damage()' to be
		called; false otherwise.
	*/
	const bool interval_reached()
	{
		if (interval_flag)
			return true;

		const double current_time = get_current_time();
		if (current_time >= next_damage_period) {
			interval_flag = true;

			next_damage_period = current_time + env->interval_seconds * 1000.0f;

			return true;
		}

		return false;
	}
	
	/*!
		Get the *current* damage dealt by this environment effect. This throws an std::logic_error
		if 'interval_reached()' has not yet returned true.
		\return Damage to deal to nearby players.
	*/
	int get_damage()
	{
		VTANK_ASSERT(interval_flag == true);
		interval_flag = false;

		return random_next(env->minimum_damage, env->maximum_damage);
	}
	
	/*!
		Check if the environment effect has expired.
		\return True if it has expired; false otherwise.
	*/
	bool has_expired() const
	{
		const double current_time = get_current_time();
		return current_time >= expire_period;
	}
};

typedef boost::shared_ptr<Active_Environment_Effect> environment_effect_ptr;

#endif