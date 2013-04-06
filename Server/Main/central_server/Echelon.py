###########################################################################
# \file   Echelon.py
# \brief  Manage the setup and initialization of the program.
# \author (C) Copyright 2009 by Vermont Technical College
#
# LICENSE
#
# This program is free software; you can redistribute it and/or modify it
# under the terms of the GNU General Public License as published by the
# Free Software Foundation; either version 2 of the License, or (at your
# option) any later version.
#
# This program is distributed in the hope that it will be useful, but
# WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANT-
# ABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public
# License for more details.
#
# You should have received a copy of the GNU General Public License along
# with this program; if not, write to the Free Software Foundation, Inc.,
# 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
#
# TODO
#
#   + ...
#
# Please send comments and bug reports to
#
#      Summer of Software Engineering
#      Vermont Technical College
#      201 Lawrence Place
#      Williston, VT 05495
#      sosebugs@summerofsoftware.org (http://www.summerofsoftware.org)
###########################################################################

import sys, time, os;


# Switch to project root path.
if os.getcwd().endswith("central_server"):
    os.chdir("..");

current_dir = os.path.abspath(os.getcwd());
sys.path.append(current_dir + "/lib");
sys.path.append(current_dir + "/gameplay_server");
sys.path.append(current_dir + "/test");
sys.path.append(current_dir + "/ice");
sys.path.append(current_dir + "/ice/generated");

import stackless, stacklesssocket;
from SendMail import MailMessage
import VersionChecker;

try:
    stacklesssocket.install();
except:
    # This throws an exception only when it's already installed, so it can be ignored.
    pass;

def debug_reporter(message):
    """
    The reporter is used for debug printing.  This function can be passed as an argument to
    some classes so that they know to debug print.
    @param message Message to print.  Should not contain a timestamp.
    """
    print time.strftime("[%H:%M:%S]"), message;

def load_slice_files(icedir=os.path.normpath("../../Ice")):
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
        debug_reporter("Loading %s..." % file);
        Ice.loadSlice(create_load_string(file));

class Force_Exit_Exception(Exception):
    """
    The Force_Exit_Exception is a signal sent notifying the top-level exception handler that
    the program should exit immediately.  These are most likely fatal errors that will disable
    the server from running.  A special log file should be written for this case.
    """
    def __init__(self, message, exit_code = 1):
        self.message = message;
        self.exit_code = exit_code;
        
    def __str__(self):
        return self.message;
    
    def what(self):
        return self.message;
    
    def exit_code(self):
        return self.exit_code;

def main(argv=[]):
    """
    This is the first function called when the program starts.  Do all initialization here.
    @param argv Optionally pass the command-line arguments.
    @return Exit code.  0 for success, non-zero for unnatural exit.
    """
    # Write pid to file.
    if not os.path.exists("./logs"):
        os.mkdir("logs");
    
    run_unit_tests  = False;
    config_file     = os.path.abspath("config.cfg");
    debug_mode      = False;
    
    # The first argument isn't important to us -- it's the program's name.
    del argv[0];
    
    for arg in argv:
        if arg == "--test" or arg == "-t":
            # Testing mode enabled.  The server will not run; it will perform unit tests.
            run_unit_tests = True;
            
        elif arg.startswith("-p") or arg.startswith("--port"):
            # Let user know how to set a port.
            print "Port cannot be set through command line, look in the config file.";
            return 1;
        
        elif arg == "-d" or arg == "--debug":
            # Enable debugging.
            debug_reporter("Debug mode enabled.");
            debug_mode = True;
        
        elif arg == "--generate-config" or arg == "-g":
            # Generate a configuration file.
            temp_config = Config.VTank_Config();
            result = temp_config.generate_config(os.path.abspath(".") + os.sep + "config.cfg");
            debug_reporter("Result: %s" % result);
            return int(result);
        
        elif arg == "--load-slice" or arg == "-s":
            # Load slice dynamically without generating files.
            load_slice_files();
        
        elif arg.startswith("--config-file="):
            # User is specifying the config file's name.
            config_file = arg[len("--config-file="):];
            if not config_file:
                print "--config-file usage:";
                print "--config-file=[FILENAME]";
                return 1;
            
            if not os.path.exists(os.path.abspath(config_file)):
                print "Config file does not exist:", config_file;
                return 1;
            
            debug_reporter("Using config file " + config_file + ".");
            
        else:
            # Unknown.
            print "Unknown option:", arg, " -- ignoring it.";
            
    # Run unit testing here.
    if run_unit_tests:
        import Unit_Test;
        return Unit_Test.run_all_tests();
    
    import Log;
    import World;
    import Config;
    import Map_Manager;
    
    if debug_mode:
        reporter_func = debug_reporter;
    else:
        reporter_func = Log.log_reporter;
    
    # Start handling the world.
    try:
        World.initialize_world(config_file, reporter_func);
        world = World.get_world();
        Map_Manager.initialize(world.get_config(), world.get_database(), reporter_func);
        
        # Write the process ID of this server to file.
        file = open('echelon.pid', 'w');
        with file:
            file.write(str(os.getpid()));
            
        stackless.tasklet(VersionChecker.get_mysql_version)();
        stackless.run();
        
        # Unless some design changes, it will likely never reach this point.
        # Instead, it will be running through the stackless scheduler.
        reporter_func("Waiting for Ice shutdown.");
        World.get_world().get_communicator().waitForShutdown();
    except Force_Exit_Exception, e:
        Log.log_print("CRITICAL_ERROR.log", "", "Startup failure: " + str(e));
        reporter_func("Critical error occurred: " + str(e));
        from sys import exit;
        return e.exit_code;
    except Exception, e:
        #sender, subject, message, to
        recipients = "cbeattie@summerofsoftware.org, asibley@summerofsoftware.org, "\
                     "msmith@summerofsoftware.org, jteasdale@summerofsoftware.org";
                     
        message = "Echelon has crashed with the following error. \n\n"\
                   "Stack trace: \n %s \n \n" \
                   "Please investigate this issue." % (e);
                   
        mailMessage = MailMessage("Echelon", "Echelon Server Crash", message, recipients);
        success = mailMessage.mail();
        
        if not success:
            Log.log_print("Failed to send server crash report!");
        
    finally:
        
        # Delete the PID file.
        try:
            os.remove('echelon.pid');
        except:
            pass;
    
    reporter_func("Echelon has shut down without issue.");
    
    return 0;

if __name__ == "__main__":
    sys.exit(main(sys.argv));
