###########################################################################
# \file Utility.py
# \brief Various utilities for VTank.
# \author Copyright 2009 by Vermont Technical College
###########################################################################
from time import time, strftime;
from VTankObject import Version;
from Exceptions import VTankException;
import datetime;
import Procedures;
import stackless;

def get_timestamp():
    """
    Get a string formatted timestamp.
    """
    return strftime("%Y-%m-%d %H:%M:%S");

def filename_today():
    """
    Get a log filename for today.
    """
    return strftime("%Y-%m-%d.log");

def string_to_version(version_string):
    """
    Convert a version string (e.g. "0.0.0.1" or "0.0.1" or "0.1") to a
    VTankObject.Version object.
    @param version_string String to convert.
    """
    version_array = version_string.split(".");
    if len(version_array) == 0:
        raise RuntimeError("Invalid version string: %s." % version_string);
    
    final_array = [0, 0, 0, 0];
    for n in xrange(0, len(version_array)):
        if n > len(final_array):
            break;
        
        final_array[n] = int(version_array[n]);
        
    return Version(final_array[0], final_array[1], final_array[2], final_array[3]);

def extract_remote_addr(current):
    """
    Extract the remote address of an Ice.Current->Connection object.
    @return Tuple address (host, port).
    """
    addr = current.con.toString().split("\n")[1].split(" = ")[1].split(":");
    return (addr[0], int(addr[1]));

def sleep_tasklet(length):
    """
    Put a tasklet (caller) to sleep for the given duration.
    @param length Time to sleep (seconds). Use decimals for subsecond precision.
    """
    start = time();
    while time() - start < length:
         stackless.schedule();

def update_last_login_time(reporter, database_obj, username):
    """
    Utility function for doing an insert on an active database. Updates
    the last login time for the given username.
    """
    result = database_obj.do_insert(Procedures.update_account(), get_timestamp(), username);
    if result < 0:
        # Less than zero indicates an error.
        if reporter != None:
            reporter("Update account last login error: %s." % str(database_obj.get_last_error()));
        raise VTankException("Internal database error. Please report this!");

def bytes_to_short(bytes):
    """
    Converts 2 bytes to a short integer.
    @param bytes Byte array (of size 2) to convert to a short.  Expects little endian always.
    @return Short integer, which fits into a python integer just fine.
    """
    return (ord(bytes[0])) + (ord(bytes[1]) << 8);

def bytes_to_int(bytes):
    """
    Converts 4 bytes to an integer.
    @param bytes Byte array (of size 4) to convert to an integer. Expects little endian.
    @return Python int.
    """
    return ((ord(bytes[0])) + (ord(bytes[1]) << 8) + 
            (ord(bytes[2]) << 16) + (ord(bytes[3]) << 24));

def int_to_dword(n):
    """
    Converts an integer to a DWORD (4 bytes).
    @param n Number to convert.
    @return String converted to the byte equivalent.
    """
    return "".join(map(chr, [x & 0xFF for x in [n, n >> 8, n >> 16, n >> 24]]));

def short_to_word(n):
    """
    Converts a short integer to a WORD (2 bytes).
    @param n Number to convert.
    @return String converted to the byte equivalent.
    """
    return "".join(map(chr, [x & 0xFF for x in [n, n >> 8]]));
