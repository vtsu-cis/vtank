###########################################################################
# \file MainSessionI.py
# \brief Main session interface for the VTank main server.
# \author Copyright 2009 by Vermont Technical College
###########################################################################

import re;
import Log;
import Ice;
import math;
import Main;
import Color;
import World;
import random;
import Utils;
import Procedures;
import Client_Types;
import Map_Manager;
import Equipment_Manager;
import VTankObject;
import VersionChecker;
from Exceptions import *;
from time import time;
from hashlib import sha1;
from Color import to_long, to_color;
from Base_Servant import Base_Servant;

TANK_HEALTH = 100;
MAX_RANK = 19;

class MainSessionI(Main.MainSession, Base_Servant):
    """
    Implements the Main.MainSession Ice servant.
    """
    def __init__(self, username, userlevel, ip_string, reporter = None, threshold = 30):
        Base_Servant.__init__(self, ip_string, Client_Types.VTANK_CLIENT_TYPE, userlevel, reporter);
        
        self.name = username;
        self.database = World.get_world().get_database();
        
        self.ranks = {};
        self.tank_list = {};
        self.active_tank = None;
        self.playing = False;
        
        self.threshold = threshold;
        
        self.refresh_tank_list();
    
    def __str__(self):
        return Base_Servant.__str__(self);
    
    def refresh_tank_list(self):
        """
        Perform a SQL search of the client's tank list and cache
        the results.
        """
        results = self.database.do_query(Procedures.get_tank_list(), self.name);
        if results == None:
            self.report("MySQL error: %s." % (self.database.get_last_error()));
            self.tank_list = {};
            return;
        
        tanks = {};
        # Travel through results row-by-row
        for set in results:
            # Format: 0:tank_name, 1:color, 2:weapon_id, 3:speed_factor, 4:armor_factor, 5:points 6:model 7:skin
            #weapon = Equipment_Manager.get_manager().get_weapon(set[2]);
            color = to_color(set[1]);
            color = VTankObject.VTankColor(color.r(), color.g(), color.b());
            tank = VTankObject.TankAttributes(set[0], set[3], set[4], set[5], TANK_HEALTH, set[6], set[7], set[2], color);
            tanks[set[0]] = tank;
            
        self.tank_list = tanks;
    
    def validate_tank_attributes(self, tank):
        """
        Validate the given set of tank attributes.
        """
        # Extract attributes.
        weapon_id   = tank.weaponID;
        speed_factor= tank.speedFactor;
        armor_factor= tank.armorFactor;
        color       = tank.color;
        model       = tank.model;
        skin        = tank.skin;
        
        manager = Equipment_Manager.get_manager();
        weapon = manager.get_weapon(weapon_id);
        if weapon == None:
            # Weapon doesn't exist.
            Log.quick_log("User %s was able to send through an invalid weapon ID (%i)." % (
                self, weapon_id));
            raise BadInformationException("Bad weapon ID.");
        
       
        if speed_factor < 0.5 or speed_factor > 1.5 or armor_factor < 0.5 or armor_factor > 1.5:
            # Invalid factor values.
            Log.quick_log("User %s was able to send through an invalid speed_factor/armor_factor." % self);
            raise BadInformationException("Minimum/Maximum allowed speed/armor breached.");
        
        difference_speed = math.fabs(1.0 - round(speed_factor, 2));
        difference_armor = math.fabs(1.0 - round(armor_factor, 2));
        if difference_speed - difference_armor > 0.001:
            Log.quick_log("User %s was able to send through an invalid speed_factor/armor_factor." % self);
            raise BadInformationException("Speed-to-armor ratio must have exactly the same distance from 100%.");
        
        if model == "":
            raise BadInformationException("Invalid model (%s)." % model);
    
    def validate_tank_name(self, tank_name, check_exists = True):
        """
        Check a tank's name to make sure it's okay. The tank name is assumed
        to belong to a 'new' tank or 'updated' tank.
        @param tank_name Name to check.
        """
        p = re.compile("[^A-Za-z0-9]");
        
        if p.search(tank_name) or tank_name == "":
            # Bad name.
            Log.log_print(Utils.filename_today(), "players", 
                "User %s was able to send through an invalid tank name." % self);
            raise BadInformationException("Tank name contains invalid characters.");
        
        if check_exists and self.tank_name_exists(tank_name):
            raise BadInformationException("Tank name already exists.");
    
    def tank_name_exists(self, tank_name):
        """
        Do a check on the database to see if a certain tank name exists.
        @return True if the name exists, otherwise false.
        """
        results = self.database.do_query(Procedures.tank_exists(), tank_name);
        if len(results) > 0:
            # Tank exists.
            return True;
        
        return False;
    
    def GetRanksOfTanks(self, tanks, current=None):
        """
        Get the ranks of a list of tanks.
        @param tanks:  A list of tank names 
        @return: A list of integers representing their ranks
        """
        self.refresh_action();
            
        account_query_result = self.database.do_query(Procedures.get_tanks_and_points(tuple(tanks)))
        
        if account_query_result == None:            
            self.report("Error:  Failed to retrieve points from database for tanks" % str(tanks));
            return;
        elif len(account_query_result) < 1:
            self.report("Error:  Failed to retrieve points from database for tanks" % str(tanks)); 
            return;
        
        ranks = [];
        for tank_name in tanks:
            for row in account_query_result:            
                if str.lower(tank_name) == str.lower(row[0]):
                    if row[1] == None:
                        ranks.append(-1);
                    else:
                        ranks.append(int(self.get_rank_from_points(int(row[1]))));
            
        return tuple(ranks);
        
    def GetRank(self, current=None):
        """
        Get the rank of a given tank.
        @param tankName: The name of the tank in question.
        @return: His current rank. 
        """
        self.refresh_action();
        
        points = self.GetAccountPoints();       
        rankNumber = self.get_rank_from_points(points);
        return int(math.floor(rankNumber));
        
        
    def GetPointsForRank(self, rankNumber, current=None):
        """
        Gets the number of points needed for a given rank.
        @param rankNumber: The rank in question.
        @return: The number of points required to advance to that rank. 
        """
        self.refresh_action();
        
        return (10*rankNumber)**2;
    
    def get_rank_from_points(self, points):
        """
        Get current rank from a player's current number of points
        @param points: The number of points he has
        @return:  His rank (float)
        """
        rank = math.sqrt(points)/10;
        
        if rank > MAX_RANK:
            return MAX_RANK;
                
        return rank;
    
    def GetPointsByTank(self, tankName):
        """
        Get the total points a tank has from the database.
        @param tankName: The name of the tank in question.
        @return: The current number of points he has earned.         
        """
         
        tank = None;
        
        try:
            self.tank_list.pop(tankName);
        except:
            Log.log_print(Utils.filename_today(), "players", 
                "User %s provided an invalid tank name for rank request." % self);
            raise KeyError;

        if tank == None:
            return None;
        
        points = tank.points; 
        
        return points;
    
    def GetAccountPoints(self, current=None):
        """
        Get the total points an account has from the database.
        @param accountName:  The account in question. 
        @return: The number of points this account has.
        """
        self.refresh_action();
        
        account_points_query = self.database.do_query(Procedures.get_account_points(), self.name);
        if account_points_query == None:
            self.report("ERROR: Failed to retrieve %s's account points from the database" % self.name);
            return None;
        elif len(account_points_query) < 1:
            return None;       
        
        return account_points_query[0][0];
        
    def get_points_for_next_rank(self, currentPoints):
        """
        Get the number of points a player needs for his next rank-up.
        @param currentPoints:  His current number of points.
        @return: The number of points he needs for the next rank. 
        """                        
        
        rankNumber = int(math.floor(self.get_rank_from_points(currentPoints)));
        nextRank = rankNumber+1;
        requiredPointsForNextRank = self.GetRequiredPointsForRank(nextRank);
        
        return requiredPointsForNextRank-currentPoints;
    
    #
    # The following functions are overridden from a generated file.
    # To view the documentation of these functions, please see:
    # MainSession.ice
    #
    
    def KeepAlive(self, current=None):
        self.refresh_action();
    
    def destroy(self, current=None):
        self.Disconnect(current);
    
    def Disconnect(self, current=None):
        World.get_world().get_tracker().remove(self.get_id());
    
    def GetTankList(self, current=None):
        self.refresh_action();
        return self.tank_list.values();
    
    def CheckClientVersion(self, current=None):        
        self.refresh_action();
        version = VersionChecker.get_version();
        return version;
    
    def CreateTank(self, tank, current=None):
        self.refresh_action();
        
        #self.validate_tank_attributes(tank);
        
        # Extract attributes.
        name            = tank.name;
        weapon_id       = tank.weaponID;
        speed_factor    = tank.speedFactor;
        armor_factor    = tank.armorFactor;
        lcolor          = to_long(tank.color);
        model           = tank.model;
        skin            = tank.skin;
        
        # Validate the tank's name.
        self.validate_tank_name(name);
        
        # Insert the tank.
        changed = self.database.do_insert(Procedures.new_tank(), name, self.name, speed_factor, 
                                          armor_factor, lcolor, weapon_id, model, skin);
        
        if not changed:
            # Nothing was changed: database error.
            Log.log_print(Utils.filename_today(), "players", 
                "User %s couldn't insert tank: DB error: %s -- " \
                "data: name=%s weapon_id=%i color=%s" % (self, 
                self.database.get_last_error(), name, weapon_id, str(lcolor)));
            raise VTankException("Internal database error. Please try again later.");
            
        # Now attempt to create the statistics table for this tank.
        changed = self.database.do_insert(Procedures.new_statistics(), name);
        
        if not changed:
            # Nothing was changed: database error.
            Log.log_print(Utils.filename_today(), "players", 
                "User %s couldn't insert statistics: DB error: %s -- " \
                "data: name=%s weapon_id=%i color=%s" % (self, 
                self.database.get_last_error(), name, weapon_id, str(lcolor)));
            raise VTankException("Internal database error. Please try again later.");
        
        self.refresh_tank_list();
        
        self.report("%s created a new tank (%s)."  % (self.name, name));
        
        return True;
    
    def UpdateTank(self, oldTankName, newTank, current=None):
        self.refresh_action();
        
        # Must check if the tank exists, and if it belongs to this account.
        results = self.database.do_query(Procedures.tank_exists_under_account(), 
                                         oldTankName, self.name);
        if len(results) != 1:
            # Tank does not exist.
            Log.log_print(Utils.filename_today(), "players",
                          "User %s tried to update a tank that doesn't exist or belong to him: %s" % (
                          self.name, oldTankName));
            raise BadInformationException("That tank does not exist.");
        
        self.validate_tank_name(newTank.name, False);
        #self.validate_tank_attributes(newTank);
        
        # Extract attributes.
        weapon_id       = newTank.weaponID;
        speed_factor    = newTank.speedFactor;
        armor_factor    = newTank.armorFactor;
        lcolor          = to_long(newTank.color);
        model           = newTank.model;
        skin            = newTank.skin;
        
        changed = self.database.do_insert(Procedures.update_tank(), 
            speed_factor, armor_factor, lcolor, weapon_id, model, skin, oldTankName);
        
        if not changed:
            # Nothing was changed: database error.
            Log.log_print(Utils.filename_today(), "players", 
                "User %s couldn't update tank: DB error: %s -- " \
                "data: oldname=%s name=%s weapon_id=%i armor_id=%i color=%s" % (self, 
                self.database.get_last_error(), oldTankName, newTank.name, weapon_id, armor_factor, str(lcolor)));
            #raise VTankException("Internal database error. Please try again later.");
            return False;
        
        self.refresh_tank_list();
        
        return True;
    
    def DeleteTank(self, tankName, current=None):
        self.refresh_action();
        
        # Must check if the tank exists, and if it belongs to this account.
        results = self.database.do_query(Procedures.tank_exists_under_account(), 
                                         tankName, self.name);
        if len(results) != 1:
            # Tank does not exist.
            Log.quick_log("User %s tried to delete a tank that doesn't exist or belong to him: %s" % (
                          self.name, tankName));
            raise BadInformationException("That tank does not exist.");
        
        changed = self.database.do_insert(Procedures.delete_tank(), tankName);
        
        if not changed:
            # Nothing was changed: database error.
            Log.log_print(Utils.filename_today(), "players", 
                "User %s couldn't delete tank: DB error: %s -- " \
                "data: name=%s" % (self, self.database.get_last_error(), tankName));
            raise VTankException("Internal database error. Please try again later.");
        
        self.refresh_tank_list();
        
        return True;
    
    def SelectTank(self, tankName, current=None):
        self.refresh_action();
        
        # Check if tank exists.
        if tankName not in self.tank_list.keys():
            Log.quick_log("User %s tried to select non-existent tank: %s." % (self.name, tankName));
            raise BadInformationException("Tank %s does not exist." % tankName);
        
        self.active_tank = self.tank_list[tankName];
        
        self.report("%s is using his tank, %s: weapon=%i" % (self.name, self.active_tank.name, 
                                                             self.active_tank.weaponID));
        
        return True;
    
    def GetGameServerList(self, current=None):
        self.refresh_action();
        
        world = World.get_world();
        return world.get_game_server_list();
    
    def RequestJoinGameServer(self, server, current=None):
        self.refresh_action();
        
        if not self.active_tank:
            raise VTankException("You must select a tank first!");
        
        world = World.get_world();
        game = world.get_game_server_by_name(server.name);
        if not game:
            self.report("Tried to get a game server by the name of \"%s\", "\
                "but it didn't exist." % (server.name));
            raise PermissionDeniedException("That server is not available.");
        # Generate a key that would be unique.
        hash_value = "!!ASDFJKL:%s%s%s" % (str(random.randint(0, 10000)), server.name, self.name);
        
        key = sha1(hash_value).hexdigest();
        joined = True;
        try:
            if not game.add_player(key, self.name, self.userlevel, self.active_tank):
                self.report("%s tried to join %s, but was already in a game." % (
                    str(self), str(game)));
                joined = False;
        except Ice.Exception, e:
            self.report("Game server %s disconnected forcefully." % server.name);
            world.kick_user_by_name(server.name);
            
            raise PermissionDeniedException("That server is not available.");
        
        if not joined:
            raise PermissionDeniedException("You are already in a game!");

        self.set_playing_game(True, game);
        return key;
    
    #def GetWeaponList(self, current=None):
    #   self.refresh_action();
        
    #    manager = Equipment_Manager.get_manager();
    #    return manager.get_weapon_list().values();
    
    def GetUtilitiesList(self, current=None):
        self.refresh_action();
        
        manager = Equipment_Manager.get_manager();
        return manager.get_utilities_list().values();
    
    def DownloadMap(self, mapName, current=None):
        self.refresh_action();
        
        self.report("%s wants to download the map, %s." % (self.name, mapName));
        map = Map_Manager.get_manager().get_map_by_filename(mapName);
        if not map:
            raise BadInformationException("That map does not exist.");
        
        vtank_map = VTankObject.Map();
        tiles = [];
        for tile in map.tiles:
            tiles.append(VTankObject.Tile(tile.tile_id, tile.object_id, tile.event_id, 
                tile.collision, tile.height, tile.type, tile.effect));
        
        vtank_map.version = map.version;
        vtank_map.title = map.get_title();
        vtank_map.filename = mapName;
        vtank_map.width = map.map_width;
        vtank_map.height = map.map_height;
        vtank_map.tileData = tiles;
        vtank_map.supportedGameModes = map.game_modes;
        
        return vtank_map;
    
    def GetMapList(self, current=None):
        self.refresh_action();
        
        return Map_Manager.get_manager().get_map_list();
    
    def HashIsValid(self, mapFileName, hash, current=None):
        self.refresh_action();
        
        return Map_Manager.get_manager().check_hash(mapFileName, hash);
    
    def SendFeedback(self, topic, message, current=None):
        self.refresh_action();
        
        self.report("Feedback from %s: [%s]: %s" % (self.name, topic, message));
    
