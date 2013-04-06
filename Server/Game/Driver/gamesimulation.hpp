/*!
	\file gamesimulation.hpp
	\brief Blueprint for a class which controls game simulations.
	\author Copyright (C) 2010 by Vermont Technical College
*/
#ifndef GAMESIMULATION_HPP
#define GAMESIMULATION_HPP

#include <tank.hpp>
#include <damageableobject.hpp>
#include <eventbuffer.hpp>

//! 
typedef boost::shared_ptr<Damageable_Object> game_object;

//! A game simulation is responsible for running the game.
/*!
	A simulation runs the game itself.
*/
class Game_Simulation
{
private:
	Event_Buffer *buffer;
	std::map<int, game_object> objects;
	
	int generate_id() const;

public:
	Game_Simulation();
	~Game_Simulation();
	
	//! Update the game state and see if any action is required.
	void update();

	//! Add a damageable object to the game.
	/*!
		\param object Object to add to the game.
		\return ID of the new object.
	*/
	int add(const game_object &object);

	//! Gets a game object from the game.
	/*!
		\param id ID to search for.
		\return The damageable object, or NULL if it wasn't found.
	*/
	Damageable_Object *get(const int id) const;
	
	//! Remove a damageable object from the game.
	/*!
		Removes the damageable object.
		\param id ID number of the object to remove.
		\return True if the object was found and removed; false otherwise.
	*/
	bool remove(const int id);

	//! Remove a damageable object from the game.
	/*!
		Removes the damageable object.
		\param object Object to remove.
		\return True if the object was found and removed; false otherwise.
	*/
	bool remove(const game_object &object);
};

#endif
