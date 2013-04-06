/*!
    \file mapmanager.hpp
    \brief Definition of the namespace which manages the game's map.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef MAPMANAGER_HPP
#define MAPMANAGER_HPP

#include <Map.hpp>
#include <tank.hpp>

/*!
    The map manager tracks the map being played on and is accessible anytime
    in a thread-safe manner. The map manager is responsible for holding the
    map object being worked with, as well as selecting a new map based on
    factors such as the number of players in a game and what the previous 
    map was playing. In other words, map selection is not random.
*/
namespace MapManager
{
    enum SelectionMode 
    {
        SELECT_RANDOM = 0,
        SELECT_ROUND_ROBIN
    };

    //! Thread safety is done via a shared mutex, since the map barely ever changes.
    extern boost::shared_mutex mutex;

    //! The current map object being worked with.
    extern Map * current_map;

    //! The current map file name.
    extern std::string current_map_filename;

    //! List of maps by filename.
    extern Ice::StringSeq map_list;

    /*!
        Ask if this class is in the middle of a rotation.
        \return True if the map manager is rotating.
    */
    bool is_rotating();
    
    /*!
        Set whether or not a rotation has begun.
        \param rotating_value True if a rotation has begun; false otherwise.
    */
    void set_rotating(bool);

    /*!
        Set how the map manager selects new maps.
    */
    void set_selection_technique(const SelectionMode);

    /*!
        Initializes the map manager. It will download a list of maps from the server
        during initialization.
    */
    void start();

    /*!
        Rotate the map. The map manager will select a new map, download it (if
        required), and replace the old map object.
    */
    void rotate();

    /*!
        Get the current map. The map is const, so it cannot be modified.
        \return Constant pointer to a Map object.
    */
    const Map * const get_current_map();

    /*!
        Get the current map filename, which is useful for clients.
        \return String containing the filename (not including paths).
    */
    const std::string get_current_map_filename();

    /*!
        Get the current map mode.
        \return VTankObject::GameMode enumeration.
    */
    const VTankObject::GameMode get_current_mode();

    /*!
        Generate starting positions initially by first gathering a list of all potential
        spawn points and then organizing them into a randomized list.
    */
    void generate_positions();

    //! Helper method for generating spawn positions in team deathmatch.
    void set_spawn_position_team_deathmatch(tank_ptr);

    /*!
        Generate a spawn point based on the current game mode.
    */
    void generate_spawn_position(tank_ptr);

    /*!
        De-initializes the map manager. Pretty much just deletes the current_map object
        if it is not null.
    */
    void shutdown();
}

#endif
