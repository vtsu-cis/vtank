/*!
	\file	main.cpp
	\brief	Entry point for the program where basic initializations 
			for the server are performed.
	\author (C) Copyright 2009 by Vermont Technical College

LICENSE

This program is free software; you can redistribute it and/or modify it
under the terms of the GNU General Public License as published by the
Free Software Foundation; either version 2 of the License, or (at your
option) any later version.

This program is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANT-
ABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public
License for more details.

You should have received a copy of the GNU General Public License along
with this program; if not, write to the Free Software Foundation, Inc.,
59 Temple Place, Suite 330, Boston, MA 02111-1307 USA

Please send comments and bug reports to

	 Summer of Software Engineering
	 Vermont Technical College
	 201 Lawrence Place
	 Williston, VT 05495
     sosebugs@summerofsoftware.org (http://www.summerofsoftware.org)
*/

#include <master.hpp>
#include <server.hpp>
#include <logger.hpp>

/*!
    Entry point of the program. Performs basic initializations 
    \param argc
    \param argv
*/
int main(const int argc, const char * const argv[])
{
    Logger::log(Logger::LOG_LEVEL_INFO, "Starting up.");
    const std::string config_file_parameter = "--Ice.Config=config.theatre";

	// Provide custom arguments.
	Ice::StringSeq args;
	args.push_back(config_file_parameter);
    for (int i = 0; i < argc; i ++) {
		args.push_back(argv[i]);
	}
    
	try {
#if defined(DEBUG) || defined(_DEBUG)
        Logger::set_log_level(Logger::LOG_LEVEL_DEBUG);
#else
        Logger::set_log_level(Logger::LOG_LEVEL_INFO);
#endif
        
        const int return_code = Server::mtg_service.main(args);

        Logger::log(Logger::LOG_LEVEL_INFO, "The server finished running.");

        return return_code;
	}
	catch (...) {
        Logger::log(Logger::LOG_LEVEL_ERROR, "Unhandled exception.");
	}

    return 1;
}
