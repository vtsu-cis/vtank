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

def sync(username, newpass):
	"""
	This function does a synchronization of ingame/website passwords for a single user.  As it
	should only be called on password change events, it unconditionally updates the game 
	password to reflect the new website password.
	"""	
        #Initiate a remote database connection. (Works even if it's on the localhost if the IP is right.)
        conn = MySQLdb.connect (host = "155.42.234.33",
                                port=3306,
                                user = "daemon",
                                passwd = "",
                                db = "VTank")

	db = conn.cursor ()

	#TODO Test this function and complete implementation by adding it to the SCT 
	# password change forms.
	query = "UPDATE Account SET password='%s' WHERE account_name='%s'" \
	% (newpass, username)
	db.execute (query) 

        #Commit & close
        conn.commit()
        db.close()

def fullsync():
	"""
	This function does a blanket synchronization of ingame game passwords with the website
	passwords.  It selects the username/passwords from the website user table, then the password
	for that user from the game table and compares the two.  If they are not the same, it will
	update the game password to be the same as the website one.
	"""
	#Initiate a remote database connection. (Works even if it's on the localhost if the IP is right.)
	conn = MySQLdb.connect (host = "155.42.234.33",
        	                port=3306,
                	        user = "daemon",
                        	passwd = "",
                        	db = "VTank")

	#Declare variables	
	usernames = []
	passwords = []
	db = conn.cursor ()
	count = 0

	#Make initial SELECT query
	query = "SELECT username, password FROM  auth_user"
	db.execute (query)

	#Append all username/passwords from the first query into lists
	while (1):
	        row = db.fetchone ()
	        if row is None:
	                break

		usernames.append(row[0])
		passwords.append(row[1])

	#Select game password for each username
	for username in usernames:
		query = "SELECT password FROM Account WHERE account_name='%s'" % (username)
		db.execute (query)
		row = db.fetchone ()

		#Check passwords against each other, update the differences
        	if passwords[count] != row[0]:
	        	subquery = "UPDATE Account SET password='%s' WHERE account_name='%s';" \
                        % (passwords[count], username)
                        db.execute (subquery)
		count=count+1	

	#Commit & close 
	conn.commit()
	db.close()
