/*!
    \file   tank.cpp
    \brief  Implementation the Tank class.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#include <master.hpp>
#include <tank.hpp>
#include <vtassert.hpp>
#include <logger.hpp>
#include <playermanager.hpp>
#include <gamemanager.hpp>
#include <pointmanager.hpp>

#define MAX_TANK_HEALTH 100

/*!
    Calculate a new velocity value given the player's speed factor.
    \param speed_factor Factor for the speed of the tank.
    \param velocity Velocity of the tank. The result is stored here.
*/
inline void calculate_velocity(const Ice::Float &speed_factor, Ice::Float &velocity)
{
    velocity *= speed_factor;
}

Tank::Tank(const GameSession::Tank &player_tank, 
           const player_ptr player_instance, const GameSession::Alliance team)
    :   tank(player_tank), 
        player(player_instance),
        move_direction(VTankObject::NONE), 
        rotate_direction(VTankObject::NONE),
        offset(0),
        respawns_at(-1),
        node(-1),
		ready(false)
{
    VTANK_ASSERT(tank.id != -1);

    float new_velocity = DEFAULT_VELOCITY;
    calculate_velocity(tank.attributes.speedFactor, new_velocity);

    float new_angle_velocity = DEFAULT_ANGULAR_VELOCITY;
    calculate_velocity(tank.attributes.speedFactor, new_angle_velocity);

    velocity = new_velocity;
    angle_velocity = new_angle_velocity;
    tank.team = team;
	weapon = Players::get_weapon_data()->get_weapon(tank.attributes.weaponID);

	charge_timer = boost::shared_ptr<InternalChargeTimer>(new InternalChargeTimer());
    charge_timer->maximum = weapon.max_charge_time_seconds * 1000; // convert to ms
}

Tank::~Tank()
{
    sync_thread.interrupt();
}

const std::string Tank::get_name() const
{
    return tank.attributes.name;
}

VTankObject::Point Tank::get_position() const
{
    return tank.position;
}

void Tank::set_position(const VTankObject::Point &position)
{
    tank.position = position;
}

GameSession::Alliance Tank::get_team() const
{
    return tank.team;
}

void Tank::set_team(const GameSession::Alliance &team)
{
    boost::lock_guard<boost::mutex> guard(mutex);

    tank.team = team;
}

const double Tank::get_angle()
{
    boost::lock_guard<boost::mutex> guard(mutex);

    return tank.angle;
}

const bool Tank::is_allied(const int id)
{
    boost::lock_guard<boost::mutex> guard(mutex);
    tank_ptr tank;
    try {
        tank = Players::tanks.get(id);
    }
    catch (const TankNotExistException &) {
        return false;
    }
    
    return (this->tank.team != GameSession::NONE) && (this->tank.team == tank->get_team());
}

void Tank::set_angle(const double new_angle)
{
    boost::lock_guard<boost::mutex> guard(mutex);

    tank.angle = new_angle;
}

int Tank::get_health() const
{
    return tank.attributes.health;
}

void Tank::set_health(const int new_health)
{
    tank.attributes.health = new_health;
}

const float Tank::get_damage_factor()
{
	float total_factor = 0.0f;

	std::vector<AppliedUtility>::iterator i = applied_utilities.begin();
	for (; i != applied_utilities.end(); ++i) {
		total_factor += i->utility.damageFactor;
	}
	
	return total_factor;
}

void Tank::inflict_damage(const int damage, const int projectile_id, 
						  const int projectile_type_id, const int owner)
{
    boost::lock_guard<boost::mutex> guard(mutex);

    tank.attributes.health -= damage;
    if (tank.attributes.health <= 0) {
        tank.alive = false;
		if (charge_timer->is_charging) {
			charge_timer->stop_charging();
		}

		applied_utilities.clear();
        respawns_at = static_cast<long>(
            IceUtil::Time::now().toMilliSeconds()) + DEFAULT_RESPAWN_TIME_MS;

        PointManager::add_death(get_id());
        PointManager::add_kill(owner);
        for (std::vector<int>::size_type i = 0; i < assist_hitters.size(); i++) {
            const int assister = assist_hitters[i];
            if (assister != owner) {
                PointManager::add_assist(assister);
            }
        }

        assist_hitters.clear();
    }
    else {
        for (std::vector<int>::size_type i = 0; i < assist_hitters.size(); i++) {
            if (assist_hitters[i] == owner) {
                // Owner is already on the assist list.
                return;
            }
        }

        assist_hitters.push_back(owner);
    }
}

void Tank::inflict_environment_damage(const int damage, const int env_id,
	const int env_type_id, const int owner)
{
	inflict_damage(damage, env_id, env_type_id, owner);
}

float Tank::get_armor_factor() const
{
    return tank.attributes.armorFactor;
}

const float Tank::get_speed_factor()
{
	return tank.attributes.speedFactor;
}

const GameSession::Tank Tank::get_tank_object()
{
    boost::lock_guard<boost::mutex> guard(mutex);

    return tank;
}

const VTankObject::Direction Tank::get_movement_direction()
{
    boost::lock_guard<boost::mutex> guard(mutex);

    return move_direction;
}

void Tank::set_movement_direction(const VTankObject::Direction direction)
{
    boost::lock_guard<boost::mutex> guard(mutex);

    VTANK_ASSERT(direction != VTankObject::LEFT && direction != VTankObject::RIGHT);

    move_direction = direction;
}

const VTankObject::Direction Tank::get_rotation_direction()
{
    boost::lock_guard<boost::mutex> guard(mutex);

    return rotate_direction;
}

void Tank::set_rotation_direction(const VTankObject::Direction direction)
{
    boost::lock_guard<boost::mutex> guard(mutex);

    VTANK_ASSERT(direction != VTankObject::FORWARD && direction != VTankObject::REVERSE);

    rotate_direction = direction;
}

const int Tank::get_node_id()
{
    boost::lock_guard<boost::mutex> guard(mutex);

    return node;
}

void Tank::set_node_id(const int id)
{
    boost::lock_guard<boost::mutex> guard(mutex);

    node = id;
}

void Tank::respawn()
{
    boost::lock_guard<boost::mutex> guard(mutex);

    respawns_at = 0;
    tank.attributes.health = DEFAULT_MAX_HEALTH;
    tank.alive = true;
}

const long Tank::get_respawn_time()
{
    boost::lock_guard<boost::mutex> guard(mutex);

    return respawns_at;
}

bool Tank::is_alive() const
{
    return tank.alive;
}

void Tank::set_alive(const bool alive)
{
    boost::lock_guard<boost::mutex> guard(mutex);

    tank.alive = alive;
    if (!alive) {
        respawns_at = static_cast<long>(IceUtil::Time::now().toMilliSeconds() + 
            DEFAULT_RESPAWN_TIME_MS);
    }
    else {
        VTANK_ASSERT(tank.attributes.health > 0);
    }
}

void Tank::set_offset(const IceUtil::Int64 &new_offset)
{
    boost::lock_guard<boost::mutex> guard(mutex);

    offset = new_offset;
}

const IceUtil::Int64 Tank::get_offset()
{
    boost::lock_guard<boost::mutex> guard(mutex);

    return offset;
}

IceUtil::Int64 Tank::transform_time(const IceUtil::Int64& timestamp)
{
    boost::lock_guard<boost::mutex> guard(mutex);

    return timestamp - offset;
}

const Ice::Identity Tank::get_ice_id() const
{
    return ice_id;
}

void Tank::set_ice_id(const Ice::Identity &id)
{
    ice_id = id;
}

const player_ptr Tank::get_player_info() const
{
    return player;
}

const double Tank::get_velocity()
{
	double total_velocity_change = 0;
	for (std::vector<AppliedUtility>::iterator i = applied_utilities.begin(); i != applied_utilities.end(); ++i) {
		double velocity_change = i->utility.speedFactor;
		total_velocity_change += velocity_change;
	}

	if (total_velocity_change > 0) {
		return velocity + (velocity * total_velocity_change);
	}

	return velocity;
}

const double Tank::get_angular_velocity()
{
	double total_velocity_change = 0;
	for (std::vector<AppliedUtility>::iterator i = applied_utilities.begin(); i != applied_utilities.end(); ++i) {
		double velocity_change = i->utility.speedFactor;
		total_velocity_change += velocity_change;
	}

	if (total_velocity_change > 0) {
		return angle_velocity * total_velocity_change;
	}

	return angle_velocity;
}

void Tank::apply_utility(const VTankObject::Utility &utility)
{
	if (utility.duration > 0) {
		applied_utilities.push_back(AppliedUtility(utility));
	}
	else {
		// Instantly apply effect.
		tank.attributes.health += utility.healthIncrease;
		if (utility.healthFactor > 0) {
			tank.attributes.health += int(float(MAX_TANK_HEALTH) * utility.healthFactor);
		}

		if (utility.healthIncrease > 0) {
			tank.attributes.health += utility.healthIncrease;
		}

		if (tank.attributes.health > MAX_TANK_HEALTH) {
			tank.attributes.health = MAX_TANK_HEALTH;
		}
	}
}

void Tank::check_utility()
{
	double rate_of_fire_factor = 0;
	for (std::vector<AppliedUtility>::iterator i = applied_utilities.begin(); 
			i != applied_utilities.end(); ++i) {
		AppliedUtility util = *i;
		util.step();

		if (util.has_expired()) {
			applied_utilities.erase(i);

			std::ostringstream formatter;
			formatter << "Utility " << util.utility.model << " has expired from " << get_name() << ".";
			Logger::log(Logger::LOG_LEVEL_DEBUG, formatter.str());

			break;
		}
		else {
			// The only power-up we need to check up on is health increase here.
			// TODO: Regeneration health packs.

			rate_of_fire_factor += util.utility.rateFactor;
		}
	}
	
	// If the player is charging a weapon, do special case handling.
	if (charge_timer->is_charging && rate_of_fire_factor > 0) {
		charge_timer->advance(rate_of_fire_factor);
	}
	else if (charge_timer->is_charging) {
		charge_timer->advance();
	}
}

/*!
    Synchronize the clocks between the server and the client. This should
    be run as a thread.
*/
void synchronize_clock_task(tank_ptr player)
{
    const int id = player->get_id();
    const std::string name = player->get_name();
    const GameSession::ClockSynchronizerPrx clock = player->get_player_info()->get_clock();

    try {
        player->get_player_info()->set_last_sync_time(IceUtil::Time::now().toMilliSecondsDouble());

        int times_synchronized = 0;
        long latencies[SYNC_REQUESTS];      // Stores average latency values.
        long offsets[SYNC_REQUESTS];  // Stores average offset values.
        // Initialize averages to 0.
        for (short i = 0; i < SYNC_REQUESTS; i++) {
            latencies[i] = 0;
            offsets[i] = 0;
        }
        
        // Milliseconds to wait before sending another request.
        const boost::posix_time::milliseconds time_to_wait = 
            boost::posix_time::milliseconds(1000);
        while (times_synchronized < SYNC_REQUESTS) {
            boost::this_thread::sleep(time_to_wait);
            
            // Stamp the current time to measure (approximate) latency.
            const long start_time = static_cast<long>(IceUtil::Time::now().toMilliSeconds());
            const long timestamp  = static_cast<long>(clock->Request());
            const long end_time   = static_cast<long>(IceUtil::Time::now().toMilliSeconds());

            player->get_player_info()->refresh_timeout();

            // Latency is divided by two to attempt to compensate for round-trip.*
            // *Note: Temporarily commented this part out. Latency and offset calc is kept separate.
            latencies[times_synchronized] = (end_time - start_time) / 2;
            offsets[times_synchronized]   = (timestamp /*+ latencies[times_synchronized]*/) - end_time;

            player->set_offset(offsets[times_synchronized]);

            times_synchronized++;
        }
        
        // Calculate the average 'climb' i.e. how much faster/slower the clock is moving.
        /*const double offset_climb = ((offsets[1] - offsets[0]) + 
            (offsets[2] - offsets[1]) + (offsets[3] - offsets[2]) + (offsets[4] - offsets[3]) +
            (offsets[5] - offsets[4])) / (time_to_wait * (SYNC_REQUESTS - 1));*/
        
        long sum = 0;
        for (int l = 0; l < SYNC_REQUESTS; l++) {
            sum += latencies[l];
        }
        sum /= SYNC_REQUESTS;
        
        player->get_player_info()->set_average_latency(sum);

		double offset_sum = 0;
		for (int o = 0; o < SYNC_REQUESTS; ++o) {
			offset_sum += offsets[o];
		}
		offset_sum /= SYNC_REQUESTS;
		//player->set_offset(static_cast<long>(offset_sum));

        const double offset_value = static_cast<double>(player->get_offset());
        
        std::ostringstream formatter;
        formatter << name << ": Latency=" << sum << ", Offset=" << offset_value;
        
        Logger::log(Logger::LOG_LEVEL_DEBUG, formatter.str());
    }
    catch (const Ice::Exception& ex) {
        std::ostringstream formatter;
        formatter << "Exception during ClockSync::Request for " << name 
            << ": " << ex.what();

        Logger::log(Logger::LOG_LEVEL_ERROR, formatter.str());

        (void)Players::remove_player(id);
    }
    catch (const boost::thread_interrupted &) {
        // Thread was interrupted.
        std::ostringstream formatter;
        formatter << "Thread interrupt during ClockSync::Request for " << name;

        Logger::log(Logger::LOG_LEVEL_INFO, formatter.str());

        (void)Players::remove_player(id);
    }
}

void Tank::do_clock_sync()
{
    sync_thread = boost::thread(boost::bind<void>(synchronize_clock_task, 
        Players::tanks.get(get_id())));
}
