/*!
    \file gamemanager.hpp
    \brief Declares functions and members that help manage the game.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef GAMEMANAGER_HPP
#define GAMEMANAGER_HPP

#include <nodemanager.hpp>
#include <utilitymanager.hpp>
#include <projectilemanager.hpp>
#include <gamehandler.hpp>
#include <weaponsettings.hpp>

namespace Players
{
    extern NodeManager nodes;

	/*!
		Gets the manager responsible for tracking in-game projectiles.
	*/
	Projectile_Manager *get_projectile_manager();
	
	/*!
		Gets the manager responsible for managing in-game nodes.
	*/
	NodeManager *get_node_manager();
	
	/*!
		Get the weapon data.
	*/
	Weapon_Settings *get_weapon_data();

    /*!
        Generate a position for a tank.
        \return Generated spawn position.
    */
    const VTankObject::Point generate_position();

	void generate_spawn_position(const tank_ptr &);

	Game_Handler *get_game_handler();

	void force_timer_zero();

	/*!
		Get any currently active utilities (e.g. utilities on the map,
		but not yet acquired).
	*/
	std::vector<ActiveUtility> get_active_utilities();

    /*!
        Start the game. This should only be called once.
    */
    void start_game();

    /*!
        Block and wait for every task in the thread pool finishes. When this function
        returns, the Gamespace has no more player actions to process.
    */
    void wait_for_tasks();

    /*!
        Get the amount of time left on the current map.
        \return Value in milliseconds.
    */
    double get_time_left();
	
	/*!
		Get the game handler for this game.
		// TODO: The game handler shouldn't be of type CTF.
		\return Game handler, or NULL if the handler is death match.
	*/
	//CTF_Helper *get_game_handler();

	//! TODO: Remove me.
	int get_red_score();

	//! TODO: Remove me.
	int get_blue_score();

    /*!
        Execute a task to process a tank movement.
        \param id ID of the tank moving.
        \param timestamp Stamp indicating when the client started to move.
        \param direction The direction the tank is moving towards. 
                         Valid values are: FORWARD, REVERSE, STOP.
        \param position For synchronization purposes, the client gives us the position
                        of his tank when he performed the action.
    */
    void move(const int&, const Ice::Long&, 
        const VTankObject::Direction, const VTankObject::Point&);

    /*!
        Execute a task to process a tank rotation.
        \param id ID of the tank moving.
        \param timestamp Stamp indicating when the client started to rotate.
        \param angle For synchronization purposes, the client gives us the angle
                     of his tank when he performed the action.
        \param direction The direction the tank is rotating towards.
                         Valid values are: LEFT, RIGHT, STOP.
    */
    void rotate(const int&, const Ice::Long&, const Ice::Double&, 
        const VTankObject::Direction);

    /*!
        Rather than actually processing this, this function immediately attempts to 
        distribute the message to other clients.
        \param id ID of the tank spinning it's turret.
        \param timestamp Not that it matters, but a stamp indicating when the client's
                         turret starting moving.
        \param angle Initial angle of the turret.
        \param direction Whether the turret is spinning left or right.
                         Valid values are: LEFT, RIGHT, STOP.
    */
    void spin_turret(const int &, const Ice::Long &, const Ice::Double &,
        const VTankObject::Direction);

    /*!
        Execute a task to procses a tank firing his weapon.
        \param id ID of the tank firing.
        \param timestamp Stamp indicating when the client fired his weapon.
        \param point Position of the mouse-click (relative to the tank).
    */
    void fire(const int &, const Ice::Long &, const VTankObject::Point &);

	/*!
		Update the server's list of possible utilities.
	*/
	void update_utility_list(const VTankObject::UtilityList &);

	/*!
		Send the status of the game to the tank.
	*/
	void send_status_to(const tank_ptr &);
}

#endif
