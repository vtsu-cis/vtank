/*!
    \file   ChatArea.cs
    \brief  State allowing for editing of a selected tank
    \author (C) Copyright 2008 by Vermont Technical College
*/
using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Client.src.service;

namespace Client.src.util.game
{
    /// <summary>
    /// Manage and display chat messages.
    /// </summary>
    public class ChatArea
    {
        #region Members

        private float wrapLength;
        private Rectangle chatArea;
        private List<ChatMessage> messages;
        private int maxMessageCount;
        private Texture2D background;
        private Rectangle backgroundRect;
        private Vector2 messagePosition;

        private const long maxMessageTime = 5000;
        private object chatLock = new object();

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_wrapLength">Maximum pixel length before the message is
        /// wrapped to a new line.</param>
        /// <param name="_chatArea">The size of the chat area</param>
        public ChatArea(float _wrapLength, Rectangle _chatArea)
        {
            wrapLength = _wrapLength;
            chatArea = _chatArea;
            messages = new List<ChatMessage>();
            background = ServiceManager.Resources.GetTexture2D("misc\\background\\chatarea");
            maxMessageCount = (int)Math.Ceiling((double)(chatArea.Height / GetStringHeight("W")));
            messagePosition = new Vector2();
        }

        /// <summary>
        /// Create a chat area with an initial text wrapping length and default
        /// bounds.
        /// </summary>
        /// <param name="_wrapLength">Maximum pixel length before the message is
        /// wrapped to a new line.</param>
        public ChatArea(float _wrapLength)
            : this(_wrapLength,
            new Rectangle(5, (int)(ServiceManager.Game.Window.ClientBounds.Height *.80f),
                (int)Math.Ceiling(_wrapLength),
                (ServiceManager.Game.Window.ClientBounds.Height / 5) - 60))
        { }

        /// <summary>
        /// Create a chat area, allowing the chat area to find a 
        /// default wrap length and bounds.
        /// </summary>
        public ChatArea()
            : this((ServiceManager.Game.Window.ClientBounds.Width / 2) - 30)
        { }

        #endregion

        #region Methods

        /// <summary>
        /// Add a chat message to the chat area.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        public void AddMessage(string message, Color color)
        {
            ChatMessage[] newMessages = WrapMessage(message, color);
            foreach (ChatMessage newMessage in newMessages)
            {
                messages.Add(newMessage);
            }
        }

        /// <summary>
        /// Remove messages whose time has been reduced to zero.
        /// </summary>
        private void RemoveExpiredMessages()
        {
        // Determine the maximum amount of messages able to be displayed.
            while (messages.Count > maxMessageCount && messages.Count > 0)
            {
                messages.RemoveAt(0);
            }

            List<ChatMessage> removeList = new List<ChatMessage>();
            for (int i = 0; i < messages.Count; i++)
            {
                ChatMessage message = messages[i];
                if (message.TimeAlive >= message.ExpireTime)
                {
                    // If the message is still alive but not fully transparent, decrease it's alpha
                    // value. This creates a fading effect.
                    message.DecrementAlpha();
                    messages[i] = message;
                }

                if (message.Color.A == 0)
                {
                    removeList.Add(message);
                }
            }

            for (int i = 0; i < removeList.Count; ++i)
            {
                messages.Remove(removeList[i]);
            }
            removeList.Clear();            
        }

        /// <summary>
        /// Wrap a message so that it fits the chat area.
        /// </summary>
        /// <param name="message">Message to wrap.</param>
        /// <returns>Message array of new messages.</returns>
        private ChatMessage[] WrapMessage(string message, Color color)
        {
            int messageWidth = GetStringWidth(message) + (int)chatArea.X;
            // Quick return:
            if (chatArea.X + messageWidth <= wrapLength)
            {
                return new ChatMessage[] { new ChatMessage(message, color) };
            }

            List<ChatMessage> tempList = new List<ChatMessage>();

            const string tab = "  ";
            string tempMessage = message;
            bool tabbed = false;
            // Check if the message is larger than the set wrapping length.
            while (tempMessage.Length > 0)
            {
                string newMessage = (tabbed) ? tab : "";
                for (int i = 0; i < tempMessage.Length; i++)
                {
                    int width = GetStringWidth(newMessage) + chatArea.X;
                    if (width >= wrapLength)
                    {
                        string dash = "";
                        if (i + 1 < tempMessage.Length && 
                            tempMessage[i] != ' ' && tempMessage[i + 1] != ' ')
                        {
                            dash = "-";
                        }

                        newMessage += tempMessage[i].ToString();
                        tempList.Add(new ChatMessage(newMessage + dash, color));
                        tempMessage = tempMessage.Substring(i + 1);
                        newMessage = "";
                        tabbed = true;

                        break;
                    }

                    newMessage += tempMessage[i];
                }

                if (newMessage.Length > 0 && newMessage != tab)
                {
                    tempList.Add(new ChatMessage(newMessage, color));

                    break;
                }
            }

            // Set expire time.
            for (int i = 0; i < tempList.Count; i++)
            {
                ChatMessage temp = tempList[i];
                temp.ExpireTime = 5000 + (1000 * tempList.Count);
                tempList[i] = temp;
            }

            return tempList.ToArray();
        }

        /// <summary>
        /// Calculate the size in pixels of the given message.
        /// </summary>
        /// <param name="str">String to calculate.</param>
        /// <returns>Width of the string in pixels.</returns>
        private int GetStringWidth(string str)
        {
            return (int)ServiceManager.Game.Font.MeasureString(str).X;
        }

        /// <summary>
        /// Calculate the size in pixels of the given message.
        /// </summary>
        /// <param name="str">String to calculate.</param>
        /// <returns>Height of the string in pixels.</returns>
        private int GetStringHeight(string str)
        {
            return (int)ServiceManager.Game.Font.MeasureString(str).Y;
        }

        /// <summary>
        /// Calculate the rectangle for the chat area background.
        /// </summary>
        private void SetChatAreaBackground()
        {
            int x = 0, y = 0, maxWidth = 0, maxHeight = 0;
            foreach (ChatMessage message in messages)
            {
                maxWidth = Math.Max(GetStringWidth(message.Message), maxWidth);
                maxHeight = Math.Max(GetStringHeight(message.Message), maxHeight);
            }

            int heightFactor = (messages.Count == 1) ? 1 : messages.Count;

            x = chatArea.X;
            y = chatArea.Bottom - (heightFactor * GetStringHeight("W")) - 5;
            maxHeight *= heightFactor;

            backgroundRect = new Rectangle(x, y, maxWidth + 15, maxHeight + 5);
        }

        #endregion

        #region OverrideMethods

        public void Update()
        {
            for (int i = 0; i < messages.Count; i++)
            {
                ChatMessage message = messages[i];
                message.Update();
                messages[i] = message;
            }

            RemoveExpiredMessages();
        }

        public void Draw()
        {
            // Obtain an approximate font height.
            float fontHeight = ServiceManager.Game.Font.MeasureString("W").Y;
            SpriteBatch batch = ServiceManager.Game.Batch;
            batch.Begin(SpriteBlendMode.AlphaBlend);


            if (messages.Count > 0)
            {
                SetChatAreaBackground();
                batch.Draw(background, backgroundRect, Color.White);
            }

            for (int i = messages.Count - 1, y = chatArea.Bottom; i >= 0; i--, y -= (int)fontHeight)
            {
                ChatMessage message = messages[i];
                Vector2 size = ServiceManager.Game.Font.MeasureString(message.Message);

                messagePosition.X = chatArea.X + 5;
                messagePosition.Y = y - 20;
                batch.DrawString(ServiceManager.Game.Font, message.Message, messagePosition, message.Color);
            }

            batch.End();
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the amount of characters the chat area will print before
        /// creating a new line in the text.
        /// </summary>
        public float WrapLength
        {
            get { return wrapLength; }
            set
            {
                if (wrapLength < 0f)
                {
                    throw new Exception("Wrap length cannot be negative.");
                }
                wrapLength = value;
                chatArea.Width = (int)wrapLength;
            }
        }

        /// <summary>
        /// Gets or sets the area on the screen of the chat area GUI.
        /// </summary>
        public Rectangle Area
        {
            get { return chatArea; }
            set { chatArea = value; }
        }
        #endregion
    }

    #region ChatMessageStruct
    /// <summary>
    /// Store information about a chat message.
    /// </summary>
    internal struct ChatMessage
    {
        private string message;
        private Color color;
        private long timeAlive;
        private long expireTime;

        /// <summary>
        /// Create a new chat message.
        /// </summary>
        /// <param name="_message">Content of the message.</param>
        /// <param name="_color">Color of the message.</param>
        public ChatMessage(string _message, Color _color)
        {
            message = _message;
            color = _color;
            timeAlive = 0;
            expireTime = 10000;
        }

        public void Update()
        {
            timeAlive += (long)(ServiceManager.Game.DeltaTime * 1000);
        }

        public void DecrementAlpha()
        {
            color.A -= (color.A - 5 >= 0) ? (byte)5 : color.A;
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        public long TimeAlive
        {
            get { return timeAlive; }
        }

        public long ExpireTime
        {
            get { return expireTime; }
            set
            {
                expireTime = value;
            }
        }
    }
    #endregion
}
