###########################################################################
# \file Equipment_Manager.py
# \brief Get a list of and manage/synchronize a list of weapons/armor.
# \author Copyright 2009 by Vermont Technical College
###########################################################################

import threading;
import Procedures;
import VTankObject;
import Utils;
import Log;

class _Manager:
    """
    Acts as a global equipment manager for fast(er) access of weapon, armor, and projectile
    information.
    """
    def __init__(self, database_obj):
        """
        Initialize the equipment manager. Doing so will grab a list of
        weapon/armor types from the database. This method must be called
        first before any other.
        @param database_obj Object to use to communicate with the database.
        """
        self._db = database_obj;
        self._lock = threading.RLock();
        
        self._weapons = {};
        self._projectiles = {};
        self._utilities  = {};
        
        # Do the database queries for the weapon/armor/proj list.
        self.refresh_weapon_list();
        self.refresh_utilities_list();
    
    def check(self):
        """
        Useless check method.
        """
        pass;
    
    def refresh_weapon_list(self):
        """
        Refresh the list of weapons by grabbing the newest ones out of
        the database.
        """
        with self._lock:
            weapons = self._db.do_query(Procedures.get_weapon_list());
            if not weapons:
                Log.log_print(Utils.filename_today(), "misc", "Error: %s" % \
                              str(self._db.get_last_error()));
            else:
                for set in weapons:
                    # weapon_id, name, cooldown, projectile_id, model, sound_effect, can_charge
                    self._weapons[set[0]] = set[1];
                        
    def refresh_utilities_list(self):
        """
        Refresh the list of utilities by grabbing the newest ones out of
        the database.
        """
        with self._lock:
            utilities = self._db.do_query(Procedures.get_utilities_list());
            if not utilities:
                Log.log_print(Utils.filename_today(), "misc", "Error: %s" %\
                              str(self._db.get_last_error()));
            else:
                for set in utilities:
                    # [0]utility_id, [3]duration, [4]damage_factor, [5]speed_factor,
                    # [6]rate_factor, [7]health_increase, [8]health_factor
                    # [1]name, [9]model, [2]description
                    self._utilities[set[0]] = VTankObject.Utility(
                        set[0], set[3], set[4], set[5], set[6], set[7], set[8], set[1], set[9], set[2]);
                        
    
    def get_weapon_list(self):
        """
        Get the full weapon list.
        """
        with self._lock:
            return self._weapons;
    
    def get_projectile_list(self):
        """
        Get the full projectile list.
        """
        with self._lock:
            return self._projectiles;
    
    def get_utilities_list(self):
        """
        Get the full utilities list.
        """
        with self._lock:
            return self._utilities.values();
    
    def get_weapon(self, id):
        with self._lock:
            if id not in self._weapons: 
                return None;
            return self._weapons[id];
    
    def get_projectile(self, id):
        with self._lock:
            if id not in self._projectiles: 
                return None;
            return self._projectiles[id];
        
    def get_utility(self, id):
        with self.lock:
            if id not in self._utilities:
                return None;
            return self._utilities[id];
    
global manager;

def get_manager():
    """
    Global, static access to the equipment manager.
    """
    global manager;
    try:
        manager.check();
    except NameError:
        manager = None;
        
    return manager;

def set_manager(_manager):
    """
    Set a manager. Must be done at least once.
    """
    global manager;
    manager = _manager;
