/*!
    \file   master.hpp
    \brief  Definitions, declarations, and includes needed by the program.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifdef MASTER_HPP
#error Do not include this more than once.
#else
#define MASTER_HPP

// Bring in values for TARGET (this is first because it might be used in following headers).
#include <target.hpp>

// Standard
#include <algorithm>
#include <exception>
#include <fstream>
#include <iostream>
#include <iterator>
#include <map>
#include <sstream>
#include <stdexcept>
#include <string>
#include <vector>

// Operating system specific
#if TARGET == WINTARGET
#include <direct.h>     // For _mkdir().
#include <io.h>         // For access().
#elif TARGET == LINTARGET
#include <unistd.h>
#include <sys/stat.h>
#include <sys/types.h>
#endif

// External source
#include <Ice/Ice.h>
#include <Ice/Service.h>
#include <Glacier2/Session.h>
#include <Glacier2/Router.h>
#include <boost/threadpool.hpp>
#include <boost/thread.hpp>
#include <boost/thread/locks.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/bind.hpp>
#include <boost/date_time/local_time/local_time.hpp>

// Slice generated
#include <IGame.h>
#include <GameSession.h>
#include <VTankObjects.h>
#include <Exception.h>
#include <Main.h>
#include <MainToGameSession.h>
#include <ClockSync.h>

// Custom
#include <macros.hpp>

#endif
