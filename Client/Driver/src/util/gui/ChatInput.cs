/*!
    \file   ChatInput.cs
    \brief  Allows for entering text into the chat area
    \author (C) Copyright 2008 by Vermont Technical College
*/
using System;
using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using GameForms.Forms;
using Client.src.service;
using TomShane.Neoforce.Controls;

namespace Client.src.util.game
{
    /// <summary>
    /// Convenience class for entering messages into a chat box.
    /// </summary>
    public class ChatInput
    {
        #region Members
        private bool focused = false;
        private bool visible = false;
        private ChatInputForm form;
        private DateTime lastEnterPress;
        public static long ENTER_TIME_WAIT = 300L;
        public TextBox Box { get { return form.Box; } }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="position">Position of the chat input</param>
        /// <param name="width">Width of the input</param>
        /// <param name="game">Current game</param>
        public ChatInput(Vector2 position, float width)
        {
            //forms = new FormCollection(game.Window, game.Services, ref game.graphics);

            position.Y -= 50;
            form = new ChatInputForm(ServiceManager.Game.Manager, position, (int)width);
            form.EnterPressed += new TomShane.Neoforce.Controls.EventHandler(EnterPressed);
            lastEnterPress = DateTime.Now;
        }

        /// <summary>
        /// Logical Updates to the chat input
        /// </summary>
        public void Update()
        {
            if (focused)
            {
                if (!form.Box.Visible)
                {
                    Hide();
                    return;
                }
            }
        }

        public void Draw(GameTime gameTime)
        {

        }

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <returns>Text from the input box</returns>
        public string GetText()
        {
            return form.Message;
        }

        /// <summary>
        /// Method to prevent message spamming
        /// </summary>
        /// <returns>If the user can send a message</returns>
        public bool AcceptEnter()
        {
            /*TimeSpan elapsed = DateTime.Now - lastEnterPress;
            if (Visible)
            {
                if (elapsed.TotalMilliseconds > ENTER_TIME_WAIT / 2)
                    return true;
            }
            else
            {
                if (elapsed.TotalMilliseconds > ENTER_TIME_WAIT)
                    return true;
            }

            return false;*/
            return true;
        }

        /// <summary>
        /// Show the text box.
        /// </summary>
        public void Show()
        {
            Visible = true;
            Focused = true;
            
            form.Box.Show();
            form.Box.Focused = true;
            form.Message = "";
            form.Clear();
            foreach (Control c in ServiceManager.Game.Manager.Controls)
            {
                c.Focused = false;
            }
            form.Box.Visible = true;

            lastEnterPress = DateTime.Now;
        }

        /// <summary>
        /// Hides the text box
        /// </summary>
        public void Hide()
        {
            Visible = false;
            Focused = false;
            form.Box.Hide();
            form.Clear();
            form.Box.Visible = false;
            lastEnterPress = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets the visibility of this component.
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set 
            { 
                visible = value; 
                if (visible) 
                    form.Box.Show(); 
                else
                    form.Box.Hide(); 
            }
        }

        /// <summary>
        /// Gets or sets the focus for this component.
        /// </summary>
        public bool Focused
        {
            get { return focused; }
            set
            {
                focused = value;
                if (focused)
                {
                    ServiceManager.Game.Manager.BringToFront(form.Box);
                    form.Box.Focused = true;
                    
                    form.Box.Text = "";
                }
            }
        }

        /// <summary>
        /// Handle the event where Enter is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnterPressed(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            lastEnterPress = DateTime.Now;
        }
    }
}
