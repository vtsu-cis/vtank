###########################################################################
# \file Procedures.py
# \brief Stored SQL procedures.
# \author Copyright 2009 by Vermont Technical College
###########################################################################

def account_exists():
    """
    Generate a SQL statement that checks if an account exists.
    First parameter: account name to search for.
    @return SQL-ready query statement.
    """
    return "SELECT account_name FROM Account WHERE account_name = %s LOCK IN SHARE MODE;";

def get_account():
    """
    Generate a SQL statement for searching for a specific account.
    First parameter: account name to search for.
    Second parameter: password.
    @return SQL-ready query statement.
    """
    return "SELECT account_name, user_level, password FROM Account " \
        "WHERE account_name = %s LOCK IN SHARE MODE;";
        
def get_account_list():
    """
    Generate a SQL statement for searching for a specific account.
    @return SQL-ready query statement.
    """
    
    return "SELECT account_name, email, creation_date, last_logged_in, rank_level, user_level, points FROM Account;";

def get_userlist():
    """
    Generate a SQL statement that gets a full userlist.
    @return SQL-ready query statement.
    """
    return "SELECT account_name, email, creation_date, last_logged_in, rank_level, user_level "\
        "FROM Account LOCK IN SHARE MODE;";

def get_account_info():
    """
    Generate a SQL statement that gets information about an account.
    First parameter: account name to search for.
    @return SQL-ready query statement.
    """
    return "SELECT email, creation_date, last_logged_in, rank_level, user_level, points "\
        "FROM Account WHERE account_name = %s LOCK IN SHARE MODE;";
        
def update_account():
    """
    Generate a SQL statement that allows the user to set the last
    login time of a given account.
    First parameter: last login time.
    Second parameter: account name to search for.
    @return SQL-ready update statement.
    """
    return "UPDATE Account SET last_logged_in = %s " \
        "WHERE account_name = %s;";

def complete_update_account():
    """
    Generate a SQL statement that allows the user to update every single field of
    a given account.
    First parameter: account name.
    Second parameter: new email address.
    Third parameter: new creation date.
    Forth parameter: new login date.
    Fifth parameter: new rank level.
    Sixth parameter: new user level.
    Seventh parameter: new account name.
    @return SQL-ready update statement.
    """
    return "UPDATE Account SET account_name = %s, email = %s, creation_date = %s, last_logged_in = %s, "\
        "rank_level = %s, user_level = %s WHERE account_name = %s;";

def new_account():
    """
    Generate a SQL statement that allows the user to create a new account.
    First parameter: account name.
    Second parameter: password.
    Third parameter: creation date, or today.
    Forth parameter: last login time, should be set to 0 or the creation date.
    Fifth parameter: rank of the new-comer.
    Sixth parameter: level of privilege for the user.
    Seventh parameter: e-mail belonging to the account.
    @return SQL-ready insert statement.
    """
    return "INSERT INTO Account " \
        "(account_name, password, creation_date, last_logged_in, rank_level, user_level, " \
        "email) VALUES (%s, %s, %s, %s, %s, %s, %s);";
        
        
        
def set_user_level():
    """
    Generate a SQL statement that allows a user to set the user_level of a user
    First parameter: new user level
    Second parameter: account name 
    """
    
    return "UPDATE Account SET user_level=%s WHERE account_name=%s"
        
def get_tank_list():
    """
    Generate a SQL statement that grabs all tanks from the database
    that belongs to a certain account name.
    First parameter: account name to search for.
    @return SQL-ready query statement.
    """
    return "SELECT tank_name, color, weapon_id, speed_factor, armor_factor, points, model, skin, rank_level "\
        "FROM Tank WHERE account_name = %s LOCK IN SHARE MODE;";
        
def get_tank():
    """
    Generate a SQL statement that gets information about a tank.
    First parameter: tank name to search for.
    @return SQL-ready query statement.
    """
    return "SELECT tank_name, color, weapon_id, speed_factor, armor_factor, points, model, skin, rank_level "\
        "FROM Tank WHERE tank_name = %s LOCK IN SHARE MODE;";

def tank_exists():
    """
    Generate a SQL statement capable of checking if a tank name exists. It simply
    queries for all tanks by the name of the first parameter.
    First parameter: tank name to search for.
    @return SQL-ready query statement.
    """
    return "SELECT tank_name FROM Tank WHERE tank_name = %s LOCK IN SHARE MODE;";

def tank_exists_under_account():
    """
    Generate a SQL statement capable of checking if a tank name exists. This is basically
    the same thing as the Procedures.tank_exists() method, except it verifies that
    a certain account owns the tank.
    First parameter: tank name to search for.
    Second parameter: account name to search for.
    @return SQL-ready query statement.
    """
    return "SELECT tank_name FROM Tank WHERE tank_name = %s AND account_name = %s LOCK IN SHARE MODE;";

def new_tank():
    """
    Generate a SQL statement that inserts a new tank into the database.
    First parameter: tank name.
    Second parameter: account name, should be the owner of the tank.
    Third parameter: speed factor of the tank (float)
    Forth parameter: armor factor of the tank (float)
    Fifth parameter: color of the tank.
    Sixth parameter: ID of the weapon the tank has equipped.
    Seventh parameter: Model name.
    @return SQL-ready insert statement.
    """
    return "INSERT INTO Tank (tank_name, account_name, speed_factor, armor_factor, "\
        "color, weapon_id, model, skin) VALUES (%s, %s, %s, %s, %s, %s, %s, %s);";

def new_statistics():
    """
    Generate a SQL statement that inserts a new statistics set into the database.
    First parameter: tank name.
    @return SQL-ready insert statement.
    """
    return "INSERT INTO Statistics (tank_name) VALUES (%s);";

def update_statistics():
    """
    Generate a SQL statement that updates a set of existing statistics.
    By default, simply inserting statistics will overwrite them. You'll have to manually
    prepend such statements as this: 'total_kills + ' + total_kills if you want to increment.
    First parameter: total kills.
    Second parameter: total assists.
    Third parameter: total deaths.
    Forth parameter: total objectives completed.
    Fifth parameter: total objectives captured.
    """
    return "UPDATE Statistics SET total_kills=%s, total_assists=%s, total_deaths=%s, "\
        "total_objectives_completed=%s, total_objectives_captured=%s WHERE tank_name = %s;"
        
def get_tank_statistics():
    """
    Generate a SQL statement that gets a tank's statistics.
    First parameter: tank name. 
    """
    return "SELECT * FROM Statistics WHERE tank_name = %s;"

def update_tank():
    """
    Generate a SQL statement that updates an existing tank.
    First parameter: new speed factor (float).
    Second parameter: new armor factor (float).
    Third parameter: new color.
    Forth parameter: new weapon ID.
    Fifth parameter: new model.
    Sixth parameter: tank name to search for.
    @return SQL-ready insert statement.
    """
    return "UPDATE Tank SET speed_factor = %s, armor_factor = %s, "\
        "color = %s, weapon_id = %s, model = %s, skin = %s WHERE tank_name = %s;";

def get_tank_and_points():
    """
    Generate a SQL statement that retrieves a tank's points and its name (for comparison of 
    successful/failed retrievals).
    First parameter: tank name.
    """
    return "SELECT b.tank_name, a.points FROM Account a JOIN Tank b ON b.account_name=a.account_name WHERE b.tank_name = %s"

def get_tanks_and_points(tanks):
    """
    Generate a SQL statement that retrieves the points of many tanks.
    First parameter: A tuple of tank names
    """
    
    tank_query_string = "SELECT b.tank_name, a.points FROM Account a JOIN Tank b ON b.account_name=a.account_name WHERE b.tank_name = ";
    count = 0;
        
    for tank_name in tanks:
        if count == 0:
            tank_query_string += "\"%s\"" % tank_name; 
        else:
            tank_query_string += " or tank_name = \"%s\"" % tank_name;
        count +=1;
    
    return tank_query_string;

def get_account_from_tank():
    """
    Generate a SQL statement that retrieves an account name when a
    tank name is provided.
    First parameter: tank name.
    """
    
    return "SELECT account_name FROM Tank WHERE tank_name = %s";
        
def get_account_points():
    """
    Generate a SQL statement that gets an account's points.
    First parameter: account name.
    """
    
    return "SELECT points FROM Account WHERE account_name = %s";
    
def update_account_points():
    """
    Generate a SQL statement that updates an account's points.
    First parameter: new number of points.
    Second parameter: account name.
    """
    
    return "UPDATE Account SET points = %s WHERE account_name = %s";

def update_account_rank():
    """
    Generate a SQL statement that updates an account's rank.
    First parameter: new rank.
    Second parameter: account name.
    """
    
    return "UPDATE Account SET rank_level = %s WHERE account_name = %s";

def get_tank_points():
    """
    Generate a SQL statement that gets a tank's points.
    First parameter: tank name.
    """
    
    return "SELECT points FROM Tank WHERE tank_name = %s";

def update_tank_points():
    """
    Generate a SQL statement that updates a tank's points.
    First parameter: tank's points.
    Second parameter: tank's name.
    """
    return "UPDATE Tank SET points = %s WHERE tank_name = %s;";

def update_tank_rank():
    """
    Generate a SQL statement that updates a tank's rank.
    First parameter: new tank rank.
    Second parameter: tank's name.
    """
    return "UPDATE Tank SET rank_level = %s WHERE tank_name = %s;";

def update_tank_rank_and_points():
    """
    Generate a SQL statement that updates both a tank's rank and his points.
    First parameter: tank's points.
    Second parameter: tank's new rank.
    Third parameter: tank's name.
    """
    return "UPDATE Tank SET points = %s, rank_level = %s WHERE tank_name = %s;";

def delete_tank():
    """
    Generate a SQL statement that deletes an existing tank.
    First parameter: tank name to search for (and delete).
    @return SQL-ready insert statement.
    """
    return "DELETE FROM Tank WHERE tank_name = %s;";

def get_map_data():
    """
    Generate a SQL statement that grabs map data for a specific map from the DB.
    First parameter: filename of the map.
    @return SQL-ready query statement.
    """
    return "SELECT map_data FROM Map_Data WHERE filename = %s LOCK IN SHARE MODE;";

def new_map():
    """
    Generate a SQL statement to insert a new map into the database.
    First parameter: title of the map.
    Second parameter: filename of the map.
    Third parameter: binary map data.
    @return SQL-ready insert statement.
    """
    return "INSERT INTO Map_Data (title, filename, filesize, map_data) VALUES (%s, %s, %s, %s);";

def update_map():
    """
    Generate a SQL statement to update the map information in the database.
    First parameter: new title of the map.
    Second parameter: new filename of the map.
    Third parameter: new binary map data.
    Forth parameter: filename to search for.
    @return SQL-ready insert statement.
    """
    return "UPDATE Map_Data SET title = %s, filename = %s, map_data = %s " \
        "WHERE filename = %s;";
    
def delete_map():
    """
    Generate a SQL statement that will delete the map from the database.
    First parameter: filename of the map.
    @return SQL-ready insert statement.
    """
    return "DELETE FROM Map_Data WHERE filename = %s;";

def get_weapon_list():
    """
    Generate a SQL statement that will grab a full list of weapons.
    @return SQL-ready query statement.
    """
    return "SELECT weapon_id, name FROM Weapon LOCK IN SHARE MODE;";

def get_projectile_list():
    """
    Generate a SQL statement that will grab a full list of projectiles.
    @return SQL-ready query statement.
    """
    return "SELECT projectile_id, timeout, damage, model, speed FROM Projectile LOCK IN SHARE MODE;";

def get_utilities_list():
    """
    Generate a SQL statement that will grab a full list of utilities.
    @return SQL-ready query statement.
    """
    return "SELECT utility_id, name, description, duration, damage_factor, speed_factor,\
     rate_factor, health_increase, health_factor, model FROM Utilities LOCK IN SHARE MODE;"

def get_map_list():
    """
    Generate a SQL statement that will grab the full list of maps (only filenames/filesizes).
    @return SQL-ready query statement.
    """
    return "SELECT filename, filesize FROM Map_Data LOCK IN SHARE MODE;";

def get_map():
    """
    Generate a SQL statement that will grab a single map from the database.
    First parameter: filename of the map.
    @return SQL-ready query statement.
    """
    return "SELECT filename, title, filesize, map_data FROM Map_Data WHERE filename = %s LOCK IN SHARE MODE;";

def get_mysql_version():
    """
    Generate a SQL statement that pings MySQL for its current version.
    """
    
    return "SELECT VERSION();"
