#include <master.hpp>
#include <weaponsettings.hpp>
#include <boost/foreach.hpp>
#include <logger.hpp>
using boost::property_tree::ptree;

const static std::string WEAPON_XML_FILE = "Weapons.xml";
const static std::string PROJECTILE_XML_FILE = "Projectiles.xml";
const static std::string ENVIRONMENT_XML_FILE = "EnvironmentProperties.xml";

template<class T>
static T get_node_value(ptree::value_type &v, const std::string &node_name)
{
	return v.second.get_child(node_name).get<T>("");
}

template<class T>
static T get_node_value(ptree::value_type &v, const std::string &node_name, const T &default_value)
{
	try {
		return v.second.get_child(node_name).get<T>("", default_value);
	}
	catch (const std::exception &) {
		return default_value;
	}
}

Weapon_Settings::Weapon_Settings()
{
}

Weapon_Settings::~Weapon_Settings()
{
}

void Weapon_Settings::load()
{
	internal_load_environment();
	internal_load_projectiles();
	internal_load_weapons();
}

void Weapon_Settings::internal_load_environment()
{
	ptree data_tree;

	read_xml(ENVIRONMENT_XML_FILE, data_tree);
	
	BOOST_FOREACH(ptree::value_type &v, data_tree.get_child("environmentProperties")) {
		EnvironmentProperty env;
		env.id					= get_node_value<int>(v, "id");
		env.name				= get_node_value<std::string>(v, "name");
		env.spawn_on_wall_hit	= get_node_value<bool>(v, "triggersUponImpactWithEnvironment", false);
		env.spawn_on_player_hit = get_node_value<bool>(v, "triggersUponImpactWithPlayer", false);
		env.spawn_on_expiration = get_node_value<bool>(v, "triggersUponExpiration", false);
		env.duration_seconds	= get_node_value<float>(v, "duration", 0.0f);
		env.interval_seconds	= get_node_value<float>(v, "interval", 0.0f);
		env.aoe_radius			= get_node_value<float>(v, "areaOfEffectRadius", 0.0f);
		env.aoe_decay			= get_node_value<float>(v, "areaOfEffectDecay", 0.0f);
		env.minimum_damage		= get_node_value<int>(v, "minDamage", 0);
		env.maximum_damage		= get_node_value<int>(v, "maxDamage", 0);

		environment_list[env.id] = env;
	}
}

void Weapon_Settings::internal_load_projectiles()
{
	ptree data_tree;

	read_xml(PROJECTILE_XML_FILE, data_tree);

	BOOST_FOREACH(ptree::value_type &v, data_tree.get_child("projectiles")) {
		Projectile projectile;
		projectile.id					 = get_node_value<int>(v, "id");
		projectile.name					 = get_node_value<std::string>(v, "name");
		projectile.aoe_radius			 = get_node_value<float>(v, "areaOfEffectRadius", 0.0f);
		projectile.aoe_is_cone			 = get_node_value<bool>(v, "areaOfEffectUsesCone", false);
		projectile.aoe_decay			 = get_node_value<float>(v, "areaOfEffectDecay", 0.0f);
		projectile.cone_origin_width	 = get_node_value<int>(v, "coneOriginWidth", 0);
		projectile.cone_radius			 = get_node_value<float>(v, "coneRadius", 0.0f);
		projectile.cone_damage_full_area = get_node_value<bool>(v, "coneDamagesEntireArea", false);
		projectile.minimum_damage		 = get_node_value<int>(v, "minDamage", 0);
		projectile.maximum_damage		 = get_node_value<int>(v, "maxDamage", 0);
		projectile.is_instantaneous		 = get_node_value<bool>(v, "isInstantaneous", false);
		projectile.initial_velocity		 = get_node_value<float>(v, "initialVelocity", 0.0f);
		projectile.terminal_velocity	 = get_node_value<float>(v, "terminalVelocity", 0.0f);
		projectile.acceleration			 = get_node_value<float>(v, "acceleration", 0.0f);
		projectile.range				 = get_node_value<int>(v, "range", 0);
		projectile.range_variation		 = get_node_value<int>(v, "rangeVariation", 0);
		projectile.jump_count			 = get_node_value<int>(v, "jumpCount", 0);
		projectile.jump_range			 = get_node_value<int>(v, "jumpRange", 0);
		projectile.jump_decay			 = get_node_value<float>(v, "jumpDamageDecay", 0.0f);
		projectile.collision_radius		 = get_node_value<float>(v, "collisionRadius", BULLET_RADIUS);
		projectile.object_damage_factor  = get_node_value<float>(v, "objectDamageFactor", 1.0f);
		int environment_id				 = get_node_value<int>(v, "environmentEffectID", -1);
		
		if (environment_id > -1)
			projectile.environment_property = &environment_list[environment_id];
		else
			projectile.environment_property = NULL;

		projectile_list[projectile.id] = projectile;
	}
}

void Weapon_Settings::internal_load_weapons()
{
	ptree data_tree;

	read_xml(WEAPON_XML_FILE, data_tree);

	BOOST_FOREACH(ptree::value_type &v, data_tree.get_child("weapons")) {
		Weapon weapon;
		weapon.id								   = get_node_value<int>(v, "id");
		weapon.name								   = get_node_value<std::string>(v, "name");
		weapon.cooldown							   = get_node_value<float>(v, "cooldown", 0.0f);
		weapon.launch_angle						   = RADIANS_F(get_node_value<float>(v, "launchAngle", 0.0f));
		weapon.max_charge_time_seconds			   = get_node_value<float>(v, "maxChargeTime", 0.0f);
		weapon.projectiles_per_shot				   = get_node_value<int>(v, "projectilesPerShot", 1);
		weapon.interval_between_projectile_seconds = get_node_value<float>(v, "intervalBetweenEachProjectile", 0.0f);
		weapon.overheat_time					   = get_node_value<float>(v, "overheatTime", 0.0f);
		weapon.overheat_amount_per_shot			   = get_node_value<float>(v, "overheatAmountPerShot", 0.0f);
		weapon.overheat_recovery_speed			   = get_node_value<float>(v, "overheatRecoverySpeed", 0.0f);
		weapon.overheat_recover_start_time		   = get_node_value<float>(v, "overheatRecoverStartTime", 0.0f);
		weapon.linear_factor					   = get_node_value<float>(v, "linearFactor", 0.0f);
		weapon.exponent							   = get_node_value<float>(v, "exponent", 0.0f);
		weapon.projectile						   = projectile_list[get_node_value<int>(v, "projectileID")];
		
		weapon_list[weapon.id] = weapon;
	}
}

const EnvironmentProperty Weapon_Settings::get_environment_property(const int &id) const
{
	std::map<int, EnvironmentProperty>::const_iterator i = environment_list.find(id);
	if (i == environment_list.end()) {
		std::ostringstream formatter;
		formatter << "Environment property not found: " << id;
		throw ItemNotFoundException(formatter.str());
	}

	return i->second;
}

const Projectile Weapon_Settings::get_projectile(const int &id) const
{
	std::map<int, Projectile>::const_iterator i = projectile_list.find(id);
	if (i == projectile_list.end()) {
		std::ostringstream formatter;
		formatter << "Projectile not found: " << id;
		throw ItemNotFoundException(formatter.str());
	}

	return i->second;
}

const Weapon Weapon_Settings::get_weapon(const int &id) const
{
	std::map<int, Weapon>::const_iterator i = weapon_list.find(id);
	if (i == weapon_list.end()) {
		std::ostringstream formatter;
		formatter << "Weapon not found: " << id;
		throw ItemNotFoundException(formatter.str());
	}

	return i->second;
}
