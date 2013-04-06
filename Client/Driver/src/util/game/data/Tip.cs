using System;
using System.Collections.Generic;
using System.Text;

namespace Client.src.util.game.data
{
    /// <summary>
    /// A game tip pertaining to a specific context
    /// </summary>
    public class Tip
    {
        #region Properties
        public string Context { get; set; }
        public string Message { get; set; }
        public int ID { get; set; }
        #endregion

        public Tip(string context, string message)
        {
            this.Context = context;
            this.Message = message;
        }

        public Tip(string message, Predicate<object> predicate)
        {
            if (predicate(null))
            {
                // Display tip or something
            }
            else
            {
                // don't!
            }
        }
    }
}
