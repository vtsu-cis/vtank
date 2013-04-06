# Generate a database using the VTank/trunk/Docs Generate-MySQL-Database.sql script.

# Insert the path to mysql/bin here.  Do not include the actual program.
# Leave this blank if it's on the PATH.
PATH_TO_MYSQL_BIN = "";

# The executable name.  This should be simply "mysql".
MYSQL_NAME = "mysql";

# The location of the database SQL script to use.
SQL_SCRIPT = "../Docs/Database/MySQL_Database_Layout.sql";

# The rest of the code below shouldn't be modified.
##############################

import sys, os, time;
from sys import exit;

if len(sys.argv) > 1:
	PATH_TO_MYSQL_BIN = sys.argv[1];
	print "Path to MySQL was set to", PATH_TO_MYSQL_BIN;

# Now collect input information.
db_host = raw_input("Database host: ").strip();
db_port = raw_input("Database port: ").strip();
try:
	db_port = int(db_port);
	if db_port < 1 or db_port > 65335:
		raise ValueError, "Invalid number.";
except ValueError, e:
	print "The port must be a number between 1-65335.";
	time.sleep(3);
	exit(1);
db_user = raw_input("Database user: ").strip();
db_pass = raw_input("Database pass: ").strip();
db_name = raw_input("Database name: ").strip();

has_pass = True;
if db_pass == "":
	has_pass = False;

string = "%s%s -h%s -P%s -u%s " % (
	PATH_TO_MYSQL_BIN, MYSQL_NAME, db_host, str(db_port), db_user);

if has_pass:
	string += "-p" + db_pass + " ";
	
string += "%s < %s" % (
	db_name, SQL_SCRIPT);

print "Executing", string, "...";	

cin, cout = os.popen2(string);
cin.close();

output = cout.readlines();
output = "".join(output);

result = cout.close();

if result != None:
	print "Error: The process returned non-successful!";
	
	if output == "":
		print "Is your MySQL path really '" + PATH_TO_MYSQL_BIN + MYSQL_NAME + "'?";
		print "Is the SQL script really '" + SQL_SCRIPT + "'?";
		print "Is the SQL script valid?";
		print "Is the MySQL database up and running?";
	else:
		print "MySQL reported the following:";
		print output;
	time.sleep(3);
	exit(1);

print "Command was successfully executed!";
time.sleep(2);
exit(0);
