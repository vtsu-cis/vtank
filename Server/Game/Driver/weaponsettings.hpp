/*!
	\file weaponsettings.hpp
	\brief XML pasrser and data storage for weapon settings.
	\author Copyright (C) 2010 by Vermont Technical College
*/
#ifndef WEAPONSETTINGS_HPP
#define WEAPONSETTINGS_HPP

#include <boost/property_tree/ptree.hpp>
#include <boost/property_tree/xml_parser.hpp>
#include <weapon.hpp>

//! Exception thrown when an item wasn't found.
class ItemNotFoundException : std::exception
{
public:
	ItemNotFoundException(const char *message)
		: std::exception(message) {}

	ItemNotFoundException(const std::string &message)
		: std::exception(message.c_str()) {}
};

//! Loads weapon and projectile data from a XML database file.
class Weapon_Settings
{
private:
	std::map<int, EnvironmentProperty> environment_list;
	std::map<int, Projectile> projectile_list;
	std::map<int, Weapon> weapon_list;
	
	//! Load environment data.
	void internal_load_environment();
	
	//! Load projectile data.
	void internal_load_projectiles();
	
	//! Load weapon data.
	void internal_load_weapons();
	
public:
	Weapon_Settings();
	~Weapon_Settings();
	
	//! Load weapon settings into memory.
	/*!
		\throws Throws an exception from Boost's internal XML parser if the file
		doesn't exist, or if the file is corrupted.
	*/
	void load();
	
	//! Get an environment property from this database.
	const EnvironmentProperty get_environment_property(const int &id) const;
	
	//! Get a projectile from this database.
	const Projectile get_projectile(const int &id) const;
	
	//! Get a weapon from this database.
	const Weapon get_weapon(const int &id) const;
};

#endif
