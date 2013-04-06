/*!
    \file   projectilemanager.hpp
    \brief  Declares the Projectile_Manager class.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef PROJECTILEMANAGER_HPP
#define PROJECTILEMANAGER_HPP

#include <player.hpp>
#include <projectile.hpp>
#include <nodemanager.hpp>
#include <damageableobject.hpp>
#include <environmentmanager.hpp>

//! Define how far away a projectile spawns from a tank position.
#define PROJECTILE_SPAWN_OFFSET 50.0f

/*!
    Handle projectiles inside of the game. This includes creation and deletion.
*/
class Projectile_Manager
{
private:
    boost::mutex mutex;
    std::map<int, projectile_ptr> projectiles;
    damageable_map damageable_objects;
	Environment_Manager environment;
	// TODO: Environment manager doesn't make sense here. We only keep it here because
	// the projectile manager is here. Solution: move damageable object collection to
	// somewhere else.

    /*!
        Generate a unique projectile ID.
        \return Generated ID number.
    */
    int generate_projectile_id();

	/*!
		Actual remove procedure.
	*/
	bool do_remove(const int &);

    /*!
        Perform a collision check on a single projectile.
        \param id Projectile ID.
        \param projectile Projectile to check.
        \return True if the projectile collided with a player.
    */
    bool perform_collision_check(NodeManager &, const projectile_ptr);
    
    //! Do on-fire calculations (i.e. applying variance).
    void do_initial_calculations(const projectile_ptr &projectile);
    
    //! Do projectile delta calculations.
    bool do_projectile_calculations(const projectile_ptr &, double);

public:
    Projectile_Manager();
   ~Projectile_Manager();

   /*!
        Add a projectile to the manager.
        \param owner ID of the person who fired the projectile.
        \param angle Angle that the projectile is moving towards.
        \param position Position of the projectile.
		\param target Where the projectile is heading towards.
        \param type Type of projectile that has been fired.
		\param new_target New target of the projectile.
   */
   int add(const int &, const double &, 
       const VTankObject::Point &, const VTankObject::Point &, const Weapon &,
	   VTankObject::Point &new_target = VTankObject::Point());

   /*!
        Clears all data to a clean slate.
   */
   void reset();

   /*!
        Get a projectile by it's ID.
        \param id ID of the projectile to look for.
        \return Copy of the Active_Projectile object.
   */
   projectile_ptr get(const int &);

   /*!
        Remove a projectile from the manager.
        \param projectileId ID of the projectile to remove.
        \return True if the projectile was found and removed. False if not.
   */
    bool remove(const int &);

    /*!
        Process the projectile movements, positions, and collisions.
        \param players Array list of players.
        \param node_manager Pointer to the node manager.
        \param delta_time Time since the last process.
    */
    void process(NodeManager &, const double &);

	/*!
		Get a list of projectiles that are stored in this manager.
		\return Array list of projectiles.
	*/
	projectile_array get_projectiles();

	/*!
		Add a damageable object to consideration to the projectile manager. This object
		is expected to react to being damaged of it's own implementation.
		Note that this method depends on the implementation of 'get_id()'. If the ID is
		not correctly set, strange things may happen.
		\param object Object to add.
	*/
	void add_damageable_object(Damageable_Object *object);

	/*!
		Remove a previously added damageable object from the game.
		\return True if the damageable object was found and removed; false otherwise.
	*/
	bool remove_damageable_object(int id);
};

#endif
