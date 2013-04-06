/*!
    \file   node.cpp
    \brief  Implementation of Nodes used in NodeManagers.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#include <master.hpp>
#include <node.hpp>

Node::Node()
    : id(0)
{
    players = std::map<int, tank_ptr>();
    players.clear();
}

Node::Node(const int &node_id)
    : id(node_id)
{
    players = std::map<int, tank_ptr>();
    players.clear();
}

Node::~Node()
{
}

void Node::register_player(const int &id, const tank_ptr player)
{
    players[id] = player;
}

void Node::unregister_player(const int &id)
{
    const std::map<int, tank_ptr>::iterator it = players.find(id);
    if (it != players.end()) {
        players.erase(it);
    }
}

tank_array Node::get_players()
{
    tank_array temp;

    std::map<int, tank_ptr>::iterator it;
    for (it = players.begin(); it != players.end(); it++) {
        temp.push_back(it->second);
    }

    return temp;
}

void Node::clear() 
{
    players.clear();
}
