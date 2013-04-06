/*!
    \file   nodemanagertests.cpp.
    \brief  Unit tests for the NodeManager class.
    \author (C) Copyright 2009 by Vermont Technical College
*/

#include <master.hpp>
#include <nodemanager.hpp>
#include <nodemanagertests.hpp>
#include <UnitTestManager.hpp>
#include <Map.hpp>
#include <GameSession.h>

namespace {
    bool set_map_test()
    {
        Map test_map;
        test_map.create(100, 100, "test");
        
        NodeManager node_manager;
        node_manager.set_map(&test_map);

        // Didn't throw an exception, so this unit test passed.
        UNIT_CHECK(true);

        return true;
    }

    bool node_area_test()
    {
        // There should be exactly 4x4 nodes.
        const int width     = (NODE_WIDTH * 4) / TILE_SIZE;
        const int height    = (NODE_HEIGHT * 4) / TILE_SIZE;

        Map test_map;
        UNIT_CHECK(test_map.create(width, height, "test"));

        NodeManager node_manager;
        node_manager.set_map(&test_map);

		VTankObject::Point pos;
		pos.x = 0;
		pos.y = 0;

		tank_ptr player(new Tank(GameSession::Tank(), player_ptr(new PlayerInfo(NULL, NULL)), GameSession::NONE));
		player->set_position(pos);

        node_manager.process_position(player);

        UNIT_CHECK(player->get_node_id() == 0);

        // Shift to the right one node, making the new expected node == 1.
        pos.x = NODE_WIDTH;
		player->set_position(pos);
        node_manager.process_position(player);

        UNIT_CHECK(player->get_node_id() == 1);

        // Shift down one. The new expected node is (1 * 4) + 1 == 5
        pos.y = -NODE_HEIGHT;
		player->set_position(pos);
        node_manager.process_position(player);

        UNIT_CHECK(player->get_node_id() == 5);

        return true;
    }
}

void node_manager_register_tests()
{
    UnitTestManager::register_test(set_map_test, "NodeManager Set Map Test");
    UnitTestManager::register_test(node_area_test, "NodeManager Node Area Test");
}

