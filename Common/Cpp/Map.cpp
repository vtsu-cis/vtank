/*!
    \file   Map.cpp
    \brief  Implementation of map loading and saving module.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#include <fstream>
#include <stdexcept>
#include <string>

#include "Map.hpp"
#include "vtassert.hpp"

using namespace std;

//! Default constructor.
Map::Map()
    : map_width   (0),
      map_height  (0),
      map_title   ("Untitled"),
      last_error  ("None"),
      default_tile(0),
      version     (0),
      supported_game_modes(),
      tile_data   (NULL)
      
{
    // empty...
}
Map::Map(const Map& obj)
    : map_width           (obj.map_width),
      map_height          (obj.map_height),
      map_title           (obj.map_title),
      last_error          (obj.last_error),
      default_tile        (obj.default_tile),
      version             (obj.version),
      supported_game_modes(obj.supported_game_modes),
      tile_data           (new Tile[static_cast<unsigned>(map_width * map_height)])
{
    const int map_size = map_width * map_height;
    for (int i = 0; i < map_size; i++) {
        tile_data[i].tile_id    = obj.tile_data[i].tile_id;
        tile_data[i].object_id  = obj.tile_data[i].object_id;
        tile_data[i].event_id   = obj.tile_data[i].event_id;
        tile_data[i].height     = obj.tile_data[i].height;
        tile_data[i].type       = obj.tile_data[i].type;
        tile_data[i].effect     = obj.tile_data[i].effect;
        tile_data[i].passable   = obj.tile_data[i].passable;
    }
}
//! Destructor.
Map::~Map()
{
    delete [] tile_data;
}


//! Return last error
/*!
 * Get a user friendly error message pertaining to the last error that has occured. If there is
 * no active error the string 'None' is returned. After calling this method, the last error
 * message is reset to 'None'. Note that error messages returned by this method are not
 * terminated with any punctuation.
 *
 * \return A string containing a message about the last error.
 */
string Map::get_last_error()
{
    string temp(last_error);
    last_error = "None";
    return temp;
}


//! Create a new map with the given details.
/*!
 * \param width Width of the map in tiles.
 * \param height Height of the map in tiles.
 * \param title Title of the map.
 * \return true if the creation was a success, and otherwise false. If creation fails, the
 * get_last_error() method will return a user friendly string describing the problem.
 * \throws bad_alloc throw if memory exhausted during the create operation.
 */
bool Map::create(const int width, const int height, const string &title)
{
    bool was_successfully_created = true;

    if (width <= 0 || height <= 0) {
        last_error = "Map size cannot have a width or height less than or equal to 0";
        was_successfully_created = false;
    }
    else {
        //lint -save -e737
        // The values width and height must be positive here. Allocation request safe.

        Tile *const temp_data = new Tile[width * height];
        delete [] tile_data;

        // Allocation of space successful. Commit new information.
        tile_data  = temp_data;
        map_width  = width;
        map_height = height;
        map_title  = title;
        version    = FORMAT_VERSION;

        // Initialize every new tile in the new map.
        const int map_size = map_width * map_height;
        for (int i = 0; i < map_size; i++) {
            tile_data[i].tile_id    = default_tile;
            tile_data[i].object_id  = 0;
            tile_data[i].event_id   = 0;
            tile_data[i].passable   = true;
            tile_data[i].height     = 0;
            tile_data[i].type       = 0;
            tile_data[i].effect     = 0;
        }
        //lint -restore
    }
    return was_successfully_created;
}


//! Load a map into memory.
/*!
 * \param file_name Name of the file containing the map to load.
 * \return true on a successful load, false otherwise (in that case an appropriate message is
 * returned by the get_last_error() method).
 * \throws bad_alloc thrown if memory exhausted during the load operation.
 */
bool Map::load(const string &input_file_name)
{
    string file_input_name = input_file_name;
    bool was_successfully_loaded = true;
    if (file_input_name.substr((file_input_name.size() - 6), 6) != ".vtmap") {
        file_input_name.append(".vtmap");
    }
    fstream file(file_input_name.c_str(), ios::binary | ios::in);
    if (!file) {
        last_error = "File " + file_input_name + " failed to open.";
        return false;
    }
    // Read the map into memory.
    char   version_byte[1];
    string title;
    int    width;
    int    height;
    string supported_game_bytes;
    unsigned char width_bytes[4];
    unsigned char height_bytes[4];

    (void)file.read(version_byte, 1);
    if (version_byte[0] != FORMAT_VERSION) {
        // Version mismatch.
        last_error = "Map " + file_input_name + " is the wrong version.";
        return false;
    }

    (void)getline(file, title);

    (void)file.read(reinterpret_cast<char*>(width_bytes), 4);
    width = bytes_to_int(width_bytes, 4);
    (void)file.read(reinterpret_cast<char*>(height_bytes), 4);
    height = bytes_to_int(height_bytes, 4);
    (void)getline(file, supported_game_bytes); 
    if (width <= 0 || height <= 0) {
        last_error = "Map size cannot have a width or height less than or equal to 0";
        was_successfully_loaded = false;
    }
    else {
        //lint -save -e737
        // The values width and height must be positive here.
        Tile *const temp_data = new Tile[width * height];
        delete [] tile_data;
        // Allocation of space successful. Commit new information.
        tile_data  = temp_data;
        map_width  = width;
        map_height = height;
        map_title  = title;
        version    = version_byte[0];
        supported_game_modes.clear();
        for (string::size_type i = 0; i < supported_game_bytes.size(); i++) {
            supported_game_modes.push_back(static_cast<int>(supported_game_bytes[i]));
        }
        // Read in every tile and save it locally.
        const int map_size = width * height;
        for (int i = 0; i < map_size; i++) {
            unsigned char tile_buffer[TILE_BYTE_SIZE];
            (void)file.read(reinterpret_cast<char*>(tile_buffer), TILE_BYTE_SIZE);
            tile_data[i] = bytes_to_tile(tile_buffer);
        }
        //lint -restore
    }
    return was_successfully_loaded;
}


//! Save a map to the hard drive.
/*!
 * Saves the map to disk.
 *
 * \param file_name The name of the file into which the map will be saved.
 * \return true on successful save; false otherwise (in that case an appropriate message is
 * returned by the get_last_error() method).
 */
bool Map::save(const string &output_file_name)
{
    string file_output_name = output_file_name;
    VTANK_ASSERT(tile_data != NULL);
    if(file_output_name.substr((file_output_name.size()-6), 6) != ".vtmap") {
        file_output_name.append(".vtmap");
    }
    // Open the file for output, for binary writing, and replace old data with new data.
    fstream file(file_output_name.c_str(), ios::binary | ios::out);
    if (!file) {
        last_error = "The file " + file_output_name + " failed to open";
        return false;
    }
    VTANK_ASSERT(map_width  > 0);
    VTANK_ASSERT(map_height > 0);
    file << static_cast<char>(version); 
    file << map_title << '\n';
    // Convert the map width and map height to it's byte form and write it to the file.
    std::vector<unsigned char> bytes = int_to_bytes(map_width);
    file << bytes[0];
    file << bytes[1];
    file << bytes[2];
    file << bytes[3];
    bytes = int_to_bytes(map_height);
    file << bytes[0];
    file << bytes[1];
    file << bytes[2];
    file << bytes[3];
    for (std::vector<int>::size_type i = 0; i < supported_game_modes.size(); i++) {
        file << static_cast<char>(supported_game_modes[i]);
    }
    file << '\n';
    // Now write out the tile data.  
    // Loop through each tile, convert the tile to it's byte form and write it out.
    const int map_size = map_width * map_height;
    for (int i = 0; i < map_size; i++) {
        bytes = tile_to_bytes(tile_data[i]);
        for (std::vector<unsigned char>::size_type j = 0; j < bytes.size(); j++) {
            file << bytes[j];
        }
    }
    return true;
}


//! Resize a map.
/*!
 * This method changes the width and height of a map to the give values. Existing tiles that no
 * longer fit on the map are removed. New tiles that don't correspond to any previously existing
 * tile are given default values.
 *
 * \param width The new width of the map (in tiles).
 * \param height The new height of the map (in tiles).
 * \throws bad_alloc thrown if memory exhausted during the resize operation.
 */
bool Map::resize(const int width, const int height)
{
    VTANK_ASSERT(tile_data != NULL);
    // Do nothing if the size is unchanged
    if (width == map_width && height == map_height) {
        return true;
    }
    bool was_successfully_resized = true;
    if (width <= 0 || height <= 0) {
        last_error = "Map size cannot have a width or height less than or equal to 0";
        was_successfully_resized = false;
    }
    else {
        //lint -save -e737
        // The values width and height must be positive here. Allocation request safe.
        Tile *const temp = new Tile[width * height];
        // Prepare new map. Copy parts of old map as appropriate.
        int i = 0;
        for (int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x) {
                if (x < map_width && y < map_height) {
                    temp[i] = tile_data[y * map_width + x];
                }
                else {
                    temp[i].tile_id   = default_tile;
                    temp[i].object_id = 0;
                    temp[i].event_id  = 0;
                    temp[i].passable  = true;
                    temp[i].height    = 0;
                    temp[i].type      = 0;
                    temp[i].effect    = 0;
                }
                i++;
            }
        }
        delete [] tile_data;
        tile_data  = temp;
        map_width  = width;
        map_height = height;
        //lint -restore
    }
    return was_successfully_resized;
}


//! Set the title of the map.
/*!
 * \param title New map title.
 */
void Map::set_title(const string &title)
{
    map_title = title;
}


//! Get the title of the map.
/*!
 * \return string containing the map's title.
 */
string Map::get_title() const
{
    return map_title;
}


//!Get the width of the map.
/*!
 * \return Width of the map in tiles.
 */
int Map::get_width() const
{
    return map_width;
}


//! Get the height of the map.
/*!
 * \return Height of the map in tiles.
 */
int Map::get_height() const
{
    return map_height;
}


//!Return a tile at a specific location.
/*!
 * \param x The x position (column) of the tile.
 * \param y The y position (row) of the tile.
 * \return The specified tile.
 * \throws OutOfBoundsException thrown if the given tile coordinates are not on the map.
 */
Tile Map::get_tile(int x, int y) const
{
    VTANK_ASSERT(tile_data != NULL);
    if (x >= map_width || y >= map_height || x < 0 || y < 0) {
        throw OutOfBoundsException("Attempting to access a tile out of bounds", x, y);
    }
    return tile_data[y * map_width + x];
}

bool Map::set_tile(int x, int y, int ter_id, bool collision, short obj_id, short evt_id, int height, int type, int effect)
{
    return set_tile_id(x, y, ter_id) &&
           set_tile_object(x, y, obj_id) &&
           set_tile_event(x, y, evt_id) &&
           set_tile_collision(x, y, collision) &&
           set_tile_height(x, y, height) &&
           set_tile_type(x, y, type) &&
           set_tile_effect(x, y, effect);
}

//! Set the ID number of a tile.
/*!
 * This is the ID that referes to the image that should be drawn.
 *
 * \param x The x position (column) of the tile.
 * \param y The y position (row) of the tile.
 * \param id The tile ID value to assign to the specified position.
 * \return true if the specified position is on the map; false otherwise (in that case an
 * appropriate message is returned by the get_last_error() method).
 */
bool Map::set_tile_id(const int x, const int y, const int id)
{
    VTANK_ASSERT(tile_data != NULL);
    if (x >= map_width || y >= map_height || x < 0 || y < 0) {
        last_error = "Attempting to access a tile out of bounds";
        return false;
    }
    if (id < 0 ) {
        last_error = "Invalid tile id";
        return false;
    }
    tile_data[y * map_width + x].tile_id = id;
    return true;
}


//! Set the tile collision status.
/*!
 * When a tile's collision is non-zero, anything can pass through it. When it's zero, the tile
 * is solid and non-passable.
 *
 * \param x The x position (column) of the tile.
 * \param y The y position (row) of the tile.
 * \param is_passable True if the tile is passable by tanks, otherwise false.
 * \return true if the specified position is on the map; false otherwise (in that case an
 * appropriate message is returned by the get_last_error() method).
 */
bool Map::set_tile_collision(const int x, const int y, const bool is_passable)
{
    VTANK_ASSERT(tile_data != NULL);
    if (x >= map_width || y >= map_height || x < 0 || y < 0) {
        last_error = "Attempting to access a tile out of bounds";
        return false;
    }
    tile_data[y * map_width + x].passable = is_passable;
    return true;
}


//!
/*!
    
*/
bool Map::set_tile_object(const int x, const int y, const short id)
{
    VTANK_ASSERT(tile_data != NULL);
    if (x >= map_width || y >= map_height || x < 0 || y < 0) {
        last_error = "Attempting to access a tile out of bounds";
        return false;
    }
    if (id < 0 ) {
        last_error = "Invalid object id";
        return false;
    }
    tile_data[y * map_width + x].object_id = id;
    return true;
}


//!
/*!
    
*/
bool Map::set_tile_event(const int x, const int y, const short id)
{
    VTANK_ASSERT(tile_data != NULL);
    if (x >= map_width || y >= map_height || x < 0 || y < 0) {
        last_error = "Attempting to access a tile out of bounds";
        return false;
    }
    if (id < 0 ) {
        last_error = "Invalid event id";
        return false;
    }
    tile_data[y * map_width + x].event_id = id;
    return true;
}


//!
/*!
    
*/
bool Map::set_tile_height(const int x, const int y, const int height)
{
    VTANK_ASSERT(tile_data != NULL);
    if (x >= map_width || y >= map_height || x < 0 || y < 0) {
        last_error = "Attempting to access a tile out of bounds";
        return false;
    }
    tile_data[y * map_width + x].height = height;
    return true;
}


//!
/*!
    
*/
bool Map::set_tile_type(const int x, const int y, const int type)
{
    VTANK_ASSERT(tile_data != NULL);
    if (x >= map_width || y >= map_height || x < 0 || y < 0) {
        last_error = "Attempting to access a tile out of bounds";
        return false;
    }
    if (type < 0 ) {
        last_error = "Invalid tile type";
        return false;
    }
    tile_data[y * map_width + x].type = type;
    return true;
}


//!
/*!
    
*/
bool Map::set_tile_effect(const int x, const int y, const int effect)
{
    VTANK_ASSERT(tile_data != NULL);
    if (x >= map_width || y >= map_height || x < 0 || y < 0) {
        last_error = "Attempting to access a tile out of bounds";
        return false;
    }
    if (effect < 0 ) {
        last_error = "Invalid tile effect";
        return false;
    }
    tile_data[y * map_width + x].effect = effect;
    return true;
}


//! Get the tile collision status.
/*!
 * When a tile's collision is non-zero, anything can pass through it. When it's zero, the tile
 * is solid and non-passable.
 *
 * \param x The x position (column) of the tile.
 * \param y The y position (row) of the tile.
 * \return false if the tile is not passable; true otherwise. If the specified position is not on the
 * map, a value of false (non-passable) is returned.
 */
bool Map::get_tile_collision(const int x, const int y)
{
    VTANK_ASSERT(tile_data != NULL);
    if (x >= map_width || y >= map_height || x < 0 || y < 0) {
        last_error = "Attempting to access a tile out of bounds";
        return false;
    }
    return tile_data[y * map_width + x].passable;
}

//! Get the tile object id.
/*!
 * \param x The x position (column) of the tile.
 * \param y The y position (row) of the tile.
*/
int Map::get_tile_object(const int x, const int y) const
{
    VTANK_ASSERT(tile_data != NULL);
    if (x >= map_width || y >= map_height || x < 0 || y < 0) {
        throw OutOfBoundsException("Attempting to access a tile out of bounds", x, y);
    }
    return tile_data[y * map_width + x].object_id;
}

//! Get the tile event id.
/*!
 * \param x The x position (column) of the tile.
 * \param y The y position (row) of the tile.
*/
int Map::get_tile_event(const int x, const int y) const
{
    VTANK_ASSERT(tile_data != NULL);
    if (x >= map_width || y >= map_height || x < 0 || y < 0) {
        throw OutOfBoundsException("Attempting to access a tile out of bounds", x, y);
    }
    return tile_data[y * map_width + x].event_id;
}

//! Get the tile's height.
/*!
 * \param x The x position (column) of the tile.
 * \param y The y position (row) of the tile.
*/
int Map::get_tile_height(const int x, const int y) const
{
    VTANK_ASSERT(tile_data != NULL);
    if (x >= map_width || y >= map_height || x < 0 || y < 0) {
        throw OutOfBoundsException("Attempting to access a tile out of bounds", x, y);
    }
    return tile_data[y * map_width + x].height;
}

//! Get the tile's type.
/*!
 * \param x The x position (column) of the tile.
 * \param y The y position (row) of the tile.
*/
int Map::get_tile_type(const int x, const int y) const
{
    VTANK_ASSERT(tile_data != NULL);
    if (x >= map_width || y >= map_height || x < 0 || y < 0) {
        throw OutOfBoundsException("Attempting to access a tile out of bounds", x, y);
    }
    return tile_data[y * map_width + x].type;
}

//! Get the tile's effect.
/*!
 * \param x The x position (column) of the tile.
 * \param y The y position (row) of the tile.
*/
int Map::get_tile_effect(const int x, const int y) const
{
    VTANK_ASSERT(tile_data != NULL);
    if (x >= map_width || y >= map_height || x < 0 || y < 0) {
        throw OutOfBoundsException("Attempting to access a tile out of bounds", x, y);
    }
    return tile_data[y * map_width + x].effect;
}

//! Add a supported game mode to this map.
/*!
    
    \param mode 
*/
void Map::add_supported_game_mode(const int mode)
{
    VTANK_ASSERT(mode != 0x0A);
    if(mode < 0)
    {
        return;
    }
    // Check if the mode already exists.
    for (std::vector<int>::size_type i = 0; i < supported_game_modes.size(); i++) {
        if (mode == supported_game_modes[i]) {
            // No need to add the supported mode: it is already on the stack.
            return;
        }
    }

    supported_game_modes.push_back(mode);
}

//! Remove a supported game mode from this map.
/*!
    
    \param mode 
*/
void Map::remove_supported_game_mode(const int mode)
{
    std::vector<int>::iterator i;
    for (i = supported_game_modes.begin(); i != supported_game_modes.end(); i++) {
        if (*i == mode) {
            // Found the mode: remove it.
            (void)supported_game_modes.erase(i);

            break;
        }
    }
}

bool Map::validate_death_match() const
{
    for(int y = 0; y < get_height(); y++) {
        for(int x = 0; x < get_width();x++) {
            const Tile test = get_tile(x,y); 
            if(test.event_id == SPAWN_POINT) {
                return true;
            }
        }
    }
    return false;
}
bool Map::validate_team_death_match() const
{
    bool red = false;
    bool blue = false;
    for(int y = 0; y < get_height(); y++) {
        for(int x = 0; x < get_width();x++) {
            const Tile test = get_tile(x,y); 
            if(test.event_id == RED_SPAWN_AREA) {
                red = true;
            }
            else if(test.event_id == BLUE_SPAWN_AREA) {
                blue = true;
            }
            if(red && blue) {
                return true;
            }
        }
    }
    return false;
}
bool Map::validate_capture_the_flag() const
{
    bool red_flag = false;
    bool blue_flag = false;
    bool red_area = false;
    bool blue_area = false;
    for(int y = 0; y < get_height(); y++) {
        for(int x = 0; x < get_width();x++) {
            const Tile test = get_tile(x,y); 
            if(test.event_id == RED_SPAWN_AREA) {
                red_area = true;
            }
            else if(test.event_id == BLUE_SPAWN_AREA) {
                blue_area = true;
            }
            else if(test.event_id == RED_FLAG) {
                red_flag = true;
            }
            else if(test.event_id == BLUE_FLAG) {
                blue_flag = true;
            }
            if(red_area && blue_area && red_flag && blue_flag) {
                return true;
            }
        }
    }
    return false;
}
bool Map::validate_capture_the_base() const
{
    bool blue_base_1 = false;
	bool blue_base_2 = false;
	bool blue_base_3 = false;
	bool red_base_1  = false;
	bool red_base_2  = false;
	bool red_base_3  = false;
    for(int y = 0; y < get_height(); y++) {
        for(int x = 0; x < get_width();x++) {
            const Tile tile = get_tile(x,y); 
			if (tile.event_id == BASE_BLUE_1) {
				if (blue_base_1) return false;
				blue_base_1 = true;
			}
			else if (tile.event_id == BASE_BLUE_2) {
				if (blue_base_2) return false;
				blue_base_2 = true;
			}
			else if (tile.event_id == BASE_BLUE_3) {
				if (blue_base_3) return false;
				blue_base_3 = true;
			}
			else if (tile.event_id == BASE_RED_1) {
				if (red_base_1) return false;
				red_base_1 = true;
			}
			else if (tile.event_id == BASE_RED_2) {
				if (red_base_2) return false;
				red_base_2 = true;
			}
			else if (tile.event_id == BASE_RED_3) {
				if (red_base_3) return false;
				red_base_3 = true;
			}
        }
    }
	
	if (blue_base_1 && blue_base_2 && blue_base_3 &&
			red_base_1 && red_base_2 && red_base_3) {
		return true;
	}

    return false;
}

void Map::validate_supported_game_modes()
{
    std::vector<int> modes = get_supported_game_modes();
    for (std::vector<int>::iterator i = modes.begin(); i != modes.end(); i++) {
        if(*i == DEATH_MATCH) {
            if(!validate_death_match()) {
                remove_supported_game_mode(DEATH_MATCH);
            }
        }
        else if(*i == TEAM_DEATH_MATCH) {
            if(!validate_team_death_match()) {
                remove_supported_game_mode(TEAM_DEATH_MATCH);
            }
        }
        else if(*i == CAPTURE_THE_FLAG) {
            if(!validate_capture_the_flag()) {
                remove_supported_game_mode(CAPTURE_THE_FLAG);
            }
        }
        else if(*i == CAPTURE_THE_BASE) {
           if(!validate_capture_the_base()) {
                remove_supported_game_mode(CAPTURE_THE_BASE);
           }
        }
    }
}
