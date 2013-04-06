using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer;

namespace Renderer.SceneTools.Animation
{
    /// <summary>
    /// An animation for an Object2
    /// </summary>
    public class Animation2D
    {
        private Rectangle[] frames;
        private float frameLength;
        private float timer = 0f;
        private int currentFrame = 0;
        
        /// <summary>
        /// Create a new animation.
        /// </summary>
        /// <param name="numFrames">Number of frames in the animation.</param>
        /// <param name="width">Width of each animation cell.</param>
        /// <param name="height">Height of each animation cell.</param>
        /// <param name="xOffset">How many pixels horizontally to skip on the texture.</param>
        /// <param name="yOffset">How many pixels vertically to skip on the texture.</param>
        public Animation2D(int numFrames, int width, int height, int xOffset, int yOffset)
        {
            frames = new Rectangle[numFrames];
            frameLength = GraphicOptions.FrameLength;

            int frameWidth = width / numFrames;
            for (int i = 0; i < numFrames; i++)
            {
                frames[i] = new Rectangle(frameWidth * i + xOffset, 
                    yOffset, 
                    frameWidth, 
                    height);
            }
        }

        /// <summary>
        /// Reset the timer and the current frame back to it's original state (zero).
        /// </summary>
        public void Reset()
        {
            timer = 0f;
            currentFrame = 0;
        }


        /// <summary>
        /// Update the timer. If the timer goes beyond the frame length, the
        /// timer is reset and the current frame is incremented.
        /// </summary>
        public void Update()
        {
            timer += GraphicOptions.FrameLength;

            if (timer >= frameLength)
            {
                // Reset timer.
                timer = 0f;
                currentFrame = (currentFrame + 1) % frames.Length;
            }
        }

        /// <summary>
        /// Get the number of frames updated per second, or set
        /// a new value for the number of frames.
        /// </summary>
        public int FramesPerSecond
        {
            get { return (int)(1f / frameLength); }
            set { frameLength = 1f / (float)value; }
        }

        /// <summary>
        /// Get the area of the current frame.
        /// </summary>
        public Rectangle CurrentFrame
        {
            get { return frames[currentFrame]; }
        }

        /// <summary>
        /// Get the frame number that this animation is on.
        /// </summary>
        public int CurrentFrameNumber
        {
            get { return currentFrame; }
        }
    }
}
