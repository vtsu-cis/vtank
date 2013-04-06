/*!
    \file   utilitymanager.cpp
    \brief  Implementation of the UtilityManager class.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#include "master.hpp"
#include "utilitymanager.hpp"
#include <logger.hpp>

#define DEFAULT_SPAWN_TIME 15000
#define DEFAULT_VARIATION 5000

#define EVENT_UTILITY 7

//static inline double get_current_time()
//{
//	return static_cast<double>(IceUtil::Time::now().toMilliSeconds());
//}

static inline double get_variation(double bound)
{
	double negate = (rand() % 2 == 0 ? 1 : -1);
	double variation = negate * (rand() % (int)bound + 1);

	return variation;
}

UtilityManager::UtilityManager(const VTankObject::UtilityList &list, Map *map)
{
	init(list, map);
}

void UtilityManager::init(const VTankObject::UtilityList &list, Map *map)
{
	utils = list;
	current_map = map;
	ready_for_spawn = false;
	variation = DEFAULT_VARIATION;
	spawn_time = DEFAULT_SPAWN_TIME;
	position_index = 0;

	const double current_time = get_current_time();
	next_spawn = current_time + spawn_time + get_variation(variation);
	ready_for_spawn = false;
	
	if (map != NULL) {
		generate_spawn_points();
	}
}

UtilityManager::~UtilityManager(void)
{
}

void UtilityManager::shuffle_spawn_points()
{
	std::random_shuffle(positions.begin(), positions.end());
}

void UtilityManager::generate_spawn_points()
{
	{
		std::ostringstream formatter;
		formatter << "Generating spawn points for map: " << current_map->get_title() 
			<< " (" << current_map->get_width() << "x" << current_map->get_height() << ").";
		Logger::log(Logger::LOG_LEVEL_DEBUG, formatter.str());
	}
	
	position_index = 0;
	positions.clear();
	for (int y = 0; y < current_map->get_height(); ++y) {
		for (int x = 0; x < current_map->get_width(); ++x) {
			const Tile tile = current_map->get_tile(x, y);
            if (tile.event_id == EVENT_UTILITY) {
                // Spawn point found.
                VTankObject::Point point;
                point.x = (x * TILE_SIZE) + (TILE_SIZE / 2);
                point.y = -((y * TILE_SIZE) + (TILE_SIZE / 2));

                positions.push_back(point);
            }
		}
	}
	
	if (positions.size() == 0) {
		Logger::log(Logger::LOG_LEVEL_WARNING,
			"There are no spawn points for utilities on this map!");
		return;
	}

	shuffle_spawn_points();
}

bool UtilityManager::is_ready()
{
	if (ready_for_spawn) {
		return true;
	}

	const double current_time = get_current_time();
	if (current_time >= next_spawn) {
		ready_for_spawn = true;
	}

	return ready_for_spawn;
}

bool UtilityManager::get_next_spawn(VTankObject::Utility &util, VTankObject::Point &position,
									const std::vector<VTankObject::Point> &forbidden_list)
{
	if (positions.size() == 0) {
		return false;
	}

	if (is_ready()) {
		// Randomly select utility.
		const int util_index = rand() % utils.size();
		VTANK_ASSERT(util_index >= 0 && util_index < static_cast<int>(utils.size()));
		util = utils[util_index];
        
        VTANK_ASSERT(position_index >= 0 && position_index < static_cast<int>(positions.size()));

		// Select a spawn point.
		VTankObject::Point suggested_position = positions[position_index];
		int original = position_index;
		bool done = false;
		
        // Loop to see if our position is blacklisted.
		while (!done) {
			// Ensure suggested position is not blacklisted.
			bool is_forbidden = false;
			std::vector<VTankObject::Point>::const_iterator i = forbidden_list.begin();
			for (; i != forbidden_list.end(); ++i) {
				if (suggested_position.x == i->x && suggested_position.y == i->y) {
					// Our position is blacklisted.
					is_forbidden = true;
					break;
				}
			}

			if (!is_forbidden) {
				// Found a good match.
				position = suggested_position;
				done = true;
			}
			else {
                ++position_index;
                // Wrap position index around to 0 if necessary.
			    if (position_index >= static_cast<int>(positions.size())) {
				    shuffle_spawn_points();
				    position_index = 0;
			    }

                if (position_index == original) {
				    // Did full loop already: stack two utilities on top of each other as fallback.
				    position = suggested_position;
				    done = true;
			    }
                else {
                    suggested_position = positions[position_index];
                }
			}
		} // while (!done)
        
        Logger::debug("Utility %s has spawned at (%f, %f).", 
            util.model.c_str(), position.x, position.y);

		const double current_time = get_current_time();
		next_spawn = current_time + spawn_time + get_variation(variation);
		ready_for_spawn = false;

		return true;
	}
	
	return false;
}