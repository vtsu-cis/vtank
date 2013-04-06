###########################################################################
# \file Base_Servant.py
# \brief Serves as a base class to some Ice servants.
# \author (C) Copyright 2009 by Vermont Technical College
###########################################################################

import Ice;
from time import time;

class Base_Servant:
    """
    Acts as a base class to all session servants, such as AdminSessionI, MainSessionI,
    etc.
    """
    def __init__(self, ip_string, client_type, userlevel, reporter = None):
        """
        @param ip_string IP string, which is retrieved from current.con.toString().
        @param client_type Type of client that this is.
        @param userlevel How powerful this user is.
        @param reporter Optional reporter function for debug output.
        """
        self.ip = ip_string;
        self.type = client_type;
        self.userlevel = userlevel;
        self.reporter = reporter;
        self.last_action = time();
        self.id = Ice.Identity();
        self.ingame = False;
        self.server = None;
        
    def __str__(self):
        """
        Represent this object as a string.
        """
        addr = self.get_local_address();
        return "%s(%s)@%s:%i" % (self.name, self.type, addr[0], addr[1]);
    
    def report(self, message):
        """
        Report a message. Does nothing if self.reporter == None.
        @param message Message to send to the debug reporter.
        """
        if self.reporter:
            self.reporter(message);
    
    def get_local_address(self):
        """
        Get the local address of the client as a tuple.
        @return Tuple as: (host, port).
        """
        # See the Ice::Connection::toString() method to see how the address is formatted.
        addr = self.ip.split("\n")[0].split(" = ")[1].split(":");
        return (addr[0], int(addr[1]));
    
    def get_remote_address(self):
        """
        Get the remote address of the client as a tuple.
        @return Tuple as: (host, port).
        """
        # See the Ice::Connection::toString() method to see how the address is formatted.
        addr = self.ip.split("\n")[1].split(" = ")[1].split(":");
        return (addr[0], int(addr[1]));
        
    def refresh_action(self):
        """
        Reset the last action.
        """
        self.last_action = time();
        
    def get_last_action_time(self):
        """
        Get the timestamp of the last action performed by this janitor.
        """
        return self.last_action;

    def type(self):
        """
        Get the type of client.
        """
        return self.type;

    def get_userlevel(self):
        """
        Get the userlevel of the client.
        """
        return self.userlevel;
    
    def is_playing_game(self):
        """
        Check if the client is playing a game.
        """
        return self.ingame;
    
    def get_game_server(self):
        """
        Get the game server that this client plays on, if any.
        """
        return self.server;
    
    def set_playing_game(self, value, server = None):
        """
        Set whether or not the client is playing a game (True or False).
        """
        self.ingame = value;
        self.server = server;
    
    def set_id(self, id):
        """
        Set session ID for this user.
        @param id ID to set.
        """
        self.id = id;

    def get_id(self):
        """
        Get the session ID for this user.
        @return Session ID.
        """
        return self.id;
