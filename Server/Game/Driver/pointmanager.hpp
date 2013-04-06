/*!
    \file pointmanager.hpp
    \brief Declares the variables and functions used in the PointManager namespace.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef POINTMANAGER_HPP
#define POINTMANAGER_HPP

/*!
    The PointManager namespace globally manages statistics for each player. The
    'reset' function should be called first every time a game starts. Finally,
    to package the statistics into a neat StatisticsList (which is sent to Echelon),
    call the 'compile' function. The returned value is ready as-is. However, if the
    vector object has no values, no players were entered into the pool.
*/
namespace PointManager
{
    //! The thread pool executes tasks in it's own thread.
    extern boost::threadpool::pool thread_pool;

    //! Stores 
    extern std::map<int, VTankObject::Statistics> statistics;

    /*!
        This function resets the point manager, emptying it's cache and killing all
        executing tasks. Players must be re-added into the manager.
    */
    void reset();

    /*!
        Add a player to the point manager. Note that the player will not be removed
        until reset() is called (which is intended).
        \param username Name of the person to add.
    */
    void add_player(const int);

    /*!
        Add a kill to a user's record.
        \param username Name of the person that this function pertains to.
    */
    void add_kill(const int);
    
    /*!
        Add an assist to a user's record.
        \param username Name of the person that this function pertains to.
    */
    void add_assist(const int);

    /*!
        Add a death to a user's record.
        \param username Name of the person that this function pertains to.
    */
    void add_death(const int);

    /*!
        Add a completed objective to a user's record.
        \param username Name of the person that this function pertains to.
    */
    void add_objective_completed(const int);

    /*!
        Add a captured objective to a user's record.
        \param username Name of the person that this function pertains to.
    */
    void add_objective_captured(const int);
    
    /*!
        Compile the statistics list into a VTankObject::StatisticsList object. This
        list is compatible with the Slice-generated functions which sends the 
        statistics to Echelon.
        \param filter_players Do not compile players who aren't in the game anymore.
    */
    VTankObject::StatisticsList compile(const bool = true);

    /*!
        Same thing as compile(), except it also calculates the point value of each player.
        \return List of statistics for each player.
    */
    VTankObject::StatisticsList compile_and_calculate();
}

#endif
