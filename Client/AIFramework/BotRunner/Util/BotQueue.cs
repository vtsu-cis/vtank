using System;
using System.Collections.Generic;
using System.Text;
using AIFramework.Bot;

namespace VTankBotRunner.Util
{
    /// <summary>
    /// Synchronized bot queue.
    /// </summary>
    public class BotQueue
    {
        private Queue<VTankBot> botQueue;

        #region Properties
        /// <summary>
        /// Returns the number of bots on the queue.
        /// </summary>
        public int Count
        {
            get
            {
                lock (this)
                {
                    return botQueue.Count;
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs the queue.
        /// </summary>
        public BotQueue()
        {
            botQueue = new Queue<VTankBot>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Enqueue a bot onto the queue.
        /// </summary>
        /// <param name="bot"></param>
        public void Enqueue(VTankBot bot)
        {
            lock (this)
            {
                botQueue.Enqueue(bot);
            }
        }

        /// <summary>
        /// Dequeue the first bot on the queue.
        /// </summary>
        /// <returns></returns>
        public VTankBot Dequeue()
        {
            lock (this)
            {
                return botQueue.Dequeue();
            }
        }

        /// <summary>
        /// Clear the bot queue of all bots.
        /// </summary>
        public void Clear()
        {
            lock (this)
            {
                botQueue.Clear();
            }
        }

        /// <summary>
        /// Check of the bot queue contains a given bot.
        /// </summary>
        /// <param name="bot"></param>
        /// <returns></returns>
        public bool Contains(VTankBot bot)
        {
            lock (this)
            {
                return botQueue.Contains(bot);
            }
        }
        #endregion
    }
}
