#!/usr/bin/python
# Run the clock synchronization client.

global HOST, PORT, DELAY, SYNC_REQUESTS;

# Hostname of the target machine.
HOST = "localhost";

# Port number to talk to.
PORT = 31335;

# Directory where all of the *.ice files reside, relative to this.
SLICE_DIR = "../Ice";

# Name of the sync script that we're using.
SLICE_NAME = "ClockSync.ice";

# Delay (if any) in sending requests to the client (seconds).
DELAY = 1.0;

# How many times to perform a request.
SYNC_REQUESTS = 20;

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

def get_time():
    return long(time() * 1000);
    
def average(num_list):
    sum = 0;
    for num in num_list:
        sum += num;
    sum /= len(num_list);
    
    return sum;
    
def do_sync(proxy):
    global DELAY, SYNC_REQUESTS;
    
    times_synchronized = 0;
    offsets = [];
    latencies = [];
    while times_synchronized < SYNC_REQUESTS:
        sleep(DELAY);
        
        start = get_time();
        stamp = proxy.Request();
        end   = get_time();
        
        latency = (end - start) / 2;
        offset = (stamp + latency) - end;
        
        latencies.append(latency);
        offsets.append(offset);
        
        print "#%s: Latency=%s, Offset=%s" % (times_synchronized, latency, offset);
        
        times_synchronized += 1;
        
    average_latency = average(latencies);
    final_offset    = offsets[-1];
    
    print "Average latency: %s, Final offset: %s" % (average_latency, final_offset);

def main(argv):
    global HOST, PORT;
    if len(argv) >= 2:
        HOST = argv[1];
    
    communicator = Ice.initialize();
    proxy = communicator.stringToProxy("ClockSync:tcp -h %s -p %i" % (HOST, PORT));
    proxy = GameSession.ClockSynchronizerPrx.uncheckedCast(proxy);
    proxy.ice_ping();
    
    do_sync(proxy);
    
    return 0;
    
if __name__ == '__main__':
    sys.exit(main(sys.argv));
