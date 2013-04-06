/*!
    \file utilitymanager.hpp
    \brief Manages utilities (power-ups) for a single game instance.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef UTILITYMANAGER_HPP
#define UTILITYMANAGER_HPP
#include <Map.hpp>

//! The UtilityManager class handles when, how, and where utilities (power-ups) spawn on the map.
/*!
	UtilityManager must be given a current list of utilities from the server in order to know what power-ups
	are available. In order to decide where utilities spawn on the map, map information must be given to the
	utility manager.
*/
class UtilityManager
{
private:
	VTankObject::UtilityList utils;
	Map *current_map;
	std::vector<VTankObject::Point> positions;
	bool ready_for_spawn;
	double last_spawn;
	double spawn_time;
	double next_spawn;
	double variation;
	int position_index;

	void init(const VTankObject::UtilityList &, Map *);

	void generate_spawn_points();

	void shuffle_spawn_points();

public:
	UtilityManager()
	{ init(VTankObject::UtilityList(), NULL); }

	//! Constructs the UtilityManager based on current game information and available utilities.
	UtilityManager(const VTankObject::UtilityList &, Map *);
   ~UtilityManager();

    //! Check if the next utility is ready to spawn.
    /*!
		\return True if the next utility is ready to spawn; false otherwise.
    */
    bool is_ready();

	//! Update the current list of utilities.
	/*!
		\param list List of available utilities.
	*/
	void update_utility_list(const VTankObject::UtilityList &list) 
		{ utils = list; }

	//! Update the current map.
	/*!
		\param map The new map to take the old one's place.
	*/
	void update_map(Map *map)
		{ current_map = map; generate_spawn_points(); }
	
	//! Obtain where the next utility is going to spawn, and obtain what utility that will be.
	/*!
		\param util [out] Utility that will spawn.
		\param position [out] Where the utility will spawn.
		\param forbidden_list [in] Positions to avoid, if possible.
		\return True if the returned values are valid; false if 'is_ready()' is false.
	*/
	bool get_next_spawn(VTankObject::Utility &, VTankObject::Point &,
		const std::vector<VTankObject::Point> &);
};

struct ActiveUtility
{
	int id;
	VTankObject::Utility util;
	VTankObject::Point pos;
	
	ActiveUtility()
	{
	}

	ActiveUtility(int util_id, const VTankObject::Utility &utility, const VTankObject::Point &position)
		: id(util_id), util(utility), pos(position)
	{
	}
};

#endif
