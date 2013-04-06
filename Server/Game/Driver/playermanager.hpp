/*!
    \file playermanager.hpp
    \brief Declares objects inside of the Players namespace.
    \author (C) Copyright 2009 by Vermont Technical College
*/

#ifndef PLAYERMANAGER_HPP
#define PLAYERMANAGER_HPP

#include <player.hpp>
#include <tankmanager.hpp>

//! How many players to handle by default.
#define DEFAULT_PLAYER_LIMIT 64

//! How many milliseconds does it take to time out a client?
#define TIMEOUT 13000

/*!
    The Players namespace is responsible for managing players who are in-game.
    The point of the namespace is to be globally accessible and to be thread-safe.
*/
namespace Players
{
    extern boost::recursive_mutex mutex;
    extern Ice::Int player_limit;
    extern double timeout;
    extern boost::threadpool::pool task_pool;
    extern TankManager tanks;

    //! Collects the player's tank, and how long the player has been pending.
    struct PendingTank
    {   
        GameSession::Tank tank;
        IceUtil::Int64 waiting_time;

        /*!
            Create a pending tank.
            \param player_tank Tank to store.
            \param start_stamp When the tank became "pending".
        */
        PendingTank(const GameSession::Tank &player_tank, const IceUtil::Int64& start_stamp)
            : tank(player_tank), waiting_time(start_stamp)
        {
        }
    };
    typedef boost::shared_ptr<PendingTank> pending_ptr;

    bool manage_players();

    /*!
        Generate a unique temporary ID. This is used for player tracking within
        the game server.
        \return New ID.
    */
    int generate_unique_temp_id();

    /*!
        Add a player to the list of pending players. This is a tank added
        by the main server as a 'heads up' that a player will attempt to 
        join using a string key.
        \param key Key to identify the player.
        \param tank Tank to store in memory while waiting for the player.
    */
    void add_pending(const std::string&, const GameSession::Tank);

    /*!
        Get a tank who is pending as identified by his session key.
        \param key The ID of the pending tank.
        \return Pointer to the tank object, or NULL if the tank object doesn't exist.
    */
    pending_ptr get_pending(const std::string&);

    /*!
        Remove a player identifed by his "key".
        This function is thread-safe within the Players namespace.
        \param key Identifying key string that belongs to the pending player.
        \return True if the tank was found and removed. False if the tank did not exist.
    */
    bool remove_pending(const std::string&);

    /*!
        Add a player to the list of managed players. 
        This function is thread-safe within the Players namespace.
        \param player Player object to add.
    */
    void add_player(const tank_ptr);
    
    /*!
        Shortcut method for grabbing a tank from the tank list.
        \param id ID to identify the player.
        \return Copy of a Tank object.
    */
    const tank_ptr get_player(const int&);

    /*!
        Remove a player identifed by his ID number.
        This function is thread-safe within the Players namespace.
        \param id ID to identify the player.
        \return True if the player existed and was removed. False if the player did not exist.
    */
    bool remove_player(const int&);

    /*!
        In some cases a function or class may not have access to the player IDs, but they would
        have access to their user names. This function allows the caller to search for an ID
        given a player name.
        \param username Name to look for.
        \return ID of the player, or -1 if the player doesn't exist.
    */
    int get_player_id_by_name(const std::string&);

    /*!
        Get a list of players currently logged into the game server.
        \return GameSession::PlayerList object containing the name of each player.
    */
    GameSession::PlayerList get_player_list();
}

#endif
