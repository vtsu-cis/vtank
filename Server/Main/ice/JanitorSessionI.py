###########################################################################
# \file JanitorSessionI.py
# \brief Janitor session interface implementation.
# \author Copyright 2009 by Vermont Technical College
###########################################################################

import Ice;
import World;
import Janitor;
import VTankObject;
from Exceptions import *;
from time import time;
from Base_Servant import Base_Servant;
from Client_Types import JANITOR_CLIENT_TYPE;

class JanitorSessionI(Janitor.JanitorSession, Base_Servant):
    """
    Session that stores data about a single connected Janitor.
    """
    def __init__(self, username, userlevel, ip_string, reporter = None):
        """
        Start a Janitor session.
        @param username Username of the connected client.
        @param database_obj Access to the database.
        @param reporter Optional debug reporter.
        """
        Base_Servant.__init__(self, ip_string, JANITOR_CLIENT_TYPE, userlevel, reporter);
        
        self.name           = name;
        self.database       = World.get_world().get_database();
        self.callback       = None;
    
    def __str__(self):
        return Base_Servant.__str__(self);
    
    def get_callback(self):
        """
        The Janitor should have set a JanitorCallback object. This method will
        return that.  If it was never set, the result is None.
        """
        return self.callback;
    
    #
    # The following functions are overridden from a generated file.
    # To view the documentation of these functions, please see:
    # JanitorSession.ice
    #
    
    def destroy(self, current=None):
        World.get_world().get_tracker().remove(self.get_id());
    
    def KeepAlive(self, current=None):
        """
        The client sends his best.
        """
        self.refresh_action();
    
    def GetWeaponList(self, current=None):
        self.refresh_action();
        weapons = [];
        
        return weapons;
    
    def AddWeapon(self, weapon, current=None):
        self.refresh_action();
        raise VTankException("Not implemented.");
    
    def ModifyWeaponByName(self, oldWeaponName, newWeapon, current=None):
        self.refresh_action();
        raise VTankException("Not implemented.");
    
    def DeleteWeaponByName(self, weaponName, current=None):
        self.refresh_action();
        raise VTankException("Not implemented.");
    
    def GetProjectileList(self, current=None):
        self.refresh_action();
        return [];
    
    def AddProjectile(self, projectile, current=None):
        self.refresh_action();
        raise VTankException("Not implemented.");
    
    def ModifyProjectileById(self, id, newProjectile, current=None):
        self.refresh_action();
        raise VTankException("Not implemented.");
    
    def DeleteProjectileById(self, id, current=None):
        self.refresh_action();
        raise VTankException("Not implemented.");
    
    def SetCallback(self, callback, current=None):
        self.refresh_action();
        self.callback = callback;
    
    