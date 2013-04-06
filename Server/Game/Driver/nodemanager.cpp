/*!
    \file   nodemanager.cpp
    \brief  Implementation of the NodeManager class.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#include <master.hpp>
#include <nodemanager.hpp>
#include <vtassert.hpp>

NodeManager::NodeManager()
    : nodes(NULL), size(0)
{
}

NodeManager::~NodeManager()
{
    deallocate_nodes();
}

void NodeManager::allocate_nodes(const int &node_size)
{
    VTANK_ASSERT(node_size > 0);

    nodes = new Node[node_size];
    for (int i = 0; i < node_size; i++) {
        nodes[i].set_id(i);
    }

    size = node_size;
}

void NodeManager::deallocate_nodes()
{
    if (nodes != NULL) {
        delete [] nodes;

        nodes = NULL;
    }
}

void NodeManager::set_map(const Map *const map)
{
    boost::lock_guard<boost::mutex> guard(mutex);
    
    deallocate_nodes();

    const double map_width = map->get_width() * TILE_SIZE;
    const double map_height = map->get_height() * TILE_SIZE;
    width  = static_cast<int>(ceil(map_width / NODE_WIDTH));
    height = static_cast<int>(ceil(map_height / NODE_HEIGHT));

    allocate_nodes(width * height);
}

int NodeManager::get_node_at(const VTankObject::Point &position)
{
	boost::lock_guard<boost::mutex> guard(mutex);
	const int x = static_cast<int>(position.x / NODE_WIDTH);
    const int y = static_cast<int>(-position.y / NODE_HEIGHT);
	const int node_position = y * width + x;
	if (node_position < 0 || node_position >= size) {
		return -1;
	}

	return node_position;
}

bool NodeManager::is_near(int node1, int node2) const
{
	bool result = false;

	if (node1 == node2) {
		// Check equal.
		result = true;
	}
	else if (node1 - width == node2 || node1 + width == node2) {
		// Check top/bottom.
		result = true;
	}
	else if (node1 + 1 == node2 || node1 - 1 == node2) {
		// Check left/right.
		result = true;
	}
	else if (node1 - width - 1 == node2 || node1 + width - 1 == node2 ||
			node1 - width + 1 == node2 || node1 + width + 1 == node2) {
		// Check all corners.
		result = true;
	}
	
	return result;
}

void NodeManager::process_position(tank_ptr player)
{
    boost::lock_guard<boost::mutex> guard(mutex);

    // Calculate the player's node position.
    // (y / NODE_HEIGHT) * width + (x / NODE_WIDTH)
    const int x = static_cast<int>(player->get_position().x / NODE_WIDTH);
    const int y = static_cast<int>(-player->get_position().y / NODE_HEIGHT);
    const int node_position = y * width + x;
	const int current_node = player->get_node_id();
    if (node_position < 0 || node_position >= size) {
        // Nothing to do: Can't process a tank not on the map.
		if (current_node >= 0) {
			// Unregister from existing nodes.
			nodes[current_node].unregister_player(player->get_id());
		}

        player->set_node_id(-1);

        return;
    }
	
    if (current_node != node_position) {
        // Change nodes.
        player->set_node_id(node_position);

        if (current_node >= 0 && current_node < size)
            nodes[current_node].unregister_player(player->get_id());
        nodes[node_position].register_player(player->get_id(), player);
    }
}

void NodeManager::process_projectile(projectile_ptr projectile)
{
    boost::lock_guard<boost::mutex> guard(mutex);

    // Calculate the projectile's node position.
    // (y / NODE_HEIGHT) * width + (x / NODE_WIDTH)
    const int x = static_cast<int>(projectile->position.x / NODE_WIDTH);
    const int y = static_cast<int>(-projectile->position.y / NODE_HEIGHT);
    const int node_position = y * width + x;
    if (node_position < 0 || node_position > size) {
        // Nothing to do: Can't process a tank not on the map.
        projectile->node_id = -1;

        return;
    }
    
    const int current_node = projectile->node_id;
    if (current_node != node_position) {
        // Change nodes.
        projectile->node_id = node_position;
    }
}

void NodeManager::unregister_player(const int &id, const int &node_id)
{
    boost::lock_guard<boost::mutex> guard(mutex);
    if (node_id < 0) {
        // Player is not registered.
        return;
    }

    VTANK_ASSERT(node_id >= 0 && node_id < size);

    nodes[node_id].unregister_player(id);
}

tank_array NodeManager::get_relevant_players(const int &node_id)
{
    boost::lock_guard<boost::mutex> guard(mutex);
    if (node_id < 0) {
        return tank_array();
    }

    // Some pre-calculations.
    const int top_row = node_id - width;
    const int bottom_row = node_id + width;

	// Find which nodes we are looking at.
    const int relevant_nodes[9] = {
                        // Moving clock-wise...
        top_row - 1,    // Northwest
        top_row,        // North
        top_row + 1,    // Northeast
        node_id + 1,    // East
        bottom_row + 1, // Southeast
        bottom_row,     // South
        bottom_row - 1, // Southwest
        node_id - 1,    // West
        node_id         // Middle
    };

    tank_array players;
    for (int i = 0; i < 9; i++) {
        const int node_id = relevant_nodes[i];
        if (node_id < 0 || node_id >= size) {
            // Node is out of range.
            continue;
        }

        const tank_array temp_array = nodes[node_id].get_players();
        // TODO: "Back inserter"
        for (tank_array::size_type j = 0; j < temp_array.size(); j++) {
            players.push_back(temp_array[j]);
        }
    }

    return players;
}
