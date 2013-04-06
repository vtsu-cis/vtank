using System;
using System.Collections.Generic;
using System.Text;

namespace VTankBotRunner.Util
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
        private List<Invocation> waitBuffer;
        private Queue<Invocation> iterationBuffer;
        private bool iterating;
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
            waitBuffer = new List<Invocation>();
            iterationBuffer = new Queue<Invocation>();
            iterating = false;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Dequeues an event from the front of the event buffer while invoking the method.
        /// </summary>
        /// <returns>An InvocationTarget if there are further events, or null if
        /// there are no more events to process.</returns>
        public bool InvokeNext()
        {
            bool executed = false;
            lock (this)
            {
                iterating = true;

                if (events.Count > 0)
                {
                    Invocation invocation = events.Dequeue();
                    invocation.Invoke();
                    invocation.Dispose();
                    executed = true;
                }
                
                foreach (Invocation inv in waitBuffer)
                {
                    if (inv.Ready())
                    {
                        if (executed)
                        {
                            events.Enqueue(inv);
                        }
                        else
                        {
                            inv.Invoke();
                            inv.Dispose();
                            executed = true;
                        }
                    }
                }

                waitBuffer.RemoveAll((inv) => { return inv.Ready(); });

                iterating = false;
                ManageIteratingBuffer();
            }

            return executed;
        }

        /// <summary>
        /// Internally manage items waiting on the iteration buffer.
        /// </summary>
        private void ManageIteratingBuffer()
        {
            if (!iterating)
            {
                while (iterationBuffer.Count > 0)
                    Enqueue(iterationBuffer.Dequeue());
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
            if (eventToBuffer.Target == null)
                throw new ArgumentNullException("eventToBuffer.Target",
                    "Target of the invocation cannot be null.");

            lock (this)
            {
                if (iterating)
                    iterationBuffer.Enqueue(eventToBuffer);
                else if (eventToBuffer.DelayTimeMs > 0)
                    waitBuffer.Add(eventToBuffer);
                else
                    events.Enqueue(eventToBuffer);
            }
        }

        /// <summary>
        /// Enqueue a new invocation onto the event queue.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameter"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void Enqueue(Invocation.InvocationTarget target, object parameter)
        {
            if (target == null)
                throw new ArgumentNullException("target",
                    "Target of the invocation cannot be null!");

            Invocation inv = new Invocation(target, parameter);
            Enqueue(inv);
        }

        /// <summary>
        /// Enqueue a new invocation onto the event queue.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="parameter"></param>
        /// <param name="delayTimeMs"></param>
        public void Enqueue(Invocation.InvocationTarget target, object parameter, long delayTimeMs)
        {
            if (target == null)
                throw new ArgumentNullException("target",
                    "Target of the invocation cannot be null!");

            Invocation inv = new Invocation(target, parameter, delayTimeMs);
            Enqueue(inv);
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
