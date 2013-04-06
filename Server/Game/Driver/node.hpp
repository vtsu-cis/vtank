/*!
    \file   node.hpp
    \brief  Blueprint for a node used in NodeManagers.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef NODE_HPP
#define NODE_HPP

#include <tank.hpp>

/*!
    The Node class is essentially a utility class for dealing with players
    in the node manager. It stores an ID, which is it's equivalent position in
    the node manager's array of nodes, and a list of players. It's up to outside
    classes to register players to this node for tracking purposes.
*/
class Node
{
private:
    int id;
    std::map<int, tank_ptr> players;

public:
    /*!
        Construct a node: default constructor.
    */
    Node();

    /*!
        Construct a node.
        \param node_id ID of the node, which is it's position in the array.
    */
    Node(const int &);

    /*!
        Simple destructor that does nothing significant.
    */
    ~Node();

    /*!
        Get the ID number of this node.
        \return ID number.
    */
    int get_id() const { return id; }

    /*!
        Set a new ID for this node.
        \param new_id New ID number to assign to this node.
    */
    void set_id(const int &new_id) { id = new_id; }

    /*!
        Register a player to this node.
        \param id ID of the player.
        \param player Player to register.
    */
    void register_player(const int &, const tank_ptr);

    /*!
        Unregister a player as identified by his ID number.
        \param ID of the player.
    */
    void unregister_player(const int &);

    /*!
        Get a list of every player registered to this node.
        \return Type definition of a boost::shared_array, which holds
        an array of player_ptr types.
    */
    tank_array get_players();

    /*!
        Clear the node of it's players.
    */
    void clear();
};

#endif
