/*!
    \file   macros.hpp
    \brief  Macros and definitions that don't belong anywhere else.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef MACROS_HPP
#define MACROS_HPP

//! Number of threads dedicated to login handling/tasklets.
#define PLAYER_THREADS 2

//! Number of threads dedicated to game tasks.
#define GAME_THREADS 20

//! Number of threads dedicated to calculating points.
#define STATISTICS_THREADS 1

//! Size of the tiles.
#define TILE_SIZE 64

//! Default velocity (pixels/sec) of the tank.
#define DEFAULT_VELOCITY 275.0f

//! Default angular velocity (radians/sec) of the tank.
#define DEFAULT_ANGULAR_VELOCITY 2.666666667f

//! Default radius for tanks.
#define TANK_SPHERE_RADIUS 25.0f

//! Default radius for projectiles.
#define BULLET_RADIUS 8.0f

//! How often (in milliseconds) to perform a clock sync.
#define CLOCK_SYNC_INTERVAL 60000

//! How often (in milliseconds) to process a frame.
#define FRAME_PROCESS_INTERVAL 5

//! How many milliseconds per game.
#define TIME_PER_GAME_MS 274000

//! Max pixel value until a movement is considered illegal.
#define MAX_LEGAL_DISTANCE 1000

#define GRAVITY (-1500.0f)

//! Define the relative path to the maps folder.
#if TARGET == WINTARGET
    #define MAPS_DIR "maps\\"
#elif TARGET == LINTARGET
    #define MAPS_DIR "maps/"
#endif

#define HANDLE_UNCAUGHT_EXCEPTIONS \
catch (const std::exception &e) {\
    std::ostringstream formatter;\
    formatter << "Unhandled exception in file " << __FILE__\
        << " on line " << __LINE__ << ": " << e.what();\
    Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());\
}\
catch (...) {\
    std::ostringstream formatter;\
    formatter << "Uncaught exception in file " << __FILE__\
        << " on line " << __LINE__ << ".";\
    Logger::log(Logger::LOG_LEVEL_WARNING, formatter.str());\
}

//! How long (in milliseconds) to wait before respawning.
#define DEFAULT_RESPAWN_TIME_MS 5000

//! How much health a tank has by default.
#define DEFAULT_MAX_HEALTH 200

//! How long to wait until producing a warning in the stack.
#define STACK_THRESHOLD_MS 100

static inline double get_current_time()
{
	return static_cast<double>(IceUtil::Time::now().toMilliSeconds());
}

static inline int random_next(int minimum, int maximum)
{
	return minimum + rand() / (RAND_MAX / (maximum - minimum + 1) + 1);
}

static inline bool random_bool()
{
	return random_next(0, 1) == 0;
}

static inline float random_next_f(float minimum, float maximum)
{
	return (rand() / (static_cast<float>(RAND_MAX) + 1.0f)) * (maximum - minimum) + minimum;
}

#define PI 3.14159f

#define RADIANS(x) (x * (PI / 180.0))
#define RADIANS_F(x) (static_cast<float>(x * (PI / 180.0f)))

#endif
