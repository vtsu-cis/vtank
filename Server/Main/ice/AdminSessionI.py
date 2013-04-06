###########################################################################
# \file AdminSessionI.py
# \brief Implements the administration session interface.
# \author Copyright 2009 by Vermont Technical College
###########################################################################

import re;
import Ice;
import Admin;
import Config;
import Utils;
import Procedures;
import World;
import UserLevel;
import Client_Types;
from time import time;
from Exceptions import *;
from hashlib import sha1;
from Base_Servant import Base_Servant

class AdminSessionI(Admin.AdminSession, Base_Servant):
    """
    Ice servant that implements server-side methods for administrative clients.
    """
    def __init__(self, name, ip_string, reporter = None, threshold = 300):
        """
        Initialize an administrative session.
        @param name Administrator who opened the session.
        @param ip_string Obtained from current.con.toString().
        @param reporter Debug reporter. Null by default.
        @param threshold How long to wait before disconnecting an admin in seconds. Default is 300.
        """
        Base_Servant.__init__(self, ip_string, Client_Types.ADMIN_CLIENT_TYPE, 
                              UserLevel.ADMINISTRATOR, reporter);
        
        self.name = name;
        self.database = World.get_world().get_database();
        self.threshold = threshold;
        self.callback = None;
    
    def __str__(self):
        return Base_Servant.__str__(self);
    
    def get_callback(self):
        """
        The administrator will set a callback volunteering to take messages. This provides
        access to that callback.
        """
        return self.callback;
    
    #
    # The following functions are overridden from a generated file.
    # To view the documentation of these functions, please see:
    # CaptainVTank.ice
    #

    def destroy(self, current=None):
        World.get_world().get_tracker().remove(self.get_id());
    
    def KeepAlive(self, current=None):
        self.refresh_action();

    def GetUserList(self, current=None):
        self.refresh_action();
        
        return World.get_world().get_full_userlist();
        
    def GetFullUserList(self, current=None):
        self.refresh_action();
        
        return World.get_world().get_complete_userlist();
        
    def GetUserCount(self, current=None):
        self.refresh_action();
        
        return World.get_world().get_user_count();
    
    def GetAccountList(self, current=None):
        self.refresh_action();
        
        result = self.database.do_query(Procedures.get_account_list());
                
        accounts = [];
        
        for row in result:
            # Format: [0]account_name, [1]email, [2]creation_date, [3]last_logged_in, [4]rank_level, [5]user_level, [6]points
            accounts.append(Admin.Account(row[0], row[1], row[2], row[3], row[4], row[5], row[6]));           
        
        return accounts;        
    
    def GetAccountByName(self, accountName, current=None):
        self.refresh_action();
        
        result = self.database.do_query(Procedures.get_account_info(), accountName);
        if len(result) != 1:
            raise BadInformationException("That account does not exist.");
        
        row = result[0];
        # Format: [0]email, [1]creation_date, [2]last_logged_in, [3]rank_level, [4]user_level, [5]points
        return Admin.Account(accountName, row[0], row[1], row[2], row[3], row[4], row[5]);
    
    def CreateAccount(self, username, password, email, userlevel, current=None):
        self.refresh_action();
        
        # Validate.
        p = re.compile("[^A-Za-z0-9]");
        if p.search(username):
            # Invalid character.
            raise BadInformationException("Username contains invalid characters.");
        
        # Validating e-mails is hard to do, so let's keep it simple.
        if '@' not in email or '.' not in email:
            # Invalid e-mail.
            raise BadInformationException("Invalid e-mail address."); 
        
        # Make sure password is filled in.
        if not password:
            raise BadInformationException("Password cannot be empty.");
        
        # Hash the password.
        password = sha1(password).hexdigest();
        
        results = self.database.do_query(Procedures.account_exists(), username);
        if len(results) != 0:
            self.report("Account creation failed: %s exists." % username);
            raise BadInformationException("That account name already exists.");
        
        results = self.database.do_insert(Procedures.new_account(), username, password, 
                                          Utils.get_timestamp(), Utils.get_timestamp(), 
                                          0, userlevel, email);
        
        if results != 1:
            # No rows updated.
            self.report("Account creation failed! Reason: %s." % self.database.get_last_error());
            raise BadInformationException("Internal database error -- please report this!");
        
        self.report("Account created: %s (email: %s)." % (username, email));
    
    def EditAccountByName(self, accountName, newInformation, current=None):
        self.refresh_action();
        
        # Validate.
        p = re.compile("[^A-Za-z0-9]");
        if p.search(accountName):
            # Invalid character.
            raise BadInformationException("Old account name cannot exist (has invalid characters).");
        
        if p.search(newInformation.accountName):
            # Invalid character in new account name.
            raise BadInformationException("New account name has invalid characters.");
        
        # Validating e-mails is hard to do, so let's keep it simple.
        if '@' not in newInformation.email or '.' not in newInformation.email:
            # Invalid e-mail.
            raise BadInformationException("Invalid e-mail address."); 
        
        results = self.database.do_query(Procedures.account_exists(), accountName);
        if len(results) != 1:
            self.report("Account edit failed: %s doesn't exist." % username);
            raise BadInformationException("That account name does not exist.");
        
        pass;
    
    def SetAccountPassword(self, accountName, newPassword, current=None):
        self.refresh_action();
        pass;
    
    def DeleteAccountByName(self, accountName, current=None):
        self.refresh_action();
        pass;
    
    def KickUserByAccountName(self, accountName, reason, current=None):
        self.refresh_action();
        if World.get_world().kick_user_by_name(accountName):
            self.report("%s was kicked for: %s" % (accountName, reason));
        else:
            raise BadInformationException, "User is not online.";
    
    def BanUserByAccountName(self, accountName, reason, current=None):
        self.refresh_action();
        
        results = self.database.do_query(Procedures.account_exists(), accountName);
        if len(results) != 1:
            self.report("Account ban failed: %s doesn't exist." % accountName);
            raise BadInformationException("That account name does not exist.");
        
        result = self.database.do_insert(Procedures.set_user_level(), -99, accountName);
        
        if result == -1:
            self.report("Failed to ban account %s" % accountName);
    
    def UnbanUserByAccountName(self, accountName, current=None):
        self.refresh_action();
        
        results = self.database.do_query(Procedures.account_exists(), accountName);
        if len(results) != 1:
            self.report("Account unbanning failed: %s doesn't exist." % accountName);
            raise BadInformationException("That account name does not exist.");
        
        result = self.database.do_insert(Procedures.set_user_level(), 0, accountName);
        
        if result == -1:
            self.report("Failed to unban account %s" % accountName);
            raise VTankException("Internal database error -- please report this!");

