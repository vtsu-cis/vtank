###########################################################################
# \file Version.py
# \brief Check for the latest client version
# \author (C) Copyright 2010 by Vermont Technical College
###########################################################################
from time import time;
import os;
import stackless;
import MySQLdb;
from sys import platform
import threading
#import Log;

last_time_checked =  None;
version = None;
versionLock = threading.Lock()

"""
def get_version():
    #Get the latest version of the client from cached data
    global version;
    global last_time_checked;
    
    #3600 = 1 hour in seconds
    if last_time_checked == None or \
        version == None or \
        (last_time_checked+3600) < int(time.time()):
        print last_time_checked;
        version = update_version();
    
    if not version:
        version = "Unknown";
        
    return version;
"""
def mysql_connect():
    conn = MySQLdb.connect (host = "localhost",
                            port=3306,
                            user = "root",
                            passwd = "",
                            db = "VTank")
    
    db = conn.cursor ()
    return db
  
  
#Globals for the get_mysql_version  
now_timestamp = int(time()*1000);

#12 hours in milliseconds
twelve_hours = 43200000;        
version_check_time = now_timestamp; 

def get_mysql_version():
    """
    Infinite loop that periodically checks mysql's version
    """
    global twelve_hours;
    global version_check_time;
    
    now_timestamp = int(time()*1000);
        
    while (1):
        if now_timestamp >= version_check_time:
            db = mysql_connect();
            db.execute("SELECT VERSION();");
            db.close();
            version_check_time += twelve_hours;
        stackless.schedule();
    
def get_version():
    """
    Get the latest version of the client from version.ini
    """
    global versionLock;
    with versionLock:     
        pathToVersion = None;
        
        if platform == "win32":
            pathToVersion = "..\\..\\version.ini";
        else:
            pathToVersion =  "../../version.ini";
            
        new_version = None;
        file = None;
        
        try:
            file = open(pathToVersion, "r");
            version = new_version;
            
            contents = file.readlines();
            
            for line in contents:
                split_line = line.split("=");
                
                if len(split_line) > 1:
                    key = split_line[0];
                    value = split_line[1];
                    if key == "version":
                        new_version = value;
                        version = new_version;
                        return value;
            
        except Exception, e:
            raise VTankException("Failed to retrieve version from server." + str(e))
    
        finally:
            if file:
                file.close();

        