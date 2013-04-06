/*!
	\file event.hpp
	\brief Interface to be implemented by game event handlers.
	\author Copyright (C) 2010 by Vermont Technical College
*/
#ifndef EVENT_HPP
#define EVENT_HPP

//! Interface to be implemented by game event handlers.
class Event
{
public:
	virtual void do_action() = 0;
};

#endif
