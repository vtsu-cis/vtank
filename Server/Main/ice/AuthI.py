###########################################################################
# \file AuthI.py
# \brief Implementation of the Auth Ice interface.
# \author Copyright 2009 by Vermont Technical College
###########################################################################

import Ice;
import Main;
import World;
import VTankObject;
import Procedures;
import Utils;
import re;
from HashUtility import hash_password;

from Exceptions import *;

class AuthI(Main.Auth):
    """
    Implements the Auth interface generated from Ice.
    The AuthI interface handles basic first-time communication with
    any type of client.
    """
    def __init__(self, config_obj, database_obj, reporter=None):
        """
        Initialize the Auth interface.
        @param config_obj Configuration object.
        @param database_obj Object that connects to the server.
        @param reporter Reporter to use for debugging. Defaults to None.
        """
        self.config   = config_obj;
        self.database = database_obj;
        self.reporter = reporter;
        
    def report(self, message):
        """
        Report a message. Does nothing if self.reporter == None.
        @param message Message to send to the debug reporter.
        """
        if self.reporter:
            self.reporter(message);
    
    def CreateAccount(self, username, password, email, current=None):
        """
        Allow the user to attempt to create a new account on the server.
        """
        # Validate.
        p = re.compile("[^A-Za-z0-9]");
        if p.search(username):
            # Invalid character.
            raise BadInformationException("Username contains invalid characters.");
        
        # Validating e-mails are hard to do, so let's keep it simple.
        if '@' not in email or '.' not in email:
            # Invalid e-mail.
            raise BadInformationException("Invalid e-mail address."); 
        
        # Hash the password.
        password = hash_password(password);
        
        results = self.database.do_query(Procedures.account_exists(), username);
        if len(results) != 0:
            self.report("Sorry %s, but that account already exists." % username);
            return False;
        
        results = self.database.do_insert(Procedures.new_account(), username, password, 
                                          Utils.get_timestamp(), Utils.get_timestamp(), 
                                          0, 0, email);
        
        if results != 1:
            # No rows updated.
            self.report("Account creation failed! Reason: %s." % self.database.get_last_error());
            raise VTankException("Internal database error -- please report this!");
        
        self.report("Account created: %s (email: %s)." % (username, email));
        
        return True;
            
    def CheckCurrentVersion(self, current=None):
        """
        Allow the client to check the version of the VTank client. 
        """
        return self.config.client_version;
    
    def CheckMaxMapSize(self, current=None):
        """
        Allow the client (probably the map editor) to look at the maximum file size
        allowed for maps.
        """
        return 20971520; # 20 MB
        
    def GetGameServerList(self, current=None):
        """
        Allow clients to see what game servers are available.
        """
        world = World.get_world();
        return world.get_game_server_list();
        
    def GetPlayersOnline(self, current=None):
        """
        Allow clients to see how many players are online.
        """
        world = World.get_world();
        return world.get_user_count();
