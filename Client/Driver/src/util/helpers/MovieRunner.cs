using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using DirectShowLib;

namespace Client.src.util.game
{
    using SeeSharp.Xna.Video;

    /// <summary>
    /// Play a movie in a loop. This is accomplished by playing the video through XNA's library,
    /// recording the frames, and permanently storing them. Due to XNA's lack of ability to loop
    /// videos without skipping, this technique (or similar) is required.
    /// </summary>
    public class LoopingMovieRunner
    {
        #region Members
        private static Texture2D blankFrame;

        private bool rewindFlag;
        private GraphicsDevice graphics;
        private VideoPlayer player;
        #endregion
        
        #region Properties
        /// <summary>
        /// Gets the current frame. The frame increments automatically if the movie is playing,
        /// and loops automatically when it reaches the last frame. If the video is paused, the
        /// frame is never incremented.
        /// </summary>
        public Texture2D CurrentFrame
        {
            get
            {
                if (player == null)
                    return GetBlankFrame(graphics);

                return player.OutputFrame;
            }
        }

        /// <summary>
        /// Gets or sets whether or not a video is paused.
        /// </summary>
        public bool Paused
        {
            get
            {
                return player != null && player.CurrentState == VideoState.Paused;
            }

            set
            {
                if (value)
                {
                    Pause();
                }
                else
                {
                    Resume();
                }
            }
        }

        /// <summary>
        /// Gets whether or not the video is playing.
        /// </summary>
        public bool Playing
        {
            get
            {
                return player != null && player.CurrentState == VideoState.Playing;
            }
        }

        /// <summary>
        /// Gets the duration of the video (in seconds).
        /// </summary>
        public double Duration
        {
            get
            {
                if (player != null)
                    return player.Duration;
                
                return -1;
            }
        }

        /// <summary>
        /// Gets the current position of the video (in seconds).
        /// </summary>
        public double CurrentPosition
        {
            get
            {
                if (player != null)
                    return player.CurrentPosition;

                return -1;
            }
        }

        /// <summary>
        /// Gets the FPS of the video, or -1 if it can't be calculated.
        /// </summary>
        public int FPS
        {
            get
            {
                return player == null ? -1 : player.FramesPerSecond;
            }
        }

        /// <summary>
        /// Internal get/set for whether a video should be re-wound.
        /// </summary>
        private bool RewindFlag
        {
            get
            {
                lock (this)
                    return rewindFlag;
            }

            set
            {
                lock (this)
                    rewindFlag = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs the movie runner but does not start the video.
        /// </summary>
        /// <param name="graphics"></param>
        public LoopingMovieRunner(GraphicsDevice graphics)
        {
            rewindFlag = false;
            this.graphics = graphics;
        }

        /// <summary>
        /// Constructs the looping movie runner but does not play the video.
        /// </summary>
        /// <param name="video">Video to play.</param>
        public LoopingMovieRunner(GraphicsDevice graphics, string filename)
        {
            rewindFlag = false;
            this.graphics = graphics;
            player = new VideoPlayer(filename, graphics);
            player.OnVideoComplete += new EventHandler(Player_OnVideoComplete);
        }
        #endregion

        #region XNA Methods
        /// <summary>
        /// Dispose of the video and stop playing.
        /// </summary>
        public void UnloadContent()
        {
            try
            {
                graphics = null;
                Stop();
                player.Dispose();
                player = null;
            }
            catch
            {
                // Unloads take place upon shutdown, so exceptions don't matter.
            }
        }

        /// <summary>
        /// Update the movie runner, inserting new frames into the buffer.
        /// </summary>
        public void Update()
        {
            if (player != null && Playing && !Paused)
            {
                if (RewindFlag)
                {
                    RewindFlag = false;
                    player.Rewind();
                }

                player.Update();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Plays a video, replacing the current video player.
        /// </summary>
        /// <param name="filename"></param>
        public void Play(string filename)
        {
            Stop();
            if (player != null)
                player.Dispose();

            player = new VideoPlayer(filename, graphics);
            player.OnVideoComplete += new EventHandler(Player_OnVideoComplete);
            Play();
        }

        /// <summary>
        /// Event handler for the movie stopping.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Player_OnVideoComplete(object sender, EventArgs e)
        {
            RewindFlag = true;
        }

        /// <summary>
        /// Plays the video. Does nothing if the video is already playing.
        /// </summary>
        public void Play()
        {
            if (player != null)
                player.Play();
        }

        /// <summary>
        /// Pauses the video.
        /// </summary>
        public void Pause()
        {
            if (player != null)
                player.Pause();
        }

        /// <summary>
        /// Resumes the video. Has no effect if the video is not paused or playing.
        /// </summary>
        public void Resume()
        {
            if (player != null)
                player.Play();
        }

        /// <summary>
        /// Stops the video.
        /// </summary>
        public void Stop()
        {
            if (player != null)
                player.Stop();
        }

        /// <summary>
        /// Gets or creates a blank frame. This should be used if a frame is gotten from the movie
        /// runner while the movie is not running (as opposed to simply crashing).
        /// </summary>
        /// <returns></returns>
        private static Texture2D GetBlankFrame(GraphicsDevice graphics)
        {
            if (blankFrame != null)
            {
                return blankFrame;
            }

            Rectangle titleArea = graphics.Viewport.TitleSafeArea;
            SurfaceFormat format = graphics.DisplayMode.Format;
            blankFrame = new Texture2D(graphics, titleArea.Width, titleArea.Height,
                0, TextureUsage.Tiled, format);
            return blankFrame;
        }
        #endregion
    }
}
