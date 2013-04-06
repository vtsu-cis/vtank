/*!
	\file weapon.hpp
	\brief Store properties of weapons and related items (projectiles, environments).
	\author Copyright (C) 2010 by Vermont Technical College
*/
#ifndef WEAPON_HPP
#define WEAPON_HPP

//! Contains information about in-game environmental effects, such as fire on a ground.
struct EnvironmentProperty
{
	int id;
	std::string name;
	bool spawn_on_wall_hit;
	bool spawn_on_player_hit;
	bool spawn_on_expiration;
	float duration_seconds;
	float interval_seconds;
	float aoe_radius;
	float aoe_decay;
	int minimum_damage;
	int maximum_damage;
};

//! Contains information about a projectile.
struct Projectile
{
	int id;
	std::string name;
	float aoe_radius;
	bool aoe_is_cone;
	float aoe_decay;
	int cone_origin_width;
	float cone_radius;
	bool cone_damage_full_area;
	int minimum_damage;
	int maximum_damage;
	bool is_instantaneous;
	float initial_velocity;
	float terminal_velocity;
	float acceleration;
	int range;
	int range_variation;
	int jump_count;
	int jump_range;
	float jump_decay;
	float collision_radius;
	float object_damage_factor;
	EnvironmentProperty *environment_property;
};

//! Contains information about a weapon.
struct Weapon
{
	int id;
	std::string name;
	float cooldown;
	float launch_angle;
	float max_charge_time_seconds;
	int projectiles_per_shot;
	float interval_between_projectile_seconds;
	float overheat_time;
	float overheat_amount_per_shot;
	float overheat_recovery_speed;
	float overheat_recover_start_time;
	float linear_factor;
	float exponent;
	Projectile projectile;
};

#endif
