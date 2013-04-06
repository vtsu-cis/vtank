###########################################################################
# \file MapEditorSessionI.py
# \brief Session instances created for map editor users.
# \author Copyright 2009 by Vermont Technical College
###########################################################################

import Ice;
import World;
import MapEditor;
import Map_Manager;
import Client_Types;
import Map as _Map;
from Base_Servant import Base_Servant;
from Exceptions import *;
from time import time;
from VTankObject import Map, Tile;

class MapEditorSessionI(MapEditor.MapEditorSession, Base_Servant):
    """
    Implements the MapEditor.MapEditorSession interface.
    """
    def __init__(self, name, ip_string, userlevel, reporter = None, threshold = 300):
        """
        Create a new map editor session.
        @param username The editor's username.
        @param database_obj SQL Database object.
        @param reporter Debug reporter. Default is None.
        @param threshold How long to wait (in seconds) before a client is kicked for inactivity.
        """
        Base_Servant.__init__(self, ip_string, Client_Types.MAPEDITOR_CLIENT_TYPE, userlevel, reporter);
        
        self.name       = name;
        self.database   = World.get_world().get_database();
        self.threshold  = threshold;
    
    def __str__(self):
        return Base_Servant.__str__(self);
    
    #
    # The following functions are overridden from a generated file.
    # To view the documentation of these functions, please see:
    # MapEditorSession.ice
    #
    
    def destroy(self, current=None):
        World.get_world().get_tracker().remove(self.get_id());
    
    def KeepAlive(self, current=None):
        self.refresh_action();
    
    def GetMapList(self, current=None):
        return Map_Manager.get_manager().get_map_list();
    
    def DownloadMap(self, mapName, current=None):
        self.report("%s wants to download the map, %s." % (self.name, mapName));
        map = Map_Manager.get_manager().get_map_by_filename(mapName);
        if not map:
            raise BadInformationException("That map does not exist.");
        
        vtank_map = Map();
        tiles = [];
        for tile in map.tiles:
            tiles.append(Tile(tile.tile_id, tile.object_id, tile.event_id, tile.collision,
                              tile.height, tile.type, tile.effect));
        
        vtank_map.title = map.get_title();
        vtank_map.filename = mapName;
        vtank_map.width = map.map_width;
        vtank_map.height = map.map_height;
        vtank_map.tileData = tiles;
        vtank_map.supportedGameModes = map.game_modes;
        
        return vtank_map;
    
    def UploadMap(self, map, current=None):
        self.report("%s wants to upload the map, %s." % (self.name, map.filename));
        # Validate.
        if not map.title:
            raise BadInformationException("Map is corrupted: Title cannot be blank.");
        
        if map.width <= 0 or map.height <= 0:
            raise BadInformationException("Map is corrupted: width/height <= 0.");
        
        if not len(map.tileData):
            raise BadInformationException("Map is corrupted: has no tiles!");
        
        map_obj = _Map.Map(self.reporter);
        map_obj.create_from_ice_object(map);
        
        if not Map_Manager.get_manager().save(map.filename, map_obj):
            raise BadInformationException("Database upload failed: error unknown.");
    
    def RemoveMap(self, mapName, current=None):
        if not Map_Manager.get_manager().delete(mapName):
            raise BadInformationException("Map does not exist.");
