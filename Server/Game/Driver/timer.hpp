/*!
    \file   timer.hpp
    \brief  Implements a timer used to time games.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef TIMER_HPP
#define TIMER_HPP

//! Mini-class designed to abstract the timing of the game.
struct GameTimer
{
private:
    //! Time left for the game to end.
    double time_left;

    //! Delta time since the last advance().
    double delta_time;

    //! Last calculated time.
    double last_time;

    //! Lock used to make the timer synchronized.
    boost::shared_mutex timer_lock;
    
public:
    //! Resets the timer.
    GameTimer()
        : time_left(0), delta_time(0), last_time(0)
    {
        reset();
    }

    //! Reset the timer, causing it to go back to the time per game.
    void reset()
    {
        boost::unique_lock<boost::shared_mutex> guard(timer_lock);
        time_left   = TIME_PER_GAME_MS / 1000.0;
        delta_time  = 0;
        last_time   = IceUtil::Time::now().toSecondsDouble();
    }

    /*!
        Advance the timer by some delta amount. This also calculates delta time.
    */
    void advance()
    {
        boost::unique_lock<boost::shared_mutex> guard(timer_lock);
        const double current_time = IceUtil::Time::now().toSecondsDouble();
        delta_time = current_time - last_time;

        time_left -= delta_time;

        last_time = current_time;
    }
	
	/*!
		Force the timer to jump ahead to zero.
	*/
	void force_timer_to_zero()
	{
		boost::unique_lock<boost::shared_mutex> guard(timer_lock);
		time_left = 0;
	}

    /*!
        Get the time left on the clock.
        \return Double value containing the time (in seconds) left in the game.
    */
    double get_time()
    {
        boost::shared_lock<boost::shared_mutex> guard(timer_lock);
        return time_left;
    }

    /*!
        Get the delta time, which is the last calculated time between
        the last time advance() was called and the time previous to that
        in seconds. A new delta time is calculated with every advance()
        call.
        \return Double value containing the time (in seconds) that a frame has
        progressed.
    */
    double get_delta_time()
    {
        boost::shared_lock<boost::shared_mutex> guard(timer_lock);
        return delta_time;
    }
};

#endif
