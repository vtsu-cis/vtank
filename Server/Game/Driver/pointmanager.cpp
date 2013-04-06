/*!
    \file pointmanager.cpp
    \brief
    \author (C) Copyright 2009 by Vermont Technical College
*/
#include <master.hpp>
#include <pointmanager.hpp>
#include <macros.hpp>
#include <tank.hpp>
#include <playermanager.hpp>

//! Holds a list of types that can be executed as a task.
enum Task_Type
{
    RESET_TASK,
    ADD_TASK,
    KILL_TASK,
    ASSIST_TASK,
    DEATH_TASK,
    OBJECTIVE_TASK,
    CAPTURE_TASK
};

namespace PointManager
{
    // The thread pool should keep a size of 1 so that thread safety isn't an issue.
    boost::threadpool::pool thread_pool(STATISTICS_THREADS);
    boost::shared_mutex mutex;

    std::map<int, VTankObject::Statistics> statistics;

    /*!
        Execute a boost::threadpool::pool task.
        \param type Type of task to execute.
        \param username Person who is the base of a task.
        \param change What value to change as part of their statistic.
    */
    void do_task(const Task_Type type, const int id, const int change)
    {
        boost::shared_lock<boost::shared_mutex> guard(mutex);
        switch (type) {
        case RESET_TASK:
            statistics.clear();
            break;

        case ADD_TASK:
            // Add a player to the player list.
            try {
                const std::map<int, VTankObject::Statistics>::iterator i = statistics.find(id);
                if (i != statistics.end()) {
                    // Exists already. Do nothing.
                    return;
                }

                const tank_ptr tank = Players::tanks.get(id);

                VTankObject::Statistics stats = VTankObject::Statistics();
                stats.tankName = tank->get_name();
                stats.kills = 0;
                stats.deaths = 0;
                stats.assists = 0;
                stats.objectivesCaptured = 0;
                stats.objectivesCompleted = 0;
                stats.calculatedPoints = 0;

                statistics[id] = stats;
            }
            catch (const TankNotExistException &) {
            }
            
            break;

        case KILL_TASK:
            // Add a kill to the player's record.
            statistics[id].kills += change;
            break;

        case ASSIST_TASK:
            // Add an assist to the player's record.
            statistics[id].assists += change;
            break;

        case DEATH_TASK:
            // Add a death to the player's record.
            statistics[id].deaths += change;
            break;

        case OBJECTIVE_TASK:
            // Add an objective completion to the player's record.
            statistics[id].objectivesCompleted += change;
            break;

        case CAPTURE_TASK:
            // Add an objective capture to the player's record.
            statistics[id].objectivesCaptured += change;
            break;
        }
    }

    /*!
        Calculate the points that a player has earned. This function does not
        return a value, instead modifying the given statistics object.
        \param stats Reference to a statistics object.
    */
    void calculate_points(VTankObject::Statistics& stats) 
    {
        // TODO: Right now we'll just use arbitrary values.
        // We'll use structured values in the future.
        //const int DEATH_VALUE = 0;
        const int ASSIST_VALUE = 5;
        const int KILL_VALUE = 10;
        const int OBJECTIVE_VALUE = 20;
        const int CAPTURE_VALUE = 20;
        
        //stats.calculatedPoints += (stats.deaths * DEATH_VALUE);
        stats.calculatedPoints += (stats.kills * KILL_VALUE);
        stats.calculatedPoints += (stats.assists * ASSIST_VALUE);
        stats.calculatedPoints += (stats.objectivesCompleted * OBJECTIVE_VALUE);
        stats.calculatedPoints += (stats.objectivesCaptured * CAPTURE_VALUE);
    }

    void reset()
    {
        (void)thread_pool.schedule(boost::bind<void>(do_task, RESET_TASK, -1, 0));
    }

    void add_player(const int id)
    {
        (void)thread_pool.schedule(boost::bind<void>(do_task, ADD_TASK, id, 0));
    }

    void add_kill(const int id)
    {
        (void)thread_pool.schedule(boost::bind<void>(do_task, KILL_TASK, id, 1));
    }
    
    void add_assist(const int id)
    {
        (void)thread_pool.schedule(boost::bind<void>(do_task, ASSIST_TASK, id, 1));
    }

    void add_death(const int id)
    {
        (void)thread_pool.schedule(boost::bind<void>(do_task, DEATH_TASK, id, 1));
    }

    void add_objective_completed(const int id)
    {
        (void)thread_pool.schedule(boost::bind<void>(do_task, OBJECTIVE_TASK, id, 1));
    }

    void add_objective_captured(const int id)
    {
        (void)thread_pool.schedule(boost::bind<void>(do_task, CAPTURE_TASK, id, 1));
    }

    VTankObject::StatisticsList compile(const bool filter_players)
    {
        // Wait for the tasks to stop executing.
        thread_pool.wait();

        boost::unique_lock<boost::shared_mutex> guard(mutex);

        // Now gather statistics.
        VTankObject::StatisticsList stats;

        std::map<int, VTankObject::Statistics>::iterator i = statistics.begin();
        for (; i != statistics.end(); i++) {
            if (filter_players) {
                // Do not add players if they aren't in the game.
                try {
                    (void)Players::tanks.get(i->first);
                }
                catch (const TankNotExistException &) {
                    continue;
                }
            }
        
            stats.push_back(i->second);
        }

        return stats;
    }

    VTankObject::StatisticsList compile_and_calculate()
    {
        // Wait for the tasks to stop executing.
        thread_pool.wait();

        boost::unique_lock<boost::shared_mutex> guard(mutex);

        // Now gather statistics.
        VTankObject::StatisticsList stats;

        std::map<int, VTankObject::Statistics>::iterator i = statistics.begin();
        for (; i != statistics.end(); i++) {
            calculate_points(i->second);
			if (i->second.kills == 0 && i->second.assists == 0 && i->second.deaths == 0 &&
				i->second.objectivesCaptured == 0 && i->second.objectivesCompleted == 0) {
				continue;
			}
            stats.push_back(i->second);
        }

        return stats;
    }
}
