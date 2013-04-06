using Client.src.states.gamestate;
using Client.src.service;
namespace Client.src.events
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
        private GamePlayState game;
        #endregion

        #region Properties
        /// <summary>
        /// Gets a reference to the current game.
        /// </summary>
        public GamePlayState Game
        {
            get
            {
                if (game != null)
                {
                    return game;
                }

                State currentState = ServiceManager.StateManager.CurrentState;
                if (currentState is GamePlayState)
                {
                    game = (GamePlayState)currentState;
                    return game;
                }

                throw new System.NullReferenceException(
                    "GamePlayState is null in IEvent");
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs an IEvent class.
        /// </summary>
        /// <param name="_game"></param>
        public IEvent(GamePlayState _game)
        {
            game = _game;
        }
        #endregion

        public abstract void DoAction();
    }
}
