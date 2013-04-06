###########################################################################
# \file MTGSession.py
# \brief Properties of a spawned main-to-game session.
# \author Copyright 2009 by Vermont Technical College
###########################################################################

import Ice;
import World;
import random;
import hashlib;
import Map_Manager;
import Client_Types;
import MainToGameSession;
import VTankObject;
import Procedures;
import Map;
from time import time;
from math import sqrt, floor;
from Base_Servant import Base_Servant;

class MTGSession(MainToGameSession.MTGSession, Base_Servant):
    """
    Main-to-game server session. Allows the game server to send
    communications to the main server.
    """
    def __init__(self, name, port, approved, usingGlacier2, glacier2Host, glacier2Port,
                 client_prx, ip_string, reporter = None, threshold = 300):
        """
        Initialize a main-to-game session.
        @param name Server's name.
        @param port Port that the server listens for clients on.
        @param approved True if the server is 'approved' by this server, otherwise False.
        @param usingGlacier2 [bool] True if clients must use Glacier2 to connect.
        @param glacier2Host Host name of the Glacier2 router.
        @param glacier2Port Port number that the Glacier2 router listens on.
        @param client_prx Proxy to the client's callback.
        @param ip_string IP obtained from the current.con.toString() method.
        @param reporter Debug reporter. Optional.
        """
        Base_Servant.__init__(self, ip_string, Client_Types.THEATRE_CLIENT_TYPE, 0, reporter);
        
        self.name = name;
        self.port = port;
        self.config = World.get_world().get_config();
        self.database = World.get_world().get_database();
        self.approved = approved;
        self.client_prx = MainToGameSession.ClientSessionPrx.uncheckedCast(client_prx.ice_timeout(8000));
        self.usingGlacier2 = usingGlacier2;
        self.glacier2Host = glacier2Host;
        self.glacier2Port = glacier2Port;
        
        self.player_limit = 100;
        self.forced_limit = False;
        self.player_list = {};
        self.threshold = threshold;
        self.current_map = "";
        self.current_mode = VTankObject.GameMode.DEATHMATCH;
    
    def __str__(self):
        return Base_Servant.__str__(self);
    
    def get_callback(self):
        """
        Retrieve the client callback function.
        """
        return self.client_prx;
    
    def get_player_limit(self):
        """
        Retrieve the number of players allowed to join this game server.
        """
        return self.player_limit;
    
    def get_player_count(self):
        """
        Get the number of players currently online.
        """
        return len(self.player_list);
    
    def remove_player_if_exists(self, name):
        """
        Remove a player from the player list (and from the game server) if the user exists.
        @param name Name of the person to remove.
        """
        if name not in self.player_list:
            return False;
        
        del self.player_list[name];
        try:
            self.get_callback().RemovePlayer(name);
            
            return True;
        except Ice.Exception, e:
            self.report("%s disconnected during a RemovePlayer request. "\
                "Exception details: %s." % (str(self), str(e)));
            self.destroy();
            
        return False;
    
    def add_player(self, key, name, userlevel, tank):
        """
        Add a player to the game server.
        @param key Key to assign the player.
        @param name Username of the player (not of the tank).
        @param userlevel Userlevel of the player.
        @param tank Set of tank attributes.
        @return True if the player was added without issue. False if the player already
        exists on the game server and was not added.
        """
        if tank.name in self.player_list:
            return False;
        
        self.player_list[tank.name] = tank;
        self.client_prx.AddPlayer(key, name, userlevel, tank);
        
        return True;
    
    def force_player_limit(self, limit):
        """
        Force the game server to accept a certain amount of players.
        @param limit Number of game servers allowed.
        """
        self.limit_forced = True;
        self.player_limit = limit;
        try:
            self.client_prx.ForceMaxPlayerLimit(limit);
        except Ice.Exception, e:
            self.report("%s disconnected during a ForceMaxPlayerLimit request. "\
                "Exception details: %s." % (str(self), str(e)));
            self.destroy();
        
    def unforce_player_limit(self):
        """
        Allow the game server to enforce it's own limit.
        """
        self.limit_forced = False;
        
    def keep_alive(self):
        """
        Send a keep-alive packet to the server to attempt to keep the connection open.
        """
        try:
            self.client_prx.KeepAlive();
        except Ice.Exception, e:
            self.report("%s disconnected during a KeepAlive request. Exception details: %s." % (
                str(self), str(e))); 
            self.destroy();
            
    def award_points(self, statistics):
        """
        Gives a player ranking points based on the statistics of a game.
        @param statistics: A statistics object (kills, assists, deaths, objectives) 
        """
        
        if statistics.kills == 0 and statistics.assists == 0 and \
            statistics.objectivesCaptured == 0 and statistics.objectivesCompleted:
                return;
        
        totalPoints = 0;
        
        totalPoints += statistics.kills * 10;
        totalPoints += statistics.assists * 5;
        totalPoints += statistics.objectivesCompleted * 20;
        totalPoints += statistics.objectivesCaptured * 20;
        
        accountName_query_result = self.database.do_query(Procedures.get_account_from_tank(), statistics.tankName);  
        
        if accountName_query_result == None:            
            self.report("Error:  Could not find account for tank %s to calculate points" % statistics.tankName);
            return;
        elif len(accountName_query_result) < 1:
            self.report("Error:  Could not find account for tank %s to calculate points" % statistics.tankName); 
            return;
        else:
            accountName = accountName_query_result[0][0];       
            
        points_query_result = self.database.do_query(Procedures.get_account_points(), accountName);
   
        if points_query_result == None:            
            self.report("Error:  Failed to retrieve points for account %s from database" % accountName);
            return;
        elif len(points_query_result) < 1:
            self.report("Error:  Failed to retrieve points for account %s from database" % accountName); 
            return;
        else:
            accountPoints = points_query_result[0][0];
             
        accountPoints += totalPoints;
        
        points_insert_result = self.database.do_insert(Procedures.update_account_points(), accountPoints, accountName);        
        if points_insert_result == -1:
                # Failure.
                error = self.database.get_last_error();
                self.report("ERROR: Could not update points for player %s: %s." % (
                    statistics.tankName, str(error)));
    
    #
    # The following functions are overridden from a generated file.
    # To view the documentation of these functions, please see:
    # MainToGameSession.ice
    #
    
    def destroy(self, current=None):
        World.get_world().get_tracker().remove(self.get_id());
    
    def KeepAlive(self, current=None):
        self.refresh_action();
    
    def SetMaxPlayerLimit(self, limit, current=None):
        self.refresh_action();
        if not self.forced_limit:
            self.player_limit = limit;
    
    def PlayerLeft(self, key, current=None):
        self.refresh_action();
        if key in self.player_list:
            del self.player_list[key];
        
    def SendStatistics(self, statistics, current=None):
        self.refresh_action();
        
        if len(statistics) == 0:
            return;
        
        # TODO: Only allow for approved servers.
        
        for statistic in statistics:
            if statistic.kills == 0 and statistic.assists == 0 and statistic.deaths == 0 and\
                statistic.objectivesCompleted == 0 and statistic.objectivesCaptured == 0:
                continue;
            
            statistics_query = self.database.do_query(Procedures.get_tank_statistics(), statistic.tankName);
            
            if statistics_query == None:            
                self.report("Error:  Failed to retrieve current statistics for tank %s from database" % statistic.tankName);
                return;
            elif len(statistics_query) < 1:
                self.report("Error:  Failed to retrieve current statistics for tank %s from database" % statistic.tankName);
                return;
            else:
                current_statistics = statistics_query[0];
                
            result = self.database.do_insert(Procedures.update_statistics(), 
               str(int(current_statistics[1]) + statistic.kills), 
               str(int(current_statistics[2]) + statistic.assists), 
               str(int(current_statistics[3]) + statistic.deaths), 
               str(int(current_statistics[4]) + statistic.objectivesCompleted), 
               str(int(current_statistics[5]) + statistic.objectivesCaptured),
               statistic.tankName);            
            
            if result == -1:
                # Failure.
                error = self.database.get_last_error();
                self.report("ERROR: Could not update statistics for player %s: %s." % (
                    statistic.tankName, str(error)));
            else:
                self.award_points(statistic); 
                
            
    def GetRank(self, tankName):
        """
        Get the rank of a given tank.
        @param tankName: The name of the tank in question.
        @return: His current rank. 
        """
        points = self.GetPointsByTank(tankName);
        
        rankNumber = self.get_rank_from_points(points);
        return int(floor(rankNumber));
        
        
    def GetRequiredPointsForRank(self, rankNumber):
        """
        Looks at the player's current rank and computes the number of points needed for the next 
        rank.
        """
        return (10*rankNumber)**2;
    
    def get_rank_from_points(self, points):
        """
        Get current rank from a player's current number of points
        @param points: The number of points he has
        @return:  His rank (float)
        """
        
        return sqrt(points)/10;
    
    def GetPointsByTank(self, tankName):
        """
        Get the total points a tank has from the database.
        @param tankName: The name of the tank in question.
        @return: The current number of points he has earned.         
        """
        result = self.database.do_query(Procedures.get_tank(), tankName);
        
        if result == -1:
            error = self.database.get_last_error();
            self.report("ERROR: Failed to retrieve data for rank computation for tank %s: %s" % (tankName, str(error)));
        
        points = 0;    
        for tank in result:
            points = tank[5];
        
        return points;
        
    def get_points_for_next_rank(self, currentPoints):
        """
        Get the number of points a player needs for his next rank-up.
        @param currentPoints:  His current number of points.
        @return: The number of points he needs for the next rank. 
        """                        
        
        rankNumber = int(floor(self.get_rank_from_points(currentPoints)));
        nextRank = rankNumber+1;
        requiredPointsForNextRank = self.GetRequiredPointsForRank(nextRank);
        
        return requiredPointsForNextRank-currentPoints;
    
    def SetCurrentMap(self, mapName, current=None):
        self.refresh_action();
        
        self.current_map = mapName;
        
    def SetCurrentGameMode(self, mode, current=None):
        self.refresh_action();
        
        self.current_mode = mode;
        
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
        
    def HashIsValid(self, mapFileName, hash, current=None):
        self.refresh_action();
        
        return Map_Manager.get_manager().check_hash(mapFileName, hash);
