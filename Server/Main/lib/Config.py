###########################################################################
# \file Config.py
# \brief Central configuration class.
# \author Copyright 2009 by Vermont Technical College
###########################################################################
from ConfigParser import ConfigParser;
from VTankObject import Version;
from hashlib import sha1;
import os;
import Log;
import VTankObject;
import Utils;
import socket; # for gethostbyname()

# Make the VTank_Config object available globally.
def initialize_config():
    global config;
    # No need to re-initialize if it already exists:
    try:
        config.check();
    except NameError:
        config = VTank_Config();
        
def get_config():
    global config;
    try:
        return config;
    except NameError:
        initialize_config();
        return config;

class VTank_Config:
    """
    The VTank_Config class stores configuration settings (as read from a property file) about
    how to run the VTank server. 
    """
    # Approved list:
    approved                = [];
    
    # [Database]
    database_host           = ("localhost", 3306);
    database_user           = "root";
    database_passwd         = "";
    database_name           = "VTank";
    
    # [Server]
    server_secret           = sha1("there is a cow level").hexdigest();
    server_name             = "Bob";
    
    # [Client]
    client_version          = Version();
    
    # [Logging]
    log_prefix              = "%Y-%m-%d_";
    log_suffix              = ".log";
    player_logging_path     = "./";
    chat_logging_path       = "./";
    misc_logging_path       = "./";
    
    # [Misc]
    web_address             = "http://vtank.vtc.edu";
    allow_unapproved_game_servers = False;
    
    def read(self, filename):
        """
        Read a given configuration file to extract known properties from.
        @param filename Name of the config file.
        @return True if the file was successfully read, False if an error occurred.
        """
        config = ConfigParser();
        file_object = None;
        try:
            file_object = open(filename, 'r');
            
            config.readfp(file_object);
        except Exception, e:
            Log.log_print("STARTUP_FAIL.log", "Unable to open config file: " + str(e));
            import os;
            print "Unable to read config file:", e, os.getcwd();
            return False;
        finally:
            if file_object: file_object.close();
        
        # [Database]
        database_host           = config.get("Database", "host_ip");
        database_port           = config.getint("Database", "port");
        self.database_host      = (database_host, database_port);
        self.database_user      = config.get("Database", "user");
        self.database_passwd    = config.get("Database", "passwd");
        self.database_name      = config.get("Database", "database_name");
        
        # [Server]
        self.server_port        = config.getint("Server", "port");
        self.server_secret      = config.get("Server", "secret");
        self.server_name        = config.get("Server", "name");
        
        # [Client]
        temp_client_version     = config.get("Client", "version");
        
        # [Logging]
        self.log_prefix         = config.get("Logging", "prefix");
        self.log_suffix         = config.get("Logging", "suffix");
        self.player_logging_path= config.get("Logging", "player_path");
        self.chat_logging_path  = config.get("Logging", "chat_path");
        self.misc_logging_path  = config.get("Logging", "misc_path");
        
        # [Misc]
        self.web_address        = config.get("Misc", "web_address");
        self.allow_unapproved_game_servers = bool(config.getint("Misc", "allow_unapproved_game_servers"));
        
        # Convert client version to an Ice Version type.
        self.client_version     = Utils.string_to_version(temp_client_version);
        
        # Make sure each path exists.
        path_group = (self.player_logging_path, self.chat_logging_path, self.misc_logging_path);
            
        import os;
        for path in path_group:
            if not path.endswith(os.sep):
                path += os.sep;
            
            if not os.path.exists(path):
                os.makedirs(path);
        
        self._read_approved();
        
        return True;
    
    def _read_approved(self):
        """
        Helper function for reading in a list of approved game servers.
        """
        with open("approved.txt", 'r') as f:
            # Read in all lines. Remove lines that start with a pound sign.
            lines = [ line.strip() for line in f.readlines() if not line.startswith('#') ];
            
            # Remove extra comments.
            lines = [ line[:line.find('#')] for line in lines if line.find('#') >= 0 ];
        
        for line in lines:
            try:
                data = line.split(":");
                self.approved.append((socket.gethostbyname(data[0]), int(data[1])));
            except socket.error, e:
                print "Invalid host name:", data[0];
            except:
                print "Invalid approved game server entry:", line;
    
    def generate_config(self, filename):
        """
        Generate a first-time configuration file.  All settings are placeholders that should
        be changed.
        @param filename Name of the file that will be generated.
        @return True if the file was written okay, False if an error occurred.
        """
        config = ConfigParser();
        config.add_section("Database");
        config.set("Database", "host_ip", "localhost");
        config.set("Database", "port", "3306");
        config.set("Database", "user", "root");
        config.set("Database", "passwd", "");
        config.set("Database", "database_name", "VTank");
        
        config.add_section("Server");
        config.set("Server", "name", "RobinHood");
        config.set("Server", "port", "31337");
        config.set("Server", "secret", "there is no cow level");
        
        config.add_section("Client");
        config.set("Client", "version", "0.0.0.0");
        
        config.add_section("Logging");
        config.set("Logging", "prefix", "%Y-%m-%d_");
        config.set("Logging", "suffix", ".log")
        config.set("Logging", "player_path", "./logs/players/");
        config.set("Logging", "chat_path", "./logs/chat/");
        config.set("Logging", "misc_path", "./logs/misc/");
        
        config.add_section("Misc");
        config.set("Misc", "web_address", "https://vtank.vtc.vsc.edu");
        config.set("Misc", "allow_unapproved_game_servers", "1");
        
        with open(filename, 'wb') as f:
            print filename;
            config.write(f);
        
        return True;
    
    def check(self):
        """
        Useless check method.
        """
        pass;
    
    