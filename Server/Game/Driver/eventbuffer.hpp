#ifndef EVENTBUFFER_HPP
#define EVENTBUFFER_HPP

#include <event.hpp>

typedef boost::shared_ptr<Event> event_t;

//! Helper method for creating a new event object.
static inline event_t create_event_object(Event *event_obj)
{
	return event_t(event_obj);
}

//! Stores events in a thread-safe way.
class Event_Buffer
{
private:
	boost::mutex mutex;
	std::queue<event_t> buffer;

	//! Internal method for popping an event (see: pop()).
	bool pop_internal(event_t &item)
	{
		if (buffer.size() == 0) {
			return false;
		}

		item = buffer.front();
		buffer.pop();
	}

public:
	//! Pushes an item onto the event buffer.
	/*!
		There is currently no limit for the number of events on the buffer.
		\param item Event to push onto the buffer.
	*/
	void push(const event_t &item)
	{
		boost::lock_guard<boost::mutex> guard(mutex);
		buffer.push(item);
	}
	
	//! Pushes an item onto the event buffer.
	/*!
		There is currently no limit for the number of events on the buffer.
		\param item Event to push onto the buffer.
	*/
	void push(Event *item)
	{
		event_t event_obj = create_event_object(item);
		push(event_obj);
	}
	
	//! Asks if there are events in the event buffer.
	/*!
		\return True if there are events; false otherwise.
	*/
	bool has_events()
	{
		boost::lock_guard<boost::mutex> guard(mutex);
		return buffer.size() > 0;
	}
	
	//! Pop an item off of the buffer.
	/*!
		The popped item will appear in the out parameter except when the buffer's
		size is 0.
		\param item [out] Storage for the popped event. Ignored if the buffer's size is 0.
		\return True if an item was popped and placed into 'item'; false otherwise.
	*/
	bool pop(event_t &item)
	{
		boost::lock_guard<boost::mutex> guard(mutex);
		return pop_internal(item);
	}

	//! Pops *all* events off of the buffer, if any exist.
	/*!
		The buffer is cleared and it's contents are dumped into a vector, which
		is returned. This method is ideal when the buffer is constantly competing
		with other threads for locking.
		\return Vector filled with all of this buffer's remaining events, or none
		if the buffer was empty.
	*/
	std::vector<event_t> pop_all()
	{
		boost::lock_guard<boost::mutex> guard(mutex);
		std::vector<event_t> buf;

		while (buffer.size() > 0) {
			event_t item;
			VTANK_ASSERT(pop_internal(item));
			buf.push_back(item);
		}
		
		return buf;
	}
};

#endif