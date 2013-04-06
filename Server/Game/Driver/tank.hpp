/*!
    \file   tank.hpp
    \brief  Blueprint for the Tank class.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef TANK_HPP
#define TANK_HPP

#include <player.hpp>
#include <damageableobject.hpp>
#include <weapon.hpp>

#define DEFAULT_MAX_CHARGE_TIME 3000

//! Tracks how long a player's weapon has been charging.
struct InternalChargeTimer
{
	bool is_charging;
	double elapsed;
	double last_time_stamp;
	double maximum;

	InternalChargeTimer() 
		: is_charging(false), elapsed(0), last_time_stamp(get_current_time()), 
		maximum(DEFAULT_MAX_CHARGE_TIME)
	{
	}

	void start_charging()
	{
		last_time_stamp = get_current_time();
		elapsed = 0;
		is_charging = true;
	}

	void stop_charging()
	{
		is_charging = false;
		last_time_stamp = 0;
		elapsed = 0;
	}

	void advance(double rate_of_fire_factor = 0)
	{
		if (is_charging && elapsed < maximum) {
			const double current_time = get_current_time();
			if (rate_of_fire_factor <= 0) {
				elapsed += (current_time - last_time_stamp);
			}
			else {
				const double delta = current_time - last_time_stamp;
				elapsed += delta + (delta * rate_of_fire_factor);
			}
			last_time_stamp = current_time;
		}

		if (elapsed > maximum) {
			elapsed = maximum;
		}
	}

	//! Modify a weapon's damage with this charge timer's charge value.
	/**
		The algorithm for modifying damage is:
		ceil(base_damage + (((elapsed_time / maximum_time) * 100) / 2.5))
	*/
	int modify_damage(const int base_damage, const double linear_factor, const double exponent)
	{
		// Take charge value as a percentage of it's maximum.
		const double new_damage = ceil(base_damage + 
			pow((((elapsed / maximum) * 100.0) / linear_factor), exponent));
		
		return static_cast<int>(new_damage);
	}
};

typedef boost::shared_ptr<InternalChargeTimer> charge_ptr;

/*!
    The Tank class is, in reality, an instance of a player. Tank is not an Ice servant;
    instead, it's job is to encapsulate all data belonging to the player. It also prevents
    more than one thread from accessing the data simultaneously. Additionally it can perform
    calculations needed for things like collision detection.
*/
class Tank : Damageable_Object
{
private:
	//! Struct which handles timing utilities.
	struct AppliedUtility
	{
		VTankObject::Utility utility;
		long time_left;
		long last_step;
		long delta;

		AppliedUtility(const VTankObject::Utility &util)
		{
			utility = util;
			time_left = static_cast<long>(util.duration * 1000.0f);
			last_step = static_cast<long>(IceUtil::Time::now().toMilliSeconds());
			delta = 0;
		}

		void step()
		{
			long current_time = static_cast<long>(IceUtil::Time::now().toMilliSeconds());
			delta = current_time - last_step;
			time_left -= delta;
			last_step = current_time;
		}

		bool has_expired()
		{
			return time_left <= 0;
		}
	};

    GameSession::Tank tank;
    player_ptr player;
    VTankObject::Direction move_direction;
    VTankObject::Direction rotate_direction;
    IceUtil::Int64 offset;
    long respawns_at;
    int node;
    double velocity;
    double angle_velocity;
    std::vector<int> assist_hitters;
	std::vector<AppliedUtility> applied_utilities;
	bool ready;
	charge_ptr charge_timer;
	Weapon weapon;

    Ice::Identity ice_id;
    boost::mutex mutex;
    boost::thread sync_thread; // Thread which executes the clock sync.

public:
    /*!
        Initialize the game tank to a GameSession::Tank.
        \param tank Tank to use.
        \param player Player servant.
        \param team Team which the player belongs to.
    */
    Tank(const GameSession::Tank &, const player_ptr player, const GameSession::Alliance);

    /*!
        The destructor does nothing significant.
    */
   ~Tank();

    /*!
        Get the ID belonging to this tank.
        \return ID number that identifies this tank.
    */
    int get_id() const {
		return tank.id;
    }

	//! Sets the temporary in-game ID for this tank.
	void set_id(const int new_id)
	{
		tank.id = new_id;
	}

    /*!
        Get the player's name.
        \return Name of the player's tank.
    */
    const std::string get_name() const;

    /*!
        Get the position of this tank.
        \return Position of the tank as a VTankObject::Point.
    */
    VTankObject::Point get_position() const;

    /*!
        Get the pre-calculated velocity for this tank.
        \return Velocity of the tank.
    */
    const double get_velocity();

    /*!
        Get the pre-calculated angular velocity for this tank.
        \return Angle velocity for the tank.
    */
    const double get_angular_velocity();

	/*!
		Get whether this player is ready to receive messages.
		\return True if the player is ready; false otherwise.
	*/
	const bool is_ready() const {
		return ready;
	}
	
	/*!
		Set whether the client is ready to receive messages.
	*/
	const void set_ready(bool ready_value) {
		ready = ready_value;
	}

    /*!
        Set a new position for this tank.
        \param position New point to set the tank at.
    */
    void set_position(const VTankObject::Point &);

    /*!
        Get the team to which this player belongs.
        \return GameSession::Alliance enumeration value indicating this player's team.
    */
    GameSession::Alliance get_team() const;

    /*!
        Set the team to which this player belongs.
        \param team Team to set for this player.
    */
    void set_team(const GameSession::Alliance &);

    /*!
        Check if this tank is allied to another tank.
        \param tank Tank to test against.
        \return True if the two are allied, otherwise false.
    */
    const bool is_allied(const int);

    /*!
        Get the angle of the tank.
        \return Angle (in radians) of the tank.
    */
    const double get_angle();

    /*!
        Set a new value for the angle of the tank.
        \param new_angle Angle (in radians) to set for the tank.
    */
    void set_angle(const double);

    /*!
        Get the health of the tank.
        \return Health value of the tank.
    */
    int get_health() const;

    /*!
        Set a new health value for the client.
        \param new_health Health amount.
    */
    void set_health(const int);

    /*!
        Inflict some damage on the tank. If the value is negative, the tank is
        healed for that amount instead.
        \param damage Damage to inflict.
		\param projectile_id ID of the projectile.
		\param projectile_type_id ID of the type of the projectile.
        \param owner Player who fired the projectile (for statistics purposes).
    */
    void inflict_damage(const int, const int, const int, const int);

	/*!
		Inflict some environmental damage to the tank. If the value is negative,
		the tank is healed for that amount instead.
		\param damage Damage to inflict.
		\param env_id ID of the environment effect.
		\param env_type_id Type of environmental effect.
		\param owner ID of the person who spawned the effect.
	*/
	void inflict_environment_damage(const int damage, const int env_id,
		const int env_type_id, const int owner);

    /*!
        Get the speed factor (as a percentage) of the tank.
        \return Speed factor as a decimal.
    */
    const float get_speed_factor();

    /*!
        Get the armor factor (as a percentage) of the tank.
        \return Armor factor of the tank.
    */
    float get_armor_factor() const;

	/*!
		Check to see if this tank has any increased damage due to utilities.
		\return Damage factor of the tank. If no buffs are applied, this value
		will always be '1'.
	*/
	const float get_damage_factor();

	/*!
		Get the tank's weapon.
	*/
	const Weapon get_weapon() const
	{
		return weapon;
	}

    /*!
        Access to a copy of the tank object. This is used to deliver information to the
        player.
        \return Copy of the inner GameSession::Tank object.
    */
    const GameSession::Tank get_tank_object();

    /*!
        Get the direction in which the tank is moving, if any.
        Possible values are: FORWARD, REVERSE, NONE.
        \return Direction that the tank is moving towards.
    */
    const VTankObject::Direction get_movement_direction();

    /*!
        Set the direction that the tank is moving towards, if any.
        Accepted values are: FORWARD, REVERSE, NONE.
        \param direction New direction for the tank to set.
    */
    void set_movement_direction(const VTankObject::Direction);

    /*!
        Get the direction in which the tank is rotating, if any.
        Valid values are: LEFT, RIGHT, NONE.
        \return Direction that the tank is rotating towards.
    */
    const VTankObject::Direction get_rotation_direction();

    /*!
        Set the direction that the tank is rotating towards, if any.
        Accepted values are: LEFT, RIGHT, NONE.
        \param direction Direction to set.
    */
    void set_rotation_direction(const VTankObject::Direction);

    /*!
        Get the node at which this tank is assigned.
        \return Node ID assigned to the tank.
    */
    const int get_node_id();

    /*!
        Assign the tank a new node ID, which indicates it's position.
        \param id New node ID to assign.
    */
    void set_node_id(const int);

    /*!
        Get the Ice identity for this tank.
        \return Ice identity.
    */
    const Ice::Identity get_ice_id() const;

    /*!
        Set a new Ice identity for this tank.
        \param id Ice identity to set.
    */
    void set_ice_id(const Ice::Identity &);

    /*!
        Perform any actions necessary to make this player respawn.
        This does not include player notification.
        This also does not include position setting.
    */
    void respawn();

    /*!
        Get the timestamp in which this tank will respawn.
        \return Timestamp for when the tank will respawn.
    */
    const long get_respawn_time();

    /*!
        Ask whether or not the player is alive.
        \return True if the player is still alive (health > 0).
    */
    bool is_alive() const;

    /*!
        Set whether or not the tank is alive. It's presumed that the health of the tank is
        greater than zero.
        \param alive True if the tank is alive, false if not.
    */
    void set_alive(const bool);

    /*!
        Set a new average offset for this player.
        \param new_offset Offset to set.
    */
    void set_offset(const IceUtil::Int64 &);
    
    /**
        Get the offset for this player.
        \return Offset value.
    */
    const IceUtil::Int64 get_offset();

    /*!
        Transform a client timestamp based on how the clocks are synchronized.
        \param timestamp Stamp to transform.
        \return Converted time.
    */
    IceUtil::Int64 transform_time(const IceUtil::Int64 &);
    
    /*!
        Gets the radius of this tank.
        \return Float value containing the tank's circular radius.
    */
    float get_radius() const
        { return TANK_SPHERE_RADIUS; }

    /*!
        Access to the inner player information struct.
        \return Player information, including the callbacks.
    */
    const player_ptr get_player_info() const;

    /*!
        Perform the clock synchronization task.
    */
    void do_clock_sync();

	/*!
		Apply a utility's effects to this tank.
		\param utility Utility to apply.
	*/
	void apply_utility(const VTankObject::Utility &);

	/*!
		Check if this player has any applied utilities. If so, check if those utilities have
		expired.
	*/
	void check_utility();
	
	/*!
		Get the player's internal charge timer.
	*/
	charge_ptr get_charge_timer() { return charge_timer; }
};

//! Makes the type much more pleasant to look at.
typedef boost::shared_ptr<Tank> tank_ptr;
typedef std::vector<tank_ptr> tank_array;

#endif
