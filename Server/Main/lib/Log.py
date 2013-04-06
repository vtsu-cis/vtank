###########################################################################
# \file Log.py
# \brief Print special log messages.
# \author (C) Copyright 2008 by Vermont Technical College
###########################################################################
import os;
import stat;
import time;
import Utils;
import datetime;
import threading;

# The Log library is thread safe.
log_lock = threading.Lock();

def log_reporter(message):
    """
    This reporter takes debug messages and logs them.
    @param message Message to print. Should not contain a timestamp. 
    """
    quick_log(message);

def console_print(message):
    """
    Prints a message to standard output.
    @param message Message to print.  Do not prepend time stamp (it does this for you).
    """
    log_lock.acquire();
    now = str(time.strftime("[%H:%M] "));
    print now + str(message);
    log_lock.release();

def log_print(filename, logtype, message):
    """
    Logs a message to a given file name, using the "append" flag.
    @param filename Name of the file, not including the path.
    @param logtype A string that indicates the type of log to write. Can be:
        * "chat"
        * "misc"
        * "players"
    @param message Message to print.  A newline is automatically appended to it.
    @return True if the file was opened and the message was printed.  False if an IO error
    occurred.
    """
    if logtype != "":
        filename = "logs%c%s%c%s" % (os.sep, logtype, os.sep, filename);
    log_lock.acquire();
    f = None;
    try:
        if not filename.endswith(".log") and not filename.endswith(".txt"):
            filename += ".log";
        
        now = str(time.strftime("[%H:%M]"));
        
        f = open(filename, "a");
        
        f.write("%s %s\n" % (now, message));
    except IOError, e:
        print e;
        return False;
    finally:
        if f: f.close();
        log_lock.release();
    
    return True;

def quick_log(message):
    """
    A quick-logger, for people who don't have time.
    Automatically logs to the "players" folder using today's date as the
    filename.
    @param message Message to log.
    @return True or False according to the log_print documentation.
    """
    return log_print(Utils.filename_today(), "players", message);
    
def log_cleanup(directory, day_old_limit = 30):
    """
    The log_cleanup function is important for conserving file space.  Log files should be
    all placed into one folder (or at least, should be separated from other, important files).
    Call this function on a directory to remove all files from it that are considered too old
    to be useful.
    Note: this may be an expensive operation.  Call when nothing significant is happening.
    @param directory Directory root.  Will go into all sub-directories to find and delete all
    log files ending with ".log" or ".txt".
    @param day_old_limit The threshold (in days) to remove files based on age.  The default
    is 30 days.
    @return Number of log files deleted.
    """
    # Prevent the user from being stupid.
    if directory == os.sep:
        return 0;
    
    kill_count = 0;
    
    # Get the current date.
    year  = time.strftime("%Y");
    month = time.strftime("%m");
    day   = time.strftime("%d");
    today = datetime.date(int(year), int(month), int(day));
    
    log_lock.acquire();
    
    for root, dirs, files in os.walk(directory):
        for name in files:
            if name.endswith(".txt") or name.endswith(".log"):
                try:
                    file_stats = os.stat(os.path.join(root, name));
                    timestamp = time.localtime(file_stats[stat.ST_CTIME]);
                
                    old_year    = int(time.strftime("%Y", timestamp));
                    old_month   = int(time.strftime("%m", timestamp));
                    old_day     = int(time.strftime("%d", timestamp));
                    old_date    = datetime.date(old_year, old_month, old_day);
                    
                    # Calculate number of days.
                    diff = today - old_date;
                    if diff > day_old_limit:
                        os.remove(os.path.join(root, name));
                        kill_count += 1;
                except IOError, e:
                    continue;
                
    log_lock.release();
    
    return kill_count;
