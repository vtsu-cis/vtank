/*!
    \file   logger.hpp
    \brief  Declares functions and variables used in the event logger.
    \author (C) Copyright 2009 by Vermont Technical College
*/
#ifndef LOGGER_HPP
#define LOGGER_HPP

/*!
    The Logger namespace logs events of any kind (DEBUG, WARNING, ERROR) depending
    on what the user wishes to log. The Logger uses a threadpool with a single thread.
    It's done in a thread so that the logger task is executed immediately while the task
    that requested the log returns.
*/
namespace Logger
{
    /*!
        The user is allowed to set a "minimum" log level (the level at which the
        program will actually log). The user may select which type of message to
        save a message as via these enumerations.
    */
    enum Log_Level
    {
        LOG_LEVEL_NONE    = 4,
        LOG_LEVEL_ERROR   = 3,
        LOG_LEVEL_WARNING = 2,
        LOG_LEVEL_INFO    = 1,
        LOG_LEVEL_DEBUG   = 0
    };

    /*!
        The Stack_Logger class logs messages when entering and leaving functions.
        When LOG_LEVEL_DEBUG is set, this function will log every instance of the program
        entering and exiting a stack, provided that the programmer creates a Stack_Logger
        on the stack.
        <p>
        Example usage:<br />
        <code>
        void f()
        {
            Stack_Logger log("f()");

            // The destructor is called here.
        }
        </code>
        </p>
    */
    class Stack_Logger {
    private:
        std::string function;
        long start_time;
        bool debug_enabled;

    public:
        /*!
            Start the logger.
            \param function_name Name of the function that the stack has entered.
            \param debug Print debug messages. True by default. If this is set to false,
            it does not log debug messages, but it does keep track of how long it takes
            to exit a stack.
        */
        Stack_Logger(const std::string &, bool = true);
       ~Stack_Logger();
    };

    /*!
        Set the log level for the Logger instance. This acts as the minimum level
        that will be accepted into the logger.
        <ul>
            <li>LOG_LEVEL_NONE - Do not log messages.</li>
            <li>LOG_LEVEL_ERROR - Only log error messages.</li>
            <li>LOG_LEVEL_WARNING - Only log warning messages and above.</li>
            <li>LOG_LEVEL_INFO - Only log information messages and above.</li>
            <li>LOG_LEVEL_DEBUG - Log all types of messages.</li>
        </ul>
        \param log_level Level to set.
    */
    void set_log_level(const Log_Level);

	/*!
		Log a LOG_LEVEL_DEBUG message.
		\param message Message to log with formatting supported.
		\param ... Strings to insert into the formatting.
	*/
	void debug(const std::string, ...);

    /*!
        Execute a task to log a message.
        \param log_level Type of message. If the log level given does not meet the minimum
                         log level requirement, the message will not be logged.
        \param message Message to log.
    */
    void log(const Log_Level, const std::string);

    /*!
        Convert a log level enum type to it's string equivalent.
        \param log_level Log level enum type to convert.
        \param trailing_white_space Append spaces to the end for log formatting. True by
               default.
        \return String which holds the name of the enumeration.
    */
    std::string to_string(const Log_Level, const bool = true);
}

#endif
