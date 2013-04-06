#!/usr/bin/python
# Run the clock synchronization server.

# Port number to listen on.
PORT = 31335;

# Directory where all of the *.ice files reside, relative to this.
SLICE_DIR = "../Ice";

# Name of the sync script that we're using.
SLICE_NAME = "ClockSync.ice";

# Delay (if any) in replying to requests (seconds, can be a floating point number).
DELAY = 0;

import sys;
import os;
import Ice;
from time import time, sleep;

try:
    # The following line throws an exception if ICEROOT is not an environment variable:
    ICEROOT = os.environ["ICEROOT"];

    Ice.loadSlice("-I%s -I%s/slice --all %s/%s" % (SLICE_DIR, ICEROOT, SLICE_DIR, SLICE_NAME));
    import GameSession;
except RuntimeError, e:
    print "Error: Unable to load the slice file %s. Please contact an administrator." % (SLICE_NAME);
    sys.exit(1);

class ClockSync(GameSession.ClockSynchronizer):
    """
    Servant for the ClockSynchronizer class.
    """
    def get_time(self):
        return long(time() * 1000);
    
    def Request(self, current = None):
        print "Received request at", self.get_time();
        if DELAY:
            sleep(DELAY);
        
        return self.get_time();

def main():
    """
    The main() function starts serving clients.
    """
    communicator = Ice.initialize();
    adapter = communicator.createObjectAdapterWithEndpoints("ClockSync", "tcp -p %i" % PORT);
    adapter.add(ClockSync(), communicator.stringToIdentity("ClockSync"));
    
    # Start serving clients.
    adapter.activate();
    
    communicator.waitForShutdown();
    
    print "Finished running.";
    
    return 0;

if __name__ == '__main__':
    sys.exit(main());
