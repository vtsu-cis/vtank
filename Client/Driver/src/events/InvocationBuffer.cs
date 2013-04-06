using System;
using System.Collections.Generic;
using System.Text;

namespace Client.src.events
{
    /// <summary>
    /// The invocation buffer holds onto event callback requests from different threads.
    /// One may place an event response method into the invocation buffer and it will be
    /// called later on the GUI thread.
    /// </summary>
    public class InvocationBuffer
    {
        #region Members
        private Queue<Invocation> events;
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
        /// Constructs the InvocationBuffer class. Does nothing extraordinary.
        /// </summary>
        public InvocationBuffer()
        {
            events = new Queue<Invocation>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Dequeues an event from the front of the event buffer while invoking the method.
        /// </summary>
        /// <returns>An InvocationTarget if there are further events, or null if
        /// there are no more events to process.</returns>
        public void InvokeNext()
        {
            lock (this)
            {
                if (events.Count > 0)
                {
                    Invocation invocation = events.Dequeue();
                    invocation.Target.Invoke(invocation.Parameter);
                }
            }
        }

        /// <summary>
        /// Enqueues a new event onto the event queue, meaning it falls on the end of the stack.
        /// This will be later processed through InvokeNext().
        /// </summary>
        /// <param name="eventToBuffer"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void Enqueue(Invocation eventToBuffer)
        {
            lock (this)
            {
                events.Enqueue(eventToBuffer);
            }
        }

        /// <summary>
        /// Enqueue a new invocation onto the event queue.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameter"></param>
        public void Enqueue(Client.src.events.Invocation.InvocationTarget target, object parameter)
        {
            Enqueue(new Invocation() { Target = target, Parameter = parameter });
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
