##########################################################################
# FILE     : Client_Tracker.py
# SUBJECT  : Track a list of players connected to the server.
# AUTHOR   : (C) Copyright 2008 by Vermont Technical College
###########################################################################
from SendMail import MailMessage
import Equipment_Manager;
import Map_Manager;
import Procedures;
import threading;
import stackless;
import Glacier2;
import Utils;
import World;
import time;
import Log;
import Ice;
import re;

from MapEditorSessionI import MapEditorSessionI;
from AdminSessionI import AdminSessionI;
from MainSessionI import MainSessionI;
from MTGSession import MTGSession;
from Color import Color, to_color;
from Admin import OnlineUser;
from Client_Types import *;
from HashUtility import hash_password, check_password;
from UserLevel import *;
from time import time;
import PlayerWatchdog

from Map import Map;

# Slice
from Exceptions import *;
from VTankObject import *;
import Main;
import MapEditor;
import Admin;
import IGame;
#import Janitor;
import GameSession;
import MainToGameSession;

class Client:
    """
    Track an instance of a client. The Client class allows the programmer to assign a 
    client type to the client object itself. That makes it easier to track exactly
    what kind of client is being stored.
    """
    def __init__(self, client_type, client_object, username):
        """
        Create the Client object.
        @param client_type Type of client (see Client_Types.py).
        @param client_object Ice servant which can receive messages.
        @param username Client's username.
        """
        if client_type not in CLIENT_TYPES:
            raise RuntimeError("Invalid client type: %s (see Client_Types.py)." % client_type);
        
        self.type = client_type;
        self.obj = client_object;
        self.name = username;
        
    def __str__(self):
        return self.obj.__str__();
        
    def __eq__(self, other):
        """
        Check if two client objects are "equal". Two Client objects are considered equal
        if they share the same username, and if they share the same client type.
        @param other Other object to compare.
        @return True if the two client objects are equal.
        """
        if not isinstance(other, Client):
            raise RuntimeError("Cannot compare non-Client object.");
        
        return self.get_name() == other.get_name() and self.get_type() == other.get_type();
    
    def __ne__(self, other):
        """
        Check if two client objects are "unequal". Two Client objects are considered unequal
        if they do not have the same username OR if they do not share the same client type.
        @param other Other object to compare.
        @return True if the two client objects are unequal.
        """
        return not self.__eq__(other);
        
    def get(self):
        """
        Get the object stored within this client.
        @return Client object of type 'self.type'.
        """
        return self.obj;
    
    def get_name(self):
        """
        Helper for quickly grabbing the name of this client.
        @return Name of the client.
        """
        return self.name;
    
    def get_type(self):
        """
        Get the type of object assigned to this client.
        @return String representing the client type (see Client_Types.py).
        """
        return self.type;

class Client_Tracker(Main.SessionFactory):
    """
    Track a client's data as they connect.
    """
    # Define members.
    clients         = {};
    config          = None;
    database        = None;
    _lock           = threading.RLock();
    
    def __init__(self, config, database, reporter = None):
        """
        The constructor will create the tasklets needed for this class, but will not run the scheduler.
        @param config Configuration object.
        @param database Connection to the database for client handling.
        @param adapter Adapter 
        @param reporter Function that spools output. Has a string as a parameter.  Used for
        debugging. By default, the reporter is None (no output).
        """
        self.config             = config;
        self.database           = database;
        self.reporter           = reporter;

        stackless.tasklet(self.kick_expired)();
        stackless.tasklet(self.ping_game_servers)();
        
    def __getitem__(self, id):
        """
        The overloaded __getitem__ function will return a single player from the tracker.
        This function is invoked by the programmer using: client_list[id].
        @param id The primary key for each player is their temporary ID.
        @return Client object.
        """
        with _lock.acquire():
            return self.clients[id];
    
    def __delitem__(self, client_id):
        """
        Handle operation when the programmer invokes the "del" keyword on an item.
        @param client_id ID of the client being deleted.
        """
        self.remove(client_id);
        
    def __len__(self):
        """
        Handle len() function calls.
        @return Size of the client list.  See: size()
        """
        return self.size();
    
    def report(self, message):
        """
        Send a message to the classes reporter, if one is set.  If one is not set, it does
        nothing.
        @param message Message to print to the screen.
        """
        if self.reporter: 
            self.reporter(message);
    
    def add(self, session_id, client):
        """
        Add a client to the list of clients.  Will overwrite any clients that already exist
        with that particular address.
        @param session_id Session ID generated by Ice.
        @param client Client object to add.
        """
        with self._lock:
            for key in self.clients.keys():
                temp_client = self.clients[key];
                if client == temp_client:
                    # User is already logged in. Kick him off.
                    # Usually if a user is already logged on, they were forcefully
                    # disconnected and the server hasn't timed them out yet.
                    self.report("Kicked off duplicate client %s." % temp_client);
                    
                    self.remove(key);
                    
                    break;         
        
            self.clients[session_id] = client;
            self.feed_watchdog(client);
            
            self.report("Added client %s (%s). %i client(s) now online." % (
                client.get(), client.get_type(), self.size()));

    def feed_watchdog(self, client):        
        if PlayerWatchdog.needs_mail(client.get_name()):
            self.send_player_notification(client);

    def send_player_notification(self, client):
        #sender, subject, message, to
        recipients = "cbeattie@summerofsoftware.org, asibley@summerofsoftware.org, "\
                        "msmith@summerofsoftware.org";

        message = "A player by the name of %s has logged onto VTank. \n\n Join him if you have time!" % client.get_name()\

        mailMessage = MailMessage(client.get_name(), "VTank Player Notification", message, recipients);
        success = mailMessage.mail();
        PlayerWatchdog.add_player(client.get_name())
        print "Sending mail for " + client.get_name()
        print "Current watchdog list: "
        for obj in PlayerWatchdog.watchdog_player_list.objs:
		print obj

        if not success:
            self.report("Failed to send player notification email for user %s" % (client.get_name()));


    def remove(self, id):
        """
        Remove a client from the list.
        @param id ID of the client.
        """
        with self._lock:
            if id in self.clients:
                person = str(self.clients[id]);
                c = self.clients[id].get();
                name = c.name;
                if c.is_playing_game():
                    server = c.get_game_server();
                    tank   = c.active_tank;
                    if not server:
                        self.report("Warning: Client %s was playing a game, but the game server "\
                            "was null." % (person));
                    else:
                        server.remove_player_if_exists(tank.name);
                
                try:
                    world = World.get_world();
                    world.adapter.remove(id);
                except Ice.Exception, e:
                    self.report("Warning: Unable to remove client from adapter: %s." % (str(e)));
                
                del self.clients[id];
                
                self.report("Removed %s." % person);
                return True;
        
            return False;
    
    def contains(self, client):
        """
        Check if a client exists in the tracker.
        @param client Client object to check.
        """
        with self._lock:
            for current in self.clients.values():
                if client == current:
                    return True;
                
            return False;
    
    def get_client(self, id):
        """
        Return the client object of a given person.
        @param id ID number of the client.
        @return Client object, or None if not found.
        """
        with self._lock:
            if id in self.clients:
                return self.clients[id];
        
            return None;
    
    def get_everyone(self):
        """
        Return the entire dictionary list of players.
        @return Dictionary.
            Key: Account name
            Value: Account object
        """
        with self._lock:
            return self.clients;
    
    def size(self):
        """
        Returns the number of connected clients.
        @return An unsigned integer.
        """
        with self._lock:
            return len(self.clients);
    
    ############
    # Tasklets #
    ############
    def kick_expired(self):
        """
        Remove players who have not recently sent a message.
        This is meant to be run as a tasklet. 
        """
        while True:
            current_time = time();
            with self._lock:
                for key in self.clients.keys():
                    client = self.clients[key].get();
                    if current_time - client.get_last_action_time() > client.threshold:
                        self.remove(key);
            
            Utils.sleep_tasklet(5);
            
    def ping_game_servers(self):
        """
        Send a keep alive request to all game servers.
        """
        while True:
            with self._lock:
                for key in self.clients.keys():
                    client = self.clients[key];
                    if client.get_type() == THEATRE_CLIENT_TYPE:
                        client.get().keep_alive();
                        
            Utils.sleep_tasklet(5);
    
    ####################
    # Helper Functions #
    ####################
    def helper_generic_login(self, username, password, required_userlevel = MEMBER):
        """
        This method is a helper method intended to abstract the SQL query-process of
        retrieving account information from the database. It only does basic validation.
        @return Userlevel of the user.
        """
        # Database should return exactly 1 row.
        results = self.database.do_query(Procedures.get_account(), username);
        if results == None:
            self.report("Internal database error: %s." % str(self.database.get_last_error()));
            raise VTankException("Internal database error. Please report this immediately.");
        
        if len(results) != 1:
            # Account does not exist.
            self.report("Bad login attempt for a user, \"%s\" (no account)." % username);
            raise PermissionDeniedException("Bad username/password combination.");
        
        # Extract user-level and password.
        user_level = int(results[0][1]);
        enc_password = results[0][2];
        
        #TODO:  Use the User_Level_Lookup table for this.
        if user_level == -99:
            self.report("%s tried to log in, but is banned." % username);
            raise PermissionDeniedException("Login failed, player is banned.");
        elif user_level == -2:
            self.report("%s tried to log in, buy is suspended." % username);
            raise PermissionDeniedException("Login failed, player is suspended.");
        elif user_level < required_userlevel:
            self.report("%s tried to log in but lacks the user level." % username);
            raise PermissionDeniedException("You lack privileges.");
        
        if not check_password(password, enc_password):
            # Invalid password.
            self.report("Bad login attempt for a user, \"%s\" (bad password)." % username);
            raise PermissionDeniedException("Bad username/password combination.");
        
        return user_level;
    
    def helper_validate_name(self, username):
        """
        Helper method to validate a username in a standard way.
        @param username Username to validate.
        """
        if not username:
            raise BadInformationException("Username and password cannot be empty.");
        
        p = re.compile("[^A-Za-z0-9]");
        if p.search(username):
            # Invalid character.
            raise BadInformationException("Username contains invalid characters.");
    
    def get_game_server_by_name(self, name):
        """
        Get a game server by it's name.
        @param name Name of the server (not IP).
        """
        with self._lock:
            for key in self.clients.keys():
                server = self.clients[key];
                if server.get_type() == THEATRE_CLIENT_TYPE and server.get_name() == name:
                    return server.get();
                
            return None;
        
    def SendErrorMessage(self, username, details, stacktrace, current=None):
        """
        Emails the VTank developers with a stack trace of a client crash.
        @param username: The user who encountered the error
        @param details: A description of the circumstances surrounding the
        error.
        @param stacktrace:  The error's stack trace.     
        """
        #sender, subject, message, to
        recipients = "cbeattie@summerofsoftware.org, asibley@summerofsoftware.org, "\
                     "msmith@summerofsoftware.org, jteasdale@summerofsoftware.org";
                     
        message = "Player %s has encountered an error while playing VTank. \n\n"\
                   "The circumstances were: \n%s \n \n"\
                   "Stack trace: \n%s \n \n" \
                   "Please investigate this issue." % (username, details, stacktrace);
                   
        mailMessage = MailMessage(username, "VTank Crash Report", message, recipients);
        success = mailMessage.mail();
        
        if not success:
            self.report("Failed to send client-side error report email for user %s" % (username));
    
    #
    # The following functions are overridden from a generated file.
    # To view the documentation of these functions, please see:
    # Main.ice - Main::SessionFactory
    #
    
    def VTankLogin(self, username, password, version, current=None):
        if not username or not password or not version:
            raise BadInformationException("Username and password cannot be empty.");
        
        self.helper_validate_name(username);
        user_level = self.helper_generic_login(username, password, MEMBER);
        
        Utils.update_last_login_time(self.reporter, self.database, username);
        
        session = MainSessionI(username, user_level, current.con.toString(), self.reporter);
        
        # Success.
        self.report("%s (%s) logged in." % (session, VTANK_CLIENT_TYPE));
        Log.quick_log("%s logged in as a VTank client." % username);
        
        ice_object = current.adapter.addWithUUID(session);
        id = ice_object.ice_getIdentity();
        
        session.set_id(id);
        
        self.add(id, Client(VTANK_CLIENT_TYPE, session, username));
        
        return Main.MainSessionPrx.uncheckedCast(ice_object);
    '''
    def JanitorLogin(self, username, password, current=None):
        # Check username/password for validity.
        if not username or not password:
            self.report("Bad Janitor login attempt (empty username/password).");
            raise BadInformationException("Username and password cannot be empty.");
        
        self.helper_validate_name(username);
        
        user_level = self.helper_generic_login(username, password, DEVELOPER);
        
        session = JanitorSessionI(username, user_level, current.con.toString(), self.reporter);
        
        ice_object = current.adapter.addWithUUID(session);
        id = ice_object.ice_getIdentity();
        
        session.set_id(id);
        
        self.add(id, Client(JANITOR_CLIENT_TYPE, session, username));
        
        self.report("%s logged in." % session);
        Log.quick_log("%s logged in as a janitor." % username);
        
        return Janitor.JanitorSessionPrx.uncheckedCast(ice_object);
        '''
    
    def MapEditorLogin(self, username, password, current=None):
        # Check name/password for validity.
        if not username or not password:
            self.report("Bad login attempt from map editor (blank username/password).");
            raise BadInformationException("Username and password cannot be empty.");
        
        # These lines throw exceptions if they fail, which is allowed.
        self.helper_validate_name(username);
        user_level = self.helper_generic_login(username, password, DEVELOPER);
        
        session = MapEditorSessionI(username, current.con.toString(), user_level, self.reporter);
        
        ice_object = current.adapter.addWithUUID(session);
        id = ice_object.ice_getIdentity();
        
        session.set_id(id);
        
        self.add(id, Client(MAPEDITOR_CLIENT_TYPE, session, username));
        
        self.report("%s (%s) logged in." % (session, MAPEDITOR_CLIENT_TYPE));
        Log.quick_log("%s logged in as a map editor." % username);
        
        return MapEditor.MapEditorSessionPrx.uncheckedCast(ice_object);
    
    def AdminLogin(self, username, password, version, current=None):
        if not password:
            raise BadInformationException("Bad username or password.");
        
        self.helper_validate_name(username);
        
        self.helper_generic_login(username, password, ADMINISTRATOR);
        
        session = AdminSessionI(username, current.con.toString(), self.reporter);
        
        ice_object = current.adapter.addWithUUID(session);
        id = ice_object.ice_getIdentity();
        
        session.set_id(id);
        
        self.add(id, Client(ADMIN_CLIENT_TYPE, session, username));
        
        self.report("Administrator %s logged in." % session);
        Log.quick_log("%s logged in as an administrator.");
        
        return Admin.AdminSessionPrx.uncheckedCast(ice_object);
    
    def Join(self, servername, secret, port, usingGlacier2, 
             glacier2Host, glacier2Port, client, current=None):
        if client == None:
            # Client callback class cannot be null.
            raise BadInformationException("No callback is set.");
        
        host = Utils.extract_remote_addr(current);
        allowed = self.config.allow_unapproved_game_servers;
        
        self.report("Attempted connection from a game server from %s: %s" % (str(host), servername));
        
        approved = True;
        if host not in self.config.approved:
            # Unapproved game server.
            approved = False;
            
        if not allowed:
            # Unapproved game servers are not allowed.
            Log.quick_log("Server %s tried to log in as a game server but is not allowed." % servername);
            raise PermissionDeniedException("Unauthorized login attempt.");
        
        # Compare the secrets.
        if secret != self.config.server_secret:
            # Secret does not match.
            Log.quick_log("Server %s tried to log in as a game server but had a bad secret." % servername);
            raise PermissionDeniedException("Bad secret.");
        
        session = MTGSession(servername, port, approved, usingGlacier2, glacier2Host, glacier2Port, 
                             client, current.con.toString(), self.reporter);
                             
        ice_object = current.adapter.addWithUUID(session);
        id = ice_object.ice_getIdentity();
        
        session.set_id(id);
        
        self.add(id, Client(THEATRE_CLIENT_TYPE, session, servername));
        
        self.report("%s (%s) logged in." % (session, THEATRE_CLIENT_TYPE));
        Log.quick_log("%s joined as a game server." % servername);
        
        client.UpdateMapList(Map_Manager.get_manager().get_map_list());
        client.UpdateUtilities(Equipment_Manager.get_manager().get_utilities_list());
        
        return MainToGameSession.MTGSessionPrx.uncheckedCast(ice_object);
    
	def HealthMonitorLogin(username, password, current=None):
		if not password:
            raise BadInformationException("Bad username or password.");
			
		self.helper_validate_name(username);
		
		ice_object = current.adapter.addWithUUID(session);
        id = ice_object.ice_getIdentity();
        
		session = HealthMonitorI(username, current.con.toString(), self.reporter);
		
        session.set_id(id);
		
		self.add(id, Client(HEALTHMONITOR_CLIENT_TYPE, session, username));
		
		self.report("Health Monitor %s logged in." % username);
		
		return Monitor.HealthMonitor.uncheckedCast(ice_object);
	
    def create(self, userId, control, current=None):
        if not userId:
            raise Glacier2.PermissionDeniedException, "Please pass in a user ID as the first argument.";
        
        world = World.get_world();
        proxy = world.adapter.createProxy(
            world.communicator.stringToIdentity("SessionFactory"));
        self.report("%s is creating a session..." % userId);
        return Glacier2.SessionPrx.uncheckedCast(proxy);
