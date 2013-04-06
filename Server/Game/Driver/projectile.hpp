/*!
    \file   projectile.hpp
    \brief  Implements the Active_Projectile struct.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef PROJECTILE_HPP
#define PROJECTILE_HPP

#include <weapon.hpp>
#include <vector3.hpp>

/*!
    An active projectile holds data about a projectile that has been fired in-game.
*/
struct Active_Projectile
{
private:
	void calculate_alive_time()
	{
		if (expireTimeMillis <= 0) {
			// time = distance / velocity
			expireTimeMillis = static_cast<long>(
				(static_cast<float>(type.projectile.range) /
					type.projectile.initial_velocity) * 1000.0f);
		}
	}

public:
    int id;
    int owner;
    long millisecondsAlive;
	long expireTimeMillis;
    double angle;
    double velocity;
    int node_id;
	VTankObject::Point origin;
	VTankObject::Point target;
    VTankObject::Point position;
    Weapon type;
    float damage;
	Vector3 tip;				 // for arc calculations.
	Vector3 velocity_component; // for arc calculations.

    Active_Projectile() 
        : id(-1), owner(-1), millisecondsAlive(0), angle(0), node_id(-1),
        position(VTankObject::Point()), type(), expireTimeMillis(-1), velocity(0), damage(0)
    {}

    Active_Projectile(int projectileId, int ownerId, double projectileAngle,
		const VTankObject::Point &projectilePosition, const VTankObject::Point &a_target,
        const Weapon &weaponType)
        : id(projectileId), owner(ownerId), millisecondsAlive(0), angle(projectileAngle),
        node_id(-1), position(projectilePosition), origin(projectilePosition), type(weaponType), 
		damage(0), target(a_target)
    {
        velocity = type.projectile.initial_velocity;
		calculate_alive_time();
	}

    //! Increment the alive counter by some delta time.
    void advance(long delta_time)
    {
		calculate_alive_time();

        millisecondsAlive += delta_time;
    }

    //! Check if the projectile has expired.
    bool expired()
    {
		calculate_alive_time();

		return millisecondsAlive >= expireTimeMillis && type.launch_angle == 0;
    }
};

typedef boost::shared_ptr<Active_Projectile> projectile_ptr;
typedef std::vector<projectile_ptr> projectile_array;
#endif
