#!/usr/bin/python
# \file Create_Account.py
# \brief This script generates an account in the MySQL database for VTank.
# \author Andrew Sibley
# 
# NOTE: This library requires MySQLdb, a third-party python library.
# Installing this on a CentOS machine is easy.
# 1. Download the latest source of MySQL-python from http://sourceforge.net/projects/mysql-python
# 2. Do "tar xfz MySQL-python-1.2.2.tar.gz" (or whatever version it is)
# 3. Do "cd MySQL-python-1.2.2" (or whatever version it is)
# 4. Edit site.cfg if necessary.
# 5. Do "sudo python setup.py build"
# 6. Do "sudo python setup.py install"
# For more details, look at the README file that comes with it. 
###########################################################################
try:
    import MySQLdb as mysql;
except ImportError, e:
    print "This module requires the python MySQLdb module.";
    print "Download it at http://sourceforge.net/projects/mysql-python.";
    print "Import error:", e;
    from sys import exit;
    exit(1);

from getpass import getpass;
from hashlib import sha1;
from time import strftime;
import sys;
import re;
import threading;

import random
import hashlib;

def smart_str(s, encoding='utf-8', strings_only=False, errors='strict'):
    """
    Function provided by Django located at:
    Located at /usr/local/lib/python2.5/site-packages/django/utils
    
    @param s String to convert to the provided encoding.
    @param encoding Encoding to convert the string to.
    @param strings_only If true, don't convert (some non-string-like objects.
    @param errors Tolerance level for errors.
    @return Bytestring version of 's'.
    """
    return s;

    #if strings_only and isinstance(s, (types.NoneType, int)):
    #    return s
    #elif not isinstance(s, basestring):
    #    try:
    #        return str(s)
    #    except UnicodeEncodeError:
    #        if isinstance(s, Exception):
                # An Exception subclass containing non-ASCII data that doesn't
                # know how to print itself properly. We shouldn't raise a
                # further exception.
    #            return ' '.join([smart_str(arg, encoding, strings_only,
    #                    errors) for arg in s])
    #        return unicode(s).encode(encoding, errors)
    #elif isinstance(s, unicode):
    #    return s.encode(encoding, errors)
    #elif s and encoding != 'utf-8':
    #    return s.decode('utf-8', errors).encode(encoding, errors)
    #else:
    #    return s

def get_hexdigest(salt, raw_password):
    """
    This function was modified from the Django library located at:
    /usr/local/lib/python2.5/site-packages/django/contrib/auth/models.py
    
    Get the hexdigest version of the hash produced from sha1 when mixing in a raw password
    with a given salt.
    @param salt Salt to use with the password for extra security.
    @param raw_password Plain password to mix with the salt to produce a hash.
    @return String of the hex digest from the sha1 algorithm.
    """
    raw_password, salt = smart_str(raw_password), smart_str(salt)
    return hashlib.sha1(salt + raw_password).hexdigest();

def check_password(raw_password, enc_password):
    """
    This function was modified from the Django library located at:
    /usr/local/lib/python2.5/site-packages/django/contrib/auth/models.py
    
    Test whether the raw_password was correct. Handles encryption formats behind the scenes.
    @param raw_password Password that has not been hashed.
    @param enc_password Password that has been hashed to test against.
    @return True if the raw_password matched enc_password; false otherwise.
    """
    algo, salt, hsh = enc_password.split('$')
    return hsh == get_hexdigest(salt, raw_password);

def hash_password(raw_password):
    """
    This function was modified from the Django library located at:
    /usr/local/lib/python2.5/site-packages/django/contrib/auth/models.py
    
    Hash the password using our custom hash algorithm.
    @param raw_password Password to hash.
    @return Hashed password.
    """
    algo = 'sha1'
    salt = get_hexdigest(str(random.random()), str(random.random()))[:5]
    hsh  = get_hexdigest(salt, raw_password);
    return '%s$%s$%s' % (algo, salt, hsh)

class Database:
    """
    The purpose of this class is to abstract database transactions from the programmer.
    Inserting, updating, retrieving and removing should be trivial.
    """
    database_host   = ();
    connection      = None;
    cursor          = None;
    user            = "";
    password        = "";
    database        = "";
    last_error      = (0, "");
    version         = None;
    # Only handle 1 query at a time.    
    lock = threading.Lock();
    
    def __init__(self, database_host, user, password, database):
        """
        Start the connection to the database server.
        @param database_host Host server.  Must be a tuple.
        @param user The target user name for the database login.
        @param password The password for the login.
        @param database The database to use.
        """
        # Make sure the user passes the right kind of arguments.
        if not isinstance(database_host, tuple):
            raise RuntimeError, "Must pass a tuple (\"host\", port) as an argument.";
        if not isinstance(database_host[0], str):
            raise RuntimeError, "First argument (host name) must be a string.";
        if not isinstance(database_host[1], int):
            raise RuntimeError, "Second argument (port) must be an integer.";
        self.database_host = database_host;
        self.user = user;
        self.password = password;
        self.database = database;
    
    def __del__(self):
        """
        Destructor for the database.  Kills the connection if it wasn't dead already.
        """
        self.disconnect();
    
    def connect(self):
        """
        Tell the SQL handler to connect to the database.
        @return True on success, otherwise false.
        """
        with self.lock:
            try:
                self.connection = mysql.connect(host = self.database_host[0],
                                                port = self.database_host[1],
                                                user = self.user,
                                                passwd = self.password,
                                                db = self.database,
                                                connect_timeout = 3);
            
                self.cursor = self.connection.cursor();
                
                self.cursor.execute("SELECT VERSION();");
                
                self.version = self.cursor.fetchone();
                self.version = str(self.version[0]);
            except mysql.Error, e:
                self.last_error = e;
                return False;
            
        return True;
    
    def disconnect(self):
        """
        Tell the SQL handler to kill it's connection to the database.
        """
        with self.lock:
            if self.connection:
                if self.cursor:
                    self.cursor = None;
                self.connection.close();
                self.connection = None;
    
    def do_insert(self, sql_code, *args):
        """
        Execute an insert given the programmer's SQL code.
        @param sql_code SQL code to insert.
        @param args The MySQL database will automatically escape strings, so pass the arguments
            through this like you would through printf.
        @return -1 on failure, number of rows inserted on success.
        """
        with self.lock:
            try:
                self.connection.begin();
                
                self.cursor.execute(sql_code, (args));
                
                self.connection.commit();
                return self.cursor.rowcount;
            except mysql.Error, e:
                self.last_error = e;
                self.connection.rollback();
            
            return -1;
    
    def do_query(self, sql_code, *args):
        """
        Execute a query given the programmer's SQL code.
        @param sql_code SQL code to execute on a query.
        @param args The MySQL database will automatically escape strings, so pass the arguments
            through this like you would through printf.
        @return None on failure, or an array of rows on success.
        """
        with self.lock:
            try:
                self.cursor.execute(sql_code, (args));
                
                rows = self.cursor.fetchall();
                
                return rows;
            except mysql.Error, e:
                self.last_error = e;
                return None;
    
    def get_last_error(self):
        """
        Pops the last error that occurred.
        @return Tuple containing the error: [0] = code, [1] = message.
        """
        with self.lock:
            temp = self.last_error;
            self.last_error = (0, "");
            
        return temp;
    
    def get_version(self):
        """
        Return the version of the MySQL server.
        @return Server version string of format:
            major_version.minor_version.sub_version
        """
        return self.version;
    

def get_password():
    password = "";
    while not password:
        password = getpass("Password: ").strip();
        if not password:
            print "You must enter a non-blank password (press CTRL + C to exit).";
            continue;
            
        confirm = getpass("Confirm: ").strip();
        
        if password != confirm:
            print "Confirm password did not match original. Try again.";
            password = "";
            continue;
            
        password = hash_password(password);
        
    return password;

def get_email():
    email = "";
    while not email:
        email = raw_input("Email: ").strip();
        if not email:
            print "You must enter a non-blank e-mail (press CTRL + C to exit).";
    
    return email;
    
def get_userlevel():
    valid_userlevels = (-99, -2, -1, 0, 1, 2, 5, 10, 99);
    userlevel = 0;
    while True:
        userlevel = raw_input("Userlevel (blank for default): ").strip()
        if userlevel == "":
            userlevel = 0;
            break;
            
        try:
            userlevel = int(userlevel);
            if userlevel not in valid_userlevels:
                raise Exception();
                
            break;
        except:
            printable = ", ".join(map(str, valid_userlevels));
            print "Userlevel must be one of these numbers:", printable;
    
    return userlevel;
    
def get_timestamp():
    return strftime("%Y-%m-%d %H:%M:%S");
    
def main(argv=[]):
    db = Database(('localhost', 3306), "root", "", "VTank");
    if not db.connect():
        print db.get_last_error();
        return 1;
    
    print "Program started. Press CTRL + C or enter a blank username to exit.";
    while True:
        username    = raw_input("Username: ").strip();
        if not username:
            print "Blank username: Exitting.";
            return 0;
        
        if re.search("[^A-Za-z0-9]", username):
            print "That username contains illegal characters.";
            continue;
        
        password    = get_password();
        email       = get_email();
        userlevel   = get_userlevel();
        print "Creating a new account.";
        print "Name:", username;
        print "Email:", email;
        print "Userlevel:", userlevel;
        result = db.do_insert("""
            INSERT INTO Account (account_name, password, email, creation_date, user_level)
            VALUES (%s, %s, %s, %s, %s);
            """, username, password, email, get_timestamp(), userlevel);
        if result <= 0:
            error = db.get_last_error();
            if error[0] != 1062:
                print "MySQL error:", error;
                return 1;
            else:
                print "That username already exists.";
        else:
            print username, "was successfully added!";
    
    return 0;
    
if __name__ == "__main__":
    import sys;
    sys.exit(main(sys.argv));
