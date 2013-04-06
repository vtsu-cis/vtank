###########################################################################
# FILE     : Database.py
# SUBJECT  : Simplify SQL transactions
# AUTHOR   : (C) Copyright 2008 by Vermont Technical College
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
##
# NOTE: This library requires MySQLdb, a third-party python library.
# Installing this on a unix machine is easy.
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

import threading;

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
    