using System;
using System.Collections.Generic;
using System.Text;
using TomShane.Neoforce.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameForms.Controls;

namespace GameForms.src.Forms
{
    /// <summary>
    /// Pre-built form for game console windows.
    /// </summary>
    public class GameConsoleForm
    {
        #region Members
        private Manager manager;
        private Rectangle windowFrame;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the window used in this form.
        /// </summary>
        public Window Window
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the text area, which is where text output is shown.
        /// </summary>
        public ConsoleArea TextArea
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the input box which the user can use to enter commands through.
        /// </summary>
        public TextBox Input
        {
            get;
            private set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create and initialize the game console form components.
        /// </summary>
        /// <param name="_manager">Parent manager that oversees the windows.</param>
        /// <param name="frame">Desired width/height of the components.</param>
        public GameConsoleForm(Manager _manager, Rectangle frame)
        {
            manager = _manager;
            windowFrame = frame;

            Initialize();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initialize the components that make up the window, including the window itself.
        /// </summary>
        private void Initialize()
        {
            Window = new Window(manager);
            Window.Init();
            Window.Text = "Console";
            Window.Left = windowFrame.X;
            Window.Top = windowFrame.Y;
            Window.Width = windowFrame.Width;
            Window.Height = windowFrame.Height;
            Window.Alpha = 200;
            Window.BorderVisible = true;
            Window.CaptionVisible = false;
            Window.BackColor = Color.Black;
            Window.Center();

            TextArea = new ConsoleArea(manager);
            TextArea.Init();
            TextArea.Text = "";
            TextArea.Width = windowFrame.Width - 15;
            TextArea.Height = windowFrame.Height - 40;
            TextArea.Enabled = true;
            TextArea.CanFocus = false;
            TextArea.BackgroundColor = Color.Black;
            TextArea.TextColor = Color.Green;
            TextArea.TextPosition = ConsoleArea.TextOrigin.BOTTOM;
            TextArea.Parent = Window;

            Input = new TextBox(manager);
            Input.Init();
            Input.Text = "";
            Input.Color = Color.Black;
            Input.TextColor = Color.Green;
            Input.Top = TextArea.Top + TextArea.Height + 2;
            Input.Width = windowFrame.Width - 15;
            Input.Height = 20;
            Input.Enabled = true;
            Input.Parent = Window;
        }
        #endregion
    }
}
