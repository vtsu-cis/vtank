using System;
using System.Collections.Generic;

namespace Client.src.events
{
    /// <summary>
    /// The EventBuffer is a thread safe class which holds a list of events to be later
    /// processed during the software's main, non-threaded loop. Events are inserted into
    /// this buffer exclusively by other threads in the program.
    /// 
    /// The EventBuffer uses a Queue behind the scenes to store objects which implement
    /// IEvent. Other threads use the Push() method to put events into the back of the
    /// queue. The main thread uses Pop() to grab an event at the front of the queue.
    /// In other words, it's first-in-first-out.
    /// </summary>
    public class EventBuffer
    {
        #region Members
        private Queue<IEvent> events;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of events currently queued.
        /// </summary>
        public int Count
        {
            get
            {
                lock (this)
                {
                    return events.Count;
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs the EventBuffer class. Does nothing extraordinary.
        /// </summary>
        public EventBuffer()
        {
            events = new Queue<IEvent>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Dequeues an event from the front of the event buffer.
        /// </summary>
        /// <returns>An IEvent if there are further events, or null if
        /// there are no more events to process.</returns>
        public IEvent Dequeue()
        {
            lock (this)
            {
                if (events.Count > 0)
                {
                    return events.Dequeue();
                }

                return null;
            }
        }

        /// <summary>
        /// Pops all events, emptying the queue.
        /// </summary>
        /// <returns></returns>
        public IEvent[] PopAll()
        {
            lock (this)
            {
                IEvent[] eventArray = new IEvent[events.Count];
                events.CopyTo(eventArray, 0);
                events.Clear();

                return eventArray;
            }
        }

        /// <summary>
        /// Enqueues a new event onto the event queue, meaning it falls on the end of the stack.
        /// This will be later processed through Dequeue().
        /// </summary>
        /// <param name="eventToBuffer"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void Enqueue(IEvent eventToBuffer)
        {
            if (eventToBuffer == null)
            {
                throw new ArgumentNullException("Argument 'eventToBuffer' may not be null.");
            }

            lock (this)
            {
                events.Enqueue(eventToBuffer);
            }
        }

        /// <summary>
        /// Clears the event buffer of all it's events.
        /// </summary>
        public void Clear()
        {
            lock (this)
            {
                events.Clear();
            }
        }
        #endregion
    }
}
