###########################################################################
# \file HealthMonitorI.py
# \brief Implements the HealthMonitor session interface.
# \author Copyright 2010 by Vermont Technical College
###########################################################################

import Monitor;
import UserLevel;
import Client_Types;
from Exceptions import *;

class HealthMonitorI(Monitor.HealthMonitorEndpoint, Base_Servant):
	"""
    Ice servant that implements server-side methods for health monitors.
    """
    def __init__(self, name, ip_string, reporter = None, threshold = 1800):
        """
        Initialize a health monitor session.
        @param name Account name of the health monitor.
        @param ip_string Obtained from current.con.toString().
        @param reporter Debug reporter. Null by default.
        @param threshold How long to wait before disconnecting this health monitor. Default is 600.
        """
        Base_Servant.__init__(self, ip_string, Client_Types.HEALTH_MONITOR_CLIENT_TYPE, 
                              UserLevel.MEMBER, reporter);
        
        self.name = name;
        self.threshold = threshold;
        self.callback = None;	
		
	def __str__(self):
        return Base_Servant.__str__(self);
    
    def get_callback(self):
        """
        Provides access to the message callback.
        """
        return self.callback;
	
	#
    # The following functions are overridden from a generated file.
    # To view the documentation of these functions, please see:
    # HealthMonitor.ice
    #	
		
	def KeepAlive(self, current=None):
        self.refresh_action();