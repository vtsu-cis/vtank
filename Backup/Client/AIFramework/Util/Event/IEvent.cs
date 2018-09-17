namespace AIFramework.Util.Event
{
    /// <summary>
    /// IEvent is an abstract class meant to be implemented by those wishing to 
    /// store events in the EventBuffer class.
    /// 
    /// Example:
    /// public void AddModelEvent : IEvent
    /// {
    ///     private Model model;
    ///     
    ///     public AddModelEvent(Model _model)
    ///     {
    ///         model = _model;
    ///     }
    ///     
    ///     public override void DoAction()
    ///     {
    ///         // Game is a property of IEvent and can be used by children.
    ///         Game.DoAddModel(model);
    ///     }
    /// }
    /// 
    /// // ...
    /// void InsertModelIntoBuffer()
    /// {
    ///     Model myModel = ...;
    ///     EventBuffer buffer = ...;
    ///     buffer.Push(new AddModelEvent(myModel));
    /// }
    /// </summary>
    public abstract class IEvent
    {
        #region Member
        private AIFramework.Bot.VTankBot game;
        #endregion

        #region Properties
        /// <summary>
        /// Gets a reference to the current game.
        /// </summary>
        public AIFramework.Bot.VTankBot Bot
        {
            get
            {
                if (game != null)
                {
                    return game;
                }

                throw new System.NullReferenceException(
                    "VTankBot is null in IEvent");
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs an IEvent class.
        /// </summary>
        /// <param name="_game"></param>
        public IEvent(AIFramework.Bot.VTankBot _game)
        {
            game = _game;
        }
        #endregion

        public abstract void DoAction();

        public virtual void Dispose()
        {
            game = null;
        }
    }
}
