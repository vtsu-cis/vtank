/*!
    \file   logger.cpp
    \brief  Implementation of the EventLogger class.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#include <master.hpp>
#include <logger.hpp>
#include <ctime>

namespace Logger
{
    Log_Level level = LOG_LEVEL_DEBUG;
    boost::threadpool::pool log_pool(1);

    Stack_Logger::Stack_Logger(const std::string &function_name, bool debug)
        : function(function_name), debug_enabled(debug)
    {
        if (debug_enabled)
            log(LOG_LEVEL_DEBUG, "Entering " + function);

        start_time = static_cast<long>(IceUtil::Time::now().toMilliSeconds());
    }

    Stack_Logger::~Stack_Logger()
    {
		const long end_time = static_cast<long>(IceUtil::Time::now().toMilliSeconds());
		const long elapsed = end_time - start_time;

		if (debug_enabled) {
            debug("Exiting %s (took %d ms to finish)", function.c_str(), elapsed);
		}
		
        if (elapsed >= STACK_THRESHOLD_MS) {
            std::ostringstream formatter;
            formatter << function << " took " << elapsed << " ms to finish.";
            log(LOG_LEVEL_WARNING, formatter.str());
        }
    }

    void set_log_level(const Log_Level log_level)
    {
        level = log_level;
    }

    void log_task(const Log_Level log_level, std::string message)
    {
        // Replace new line characters with tab-new line characters.
        for (std::string::size_type i = 0; i < message.size(); i++) {
            if (message[i] == '\n') {
                message.replace(i, 1, "\n                    ");
                i += 2;
            }
        }

#if TARGET == WINTARGET
        // Check if "logs" directory exists.
        if (_access("logs", 0) != 0) {
            // Directory doesn't exist.
            if (_mkdir("logs") != 0) {
                throw std::runtime_error("Unable to create \"logs\" directory.");
            }
        }
        const char format[20] = {"logs\\%m-%d-%Y.log"};
#elif TARGET == LINTARGET
        if (access("logs", F_OK) != 0) {
            // Directory doesn't exist.
            if (mkdir("logs", 0777) != 0) {
                throw std::runtime_error("Unable to create \"logs\" directory.");
            }
        }
        const char format[20] = {"logs/%m-%d-%Y.log"};
#endif

        char log_file_buffer[20];

        // Get current date and time.
        const time_t t = time(0);
        struct tm current_time = tm();

#if TARGET == WINTARGET
        (void)localtime_s(&current_time, &t);
#elif TARGET == LINTARGET
        (void)localtime_r(&t, &current_time);
#endif

        (void)strftime(log_file_buffer, sizeof(log_file_buffer), format, &current_time);

        // Open file in append mode and set the position of the cursor to the end.
        std::ofstream out(log_file_buffer, std::ios_base::app | std::ios_base::ate);
        if (!out.is_open()) {
            // File can't be opened.
            throw std::runtime_error("Log file cannot be opened.");
        }

        const char time_format[12] = {"[%H:%M:%S] "};
        char time_buffer[12];
        (void)strftime(time_buffer, sizeof(time_buffer), time_format, &current_time);

        std::ostringstream formatter;
        formatter << time_buffer << to_string(log_level) << message;
        const std::string output = formatter.str();
        out << output << std::endl;

#if defined(DEBUG) || defined(_DEBUG)
        // It might be interesting to see the output on the console during debug mode.
        std::cout << output << std::endl;
#endif

        out.close();
    }

	void debug(const std::string message, ...)
	{
		va_list list;
		va_start(list, message);
		
		char buf[1024];
		vsprintf_s(buf, message.c_str(), list);
		va_end(list);
		
		log(Logger::LOG_LEVEL_DEBUG, std::string(buf));
	}

    void log(const Log_Level log_level, const std::string message)
    {
        if (log_level < level) {
            // Too high: we don't care about it.
            return;
        }

        log_pool.schedule(boost::bind<void>(log_task, log_level, message));
    }

    std::string to_string(const Log_Level log_level, const bool trailing_white_space)
    {
        switch (log_level) {
            case LOG_LEVEL_ERROR:
                return trailing_white_space ? "ERROR    " : "ERROR";
            case LOG_LEVEL_WARNING:
                return trailing_white_space ? "WARNING  " : "WARNING";
            case LOG_LEVEL_INFO:
                return trailing_white_space ? "INFO     " : "INFO";
            case LOG_LEVEL_DEBUG:
                return trailing_white_space ? "DEBUG    " : "DEBUG";
            default:
                return trailing_white_space ? "NONE     " : "NONE";
        }
    }
}
