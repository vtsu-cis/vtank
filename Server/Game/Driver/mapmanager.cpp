/*!
    \file mapmanager.cpp
    \brief Implementation of the map manager namespace.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#include <master.hpp>
#define SHA1_UTILITY_FUNCTIONS
#include "SHA1.h"
#include <Map.hpp>
#include <mapmanager.hpp>
#include <server.hpp>
#include <vtassert.hpp>
#include <gamemanager.hpp>
#include <macros.hpp>
#include <logger.hpp>
#include <nodemanager.hpp>
#include <playermanager.hpp>
#include <utility.hpp>

#define EVENT_DEATHMATCH_SPAWN 1
#define EVENT_TEAM_DEATHMATCH_RED 2
#define EVENT_TEAM_DEATHMATCH_BLUE 3

#define MODE_DEATHMATCH 0
#define MODE_TEAMDEATHMATCH 1
#define MODE_CAPTURETHEFLAG 2
#define MODE_CAPTURETHEBASE 3

namespace MapManager
{
    boost::shared_mutex mutex;
    boost::mutex rotate_mutex;
    Map * current_map = NULL;
    std::string current_map_filename = "";
    VTankObject::GameMode current_game_mode = VTankObject::DEATHMATCH;
    Ice::StringSeq map_list;
    bool rotating;
    int current_map_id = -1;
    SelectionMode selection_technique = SELECT_ROUND_ROBIN;

    std::vector<VTankObject::Point> generated_positions;
    std::vector<VTankObject::Point> red_positions;
    std::vector<VTankObject::Point> blue_positions;

    std::vector<VTankObject::Point>::size_type position_index = 0;
    std::vector<VTankObject::Point>::size_type red_index = 0;
    std::vector<VTankObject::Point>::size_type blue_index = 0;

    bool is_rotating()
    {
        boost::lock_guard<boost::mutex> guard(rotate_mutex);
        return rotating;
    }

    void set_rotating(bool rotating_value)
    {
        boost::lock_guard<boost::mutex> guard(rotate_mutex);
        rotating = rotating_value;
    }

    void set_selection_technique(const SelectionMode mode) 
    {
        boost::lock_guard<boost::mutex> guard(rotate_mutex);
        selection_technique = mode;
    }

    void start()
    {
        // Check if MAPS_DIR directory exists.
#if TARGET == WINTARGET
        if (_access(MAPS_DIR, 0) != 0) {
            // Directory doesn't exist.
            if (_mkdir(MAPS_DIR) != 0) {
                throw std::runtime_error("Unable to create " MAPS_DIR " directory.");
            }
        }
#elif TARGET == LINTARGET
        if (access(MAPS_DIR, F_OK) != 0) {
            // Directory doesn't exist.
            if (mkdir(MAPS_DIR, 0777) != 0) {
                throw std::runtime_error("Unable to create " MAPS_DIR " directory.");
            }
        }
#endif
    }

	/*!
		Convenience function for downloading a map.
	*/
	void download_map()
	{
		const std::string file_path = MAPS_DIR + current_map_filename;

#if TARGET == WINTARGET
        const int access_value = _access(file_path.c_str(), 0);
#elif TARGET == LINTARGET
        const int access_value = access(file_path.c_str(), F_OK);
#endif
		bool needs_download = access_value != 0;
		if (!needs_download) {
			// The map exists, so check the hash for integrity.
			CSHA1 sha1;
			if (!sha1.HashFile(file_path.c_str())) {
				// The hash failed for some reason -- re-download.
                Logger::log(Logger::LOG_LEVEL_WARNING, "Could not run hash for map file.");
				needs_download = true;
			}
			else {
				sha1.Final();
				TCHAR hash[41];
				sha1.ReportHash(hash, CSHA1::REPORT_HEX_SHORT);
				hash[40] = '\0';

				// Convert the TCHAR sequence to a string, then make it lower-case.
				std::string string_hash = std::string((char *)(hash));
				string_hash = Utility::to_lower(string_hash);

				std::ostringstream formatter;
				formatter << "Hash for " << current_map_filename << ": " << string_hash;

				Logger::log(Logger::LOG_LEVEL_DEBUG, formatter.str());

				// Ask the server if the hash is a valid one for this map.
				if (!Server::mtg_service.get_proxy()->HashIsValid(
					current_map_filename, string_hash)) {
					std::ostringstream formatter;
					formatter << "Hash for " << current_map_filename << " invalid, must re-download map.";

					Logger::log(Logger::LOG_LEVEL_DEBUG, formatter.str());
					needs_download = true;
				}
			}
		}

	    current_map = new Map();
	    if (needs_download) {
		    // Map doesn't exist.
		    VTankObject::Map map = Server::mtg_service.get_proxy()->DownloadMap(
                current_map_filename);
		    if (!current_map->create(map.width, map.height, map.title)) {
			    std::ostringstream formatter;
			    formatter << "Map::create failed: " << current_map->get_last_error();

			    Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());

			    throw std::runtime_error("Unable to create a new map from a downloaded map.");
		    }

            for (VTankObject::SupportedGameModes::size_type i = 0;
                i < map.supportedGameModes.size(); i++) {
                    current_map->add_supported_game_mode(map.supportedGameModes[i]);
            }

		    for (int y = 0; y < map.height; y++) {
			    for (int x = 0; x < map.width; x++) {
				    const unsigned position = y * map.width + x;
				    VTankObject::Tile tile = map.tileData[position];
				    current_map->set_tile_id(x, y, tile.id);
                    current_map->set_tile_object(x, y, tile.objectId);
                    current_map->set_tile_event(x, y, tile.eventId);
				    current_map->set_tile_collision(x, y, tile.passable);
                    current_map->set_tile_height(x, y, tile.height);
                    current_map->set_tile_type(x, y, tile.type);
                    current_map->set_tile_effect(x, y, tile.effect);
			    }
		    }

		    if (!current_map->save(file_path)) {
			    std::ostringstream formatter;
			    formatter << "Map::save failed: " << current_map->get_last_error();

			    Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());

			    throw std::runtime_error("Unable to save downloaded map.");
		    }

            std::ostringstream formatter;
            formatter << "Downloaded map " << current_map_filename << ".";

            Logger::log(Logger::LOG_LEVEL_INFO, formatter.str());
	    }
	    else {
		    if (!current_map->load(file_path)) {
			    std::ostringstream formatter;
			    formatter << "Map::load failed: " << current_map->get_last_error();

			    Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());

			    throw std::runtime_error("Unable to load map.");
		    }
	    }
	}

    /*!
        Select a map to play on.
    */
    void select_map()
    {
        if (selection_technique == SELECT_RANDOM) {
            // Random: Select any map except for the last map.
            while (true) {
                const int last_id = current_map_id;
                const Ice::StringSeq::size_type id = rand() % map_list.size();

                VTANK_ASSERT(id >= 0 && id < map_list.size());

                if (last_id == id && map_list.size() > 1)
                    // Don't use the same map twice.
                    continue;

                current_map_filename = map_list[id];
                current_map_id = id;

                break;
            }
        }
        else if (selection_technique == SELECT_ROUND_ROBIN) {
            // Round robin: Scroll through each map one-by-one.
			if (current_map_id < 0) {
				int id = rand() % static_cast<int>(map_list.size());

				VTANK_ASSERT(id >= 0 && id < static_cast<int>(map_list.size()));

				current_map_id = id;
			}

            Ice::StringSeq::size_type new_id = current_map_id + 1;
            if (new_id < 0 || new_id >= map_list.size()) {
                new_id = 0;
            }

            current_map_filename = map_list[new_id];
            current_map_id = new_id;
        }
    }

    /*!
        Ask if it's legal to play the current map. It's illegal if it is corrupted
        (it's only considered corrupted if it supports zero game modes, which should be
        disallowed) or if there aren't enough players for the current map.
    */
    bool is_legal() 
    {
        const std::vector<int> game_modes = current_map->get_supported_game_modes();
        if (game_modes.size() == 0) {
            // We can't play on a map where no game modes are supported.
            return false;
        }

        if (Utility::contains(game_modes, MODE_DEATHMATCH)) {
            // No player size required.
            return true;
        }

		// TODO: Temporarily disabled.
        /*if (Players::tanks.size() < 4) {
            // Too few players.
            return false;
        }*/

        // There are no objections to playing.
        return true;
    }
    
    /*!
        Select a game mode.
    */
    void select_game_mode() 
    {
        // TODO: Choose game mode more intelligently.
		std::vector<int> game_modes = current_map->get_supported_game_modes();
		
		// TODO: Temporary code. Remove me later, and uncomment below.
		if (Utility::contains(game_modes, MODE_CAPTURETHEBASE)) {
			current_game_mode = VTankObject::CAPTURETHEBASE;
		}
		else if (Utility::contains(game_modes, MODE_CAPTURETHEFLAG)) {
			current_game_mode = VTankObject::CAPTURETHEFLAG;
		}
		else {
			current_game_mode = VTankObject::DEATHMATCH;
		}

        /*if (game_modes.size() > 0 && Players::tanks.size() >= 4) {
            // Shuffle the game mode list.
            std::random_shuffle(game_modes.begin(), game_modes.end());
            
            // Peek at the first one.
            const int game_mode = game_modes[0];
            switch (game_mode) {
            case MODE_TEAMDEATHMATCH:
                current_game_mode = VTankObject::TEAMDEATHMATCH;
                break;
            case MODE_CAPTURETHEFLAG:
				current_game_mode = VTankObject::CAPTURETHEFLAG;
                break;
			case MODE_CAPTURETHEBASE:
				current_game_mode = VTankObject::CAPTURETHEBASE;
				break;
            default:
                current_game_mode = VTankObject::DEATHMATCH;
            };

        }
        else {
			VTANK_ASSERT(Utility::contains(game_modes, MODE_DEATHMATCH));
            current_game_mode = VTankObject::DEATHMATCH;
        }*/
    }

    void rotate()
    {
        //set_rotating(true);

        {
            boost::unique_lock<boost::shared_mutex> guard(mutex);
            // De-allocate the current map, if it is not null.
            if (current_map != NULL) {
                delete current_map;
				current_map = NULL;
            }

            if (map_list.size() == 0) {
                // Nothing to do, no maps.
                return;
            }
            
            bool selecting_map = true;
            while (selecting_map) {
                // TODO: Decide which map we want to play on more intelligently.
                select_map();

		        download_map();
                
                selecting_map = !is_legal();
				if (selecting_map) {
					std::ostringstream formatter;
					formatter << "Skipping map " << current_map_filename << ".";

					Logger::log(Logger::LOG_LEVEL_DEBUG, formatter.str());
				}
            }

            select_game_mode();

            generate_positions();

            Players::nodes.set_map(current_map);
            Server::mtg_service.get_proxy()->SetCurrentMap(current_map_filename);
            Server::mtg_service.get_proxy()->SetCurrentGameMode(current_game_mode);

            std::ostringstream formatter;
            formatter << "Rotated to the next map: " << current_map->get_title()
                << ", game mode: " << Utility::to_string(current_game_mode);

            Logger::log(Logger::LOG_LEVEL_INFO, formatter.str());
        }

        //set_rotating(false);
    }

    const Map * const get_current_map()
    {
        boost::shared_lock<boost::shared_mutex> guard(mutex);
        return current_map;
    }

    const std::string get_current_map_filename()
    {
        boost::shared_lock<boost::shared_mutex> guard(mutex);
        return current_map_filename;
    }

    const VTankObject::GameMode get_current_mode()
    {
        boost::shared_lock<boost::shared_mutex> guard(mutex);
        return current_game_mode;
    }

    void generate_positions()
    {
        generated_positions.clear();
        red_positions.clear();
        blue_positions.clear();
        if (current_game_mode == VTankObject::DEATHMATCH) {
            for (int y = 0; y < current_map->get_height(); y++) {
                for (int x = 0; x < current_map->get_width(); x++) {
                    const Tile tile = current_map->get_tile(x, y);
                    if (tile.event_id == EVENT_DEATHMATCH_SPAWN) {
                        // Spawn point found.
                        VTankObject::Point point;
                        point.x = (x * TILE_SIZE) + (TILE_SIZE / 2);
                        point.y = -((y * TILE_SIZE) + (TILE_SIZE / 2));

                        generated_positions.push_back(point);
                    }
                }
            }

            std::random_shuffle(generated_positions.begin(), generated_positions.end());
        }
        else /*if (current_game_mode == VTankObject::TEAMDEATHMATCH)*/ {
            for (int y = 0; y < current_map->get_height(); y++) {
                for (int x = 0; x < current_map->get_width(); x++) {
                    const Tile tile = current_map->get_tile(x, y);
                    if (tile.event_id == EVENT_TEAM_DEATHMATCH_RED) {
                        VTankObject::Point point;
                        point.x = (x * TILE_SIZE) + (TILE_SIZE / 2);
                        point.y = -((y * TILE_SIZE) + (TILE_SIZE / 2));

                        red_positions.push_back(point);
                    }
                    else if (tile.event_id == EVENT_TEAM_DEATHMATCH_BLUE) {
                        VTankObject::Point point;
                        point.x = (x * TILE_SIZE) + (TILE_SIZE / 2);
                        point.y = -((y * TILE_SIZE) + (TILE_SIZE / 2));

                        blue_positions.push_back(point);
                    }
                }
            }

            std::random_shuffle(red_positions.begin(), red_positions.end());
            std::random_shuffle(blue_positions.begin(), blue_positions.end());
        }

        position_index = 0;
        red_index = 0;
        blue_index = 0;
    }

    void set_spawn_position_team_deathmatch(tank_ptr tank)
    {
        VTANK_ASSERT(tank->get_team() != GameSession::NONE);

        const GameSession::Alliance team = tank->get_team();
        if (team == GameSession::RED) {
            VTankObject::Point p = red_positions[red_index++];

            if (red_index >= red_positions.size()) {
                std::random_shuffle(red_positions.begin(), red_positions.end());
                red_index = 0;
            }

            tank->set_position(p);
        }
        else if (team == GameSession::BLUE) {
            VTankObject::Point p = blue_positions[blue_index++];

            if (blue_index >= blue_positions.size()) {
                std::random_shuffle(blue_positions.begin(), blue_positions.end());
                blue_index = 0;
            }

            tank->set_position(p);
        }
    }

    void generate_spawn_position(tank_ptr tank)
    {
        boost::unique_lock<boost::shared_mutex> guard(mutex);
        if (current_game_mode != VTankObject::DEATHMATCH) {
            set_spawn_position_team_deathmatch(tank);
        }
        else {
            try {
                VTANK_ASSERT(position_index >= 0 && position_index < generated_positions.size());
            }
            catch (const std::logic_error &e) {
                std::ostringstream formatter;
                formatter << "Logic error in mapmanager.cpp:generate_spawn_position: " << e.what();
                Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());
            }
            /*std::cout << "Generating spawn at position " << position_index << 
                " (size=" << generated_positions.size() << ")" << std::endl;*/
            VTankObject::Point p = generated_positions[position_index++];

            if (position_index >= generated_positions.size()) {
                std::random_shuffle(generated_positions.begin(), generated_positions.end());
                position_index = 0;
            }

            tank->set_position(p);
        }
    }

    void shutdown()
    {
        boost::unique_lock<boost::shared_mutex> guard(mutex);
        // If a map is loaded into memory, de-allocate it.
        if (current_map != NULL) {
            delete current_map;
            // Point current_map to NULL to satisfy any further checks for current_map != NULL.
            current_map = NULL;
        }
    }
};
