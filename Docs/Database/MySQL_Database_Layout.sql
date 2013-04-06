-- Load using:
-- 	mysql -u[name] -p[password] < MySQL_Database_Layout.sql
DROP DATABASE IF EXISTS VTank;
CREATE DATABASE VTank;
USE VTank;

CREATE TABLE User_Level_Lookup
(
    id INT(3) NOT NULL PRIMARY KEY, -- access level
    name VARCHAR(15) NOT NULL UNIQUE -- name describing user.
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- TODO: May not need this.
ALTER TABLE User_Level_Lookup ADD INDEX (id);

-- Generate data for the User_Level_Lookup table.
INSERT INTO User_Level_Lookup VALUES (-99, "Banned");
INSERT INTO User_Level_Lookup VALUES (-2, "Suspended");
INSERT INTO User_Level_Lookup VALUES (-1, "Troublemaker");
INSERT INTO User_Level_Lookup VALUES (0, "Member");
INSERT INTO User_Level_Lookup VALUES (1, "Contributor"); -- Basically the same as Member, but more valuable.
INSERT INTO User_Level_Lookup VALUES (2, "Tester");
INSERT INTO User_Level_Lookup VALUES (5, "Developer");
INSERT INTO User_Level_Lookup VALUES (10, "Moderator");
INSERT INTO User_Level_Lookup VALUES (99, "Administrator");

CREATE TABLE Ranks
(
    id INT(3) NOT NULL PRIMARY KEY, -- level, higher number == higher rank
    name VARCHAR(32) NOT NULL UNIQUE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- TODO: May not need this.
ALTER TABLE Ranks ADD INDEX (id);

-- Generate data for the Ranks table. 
INSERT INTO Ranks VALUES (0,  "Private");
INSERT INTO Ranks VALUES (1,  "Private 2");
INSERT INTO Ranks VALUES (2,  "Private First Class");
INSERT INTO Ranks VALUES (3,  "Specialist");
INSERT INTO Ranks VALUES (4,  "Corporal");
INSERT INTO Ranks VALUES (5,  "Sergeant");
INSERT INTO Ranks VALUES (6,  "Staff Sergeant");
INSERT INTO Ranks VALUES (7,  "Sergeant First Class");
INSERT INTO Ranks VALUES (8,  "Master Sergeant");
INSERT INTO Ranks VALUES (9,  "First Sergeant");
INSERT INTO Ranks VALUES (10, "Sergeant Major");
INSERT INTO Ranks VALUES (11, "Command Sergeant Major");
INSERT INTO Ranks VALUES (12, "Sergeant Major of the Army");
INSERT INTO Ranks VALUES (13, "Warrant Officer");
INSERT INTO Ranks VALUES (14, "Chief Warrant Officer 2");
INSERT INTO Ranks VALUES (15, "Chief Warrant Officer 3");
INSERT INTO Ranks VALUES (16, "Chief Warrant Officer 4");
INSERT INTO Ranks VALUES (17, "Chief Warrant Officer 5");
INSERT INTO Ranks VALUES (18, "Second Lieutenant");
INSERT INTO Ranks VALUES (19, "First Lieutenant");
INSERT INTO Ranks VALUES (20, "Captain");
INSERT INTO Ranks VALUES (21, "Major");
INSERT INTO Ranks VALUES (22, "Lieutenant Colonel");
INSERT INTO Ranks VALUES (23, "Colonel");
INSERT INTO Ranks VALUES (24, "Brigadier General");
INSERT INTO Ranks VALUES (25, "Major General");
INSERT INTO Ranks VALUES (26, "Lieutentant General");
INSERT INTO Ranks VALUES (27, "General");

CREATE TABLE Account
(
	account_name  VARCHAR(30) NOT NULL,
	password  VARCHAR(51) NOT NULL,
    email	VARCHAR(40) NOT NULL,
	creation_date  VARCHAR(20) NOT NULL,
	last_logged_in  VARCHAR(20) NOT NULL DEFAULT 0,
    rank_level  INT(3) NOT NULL DEFAULT 0,
    user_level INT(3) NOT NULL DEFAULT 0,
    points BIGINT NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

ALTER TABLE Account ADD PRIMARY KEY (account_name);
ALTER TABLE Account ADD INDEX (account_name);

CREATE TABLE Map_Data
(
	filename VARCHAR(30) NOT NULL PRIMARY KEY,
    title	VARCHAR(37) NOT NULL,
    filesize INT(5) NOT NULL,
	map_data  LONGBLOB NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

ALTER TABLE Map_Data ADD INDEX (filename);

CREATE TABLE Projectile
(
	projectile_id  INT(6) NOT NULL PRIMARY KEY AUTO_INCREMENT,
	timeout  INT(4) NOT NULL,
	damage  INT(4) NOT NULL,
	model  VARCHAR(20) NOT NULL,
	speed  INT(5) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

ALTER TABLE Projectile ADD INDEX (projectile_id);

-- Generate a 'test' row for projectiles.
INSERT INTO Projectile (timeout, damage, model, speed)
    VALUES (1333,   30, "shell",        1750);
INSERT INTO Projectile (timeout, damage, model, speed)
    VALUES (900,    4,  "minibullet",   2000);
INSERT INTO Projectile (timeout, damage, model, speed)
    VALUES (1666,   40, "missile",      1500);
INSERT INTO Projectile (timeout, damage, model, speed)
    VALUES (1933,   18, "rocket",      1200);
INSERT INTO Projectile (timeout, damage, model, speed)
	VALUES (0, 5, "beam", 3000);

CREATE TABLE Statistics
(
	tank_name  VARCHAR(12) NOT NULL PRIMARY KEY,
	total_kills  BIGINT NOT NULL DEFAULT 0,
    total_assists  BIGINT NOT NULL DEFAULT 0,
	total_deaths  BIGINT NOT NULL DEFAULT 0,
    total_objectives_completed  BIGINT NOT NULL DEFAULT 0,
    total_objectives_captured  BIGINT NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

ALTER TABLE Statistics ADD INDEX (tank_name);

CREATE TABLE Tank
(
	tank_name  VARCHAR(12) NOT NULL,
	account_name  VARCHAR(12) NOT NULL,
	color  BIGINT NOT NULL,
	weapon_id  INT(5) NOT NULL DEFAULT 1,
    speed_factor  FLOAT NOT NULL DEFAULT 1.0,
    armor_factor  FLOAT NOT NULL DEFAULT 1.0,
	model  VARCHAR(20) NOT NULL,
    points  BIGINT NOT NULL DEFAULT 0,
    rank_level INT(3) NOT NULL DEFAULT 0
	skin VARCHAR(20) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

ALTER TABLE Tank ADD PRIMARY KEY (tank_name);
ALTER TABLE Tank ADD INDEX (tank_name);

CREATE TABLE Utilities
(
	utility_id  	INT(6) NOT NULL PRIMARY KEY AUTO_INCREMENT,
	name            VARCHAR(30) NOT NULL,
	description		VARCHAR(100) NOT NULL,
	duration  	    FLOAT NOT NULL, -- How long the effect lasts.
	damage_factor  	FLOAT NOT NULL, -- % of damage increase.
	speed_factor    FLOAT NOT NULL, -- % of speed increase.
	rate_factor  	FLOAT NOT NULL, -- % of firing rate increase.
	health_increase INT NOT NULL,   -- (Fixed) health points gained.
	health_factor   FLOAT NOT NULL, -- % of health increase.
	model  			VARCHAR(20) NOT NULL
);

-- Generate 'test' rows for Utilities.
INSERT INTO Utilities (name, description, duration, damage_factor, speed_factor, rate_factor, health_increase, health_factor, model)
		VALUES ("Health Pack", "Instantly restores health", 0, 0, 0, 0, 0, 0.5, "insta_health");
INSERT INTO Utilities (name, description, duration, damage_factor, speed_factor, rate_factor, health_increase, health_factor, model) 
		VALUES ("Rapid Fire", "Increases fire rate", 15, 0, 0, .3, 0, 0, "ammo_speed");
INSERT INTO Utilities (name, description, duration, damage_factor, speed_factor, rate_factor,health_increase, health_factor, model)	
		VALUES ("Damage Bonus", "Boosts weapon damage", 15, .25, 0, 0, 0, 0, "ammo_power");
INSERT INTO Utilities (name, description, duration, damage_factor, speed_factor, rate_factor,health_increase, health_factor, model) 
		VALUES ("Speed Boost", "Increases tank speed", 15, 0, .25, 0, 0, 0, "speed_powerup");
--INSERT INTO Utilities (name, description, duration, damage_factor, speed_factor, rate_factor,
--                      health_increase, health_factor, model)
--    VALUES ("Regeneration Pack", "Regenerates your tank's health", 9, 0, 0, 0, 0, 1.0, "health_regen");

CREATE TABLE Weapon
(
	weapon_id  INT(5) NOT NULL PRIMARY KEY AUTO_INCREMENT,
	name  VARCHAR(20) NOT NULL,
	cooldown  FLOAT NOT NULL,
	projectile_id  INT(5) NOT NULL,
	model  VARCHAR(20) NOT NULL,
    sound_effect  VARCHAR(20) NOT NULL,
	can_charge BOOLEAN NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Generate 'test' row for Weapon.
INSERT INTO Weapon (name, cooldown, projectile_id, model, sound_effect)
    VALUES ("Heavy Cannon", 1.5,    3,  "Heavy Cannon", "scannont1");
INSERT INTO Weapon (name, cooldown, projectile_id, model, sound_effect)
    VALUES ("Minigun",          0.1,    2,  "Minigun",      "minigun");
INSERT INTO Weapon (name, cooldown, projectile_id, model, sound_effect)
    VALUES ("Rocket",          0.5,    4,  "Rocket",      "scannont1");
INSERT INTO Weapon (name, cooldown, projectile_id, model, sound_effect)
	VALUES ("Laser Cannon",    0.5,     5,  "Laser",         "laser")

-- ALTER TABLE Weapon ADD PRIMARY KEY (weapon_id);
ALTER TABLE Statistics ADD FOREIGN KEY R_1 (tank_name) REFERENCES Tank(tank_name)
    ON UPDATE CASCADE
    ON DELETE CASCADE;
ALTER TABLE Tank ADD FOREIGN KEY R_2 (account_name) REFERENCES Account(account_name)
    ON UPDATE CASCADE
    ON DELETE CASCADE; -- Do not allow the account to be deleted without the tanks being deleted.
ALTER TABLE Weapon ADD FOREIGN KEY R_5 (projectile_id) REFERENCES Projectile(projectile_id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT; -- Set to a 'default' projectile ID if the projectile is deleted.
ALTER TABLE Account ADD FOREIGN KEY R_7 (user_level) REFERENCES User_Level_Lookup(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT; -- Set to the default userlevel.
ALTER TABLE Account ADD FOREIGN KEY R_8 (rank_level) REFERENCES Ranks(id)
    ON UPDATE CASCADE
    ON DELETE RESTRICT; -- Set to default rank.

-- Permissions
-- Permissions to update/select/delete should be done manually since it requires a password.
GRANT SELECT ON VTank.Weapon TO 'public'@'%' IDENTIFIED BY 'goldfish';
GRANT SELECT ON VTank.Utilities TO 'public'@'%' IDENTIFIED BY 'goldfish';
GRANT SELECT ON VTank.Statistics TO 'public'@'%' IDENTIFIED BY 'goldfish';
GRANT SELECT ON VTank.Projectile TO 'public'@'%' IDENTIFIED BY 'goldfish';
GRANT SELECT ON VTank.Map_Data TO 'public'@'%' IDENTIFIED BY 'goldfish';
GRANT SELECT ON VTank.User_Level_Lookup TO 'public'@'%' IDENTIFIED BY 'goldfish';

-- New database: VTankPending
-- The VTankPending database holds new data to be loaded next patch.
DROP DATABASE IF EXISTS VTankPending;
CREATE DATABASE VTankPending;
USE VTankPending;

-- Create mirror images (that should also keep all of the structure of the original) of the VTank database tables.
CREATE TABLE Projectile LIKE VTank.Projectile;
CREATE TABLE Weapon LIKE VTank.Weapon;
CREATE TABLE Utilities LIKE VTank.Utilities;
CREATE TABLE Map_Data LIKE VTank.Map_Data;
