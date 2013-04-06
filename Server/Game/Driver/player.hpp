/*!
    \file   player.hpp
    \brief  Blueprint for the Player class, which is an Ice GameSession::CurrentGame servant.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef PLAYER_HPP
#define PLAYER_HPP

//! How many times do we request a clock synchronization?
#define SYNC_REQUESTS 6

#include <vtassert.hpp>

/*!
    The Player class is an Ice servant. It implements the GameSession::CurrentGame 
    interface defined in the GameSession.ice file. More documentation on what
    this class is specificially meant for is available in the GameSession.ice file.
*/
class Player : virtual public GameSession::GameInfo
{
private:
    int id;

protected:
    /*!
        The destructor does not do anything. It's up to whatever manages players to
        destroy this class and remove it from any list it resides in.
    */
    virtual ~Player();

public:
	/*!
        Simple constructor.
        \param player_id ID of the player.
    */
    Player(const int);
	
	/* The following functions are implemented by the generated GameSession.hpp file.
	 * Please see the documentation for GameSession in the file: GameSession.ice.
	 */
	virtual void destroy(const Ice::Current& = Ice::Current());
	virtual GameSession::PlayerList GetPlayerList(const Ice::Current& = Ice::Current());
	virtual std::string GetCurrentMapName(const Ice::Current& = Ice::Current());
	virtual Ice::Double GetTimeLeft(const Ice::Current& = Ice::Current());
    virtual VTankObject::GameMode GetGameMode(const Ice::Current& = Ice::Current());
    virtual VTankObject::StatisticsList GetScoreboard(const Ice::Current& = Ice::Current());
	virtual GameSession::ScoreboardTotals GetTeamTotals(const Ice::Current& = Ice::Current());
    virtual void KeepAlive(const Ice::Current& = Ice::Current());
    virtual void Move(Ice::Long, const VTankObject::Point&, VTankObject::Direction,
        const Ice::Current& = Ice::Current());
	virtual void Rotate(Ice::Long, Ice::Double, VTankObject::Direction, 
		const Ice::Current& = Ice::Current());
	virtual void SpinTurret(Ice::Long, Ice::Double, VTankObject::Direction, 
		const Ice::Current& = Ice::Current());
	virtual void Fire(Ice::Long, const VTankObject::Point&, const Ice::Current& = Ice::Current());
    virtual void SendMessage(const std::string&, const Ice::Current& = Ice::Current());
	virtual void Ready(const Ice::Current& = Ice::Current());
	virtual void StartCharging(const Ice::Current & = Ice::Current());
};

/*!
    Encapsulates information about a player, removing the need to keep track of things
    together with the Player class (for instance, callbacks set to this player are
    tracked separately, so that they're not inside of the Player object). This prevents
    null pointer errors.
*/
struct PlayerInfo
{
private:
    GameSession::ClientEventCallbackPrx callback;
    GameSession::ClockSynchronizerPrx   clock_callback;
    double last_time;
    double last_time_sync;

    // Related to clock:
    long average_latency; // Average latency.

public:
    PlayerInfo(const GameSession::ClientEventCallbackPrx &player_callback,
        const GameSession::ClockSynchronizerPrx &clock)
        : callback(player_callback), clock_callback(clock)
    {
        refresh_timeout();
    }

    //! Interrupts the thread if one is running.
   ~PlayerInfo()
    {
    }

    /*!
		Get the player's callback proxy.
        \return Proxy pointing to the client's callback.
	*/
	const GameSession::ClientEventCallbackPrx get_callback() const 
    { 
        return callback; 
    }

    /*!
        Access to the ClockSynchronizer interface on the client.
        \return Proxy pointing to the client's clock.
    */
    const GameSession::ClockSynchronizerPrx get_clock() const 
    { 
        return clock_callback; 
    }

    /*!
		Get the last time a client has performed an action, refreshing it's timeout.
		\return Milliseconds since the last response.
	*/
    double get_last_action_time() const 
    {
        return last_time;
    }

    //! Get the last time a clock sync occurred.
    double get_last_sync_time() const
    {
        return last_time_sync;
    }
    
    //! Set the last time a sync was performed.
    void set_last_sync_time(const double last)
    {
        last_time_sync = last;
    }

    /*!
        Set a new average latency for this player.
        \param latency New average latency.
    */
    void set_average_latency(const long &latency) 
    { 
        average_latency = latency; 
    }

    /*!
		This should be called any time the player sends a message via Ice. This lets the
		player manager know the last time the player sent a message. If the player is
		taking too long, the player will be timed out.
	*/
	void refresh_timeout()
    {
        last_time = IceUtil::Time::now().toMilliSecondsDouble();
    }
};

typedef boost::shared_ptr<PlayerInfo> player_ptr;
typedef std::vector<player_ptr> player_array;
typedef std::vector<player_ptr>::iterator player_array_iterator;

#endif