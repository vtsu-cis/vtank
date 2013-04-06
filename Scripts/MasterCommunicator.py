#!/usr/bin/python
# Library for use in Python scripts which need to perform communications with Echelon.
# Written by Andrew Sibley
# Last updated 07/25/2009

# ATTENTION - This property must be configured! Make sure it's correct.
# The directory in which the VTank slice files are located.
SLICE_DIRECTORY = "/vtank/Ice";

import Ice;
import os;
import sys;
from sys import stderr;

def load_slice_files(icedir):
    """
    Load slice files. This can be done dynamically in Python. Note that this requires
    the ICEROOT environment variable to be (correctly) set.
    """
    import Ice;
    
    # The following line throws an exception if ICEROOT is not an environment variable:
    iceroot = os.environ["ICEROOT"];
    if not iceroot:
        print "WARNING: ICEROOT is not defined! It's required to load slice files dynamically!";
    
    def create_load_string(target_file):
        """
        Create a string which loads Slice definition files.
        @param target_file File to load.
        @return Formatted string.
        """
        return "-I\"%s/slice\" -I\"%s\" --all %s/%s" % (
            iceroot, icedir, icedir, target_file);
    
    ice_files = [ file for file in os.listdir(icedir) if file.endswith(".ice") ];
    for file in ice_files:
        Ice.loadSlice(create_load_string(file));
        
load_slice_files(SLICE_DIRECTORY);

import VTankObject;
import Main;
import Exceptions;

class MasterCommunicator:
    """
    The class which is created to abstract the difficulties of connecting to Echelon.
    """
    adapter = "Auth";
    host = "echelon.cis.vtc.edu";
    port = 31337;
    timeout = 10000; # Milliseconds.
    session = None;
    communicator = None;
    
    def __init__(self):
        """
        Start the communicator. Note that this does not actually perform the connection.
        Call "Connect()" to do that.
        This method calls Initialize().
        """
        self.Initialize();
        
    def __del__(self):
        """
        Destructor called when the object is deleted. This performs a last attempt to close the
        connection to Echelon.
        """
        try:
            self.Disconnect();
        except:
            pass;
            
        try:
            self.Shutdown();
        except:
            pass;
    
    def Initialize(self, options = []):
        """
        Initialize components required for the communicator. This must be called if Shutdown
        is invoked, and this needs to be used again.
        @param options A list containing string options to be used in the initialization. 
            Don't touch these options unless you understand Ice.
        """
        if options == None or not isinstance(options, list):
            raise Exception("The 'options' parameter must be a list!");
            
        if self.communicator != None:
            print >> stderr, "Warning: The communicator has already been initialized.";
            self.Shutdown();
        
        if not len(options):
            # Initialize communicator without options.
            self.communicator = Ice.initialize();
        else:
            # Initialize communicator with options.
            self.communicator = Ice.initialize(options);
    
    def Connect(self):
        """
        Using the local 'host' and 'port' properties, connect to the target server.
        @throws Ice.Exception An Ice exception will be thrown if it can't connect to the target
            server or if the target server doesn't have a valid Auth object. This means that 
            either the target server is unavailable or a bug exists in the communicator.
        """
        if not self.communicator:
            raise Exception("You must call Initialize() first!");
        
        self.session = Main.AuthPrx.uncheckedCast(self.communicator.stringToProxy(
            "%s:tcp -h %s -p %i -t %i" % (self.adapter, self.host, self.port, self.timeout)));
        # Pinging once will create the connection, and possibly throw an exception.
        self.session.ice_ping();
    
    def CreateAccount(self, name, passwd, email):
        """
        Create an account. This uses the opened session to ask Echelon to create the given
        account.
        @param name Username to create.
        @param passwd Password (plaintext, not hashed or encrypted) to assign the account.
        @param email E-mail address that belongs to the account.
        @return True if the account creation succeeded; False otherwise.
        """
        if not self.session:
            raise Exception("You must call Connect() first!");
            
        try:
            return self.session.CreateAccount(name, passwd, email);
        except Exceptions.BadInformationException, e:
            print >> stderr, "Warning: The given information is incorrect:", e;
        except Exceptions.VTankException, e:
            # VTankException indicates an internal error.
            print >> stderr, "Warning:", e;
        except Ice.Exception, e:
            print >> stderr, "Error: Disconnected:", e;
            self.Disconnect();
            raise Exception("You have lost connection to Echelon.");
        
        return False;
    
    def GetGameServerList(self):
        """
        Simply gets the list of game servers.
        @throws Exception Thrown if the connection was lost.
        @return 2D array of servers formatted like:
            [
                [
                    "host name",
                    port number,
                    "server name",
                    is approved (bool),
                    is using Glacier2 (bool),
                    number of players online,
                    player limit,
                    "current map name",
                    "game mode"
                ],
                [
                    ...
                ],
                ...
            ]
        """
        if not self.session:
            raise Exception("You must call Connect() first!");
        
        severs = None;
        try:
            servers = self.session.GetGameServerList();
        except Ice.Exception, e:
            # Disconnected.
            print >> stderr, "Error: Disconnected:", e;
            self.Disconnect();
            raise Exception("You have lost connection to Echelon");
        
        # It's not ideal for the user using this library to work with VTankObject.ServerInfo
        # objects, so instead, convert it to a 2D array.
        def to_string(gameMode):
            """
            Convert a game mode to it's proper string format.
            @param gameMode Mode to convert.
            @return String version of the mode.
            """
            if gameMode == VTankObject.GameMode.DEATHMATCH:
                return "Deathmatch";
            elif gameMode == VTankObject.GameMode.TEAMDEATHMATCH:
                return "Team Deathmatch";
            elif gameMode == VTankObject.GameMode.CAPTURETHEBASE:
                return "Capture the Base";
            elif gameMode == VTankObject.GameMode.CAPTURETHEFLAG:
                return "Capture the Flag";
            
            return "Unknown";
        
        new_servers = [];
        for server in servers:
            new_servers.append(
                [
                    server.host,
                    server.port,
                    server.name,
                    server.approved,
                    server.usingGlacier2,
                    server.players,
                    server.playerLimit,
                    server.currentMap,
                    to_string(server.gameMode)
                ]
            );
        
        return new_servers;
    
    def CheckCurrentVersion(self):
        """
        Check the current version of the VTank client.
        @throws Exception Thrown if the connection was lost.
        @return String containing the version in the format:
            "major.minor.build.revision"
        """
        if not self.session:
            raise Exception("You must call Connect() first!");
            
        version = None;
        try:
            version = self.session.CheckCurrentVersion();
        except Ice.Exception, e:
            # Disconnected.
            print >> stderr, "Error: Disconnected:", e;
            self.Disconnect();
            raise Exception("You have lost connection to Echelon.");
        
        # The version is a VTankObject.Version object, so to make things simpler, convert it 
        # to a regular string.
        version = "%i.%i.%i.%i" % (
            version.major, version.minor, version.build, version.revision);
        
        return version;
    
    def GetPlayersOnline(self):
        """
        Get the current number of players playing a game or logged into Echelon.
        @throws Exception Thrown if the connection was lost.
        """
        if not self.session:
            raise Exception("You must call Connect() first!");
            
        online = 0;
        try:
            online = self.session.GetPlayersOnline();
        except Ice.Exception, e:
            print >> stderr, "Warning: Disconnected:", e;
            self.Disconnect();
            raise Exception("You have lost connection.");
            
        return online;
    
    def Disconnect(self):
        """
        Disconnect from Echelon. This will do nothing if it has no connection.
        """
        try:
            # This is a placeholder for future operations. The 'Auth' object doesn't need to
            # be disconnected because it doesn't really hold a session.
            pass;
        except:
            pass;
        finally:
            self.session = None;
            
    def Shutdown(self):
        """
        Shut down all communications operations.
        """
        try:
            if self.communicator != None:
                self.communicator.shutdown();
        except:
            pass;
        finally:
            self.communicator = None;
