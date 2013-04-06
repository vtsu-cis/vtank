/*!
	\file   gamehandler.hpp
	\brief  Abstract class which every special game type must implement.
	\author (C) Copyright 2010 by Vermont Technical College
*/
#ifndef GAMEHANDLER_HPP
#define GAMEHANDLER_HPP
//! Abstract interface meant to be implemented by game mode controllers.
class Game_Handler {
public:
	//! Gets the current score for the red team.
	virtual const int get_red_score() const = 0;

	//! Gets the current score for the blue team.
	virtual const int get_blue_score() const = 0;

	//! Gets the team currently winning.
	virtual const GameSession::Alliance get_winning_team() const = 0;

	//! Update the status of the game.
	virtual void update(const tank_array &) = 0;
	
	//! Checks if the game handler has custom spawn points.
	virtual bool has_custom_spawn_points() = 0;
	
	//! Spawns a player if 'has_custom_spawn_points()' returns true.
	virtual void spawn(const tank_ptr &) {};
	
	//! Send the game status to a new tank so that they understand what's going on.
	virtual void send_status_to(const tank_ptr &) {};
};
#endif