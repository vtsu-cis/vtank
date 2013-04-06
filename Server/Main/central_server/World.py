###########################################################################
# \file World.py
# \brief Statically stored information about in-game people.
# \author Copyright 2009 by Vermont Technical College
###########################################################################

import Config;
import Log;
import Procedures;
import Equipment_Manager;
import Map_Manager;
import stackless;
import Ice;
import sys;
from VTankObject import ServerInfo, GameMode;
from Admin import OnlineUser;
from AuthI import AuthI;
from Database import Database;
from Client_Tracker import Client_Tracker;
from Color import Color, to_color;
from Echelon import Force_Exit_Exception;
from Client_Types import *;
from Exceptions import *;

global world;

def initialize_world(config_file, reporter):
    """
    Set up a new world object.
    """
    global world;
    temp_world = World_Handler(config_file, reporter);
    if temp_world == None:
        raise RuntimeError("Cannot create new World: One already exists.");
    world = temp_world;

def get_world():
    """
    Static access to the world handler.
    """
    global world;
    try:
        return world;
    except NameError, e:
        # Thrown if 'world' has not been declared.
        world = None;
    
    return world;

class World_Handler:
    """
    The purpose of the World Handler is to provide a central location for data transactions.
    This is where clients are accepted and handled.  This also provides the main tasklet that
    supervises the stackless scheduler flow.
    """
    running         = True;
    client_tracker  = None;
    database        = None;
    
    # Define Ice communication objects.
    communicator    = None;
    adapter         = None;
    
    def __init__(self, config_file = "config.cfg", reporter = None):
        """
        Construct a world handler instance.  Also reads the configuration file and
        creates new handler objects that belong under main tasklet observation.
        @param config_file Configuration file to read.  The default file name is "config.cfg".
        """
        self.reporter = reporter;
        
        # Read the configuration file.
        self.config = Config.VTank_Config();
        if not self.config.read(config_file):
            raise Force_Exit_Exception, "Reading configuration file failed!";
        
        # Create a database object.
        self.database = Database(self.config.database_host, self.config.database_user, 
            self.config.database_passwd, self.config.database_name);
        
        if not self.database.connect():
            # Connection to DB failed! This is considered a fatal error.
            raise Force_Exit_Exception, "Unable to connect to database: " \
                + str(self.database.get_last_error());
                
        Equipment_Manager.set_manager(Equipment_Manager._Manager(self.database));
        
        self.initialize_ice();
        
        self.client_tracker = Client_Tracker(self.config, self.database, reporter);
        
        # Prevent constructor from being called again.
        World_Handler.__init__ = World_Handler.__nomore__;
    
    def __del__(self):
        """
        Destructor. Called when this class is cleaned up by the garbage collector.
        """
        try:
            if self.communicator:
                self.communicator.destroy();
        except Ice.Exception:
            pass;
    
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
    
    def get_game_server_list(self):
        """
        Compile a list of game servers and return it.
        """
        server_list = [];
        clients = self.client_tracker.get_everyone();
        for client in clients.values():
            if client.get_type() == THEATRE_CLIENT_TYPE:
                c = client.get();
                if c.usingGlacier2:
                    # Game server uses Glacier2. Use host/port of Glacier2 server.
                    host = c.glacier2Host;
                    port = c.glacier2Port;
                else:
                    # Game server does not use glacier2.
                    host = c.get_remote_address()[0];
                    port = c.port;
                
                server_list.append(ServerInfo(host, port, c.name, c.approved, c.usingGlacier2,
                    c.get_player_count(), c.get_player_limit(), c.current_map,
                    c.current_mode));
                
        return server_list;
    
    def update_game_server_maps(self):
        """
        Get the list of game play servers and notify them of the new map list.
        """
        clients = self.client_tracker.get_everyone();
        for client in clients.values():
            if client.get_type() == THEATRE_CLIENT_TYPE:
                c = client.get();
                c.get_callback().UpdateMapList(Map_Manager.get_manager().get_map_list());
                
    def update_game_server_utilities(self):
        """
        Get the list of game servers and notify them of the current utility list.
        """
        clients = self.client_tracker.get_everyone();
        for client in clients.values():
            if client.get_type() == THEATRE_CLIENT_TYPE:
                c = client.get();
                c.get_callback().UpdateUtilities(Equipment_Manager.get_manager().get_utilities_list());
    
    def get_game_server_by_name(self, name):
        """
        Get a game server by it's name.
        """
        return self.client_tracker.get_game_server_by_name(name);
    
    def get_user_count(self):
        """
        Get a count for the users.
        """
        return self.client_tracker.size();
    
    def get_full_userlist(self):
        """
        Construct an array of OnlineUser structs.
        """
        userlist = [];
        everyone = self.client_tracker.get_everyone();
        for client in everyone.values():
            userlist.append(OnlineUser(client.get_name(), client.get_type(), 
                                       client.get().userlevel, client.get().is_playing_game(), True));
        
        return userlist;
    
    def get_complete_userlist(self):
        """
        Get the full list of users who are online and offline.
        @return Array of OnlineUser structs.
        """
        userlist = self.get_full_userlist();
        results = self.database.do_query(Procedures.get_userlist());
        if results == None:
            self.report("Internal database error: %s." % str(self.database.get_last_error()));
            raise VTankException("Internal database error. Please report this immediately.");
            
        for row in results:
            username = row[0];
            email = row[1];
            userlevel = row[5];
            
            exists = False;
            for existing_user in userlist:
                if existing_user.username.lower() == username.lower():
                    exists = True;
                    break;
            
            if not exists:
                userlist.append(OnlineUser(username, "Offline", userlevel, False, False));
            
        return userlist;
    
    def kick_user_by_name(self, name):
        """
        Kick a username off given his name.
        @param name Name of the user to kick.
        @return True if the user was found and kicked.
        """
        everyone = self.client_tracker.get_everyone();
        for client in everyone.values():
            if name == client.get_name():
                client.get().destroy();
                
                return True;
            
        return False;
    
    def get_communicator(self):
        """
        Grants public access to this object's communicator.
        """
        return self.communicator;
    
    def get_config(self):
        """
        Public access to the configuration.
        @return Config object.
        """
        return self.config;
    
    def get_database(self):
        """
        Public access to the database.
        """
        return self.database;
    
    def get_tracker(self):
        """
        Public access to the client tracker.
        """
        return self.client_tracker;
    
    def initialize_ice(self):
        """
        Initialize the Ice module and activate the adapter.
        """
        self.communicator = Ice.initialize([
            "--Ice.ThreadPool.Server.SizeMax=20", 
            "--Ice.ThreadPool.Server.Size=5",
            "--Ice.MessageSizeMax=20971520",
            "--Ice.ThreadPool.Client.SizeMax=5",
            "--Ice.ThreadPool.Client.Size=1"
        ]);
        self.adapter = self.communicator.createObjectAdapterWithEndpoints(
            "Main", "tcp -p %i" % self.config.server_port);
            
        self.auth           = AuthI(self.config, self.database, self.reporter);
        self.sessionFactory = Client_Tracker(self.config, self.database, self.reporter);
        
        self.adapter.add(self.auth, self.communicator.stringToIdentity("Auth"));
        self.adapter.add(self.sessionFactory, 
                         self.communicator.stringToIdentity("SessionFactory"));
        
        self.adapter.activate();
        
        self.report("Adapter activated. Ice has been initialized.");
    
