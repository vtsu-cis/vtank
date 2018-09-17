using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;

namespace Audio
{
    public class SoundCue : IDisposable
    {
        #region Members
        private Vector3 position;
        private Vector3 velocity;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the sound cue's name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Internal method for the cue.
        /// </summary>
        public Cue Cue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the position of the sound. It does not matter unless the 
        /// Apply3DEffects() method has been called.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
                Emitter.Position = position;
            }
        }

        /// <summary>
        /// Gets or sets the velocity of the sound. It does not matter unless the
        /// Apply3DEffects() method has been called.
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                return velocity;
            }

            set
            {
                velocity = value;
                Emitter.Velocity = velocity;
            }
        }

        /// <summary>
        /// Gets or sets whether or not this sound cue should loop.
        /// </summary>
        public bool Loop
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the emitter which represents the origin of the sound. Does not matter
        /// unless the Apply3DEffects() method has been called.
        /// </summary>
        public AudioEmitter Emitter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the symbolic object for the user's speakers. Does not matter unless the
        /// Apply3DEffects() method has been called.
        /// </summary>
        public AudioListener Listener
        {
            get;
            set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new sound cue with the default position of (0, 0, 0), and with a 
        /// default velocity of (0, 0, 0).
        /// </summary>
        /// <param name="cue">Sound cue to play from.</param>
        /// <param name="_soundName">Name of the cue.</param>
        public SoundCue(Cue _cue, string _soundName)
            : this(_cue, _soundName, new Vector3(0, 0, 0), new Vector3(0, 0, 0))
        {
        }

        /// <summary>
        /// Creates a new sound cue.
        /// </summary>
        /// <param name="_cue">Sound cue to play from.</param>
        /// <param name="_soundName">Name of the cue.</param>
        /// <param name="_position">Position of the cue.</param>
        /// <param name="_velocity">Velocity of the cue.</param>
        public SoundCue(Cue _cue, string _soundName, Vector3 _position, Vector3 _velocity)
        {
            Initialize(_cue, _soundName, _position, _velocity);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initialize the sound cue.
        /// </summary>
        /// <param name="_cue">The cue object.</param>
        /// <param name="_soundName">Name of the cue.</param>
        /// <param name="_position">Position of the cue.</param>
        /// <param name="_velocity">Velocity of the cue.</param>
        private void Initialize(Cue _cue, string _soundName, Vector3 _position, Vector3 _velocity)
        {
            Emitter = new AudioEmitter();
            Name = _soundName;
            Position = _position;
            Velocity = _velocity;
            Loop = false;
            Cue = _cue;
        }

        /// <summary>
        /// Instruct the sound cue to play the sound in 3D space.
        /// </summary>
        /// <param name="listener">The symbolic object representing the user's speakers.</param>
        public void Apply3DEffects(AudioListener listener)
        {
            Listener = listener;
            Cue.Apply3D(Listener, Emitter);
        }

        /// <summary>
        /// Play this sound cue.
        /// </summary>
        public void Play()
        {
            Cue.Play();
        }

        /// <summary>
        /// Pause the sound cue if it is playing. Does nothing if it is not playing.
        /// </summary>
        public void Pause()
        {
            if (Cue.IsPlaying)
            {
                Cue.Pause();
            }
        }

        /// <summary>
        /// Unpauses the sound cue if it is pause. If it is not paused, it starts the sound
        /// cue from the beginning.
        /// </summary>
        public void Unpause()
        {
            if (Cue.IsPaused)
            {
                Cue.Resume();
            }
            else
            {
                Play();
            }
        }

        /// <summary>
        /// Stop the sound cue.
        /// </summary>
        public void Stop()
        {
            Cue.Stop(AudioStopOptions.Immediate);
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Cue.Dispose();
        }

        #endregion
    }
}
