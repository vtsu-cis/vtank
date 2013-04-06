/*!
    \file   nodemanager.hpp
    \brief  Blueprint for the node manager, which tracks nodes and 
            distributes messages.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef NODEMANAGER_HPP
#define NODEMANAGER_HPP

// This could be dynamically sized, perhaps 10% of the map. To be continued.
#define NODE_WIDTH 832
#define NODE_HEIGHT 640

#include <node.hpp>
#include <Map.hpp>
#include <tank.hpp>
#include <projectile.hpp>

/*!
    The NodeManager class is a specially created graph class that tracks
    each section of the map as a grid. The map is split into large parts
    (as decided by this class) and processed individually. Neighboring nodes
    are tracked as well for the purpose of distributing messages only to
    relevant nodes. For example, if a tank in Node (1, 1) performs an action,
    only tanks in node (0, 0), (0, 1), (1, 0) -- every node in all eight 
    directions -- are notified of the action.
*/
class NodeManager
{
private:
    boost::mutex mutex;
    Node *nodes;
    int size;
	int width;
	int height;

    /*!
        Allocate the nodes.
        \param node_size Number of nodes to allocate.
        \throws std::exception Exception may be thrown if it tries to allocate
        nodes with too many elements.
    */
    void allocate_nodes(const int &);

    /*!
        Deallocate the nodes. Performs a simple delete[] operation.
    */
    void deallocate_nodes();

public:
    /*!
        Initializes the thread pool and nothing else. set_map() should be called soon
        after this constructor.
        TODO: Maybe current_map can be initialized here.
    */
    NodeManager();

    /*!
        Deallocates the nodes, if they were allocated.
    */
    ~NodeManager();

    /*!
        Change the map that the manager uses, causing it to deallocate it's
        current set of nodes and re-allocate them. Note that this clears the
        list of players, so they must be re-registered.
        \param map Map to set.
    */
    void set_map(const Map *const);

	/*!
		Ask which node is present at the given position.
		\param position Positon to check the node at.
		\return ID of the node at the given position; -1 if it falls off the map.
	*/
	int get_node_at(const VTankObject::Point &);

	/*!
		See if the node ID of one node is close to the node ID of another node.
		\param node1 First node to check.
		\param node2 Second node to check.
		\return True if node1 is near node2; false otherwise.
	*/
	bool is_near(int node1, int node2) const;

    /*!
        Process the position of a player. This will register the player with the 
        appropriate node, or perhaps will do nothing.
        \param player Player to process.
    */
    void process_position(tank_ptr);

    /*!
        Process the position of the projectile. While it doesn't register the projectile
        internally, it still sets a new node ID for the projectile.
        \param projectile Projectile to process.
    */
    void process_projectile(projectile_ptr projectile);

    /*!
        Remove a player from a node. This is used for when a player leaves the game.
        \param id ID of the player.
        \param node_id ID of the node.
    */
    void unregister_player(const int &, const int &);
    
    /*!
        Get a collection of players who are relevant to the given node. A relevant
        node is any node neighboring the given node (including the given node) in
        all eight directions.
        \param node_id ID of the node that the program is interested in.
        \return Array of players collected from each relevant node.
    */
    tank_array get_relevant_players(const int &);

	/*!
		Get the number of nodes in the node manager.
		\return Number of nodes allocated for the node manager.
	*/
	const int get_size() const { return size; }
};

#endif
