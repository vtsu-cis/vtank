/*! \file    MapTests.cpp
    \brief   Tests for the Map Class.
    \author  (C) Copyright 2009 by Vermont Technical College
*/
#include <fstream>
#include <stdio.h>
#include "MapTests.hpp"
#include <UnitTestManager.hpp>
#include <Map.hpp>

namespace {
    bool test_create()
    {
        Map test;
        //Test create negative x
        UNIT_CHECK(!test.create(-1, 1, "test"));
        //Test create negative y
        UNIT_CHECK(!test.create(1, -1, "test"));
        //Test create negative x and y
        UNIT_CHECK(!test.create(-1, -1, "test"));
        //Test create 0 value x
        UNIT_CHECK(!test.create(0, 1, "test"));
        //Test create 0 value y
        UNIT_CHECK(!test.create(1, 0, "test"));
        //Test create 0 value x and y
        UNIT_CHECK(!test.create(0, 0, "test"));
        //Test create "normal" conditions
        UNIT_CHECK(test.create(1, 1, "test"));
        //Test create with empty string
        UNIT_CHECK(test.create(1, 1, ""));
        return true;
    }
    
    bool test_get_last_error()
    {
        Map test;
        test.create(-1, 1, "test");
        //Test that it get expected error
        UNIT_CHECK(test.get_last_error() == ("Map size cannot have a width or height less than or equal to 0"));
        //Test that it resets last_error to None
        UNIT_CHECK(test.get_last_error() == ("None"));
        return true;
    }

    bool test_save_map()
    {
        Map test;
        test.create(1, 1, "test");
        //Test saving without extension .vtmap
        UNIT_CHECK(test.save("testmap"));
        std::ifstream file("testmap.vtmap");
        //Test if the file saved with the appended extension .vtmap
        UNIT_CHECK(file);
        file.close();
        //Test "normal" save conditions
        UNIT_CHECK(test.save("testmap.vtmap"));
        Map test2;
        //Test saving map with Null tile data and height and width of 0
        try {
            test2.save("testmap.vtmap");
            UNIT_CHECK(false);
        }
        catch (...)
        {
            UNIT_CHECK(true);
        }
        remove("testmap.vtmap");
        return true;  
    }

    bool test_load_map()
    {
        Map test;
        //Test for loading files that don't exsist
        UNIT_CHECK(!test.load("testmap.vtmap"));
        test.create(1, 1, "test");
        test.save("testmap.vtmap");
        //Test for loading files that don't end in .vtmap
        UNIT_CHECK(test.load("testmap"));
        //Test for loading "normal" conditions
        UNIT_CHECK(test.load("testmap.vtmap"));
        Map test2;
        std::fstream file("testmap.vtmap");
        file << "Untitled"  << std::endl;
        file << 0  << std::endl;
        file << 0 << std::endl;
        file.close();
        //Test for loading Null tile data and height and width of 0
        UNIT_CHECK(!test2.load("testmap.vtmap"));
        remove("testmap.vtmap");
        return true;
    }

    bool test_resize_map()
    {
        Map test;
        test.create(1, 1, "test");
        //Test expand in x
        UNIT_CHECK(test.resize(2, 1));
        //Test shrink in x
        UNIT_CHECK(test.resize(1, 1));
        //Test expand in y
        UNIT_CHECK(test.resize(1, 2));
        //Test shrink in y
        UNIT_CHECK(test.resize(1, 1));
        //Test resizing to same size
        UNIT_CHECK(test.resize(1, 1));
        //Test resizing to negative x
        UNIT_CHECK(!test.resize(-1, 1));
        //Test resizing to negative y
        UNIT_CHECK(!test.resize(1, -1));
        //Test resizing to negative x and y
        UNIT_CHECK(!test.resize(-1, -1));
        return true;
    }

    bool test_get_title()
    {
        Map test;
        //Test that title is "Untitled"
        UNIT_CHECK(test.get_title() == "Untitled");
        test.create(1, 1, "test");
        //Test that title is "test"
        UNIT_CHECK(test.get_title() == "test");
        test.create(1, 1, "");
        //Test that title is empty string
        UNIT_CHECK(test.get_title() == "");
        return true;
    }

    bool test_set_title()
    {
        Map test;
        test.set_title("");
        //Test setting title to empty
        UNIT_CHECK(test.get_title() == "");
        test.set_title("test");
        //Test setting title "normal" conditions
        UNIT_CHECK(test.get_title() == "test");
        return true;
    }

    bool test_get_width()
    {
        Map test;
        //Test that initial width is zero
        UNIT_CHECK(test.get_width() == 0);
        test.create(1, 1, "test");
        //Test that width is now 1
        UNIT_CHECK(test.get_width() == 1);
        return true;
    }

    bool test_get_height()
    {
        Map test;
        //Test that initial height is zero
        UNIT_CHECK(test.get_height() == 0);
        test.create(1, 1, "test");
        //Test that height is now 1
        UNIT_CHECK(test.get_height() == 1);
        return true;
    }

    bool test_get_tile()
    {
        Map test;
        //Test can't get tile that is NULL
        try {
            test.get_tile(1, 1);
            UNIT_CHECK(false);
        }
        catch (...)
        {
            UNIT_CHECK(true);
        }
        test.create(4, 4, "test");
        Tile x;
        x.tile_id = 0;
        x.passable = 1;
        x.effect = 0;
        x.height = 0;
        x.type = 0;
        x.object_id = 0;
        x.event_id = 0;
        //Test returns "normal" conditions
        UNIT_CHECK(test.get_tile(1, 1) == x);
        //Test negative x
        try {
            test.get_tile(-1, 1);
            UNIT_CHECK(false);
        }
        catch(...)
        {
            UNIT_CHECK(true);            
        }
        //Test negative y
        try {
            test.get_tile(1, -1);
            UNIT_CHECK(false);
        }
        catch(...)
        {
            UNIT_CHECK(true);
        }
        //Test negative x and y
        try {
            test.get_tile(-1, -1);
            UNIT_CHECK(false);
        }
        catch(...)
        {
            UNIT_CHECK(true);
        }
        return true;
    }

    bool test_set_tile_id()
    {
        Map test;
        //Test setting while tile_data is NULL
        try {
            test.set_tile_id(1, 1, 1);
            UNIT_CHECK(false);
        }
        catch(...)
        {
            UNIT_CHECK(true);
        }
        test.create(5,5,"test.vtmap");
        //Test setting id y = 0
        UNIT_CHECK(test.set_tile_id(1, 0, 1));
        //Test setting id x = 0
        UNIT_CHECK(test.set_tile_id(0, 1, 1));
        //Test setting id x and y = 0
        UNIT_CHECK(test.set_tile_id(0, 0, 1));
        //Test setting id in "normal" conditions
        UNIT_CHECK(test.set_tile_id(1, 1, 1));
        //Test setting id x is negative
        UNIT_CHECK(!test.set_tile_id(-1, 1, 1));
        //Test setting id y is negative
        UNIT_CHECK(!test.set_tile_id(1, -1, 1));
        //Test setting id x and y are negative
        UNIT_CHECK(!test.set_tile_id(-1, -1, 1));
        //Test setting id, id is negative
        UNIT_CHECK(!test.set_tile_id(1, 1, -1));
        return true;
    }

    bool test_set_tile_collision()
    {
        Map test;
        //Test setting collision while tile_data is null
        try {
            test.set_tile_collision(1, 1, false);
            UNIT_CHECK(false);
        }
        catch(...)
        {
            UNIT_CHECK(true);
        }
        test.create(5, 5, "test.vtmap");
        //Test setting collision with x = 0
        UNIT_CHECK(test.set_tile_collision(0, 1, false));
        //Test setting collision with y = 0
        UNIT_CHECK(test.set_tile_collision(1, 0, false));
        //Test setting collision with x and y = 0
        UNIT_CHECK(test.set_tile_collision(0, 0, false));
        //Test setting collision with x negative
        UNIT_CHECK(!test.set_tile_collision(-1, 1, false));
        //Test setting collision with y negative
        UNIT_CHECK(!test.set_tile_collision(1, -1, false));
        //Test setting collision with x and y negative
        UNIT_CHECK(!test.set_tile_collision(-1, -1, false));
        //Test setting collision with collision equal to true
        UNIT_CHECK(test.set_tile_collision(1, 1, true));
        //Test setting collision with collision equal to false
        UNIT_CHECK(test.set_tile_collision(1, 1, false));
        return true;
    }

    bool test_set_tile_object()
    {
         Map test;
        //Test setting while tile_data is NULL
        try {
            test.set_tile_object(1, 1, 1);
            UNIT_CHECK(false);
        }
        catch(...)
        {
            UNIT_CHECK(true);
        }
        test.create(5,5,"test.vtmap");
        //Test setting id y = 0
        UNIT_CHECK(test.set_tile_object(1, 0, 1));
        //Test setting id x = 0
        UNIT_CHECK(test.set_tile_object(0, 1, 1));
        //Test setting id x and y = 0
        UNIT_CHECK(test.set_tile_object(0, 0, 1));
        //Test setting id in "normal" conditions
        UNIT_CHECK(test.set_tile_object(1, 1, 1));
        //Test setting id x is negative
        UNIT_CHECK(!test.set_tile_object(-1, 1, 1));
        //Test setting id y is negative
        UNIT_CHECK(!test.set_tile_object(1, -1, 1));
        //Test setting id x and y are negative
        UNIT_CHECK(!test.set_tile_object(-1, -1, 1));
        //Test setting id, id is negative
        UNIT_CHECK(!test.set_tile_object(1, 1, -1));
        return true;
    }
    
    bool test_set_tile_event()
    {
         Map test;
        //Test setting while tile_data is NULL
        try {
            test.set_tile_event(1, 1, 1);
            UNIT_CHECK(false);
        }
        catch(...)
        {
            UNIT_CHECK(true);
        }
        test.create(5, 5, "test.vtmap");
        //Test setting id y = 0
        UNIT_CHECK(test.set_tile_event(1, 0, 1));
        //Test setting id x = 0
        UNIT_CHECK(test.set_tile_event(0, 1, 1));
        //Test setting id x and y = 0
        UNIT_CHECK(test.set_tile_event(0, 0, 1));
        //Test setting id in "normal" conditions
        UNIT_CHECK(test.set_tile_event(1, 1, 1));
        //Test setting id x is negative
        UNIT_CHECK(!test.set_tile_event(-1, 1, 1));
        //Test setting id y is negative
        UNIT_CHECK(!test.set_tile_event(1, -1, 1));
        //Test setting id x and y are negative
        UNIT_CHECK(!test.set_tile_event(-1, -1, 1));
        //Test setting id, id is negative
        UNIT_CHECK(!test.set_tile_event(1, 1, -1));
        return true;
    }

    bool test_set_tile_height()
    {
        Map test;
        //Test setting while tile_data is NULL
        try {
            test.set_tile_height(1, 1, 1);
            UNIT_CHECK(false);
        }
        catch(...)
        {
            UNIT_CHECK(true);
        }
        test.create(5, 5, "test.vtmap");
        //Test setting height y = 0
        UNIT_CHECK(test.set_tile_height(1, 0, 1));
        //Test setting height x = 0
        UNIT_CHECK(test.set_tile_height(0, 1, 1));
        //Test setting height x and y = 0
        UNIT_CHECK(test.set_tile_height(0, 0, 1));
        //Test setting height in "normal" conditions
        UNIT_CHECK(test.set_tile_height(1, 1, 1));
        //Test setting height x is negative
        UNIT_CHECK(!test.set_tile_height(-1, 1, 1));
        //Test setting height y is negative
        UNIT_CHECK(!test.set_tile_height(1, -1, 1));
        //Test setting height x and y are negative
        UNIT_CHECK(!test.set_tile_height(-1, -1, 1));
        //Test setting height, height is negative
        UNIT_CHECK(test.set_tile_height(1, 1, -1));
        return true;
    }

    bool test_set_tile_type()
    {
         Map test;
        //Test setting while tile_data is NULL
        try {
            test.set_tile_type(1, 1, 1);
            UNIT_CHECK(false);
        }
        catch(...)
        {
            UNIT_CHECK(true);
        }
        test.create(5, 5, "test.vtmap");
        //Test setting type y = 0
        UNIT_CHECK(test.set_tile_type(1, 0, 1));
        //Test setting type x = 0
        UNIT_CHECK(test.set_tile_type(0, 1, 1));
        //Test setting type x and y = 0
        UNIT_CHECK(test.set_tile_type(0, 0, 1));
        //Test setting type in "normal" conditions
        UNIT_CHECK(test.set_tile_type(1, 1, 1));
        //Test setting type x is negative
        UNIT_CHECK(!test.set_tile_type(-1, 1, 1));
        //Test setting type y is negative
        UNIT_CHECK(!test.set_tile_type(1, -1, 1));
        //Test setting type x and y are negative
        UNIT_CHECK(!test.set_tile_type(-1, -1, 1));
        //Test setting type, type is negative
        UNIT_CHECK(!test.set_tile_type(1, 1, -1));
        return true;
    }
    bool test_set_tile_effect()
    {
         Map test;
        //Test setting while tile_data is NULL
        try {
            test.set_tile_effect(1, 1, 1);
            UNIT_CHECK(false);
        }
        catch(...)
        {
            UNIT_CHECK(true);
        }
        test.create(5, 5, "test.vtmap");
        //Test setting effect y = 0
        UNIT_CHECK(test.set_tile_effect(1, 0, 1));
        //Test setting effect x = 0
        UNIT_CHECK(test.set_tile_effect(0, 1, 1));
        //Test setting effect x and y = 0
        UNIT_CHECK(test.set_tile_effect(0, 0, 1));
        //Test setting effect in "normal" conditions
        UNIT_CHECK(test.set_tile_effect(1, 1, 1));
        //Test setting effect x is negative
        UNIT_CHECK(!test.set_tile_effect(-1, 1, 1));
        //Test setting effect y is negative
        UNIT_CHECK(!test.set_tile_effect(1, -1, 1));
        //Test setting effect x and y are negative
        UNIT_CHECK(!test.set_tile_effect(-1, -1, 1));
        //Test setting effect, effect is negative
        UNIT_CHECK(!test.set_tile_effect(1, 1, -1));
        return true;
    }
    bool test_get_tile_collision()
    {
        Map test;
        //Test getting collision while tile_data is NULL
        try {
            test.get_tile_collision(1, 1);
            UNIT_CHECK(false);
        }
        catch(...)
        {
            UNIT_CHECK(true);
        }
        test.create(5, 5, "test.vtmap");
        //Test getting collision y = 0
        UNIT_CHECK(test.get_tile_collision(1, 0));
        //Test getting collision x = 0
        UNIT_CHECK(test.get_tile_collision(0, 1));
        //Test getting collision x and y = 0
        UNIT_CHECK(test.get_tile_collision(0, 0));
        //Test getting collision in "normal" conditions
        UNIT_CHECK(test.get_tile_collision(1, 1));
        //Test getting collision x is negative
        UNIT_CHECK(!test.get_tile_collision(-1, 1));
        //Test getting collision y is negative
        UNIT_CHECK(!test.get_tile_collision(1, -1));
        //Test getting collision x and y are negative
        UNIT_CHECK(!test.get_tile_collision(-1, -1));
        test.set_tile_collision(1, 1, true);
        //Test getting collision when true
        UNIT_CHECK(test.get_tile_collision(1, 1));
        test.set_tile_collision(1, 1, false);
        //Test getting collision when false
        UNIT_CHECK(!test.get_tile_collision(1, 1));
        return true;
    }

    bool test_add_supported_game_mode()
    {
        Map test;
        //Test adding a supported game type
        try {
            test.add_supported_game_mode(1);
            UNIT_CHECK(true);
        }
        catch(...) {
            UNIT_CHECK(false);
        }
        //Test to make sure it acually exsists in the supported_game_modes
        std::vector<int> x = test.get_supported_game_modes();
        std::vector<int>::iterator i;
        for (i = x.begin(); i != x.end(); i++) {
            if (*i == 1) {
                UNIT_CHECK(true);
            }
        }
        return true;
    }
    
    bool test_remove_supported_game_mode()
    {
        Map test;
        test.add_supported_game_mode(1);
        //Test removing a non-exsistent mode
        try {
            test.remove_supported_game_mode(2);
            UNIT_CHECK(true);
        }
        catch(...) {
            UNIT_CHECK(false);
        }
        //Test removing a pre-exsisting mode
        try {
            test.remove_supported_game_mode(1);
              UNIT_CHECK(true);
        }
        catch(...) {
            UNIT_CHECK(false);
        }
        return true;
    }

    bool test_validate_death_match()
    {
        Map test;
        //Test validating with height and width of map being 0 and tile_data = null
        UNIT_CHECK(!test.validate_death_match());
        test.create(5, 5, "test.vtmap");
        //Test validating without spawn point on map
        UNIT_CHECK(!test.validate_death_match());
        test.set_tile_event(1,1,SPAWN_POINT);
        //Test validating with the spawn point
        UNIT_CHECK(test.validate_death_match());
        return true;
    }

    bool test_validate_team_death_match()
    {
        Map test;
        //Test validating with height and width of map being 0 and tile_data = null
        UNIT_CHECK(!test.validate_team_death_match());
        test.create(5, 5, "test.vtmap");
        //Test validating without spawn areas on map
        UNIT_CHECK(!test.validate_team_death_match());
        test.set_tile_event(1,1,RED_SPAWN_AREA);
        //Test validating with just red spawn area
        UNIT_CHECK(!test.validate_team_death_match());
        test.set_tile_event(1,1,BLUE_SPAWN_AREA);
        //Test validating with just blue spawn area
        UNIT_CHECK(!test.validate_team_death_match());
        test.set_tile_event(1,2,RED_SPAWN_AREA);
        //Test validating with blue and red spawn areas
        UNIT_CHECK(test.validate_team_death_match());
        return true;
    }

    bool test_validate_capture_the_flag()
    {
         Map test;
        //Test validating with height and width of map being 0 and tile_data = null
        UNIT_CHECK(!test.validate_capture_the_flag());
        test.create(5, 5, "test.vtmap");
        //Test validating without red_flag, blue_flag, red_spawn, or blue_spawn areas on map
        UNIT_CHECK(!test.validate_capture_the_flag());
        test.set_tile_event(1, 1, RED_SPAWN_AREA);
        //Test validating with just red spawn area
        UNIT_CHECK(!test.validate_capture_the_flag());
        test.set_tile_event(1, 1, BLUE_SPAWN_AREA);
        //Test validating with just blue spawn area
        UNIT_CHECK(!test.validate_capture_the_flag());
        test.set_tile_event(1, 1, RED_FLAG);
        //Test validating with just red flag
        UNIT_CHECK(!test.validate_capture_the_flag());
        test.set_tile_event(1, 1, BLUE_FLAG);
        //Test validating with just blue flag
        UNIT_CHECK(!test.validate_capture_the_flag());
        test.set_tile_event(1, 2, RED_FLAG);
        test.set_tile_event(2, 1, BLUE_SPAWN_AREA);
        test.set_tile_event(2, 2, RED_SPAWN_AREA);
        //Test validating with red flag, blue flag, red spawn, and blue spawn
        UNIT_CHECK(test.validate_capture_the_flag());
        return true;
    }

    bool test_validate_capture_the_base()
    {
		// TODO: Re-write this.
        //Map test;
        //Test validating with height and width of map being 0 and tile_data = null
        /*UNIT_CHECK(!test.validate_capture_the_base());
        test.create(5, 5, "test.vtmap");
        //Test validating without spawn areas on map
        UNIT_CHECK(!test.validate_capture_the_base());
        test.set_tile_event(1,1,SPAWN_POINT);
        //Test validating with just spawn point
        UNIT_CHECK(!test.validate_capture_the_base());
        test.set_tile_event(1,1,KING_AREA);
        //Test validating with just king area
        UNIT_CHECK(!test.validate_capture_the_base());
        test.set_tile_event(1,2,SPAWN_POINT);
        //Test validating with spawn point and king area
        UNIT_CHECK(test.validate_capture_the_base());*/
		return true;
    }

    bool test_validate_supported_game_modes()
    {
        Map test;
        std::vector<int> modes;
        //Test validating game modes while supported game modes is null
        try {
            test.validate_supported_game_modes();
            UNIT_CHECK(true);
        }
        catch (...) {
            UNIT_CHECK(false);
        }
        test.create(5, 5, "test.vtmap");
        test.validate_supported_game_modes();
        modes = test.get_supported_game_modes();
        //Test validating game modes with none valid
        if(modes.begin() == modes.end()) {
            UNIT_CHECK(true); }
        else {
            UNIT_CHECK(false); }
        test.set_tile_event(1, 1, SPAWN_POINT);
        test.add_supported_game_mode(DEATH_MATCH);
        test.validate_supported_game_modes();
        modes = test.get_supported_game_modes();
        //Test validating game modes with just death match
        if(*modes.begin() == DEATH_MATCH) {
            UNIT_CHECK(true); }
        else {
            UNIT_CHECK(false); }
        //More to add to this test once KOTH and CTF are implemented
        return true;
    }

    bool test_int_to_bytes()
    {
        std::vector<unsigned char> y(4);
        y[0] = 0;
        y[1] = 0;
        y[2] = 0;
        y[3] = 0;
        std::vector<unsigned char> x = int_to_bytes(0);
        //Test if 0 is converted properly
        UNIT_CHECK(x == y);
        y[0] = 255;
        y[1] = 0;
        y[2] = 0;
        y[3] = 0;
        x = int_to_bytes(255);
        //Test if 255 is converted properly
        UNIT_CHECK(x == y);
        y[0] = 0;
        y[1] = 1;
        y[2] = 0;
        y[3] = 0;
        x = int_to_bytes(256);
        //Test if 256 is converted properly
        UNIT_CHECK(x == y);
        y[0] = 255;
        y[1] = 255;
        y[2] = 0;
        y[3] = 0;
        x = int_to_bytes(65535);
        //Test if 65535 is converted properly
        UNIT_CHECK(x == y);
        y[0] = 0;
        y[1] = 0;
        y[2] = 1;
        y[3] = 0;
        x = int_to_bytes(65536);
        //Test if 65536 is converted properly
        UNIT_CHECK(x == y);
        y[0] = 255;
        y[1] = 255;
        y[2] = 255;
        y[3] = 0;
        x = int_to_bytes(16777215);
        //Test if 16777215 is converted properly
        UNIT_CHECK(x == y);
        y[0] = 0;
        y[1] = 0;
        y[2] = 0;
        y[3] = 1;
        x = int_to_bytes(16777216);
        //Test if 16777216 is converted properly
        UNIT_CHECK(x == y);
        y[0] = 255;
        y[1] = 255;
        y[2] = 255;
        y[3] = 255;
        x = int_to_bytes(4294967295);
        //Test if max size 4294967295 is converted properly
        UNIT_CHECK(x == y);
        return true;
    }

    bool test_bytes_to_int()
    {
        unsigned char x[4] = {'\x0', '\x0', '\x0', '\x0'};
        int y = bytes_to_int(x,4);
        //Test if 0 is converted properly as 4 bit
        UNIT_CHECK(y == 0);
        y = bytes_to_int(x,2);
        //Test if 0 is converted properly as 2 bit
        UNIT_CHECK(y == 0);
        x[0] = '\xFF';
        x[1] = '\x0';
        x[2] = '\x0';
        x[3] = '\x0';
        y = bytes_to_int(x,4);
        //Test if 255 is converted properly as 4 bit
        UNIT_CHECK(y == 255);
        y = bytes_to_int(x,2);
        //Test if 255 is converted properly as 2 bit
        UNIT_CHECK(y == 255);
        x[0] = '\x0';
        x[1] = '\x1';
        x[2] = '\x0';
        x[3] = '\x0';
        y = bytes_to_int(x,4);
        //Test if 256 is converted properly as 4 bit
        UNIT_CHECK(y == 256);
        y = bytes_to_int(x,2);
        //Test if 256 is converted properly as 2 bit
        UNIT_CHECK(y == 256);
        x[0] = '\xFF';
        x[1] = '\xFF';
        x[2] = '\x0';
        x[3] = '\x0';
        y = bytes_to_int(x,4);
        //Test if 65535 is converted properly as 4 bit
        UNIT_CHECK(y == 65535);
        y = bytes_to_int(x,2);
        //Test if 65535 is converted properly as 2 bit
        UNIT_CHECK(y == 65535);
        x[0] = '\x0';
        x[1] = '\x0';
        x[2] = '\x1';
        x[3] = '\x0';
        y = bytes_to_int(x,4);
        //Test if 65536 is converted properly as 4 bit
        UNIT_CHECK(y == 65536);
        y = bytes_to_int(x,2);
        //Test if 65536 is converted wrong as 2 bit
        UNIT_CHECK(y != 65536);
        x[0] = '\xFF';
        x[1] = '\xFF';
        x[2] = '\xFF';
        x[3] = '\x0';
        y = bytes_to_int(x,4);
        //Test if 16777215 is converted properly as 4 bit
        UNIT_CHECK(y == 16777215);
        y = bytes_to_int(x,2);
        //Test if 16777215 is converted wrong as 2 bit
        UNIT_CHECK(y != 16777215);
        x[0] = '\x0';
        x[1] = '\x0';
        x[2] = '\x0';
        x[3] = '\x1';
        y = bytes_to_int(x,4);
        //Test if 16777216 is converted properly as 4 bit
        UNIT_CHECK(y == 16777216);
        y = bytes_to_int(x,2);
        //Test if 16777216 is converted wrong as 2 bit
        UNIT_CHECK(y != 16777216);
        x[0] = '\xFF';
        x[1] = '\xFF';
        x[2] = '\xFF';
        x[3] = '\xFF';
        y = bytes_to_int(x,4);
        //Test if max size 4294967295 is converted properly as 4 bit
        UNIT_CHECK(y == 4294967295);
        y = bytes_to_int(x,2);
        //Test if 4294967295 is converted wrong as 2 bit
        UNIT_CHECK(y != 4294967295);
        y = bytes_to_int(x,3);
        //Test if nothing can be converted as 3 bit
        UNIT_CHECK(y == -1);
        return true;
    }

    bool test_tile_to_bytes()
    {
        Tile x;
        std::vector<unsigned char> returned_bytes = tile_to_bytes(x);
        std::vector<unsigned char> bytes = int_to_bytes(0); //tile_id
        std::vector<unsigned char> temp = int_to_bytes(0); //object_id
        bytes.insert(bytes.end(), temp[0]);
        bytes.insert(bytes.end(), temp[1]);
        temp = int_to_bytes(0); //event_id
        bytes.insert(bytes.end(), temp[0]);
        bytes.insert(bytes.end(), temp[1]);
        bytes.insert(bytes.end(), '\x1'); // passible
        bytes.insert(bytes.end(), '\x0'); // height
        bytes.insert(bytes.end(), '\x0'); // type
        bytes.insert(bytes.end(), '\x0'); // effect
        // test "0" tile
        UNIT_CHECK(returned_bytes == bytes);
        bytes.clear();
        x.tile_id = 255;
        x.object_id = 255;
        x.event_id = 255;
        returned_bytes = tile_to_bytes(x);
        temp = int_to_bytes(255); //tile_id
        bytes.insert(bytes.end(), temp[0]);
        bytes.insert(bytes.end(), temp[1]);
        bytes.insert(bytes.end(), temp[2]);
        bytes.insert(bytes.end(), temp[3]);
        temp = int_to_bytes(255); //object_id
        bytes.insert(bytes.end(), temp[0]);
        bytes.insert(bytes.end(), temp[1]);
        temp = int_to_bytes(255); //event_id
        bytes.insert(bytes.end(), temp[0]);
        bytes.insert(bytes.end(), temp[1]);
        bytes.insert(bytes.end(), '\x1'); // passible
        bytes.insert(bytes.end(), '\x0'); // height
        bytes.insert(bytes.end(), '\x0'); // type
        bytes.insert(bytes.end(), '\x0'); // effect
        // test "255" tile
        UNIT_CHECK(returned_bytes == bytes);
        bytes.clear();
        x.tile_id = 256;
        x.object_id = 256;
        x.event_id = 256;
        returned_bytes = tile_to_bytes(x);
        temp = int_to_bytes(256); //tile_id
        bytes.insert(bytes.end(), temp[0]);
        bytes.insert(bytes.end(), temp[1]);
        bytes.insert(bytes.end(), temp[2]);
        bytes.insert(bytes.end(), temp[3]);
        temp = int_to_bytes(256); //object_id
        bytes.insert(bytes.end(), temp[0]);
        bytes.insert(bytes.end(), temp[1]);
        temp = int_to_bytes(256); //event_id
        bytes.insert(bytes.end(), temp[0]);
        bytes.insert(bytes.end(), temp[1]);
        bytes.insert(bytes.end(), '\x1'); // passible
        bytes.insert(bytes.end(), '\x0'); // height
        bytes.insert(bytes.end(), '\x0'); // type
        bytes.insert(bytes.end(), '\x0'); // effect
        // test "256" tile
        UNIT_CHECK(returned_bytes == bytes);
        return true;
    }

    bool test_bytes_to_tile()
    {
        //tile_id of 0
        
        // passible of 1 for true
        // height of 0
        // type of 0
        // effect of 0
        unsigned char bytes[12] = {'\x0', '\x0', '\x0', '\x0', '\x0', '\x0', '\x0', '\x0', '\x1', '\x0', '\x0', '\x0'};
        Tile x = bytes_to_tile(bytes);
        Tile y;
        // test "0" tile
        UNIT_CHECK(x == y);
        bytes[0] = '\xFF';
        bytes[1] = '\x0';
        bytes[2] = '\x0';
        bytes[3] = '\x0';//tile_id of 255
        bytes[4] = '\xFF';
        bytes[5] = '\x0';//object_id of 255
        bytes[6] = '\xFF';
        bytes[7] = '\x0';//event_id of 255
        y.tile_id = 255;
        y.object_id = 255;
        y.event_id = 255;
        x = bytes_to_tile(bytes);
        // test "255" tile
        UNIT_CHECK(x == y);
        bytes[0] = '\x0';
        bytes[1] = '\x1';
        bytes[2] = '\x0';
        bytes[3] = '\x0';//tile_id of 256
        bytes[4] = '\x0';
        bytes[5] = '\x1';//object_id of 256
        bytes[6] = '\x0';
        bytes[7] = '\x1';//event_id of 256
        y.tile_id = 256;
        y.object_id = 256;
        y.event_id = 256;
        x = bytes_to_tile(bytes);
        // test "256" tile
        UNIT_CHECK(x == y);
        return true;
    }
}

void map_register_tests()
{
    UnitTestManager::register_test(test_create, "Map Create Tests");
    UnitTestManager::register_test(test_get_last_error, "Map GetLastError Tests");
    UnitTestManager::register_test(test_save_map, "Map Save Test");
    UnitTestManager::register_test(test_load_map, "Map Load Test");
    UnitTestManager::register_test(test_resize_map, "Map Resize Test");
    UnitTestManager::register_test(test_get_title, "Map GetTitle Test");
    UnitTestManager::register_test(test_set_title, "Map SetTitle Test");
    UnitTestManager::register_test(test_get_width, "Map GetWidth Test");
    UnitTestManager::register_test(test_get_height, "Map GetHeight Test");
    UnitTestManager::register_test(test_get_tile, "Map GetTile Test");
    UnitTestManager::register_test(test_set_tile_id, "Map SetTileId Test");
    UnitTestManager::register_test(test_set_tile_collision, "Map SetTileCollision Test");
    UnitTestManager::register_test(test_set_tile_object, "Map SetTileObject Test");
    UnitTestManager::register_test(test_set_tile_event, "Map SetTileEvent Test");
    UnitTestManager::register_test(test_set_tile_height, "Map SetTileHeight Test");
    UnitTestManager::register_test(test_set_tile_type, "Map SetTileType Test");
    UnitTestManager::register_test(test_set_tile_effect, "Map SetTileEffect Test");
    UnitTestManager::register_test(test_get_tile_collision, "Map GetTileCollision Test");
    UnitTestManager::register_test(test_add_supported_game_mode, "Map AddSupportedGameMode Test");
    UnitTestManager::register_test(test_remove_supported_game_mode, "Map RemoveSupportedGameMode Test");
    UnitTestManager::register_test(test_validate_death_match, "Map ValidateDeathMatch Test");
    UnitTestManager::register_test(test_validate_team_death_match, "Map ValidateTeamDeathMatch Test");
    UnitTestManager::register_test(test_validate_capture_the_flag, "Map ValidateCaptureTheFlag Test");
    UnitTestManager::register_test(test_validate_capture_the_base, "Map ValidateCaptureTheBase Test");
    UnitTestManager::register_test(test_validate_supported_game_modes, "Map ValidateSupportedGameModes Test");
    UnitTestManager::register_test(test_int_to_bytes, "Map IntToBytes Test");
    UnitTestManager::register_test(test_bytes_to_int, "Map BytesToInt Test");
    UnitTestManager::register_test(test_tile_to_bytes, "Map TileToBytes Test");
    UnitTestManager::register_test(test_bytes_to_tile, "Map BytesToTile Test");
}
