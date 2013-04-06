###########################################################################
# \file Map_Manager.py
# \brief Manages the maps.
# \author (C) Copyright 2008 by Vermont Technical College
###########################################################################

import Procedures;
from Map import Map, Tile;
import Log;
import sys;
import World;

# How many maps (in bytes) the map manager stores at once.
CACHE_LIMIT = 2097152;

class Map_Manager:
    """
    Store and cache maps retrieved from the database.
    """
    # Map filename=>Map object
    maps = {};
    size = 0;
    
    def __init__(self, config_obj, database_obj, reporter = None):
        """
        Initialize the map manager. This will automatically read in maps from the database.
        @param config_obj Configurations for VTank.
        @param database_obj Database object for database communication.
        @param reporter Optionally include a debug reporter for debug output.
        """
        self.config     = config_obj;
        self.database   = database_obj;
        self.reporter   = reporter;
        
        # Unset the constructor so that it cannot be called again.
        Map_Manager.__init__ = Map_Manager.__nomore__;
        
        self.refresh_map_list();
    
    def __nomore__(self, a, b):
        """
        Secondary constructor created once this class is already instantiated.
        Prevents more than one instance from running.
        """
        return None;
    
    def report(self, message):
        """
        Send a message to the classes reporter, if one is set.  If one is not set, it does
        nothing.
        @param message Message to print to the screen.
        """
        if self.reporter: 
            self.reporter(message);
            
    def refresh_map_list(self):
        """
        This obtains a list of maps from the database. While it records all of the map
        names, it does not necessarily cache the maps. Map caching is dealt with by
        the rest of the class.
        """
        self.report("Refreshing maps...");
        
        self.size = 0;
        self.maps = {};
        results = self.database.do_query(Procedures.get_map_list());
        if results == None:
            error = str(self.database.get_last_error());
            self.report("Database error: %s." % error);
            raise RuntimeError, error; 
        if not len(results):
            # No maps.
            self.report("No maps to download.");
            self.maps = {};
            return;
        
        for set in results:
            # [0]filename, [1]filesize
            filename = set[0];
            filesize = set[1];
            if self.size + filesize > CACHE_LIMIT:
                # Do not download map: it breaks the cache limit.
                self.maps[filename] = None;
            else:
                # Download map.
                downloaded_map = self.download_map(filename);
                if not downloaded_map:
                    self.report("Didn't download map: %s." % filename);
                    continue;
                
                self.maps[filename] = downloaded_map;
                self.size += filesize;
            
            self.report("Added map: %s (size=%i, totalsize=%i)." % (
                filename, filesize, self.size));
                
        World.get_world().update_game_server_maps();
        
    def download_map(self, filename):
        """
        Download a map.
        @param filename Name of the map.
        @return Map object on success, None on error.
        """
        # [0]filename, [1]title, [2]filesize, [3]map_data
        map_data = self.database.do_query(Procedures.get_map(), filename);
        map = Map(self.reporter);
        try:
            if not map.read_map(map_data[0][3]):
                raise Exception, "Unknown corruption error." % filename;
        except Exception, e:
            self.report("Map file %s is corrupted (%s). Discarding." % (filename, str(e)));
            Log.quick_log("The map %s is corrupted: %s." % (filename, str(e)));
            return None;
        
        return map;
        
    def get_map_list(self):
        """
        Returns a list of map filenames.
        """
        return self.maps.keys();
        
    def get_map_by_filename(self, filename):
        """
        Get map data given it's filename.
        @param filename Name of the map.
        """
        if filename not in self.maps:
            return None;
        
        map = self.maps[filename];
        if map == None:
            # Map is not cached.
            map = download_map(filename);
            if not map:
                return None;
        
        return map;
    
    def check_hash(self, filename, hash):
        """
        Check if the given hash is a valid one. The hash is valid if it matches the value
        exactly of the hash of the map in question.
        @param filename Name of the map.
        @param hash Hash to test.
        @return True if the hash is valid, False otherwise.
        """
        # TODO: Cache the hash values, if nothing else.
        if filename not in self.maps:
            # The check fails because the map doesn't exist.
            return False;
        
        map = self.maps[filename];
        if map == None:
            # Map is not cached.
            map = download_map(Filename);
            if not map:
                # The map doesn't exist in the database.
                self.report("Tried to download map %s but it didn't exist in the database!" % filename);
                return False;
        
        # Check if the map's hash matches the given hash and return the conclusion.
        return map.checksum() == hash;
    
    def save(self, filename, map_object):
        """
        Save a map object in the database.
        @param filename Name of the file.
        @param map_object Map to save.
        @return True if successful.
        """
        result = self.database.do_insert(Procedures.new_map(), 
            map_object.title, filename, map_object.filesize, map_object.get_raw_data());
        
        if result == None:
            error = str(self.database.get_last_error());
            self.report("Database insert failed: %s." % error);
            Log.quick_log("Database insert failed: %s." % error);
            return False;
        
        if result <= 0:
            error = str(self.database.get_last_error());
            self.report("Couldn't save map %s: probably already exists. (error=%s)" % (
                filename, error));
            Log.quick_log("Couldn't save map %s: probably already exists. (error=%s)" % (
                filename, error));
            return False;
        
        self.refresh_map_list();
        
        return True;
    
    def delete(self, filename):
        """
        Delete a map from the database.
        @param filename Name of the map to remove.
        @return True if the map was removed without issue.
        """
        result = self.database.do_insert(Procedures.delete_map(), filename);
        if result <= 0:
            # No rows affected.
            error = str(self.database.get_last_error());
            self.report("Couldn't delete map %s. (error=%s)" % (
                filename, error));
            Log.quick_log("Couldn't save map %s. (error=%s)" % (
                filename, error));
            return False;
        
        self.refresh_map_list();
                
        return result == 1;

global map_manager;

def initialize(config_obj, database_obj, reporter):
    """
    Set up a new map manager object.
    @param config_obj Configuration object to use.
    @param database_obj Database object to use.
    @param reporter Debug reporter, if one is available.
    @return Map manager object.
    @raise RuntimeError Raised when this was already initialized. 
    """
    global map_manager;
    temp_manager = Map_Manager(config_obj, database_obj, reporter);
    if temp_manager == None:
        raise RuntimeError("Cannot create new map manager: One already exists.");
    map_manager = temp_manager;
    return map_manager;

def get_manager():
    """
    Static access to the map manager.
    @return Map manager object, or None if one has not been created.
    """
    global map_manager;
    try:
        return map_manager;
    except NameError, e:
        # Thrown if 'map_manager' has not been declared.
        map_manager = None;
    
    return map_manager;
