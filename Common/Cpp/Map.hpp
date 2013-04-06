/*!
    \file   Map.hpp
    \brief  Interface for map data loading and saving module.
    \author (C) Copyright 2009 by Vermont Technical College

*/

#ifndef MAP_HPP
#define MAP_HPP

#include <stdexcept>
#include <string>
#include <vector>
#include "vtassert.hpp"

//! The format version of the map. If the format changes, this number should increment.
const int FORMAT_VERSION = 1;

//! The external tile size must be 12 because of how it writes tiles to disc.
const int TILE_BYTE_SIZE = 12;

// The game modes
const int DEATH_MATCH      = 0;
const int TEAM_DEATH_MATCH = 1;
const int CAPTURE_THE_FLAG = 2;
const int CAPTURE_THE_BASE = 3;

//The event ID's
const int SPAWN_POINT     = 1;
const int RED_SPAWN_AREA  = 2;
const int BLUE_SPAWN_AREA = 3;
const int RED_FLAG        = 4;
const int BLUE_FLAG       = 5;
const int BASE_BLUE_1     = 8;
const int BASE_BLUE_2	  = 9;
const int BASE_BLUE_3	  = 10;
const int BASE_RED_1	  = 11;
const int BASE_RED_2	  = 12;
const int BASE_RED_3	  = 13;


//! Track texture and collision data.
/*!
 * The Tile struct will track which OpenGL image should be displayed when this Tile is
 * referenced. It will also track the collision layer of tiles, as well as the tile's internal
 * ID.
 *
 * \param tile_id Internal tile ID. This is automatically assigned by the Map class when it
 * loads tiles.
 * \param object_id Internal object ID. This is automatically assigned by the Map class when it loads tiles.
 * \param event_id Internal event ID. This is automatically assigned by the Map class when it loads tiles.
 * \param passable True if the Tile is able to be passed through, otherwise False.
 * \param height Height of the tile. Defaults to zero.
 * \param type Type of tile. Defaults to zero.
 * \param effect Effect that the tile applies. Defaults to zero.
 */
struct Tile
{
    int                 tile_id;
    int                 object_id;
    int                 event_id;
    int                 height;
    int                 type;
    int                 effect;
    bool                passable;

    Tile() : tile_id(0), object_id(0), event_id(0), 
        height(0), type(0), effect(0), passable(true) {}
};

/*!
 * Overload of the == operator to compare two tiles for equality
 */
inline bool operator==(const Tile &left, const Tile &right)
{
    return (left.tile_id == right.tile_id) && 
           (left.object_id == right.object_id) &&
           (left.event_id == right.event_id) &&
           (left.passable == right.passable) &&
           (left.height == right.height) &&
           (left.type == right.type) &&
           (left.effect == right.effect);
}

/*!
 * Overload of the != operator to compare two tiles for inequality
 */
inline bool operator!=(const Tile &left, const Tile &right)
{
    return !(left == right);
}
//! Convert an int to it's unsigned character equivalent.
/*!
    Convert an integer to it's byte-form equivalent. The returned vector will always consist of 4 bytes.
    \param n Number to convert.
    \return Vector of unsigned character values.
*/
static const std::vector<unsigned char> int_to_bytes(const int n)
{
    const unsigned int x = static_cast<unsigned>(n);
    std::vector<unsigned char> bytes(4);
    bytes[0] = x         & 0xFF;
    bytes[1] = (x >> 8 ) & 0xFF;
    bytes[2] = (x >> 16) & 0xFF;
    bytes[3] = (x >> 24) & 0xFF;
    
    return bytes;
}

//! Convert 4 bytes back into an integer.
/*!
    Convert 4 bytes back into it's integer form. Note that this method is not as safe as
    the overloaded one because this doesn't perform bounds checks.
    \param bytes Bytes to convert to an integer.
    \return Result of the conversion.
*/
static const int bytes_to_int(const unsigned char *const bytes, int how_many_bytes)
{ 
    std::vector<unsigned char> byte_vector(static_cast<unsigned>(how_many_bytes));
    for (unsigned int i = 0; i < static_cast<unsigned>(how_many_bytes); i++) {
        byte_vector[i] = bytes[i];
    }
    unsigned int value = -1;
    if(byte_vector.size() == 2)
        value = (byte_vector[0]) + ((byte_vector[1]) << 8);
    if(byte_vector.size() == 4)
        value = (byte_vector[0]) + ((byte_vector[1]) << 8) + 
        ((byte_vector[2]) << 16) + ((byte_vector[3]) << 24);
    return static_cast<int>(value);
}


static const Tile bytes_to_tile(const unsigned char *const bytes)
{
    Tile t;
    t.tile_id   = bytes_to_int(bytes, 4);
    t.object_id = bytes_to_int((bytes + 4), 2);
    t.event_id  = bytes_to_int((bytes + 6), 2);
    t.passable  = bytes[8] == 0 ? false : true;
    t.height    = bytes[9];
    t.type      = bytes[10];
    t.effect    = bytes[11];
    return t;
}

//! Convert a tile to it's equivalent byte form.
/*!
    Convert a tile from it's native struct to the equivalent byte form.
    \param tile Tile to convert.
    \return Converted vector containing the new bytes.
*/
static const std::vector<unsigned char> tile_to_bytes(const Tile &tile)
{
    std::vector<unsigned char> bytes(TILE_BYTE_SIZE);
    unsigned int j = 0;
    
    std::vector<unsigned char> temp_bytes = int_to_bytes(tile.tile_id);
    for (unsigned int i = 0; i < 4; i++, j++) {
        bytes[j] = temp_bytes[i];
    }
    
    temp_bytes = int_to_bytes(tile.object_id);
    for (unsigned int i = 0; i < 2; i++, j++) {
        bytes[j] = temp_bytes[i];
    }
    
    temp_bytes = int_to_bytes(tile.event_id);
    for (unsigned int i = 0; i < 2; i++, j++) {
        bytes[j] = temp_bytes[i];
    }
    
    bytes[j++] = static_cast<unsigned char>(tile.passable ? 0x01 : 0x00);
    bytes[j++] = static_cast<unsigned char>(tile.height);
    bytes[j++] = static_cast<unsigned char>(tile.type);
    bytes[j++] = static_cast<unsigned char>(tile.effect);
    
    return bytes;
}



//! A special exception for trying to access an out-of-bounds tile.
/*!
 * The Out Of Bounds Exception is thrown from functions called where it cannot necessarily
 * return an error code that the user can easily check. The alternative is to return NULL and
 * risk a segfault, or do no error checking.
 */
class OutOfBoundsException : public std::runtime_error {
private:
    int attempted_x;
    int attempted_y;
public:
    //! The default constructor that details the error.
    /*!
     * \param message User friendly message explaining the error.
     * \param x The X tile requested.
     * \param y The Y tile requested.
     */
    OutOfBoundsException(const std::string &message, const int x, const int y)
        : std::runtime_error(message), attempted_x(x), attempted_y(y) { }

    //! Empty destructor.
   ~OutOfBoundsException() throw() {}

    //! Gets the (x, y) coordinate of the invalid access attempt.
    /*!
     * \param x Pass a pointer to an integer to retrieve the X access attempt.
     * \param y Pass a pointer to an integer to retrieve the Y access attempt.
     */
    void where(int *x, int *y) const
    {
        *x = attempted_x;
        *y = attempted_y;
    }
};


//! Representation of a map.
/**
 * The Map class tracks tile data on an (x, y) basis. When the map is loaded, tiles are brought
 * into a 2D array.
 *
 * In general if a method of this class encounters an error condition, it returns an appropriate
 * error code (often 'false') and records a user friendly error message that can be retrieved
 * using the get_last_error() method. The methods of this class do not throw exceptions except
 * for get_tile() (which throws an OutOfBoundsException if requested to provide a tile that does
 * not exist in the map).
 */
class Map {
private:
    int              map_width;
    int              map_height;
    std::string      map_title;
    std::string      last_error;
    int              default_tile;
    int              version;
    std::vector<int> supported_game_modes;
    Tile             *tile_data;

public:
    Map();
    Map(const Map& obj);
   ~Map();

    std::string get_last_error();

    bool create(int width, int height, const std::string &title);
    bool load  (const std::string &file_name);
    bool save  (const std::string &file_name);
    bool resize(int width, int height);

    void        set_title(const std::string &title);
    std::string get_title() const;

    int  get_width () const;
    int  get_height() const;

    Tile get_tile(int x, int y) const;
    bool set_tile(int x, int y, int ter_id, bool collision, short obj_id, short evt_id, int height, int type, int effect);

    bool set_tile_id       (const int x, const int y, const int id);
    bool set_tile_collision(const int x, const int y, const bool collision);
    bool set_tile_object   (const int x, const int y, const short id);
    bool set_tile_event    (const int x, const int y, const short id);
    bool set_tile_height   (const int x, const int y, const int height);
    bool set_tile_type     (const int x, const int y, const int type);
    bool set_tile_effect   (const int x, const int y, const int effect);
    void set_default_tile  (const int new_tile) { default_tile = new_tile; }
    int get_default_tile() const { return default_tile; }
    void add_supported_game_mode(const int);
    void remove_supported_game_mode(const int);

    bool get_tile_collision (int, int);
    int  get_tile_object    (int, int) const;
    int  get_tile_event     (int, int) const;
    int  get_tile_height    (int, int) const;
    int  get_tile_type      (int, int) const;
    int  get_tile_effect    (int, int) const;
    const std::vector<int> get_supported_game_modes() const { return supported_game_modes; }

    bool validate_death_match()      const;
    bool validate_team_death_match() const;
    bool validate_capture_the_flag() const;
    bool validate_capture_the_base() const;
    void validate_supported_game_modes();
    
    const int  get_version() const { return static_cast<int>(version); }
};

#endif
