using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Client.src.service;
using Client.src.config;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Windows.Forms;

namespace Client.src.util.gui
{
    /// <summary>
    /// A Help Overlay UI component that displays VTank's control listing
    /// </summary>
    public class HelpOverlay
    {
        #region Fields
        private Rectangle displayRect;
        private Options options;
        private Texture2D separatorLine;
        private List<string[]> helpText;
        private int longestString;
        #endregion

        #region Constants
        private readonly int width = 500;
        private readonly int height = 520;
        private readonly int padding = 20;
        private readonly int lineThickness = 2;
        private readonly Color backgroundColor = new Color(Color.Black, 200);
        private readonly SpriteFont headerFont = ServiceManager.Resources.GetFont("stencil");
        private readonly SpriteFont normalFont = ServiceManager.Resources.GetFont("kootenay9");
        #endregion

        #region Properties
        public bool Enabled { get; set; }
        #endregion

        #region Constructors

        /// <summary>
        /// Create a Help Overlay component with the default positioning and scale.
        /// 
        /// (Center Screen, fixed scale)
        /// </summary>
        public HelpOverlay()
        {
            Vector2 screenCenter = new Vector2(ServiceManager.Game.GraphicsDevice.Viewport.Width / 2,
                ServiceManager.Game.GraphicsDevice.Viewport.Height / 2);

            int leftBound = (int)screenCenter.X - width / 2 - padding;
            int topBound = (int)screenCenter.Y - height / 2 + padding;

            displayRect = new Rectangle(leftBound-padding,
                                        topBound-padding,
                                        width + padding*2,
                                        height + padding*2);

            options = ServiceManager.Game.Options;

            this.Enabled = false;

            separatorLine = new Texture2D(ServiceManager.Game.GraphicsDevice, 1, 1);
            separatorLine.SetData<Color>(new Color[] { Color.White });

            helpText = new List<string[]>();
            LoadHelpText();            
        }

        #endregion

        #region Methods

        #region Helpers

        /// <summary>
        /// Initialize the helpText collection with string arrays containing the
        /// action -> command pairs.
        /// </summary>
        private void LoadHelpText()
        {
            helpText.Add(new string[] {"Fire", "Left Mouse"});
            KeysConverter converter = new KeysConverter();

            helpText.Add(new string[] { "Move Forward", converter.ConvertToString(options.KeySettings.Forward) });
            helpText.Add(new string[] { "Move Backward", converter.ConvertToString(options.KeySettings.Backward) });
            helpText.Add(new string[] { "Rotate Left", converter.ConvertToString(options.KeySettings.RotateLeft) });
            helpText.Add(new string[] { "Rotate Right", converter.ConvertToString(options.KeySettings.RotateRight) });

            helpText.Add(new string[] { "Toggle Camera", converter.ConvertToString(options.KeySettings.Camera) });
            helpText.Add(new string[] { "Toggle Minimap", converter.ConvertToString(options.KeySettings.Minimap) });

            helpText.Add(new string[] { "View Scoreboard", converter.ConvertToString(options.KeySettings.Score) });
            helpText.Add(new string[] { "Exit Game", converter.ConvertToString(options.KeySettings.Menu) });
        }

        /// <summary>
        /// Find the longest [0] index string in helpText, used to determine how much padding to
        /// put between columns.
        /// </summary>
        /// <param name="scale">The current text scale being used</param>
        /// <returns>The length(px) of the longest string</returns>
        private int FindLongestString(float scale)
        {
            longestString = 0;

            foreach (string[] str in helpText)
            {
                if (ServiceManager.Game.Font.MeasureString(str[0]).X*scale > longestString)
                    longestString = (int)(ServiceManager.Game.Font.MeasureString(str[0]).X*scale);
            }

            return longestString;
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Draw a horizontal line across the display rectangle at the given Y position
        /// </summary>
        /// <param name="yPos">y position to draw at</param>
        /// <param name="color">Color to draw the line</param>
        private void DrawHorizontalLine(int yPos, Color color)
        {
            Rectangle rect = new Rectangle((int)displayRect.Left+padding,
                                           yPos, 
                                           (int)displayRect.Width-padding*2,
                                           lineThickness);

            ServiceManager.Game.Batch.Draw(separatorLine, rect, color);
        }

        /// <summary>
        /// Draw a given string with the specified font, position, color and scale
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <param name="scale"></param>
        private void DrawString(string text, SpriteFont font, Vector2 position, Color color, float scale)
        {
            ServiceManager.Game.Batch.DrawString(font,
                                                 text,
                                                 position,
                                                 color,
                                                 0.0f,
                                                 Vector2.Zero,
                                                 scale,
                                                 SpriteEffects.None,
                                                 0.0f);
        }

        #endregion

        /// <summary>
        /// Draw this component
        /// </summary>
        public void Draw()
        {
            if (this.Enabled == false)
                return;


            const int vPadding = 20;
            const int columnPadding = 50;

            
            int currentYOffset = 0;
            float currentScale = 2.0f;

            int stringHeight = (int)this.normalFont.MeasureString("W").Y;

            ServiceManager.Game.Batch.Begin();
            ServiceManager.Game.Batch.Draw(ServiceManager.Resources.GetTexture2D(
                "textures\\misc\\Scoreboard\\scoreback"), displayRect, backgroundColor);

            DrawString("VTank Help", 
                        this.headerFont,
                        new Vector2(displayRect.Left + displayRect.Width / 2 - (headerFont.MeasureString("VTank Help").X * currentScale) / 2, displayRect.Top + vPadding),
                        Color.Red,
                        currentScale);

            currentYOffset += (int)(headerFont.MeasureString("VTank Help").Y * currentScale) + vPadding;

            int column2XPos = displayRect.Left + padding + columnPadding + FindLongestString(currentScale);

            DrawHorizontalLine(displayRect.Top + currentYOffset, Color.White);

            currentYOffset += lineThickness + vPadding;

            foreach (string[] line in helpText)
            {
                DrawString(line[0], 
                           this.normalFont, 
                           new Vector2(displayRect.Left + padding, displayRect.Top + currentYOffset), 
                           Color.LightSlateGray, 
                           currentScale);

                DrawString(line[1], 
                           this.normalFont, 
                           new Vector2(column2XPos, displayRect.Top + currentYOffset), 
                           Color.White, 
                           currentScale);

                currentYOffset += stringHeight + vPadding;
            }

            currentYOffset += stringHeight + vPadding;

            string closeString = "(Press F1 to close help)";
            DrawString(closeString,
                       this.normalFont,
                       new Vector2(displayRect.Left + displayRect.Width / 2 - (this.normalFont.MeasureString(closeString).X * currentScale) / 2,
                                   displayRect.Top + currentYOffset), 
                       Color.LightGray, 
                       currentScale);

            ServiceManager.Game.Batch.End();
        }
        #endregion
    }
}
