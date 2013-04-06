#!/usr/bin/python
# \file fullsync.py
# \brief This script synchronizes the VTank Account table and the Website auth_user table 
# \author Michael Smith
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
	import MySQLdb
except ImportError, e:
	print "This module requires the python MySQLdb module.";
	print "Download it at http://sourceforge.net/projects/mysql-python.";
	print "Import error:", e;
	from sys import exit;
	exit(1);

from time import strftime

def get_timestamp():
        return strftime("%Y-%m-%d %H:%M:%S")

def website_insert(table, username, info):
        """
        Constructs a query that inserts the full credentials into the Website database for
         a user that exists in the VTank game database.
        """

	query = """INSERT INTO auth_user VALUES(NULL, '%s', '', '', '%s', '%s',
	 0, 1, 0, '%s', '%s');""" % (username, info[1], info[0], get_timestamp(), get_timestamp())

	db.execute(query)

def vtank_insert(table, username, info):
        """
        Constructs a query that inserts the full credentials into the VTank database for a user
        that exists in the Website database.
	"""

        query = "INSERT INTO Account VALUES('%s', '%s', '%s', '%s', '%s', 0, 5);" % \
	(username, info[0], info[1], get_timestamp(), get_timestamp())

	db.execute(query)

def get_secondary_fields(table, column, user):
	"""
	Gets the password, email and creation date for users missing from its sister table.
	"""
	global db;
        if table is "Account":
		query = "SELECT password, email, creation_date FROM %s WHERE %s ='%s';" % \
		(table, column, user)

	elif table is "auth_user":
		query = "SELECT password, email date_joined FROM %s WHERE %s ='%s';" % \
		(table, column, user)

        db.execute (query)
        row = db.fetchone ()
        return row

def add_user(table, username, info):
	"""
	Adds the missing users to the corresponding table, note that if table is Account, the 
	users will be inserted into the website account table, and vice versa.
	"""
	global db;
	if table is "Account":
		query = website_insert(table, username, info)
	elif table is "auth_user":
		query = vtank_insert(table, username, info)

def sync(table1, table2, column1, column2):
	"""
	This is the primary function if this script, it does a left join to identify and select
	users that are missing from table2 with respect to table 1, then takes the relevant 
	credentials and inserts a new user into table2. 
	"""
	#Initiate remote MySQL connection
        conn = MySQLdb.connect (host = "155.42.234.33", 
				port=3306, 
				user = "daemon", 
				passwd = "", 
				db = "VTank")

        global db
        db = conn.cursor ()

	#Select missing users.
 	query = "SELECT %s.* FROM %s LEFT JOIN %s ON %s.%s = %s.%s WHERE %s.%s IS NULL;" % \
	(table1, table1, table2, table1, column1, table2, column2, table2, column2)
	db.execute (query)
        results = []

	#Append all missing users to a list on the fly.
        while (1):
                row = db.fetchone ()
                if row == None:
                        break
		
	        if table1 == "auth_user":
                        results.append(row[1])
                elif table1 is "Account":
                        results.append(row[0])

	#Get the secondary information for each user, then add them.
        for user in results:
        	if table1 == "Account":
                        getinfo = get_secondary_fields(table1, column1, user)
                        add_user(table1, user, getinfo)
		elif table1 == "auth_user":
			getinfo = get_secondary_fields(table1, column1, user)
			add_user(table1, user, getinfo)
	
	#Commit changes, then close.
	conn.commit()
        db.close()

