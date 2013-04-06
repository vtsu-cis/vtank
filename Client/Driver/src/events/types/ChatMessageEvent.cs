namespace Client.src.events.types
{
    using System;
    using Client.src.states.gamestate;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Event which adds a chat message into the buffer.
    /// </summary>
    public class ChatMessageEvent : IEvent
    {
        private string message;
        private Color color;

        public ChatMessageEvent(GamePlayState game, string message, Color color)
            : base(game)
        {
            this.message = message;
            this.color = color;
        }

        public override void DoAction()
        {
            Game.Chat.AddMessage(message, color);
        }
    }
}
