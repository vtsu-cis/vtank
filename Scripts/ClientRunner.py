# Run N instances of the Mock_Client script.
import time;
import thread;
from copy import copy;
from getpass import getpass;
from threading import Thread, Lock;

global lock, threads;
lock = Lock();
threads = 0;

def client_runner(threadname, username, password):
    global lock, threads;
    with lock:
        threads += 1;
    
    tries = 0;
    max_tries = 5;
    
    try:
        while tries < max_tries:
            import Mock_Client;
            if Mock_Client.main("glacier2a.cis.vtc.edu", 4063, username, password) != 0:
                print "Warning: client %s exited with an error. Trying again..." % username;
                
                tries += 1;
            else:
                print "Client %s exited without issue." % username;
                
                break;
    except Exception, e:
        print "Warning: client %s exited with an error: %s" % (username, str(e));
    finally:
        with lock:
            threads -= 1;
    
def main(argv=[]):
    global lock, threads;
    
    clients = int(raw_input("How many clients to run? ").strip());
    if clients <= 0:
        print "Must run at least one client.";
        print "Exitting...";
        return 1;
    
    # Seconds to wait in-between client runs.
    rest_time = 2;
    
    for i in xrange(1, clients + 1):
        username = "test" + str(i);
        password = "1";
        print "Creating client:", username;
        
        import Mock_Client;
        thread.start_new_thread(client_runner, ("Thread#%i" % (i), username, password));
        
        time.sleep(rest_time);
        
    while True:
        time.sleep(1);
        with lock:
            if threads <= 0:
                print "All clients have stopped running.";
                break;
                
    print "Finished running.";
    raw_input("Press [ENTER] to continue.");
    
    return 0;
    
if __name__ == '__main__':
    import sys;
    sys.exit(main(sys.argv));
