/*!
	\file   damageableobject.hpp
	\brief  Abstract class which every in-game damageable object must implement.
	\author (C) Copyright 2010 by Vermont Technical College
*/
#ifndef DAMAGEABLEOBJECT_HPP
#define DAMAGEABLEOBJECT_HPP

//! Interface which each object in the game (i.e. tank, base, etc) must implement.
class Damageable_Object
{
public:
	virtual void set_id(const int) = 0;
	virtual int get_id() const = 0;
    virtual int get_health() const = 0;
    virtual void inflict_damage(const int damage, const int projectile_id, 
		const int projectile_type_id, const int owner) = 0;
    virtual bool is_alive() const = 0;
    virtual VTankObject::Point get_position() const = 0;
    virtual void set_position(const VTankObject::Point &position) = 0;
    virtual float get_radius() const = 0;
    virtual GameSession::Alliance get_team() const = 0;
    virtual void set_team(const GameSession::Alliance &team) = 0;
    virtual float get_armor_factor() const = 0;
	virtual void inflict_environment_damage(const int damage, const int env_id,
		const int env_type_id, const int owner) = 0;
};

typedef std::vector<Damageable_Object *>   damageable_list;
typedef std::map<int, Damageable_Object *> damageable_map;

#endif