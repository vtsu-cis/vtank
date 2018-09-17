using System;
using System.Collections.Generic;
using System.Text;
using Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Audio
{
    /// <summary>
    /// Interface class for all audio needs. Contains methods for loading and playing sounds 
    /// in a standard way or in a 3D world. 
    /// </summary>
    public class xAudio : IDisposable
    {
        #region Members
        private AudioEngine audioEngine;
        private WaveBank waveBank;
        private SoundBank soundBank;
        private List<SoundCue> activeSounds;
        private bool muted = false;
        private AudioListener listener;
        #endregion

        #region Properties
        
        #endregion

        #region Constructors
        /// <summary>
        /// Create and initialize the audio manager.
        /// </summary>
        /// <param name="xactProjectFile">The XACT project file (*.xgs).</param>
        /// <param name="waveBankFile">The wave bank file (*.xwb).</param>
        /// <param name="soundBankFile">The sound bank file (*.xsb).</param>
        public xAudio(string xactProjectFile, string waveBankFile, string soundBankFile)
        {
            Initialize(xactProjectFile, waveBankFile, soundBankFile);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initializes audio manager. Must be called before attempting to play any sound. 
        /// </summary>
        /// <param name="audio"></param>
        private void Initialize(string audio, string wave, string sound)
        {
            audioEngine = new AudioEngine(audio);
            waveBank = new WaveBank(audioEngine, wave);
            soundBank = new SoundBank(audioEngine, sound);
            activeSounds = new List<SoundCue>(100);
            listener = new AudioListener();
        }
        #endregion

        #region Sound Controls
        /// <summary>
        /// Plays a sound. 
        /// </summary>
        /// <param name="_soundName">Name of the sound cue.</param>
        /// <param name="_loop">True to loop the cue; otherwise false.</param>
        public void PlaySound(String _soundName, bool _loop)
        {
            Play3DSound(_soundName, Vector3.Zero, Vector3.Zero, Vector3.Zero, _loop);
        }

        /// <summary>
        /// Plays a sound based on location in a 3D world.
        /// </summary>
        /// <param name="_soundName">Name of the sound cue.</param>
        /// <param name="_playerPosition">Position of the player.</param>
        /// <param name="_position">Position of the sound -- basically, the position of the
        /// object which emits sound.</param>
        /// <param name="_velocity">Speed/direction in which the sound is moving.</param>
        /// <param name="_loop">True to loop the sound over and over; false otherwise.</param>
        public SoundCue Play3DSound(String _soundName, Vector3 _playerPosition,
            Vector3 _position, Vector3 _velocity, bool _loop)
        {
            SoundCue newCue = new SoundCue(soundBank.GetCue(_soundName), _soundName);
            newCue.Loop = _loop;

            listener.Position = _playerPosition;
            newCue.Emitter.Velocity = _velocity;
            newCue.Emitter.Position = _position;
            newCue.Apply3DEffects(listener);

            activeSounds.Add(newCue);

            if (!muted)
            {
                newCue.Play();
            }

            return newCue;
        }

        /// <summary>
        /// Check to see if a sound is currently playing.
        /// </summary>
        /// <param name="cue"></param>
        /// <returns></returns>
        public bool SoundIsPlaying(SoundCue cue)
        {
            return activeSounds.Contains(cue);
        }



        /// <summary>
        /// Perform necessary audio updates/cleaning on the audio manager.
        /// </summary>
        /// <param name="newSoundPosition"></param>
        /// <param name="newPlayerPosition"></param>
        public void Update(Vector3 newPlayerPosition)
        {
            listener.Position = newPlayerPosition;
            Stack<SoundCue> removableItems = new Stack<SoundCue>(activeSounds.Count);
            Stack<SoundCue> loopingItems = new Stack<SoundCue>();
            for (int i = 0; i < activeSounds.Count; i++)
            {
                SoundCue cue = activeSounds[i];

                if (cue.Cue.IsStopped)
                {
                    if (cue.Loop)
                    {
                        loopingItems.Push(cue);
                    }

                    removableItems.Push(cue);
                }
                else
                {
                    if (cue.Listener.Position != newPlayerPosition)
                    {
                        cue.Listener.Position = newPlayerPosition;
                        //cue.Apply3DEffects(cue.Listener);
                    }
                }
            }

            // Remove expired items.
            while (removableItems.Count > 0)
            {
                SoundCue cue = removableItems.Pop();
                activeSounds.Remove(cue);
                try
                {
                    cue.Dispose();
                }
                catch (Exception e) { Console.WriteLine(e); }
            }

            // Re-play items that are looping.
            while (loopingItems.Count > 0)
            {
                SoundCue cue = loopingItems.Pop();
                Play3DSound(cue.Name, newPlayerPosition, 
                    cue.Position, cue.Velocity, true);
            }

            audioEngine.Update();
        }

        /// <summary>
        /// Pause a sound if it's playing.
        /// </summary>
        /// <param name="_soundName">Name of the sound cue.</param>
        public void PauseSound(String _soundName)
        {
            lock (this)
            {
                if (muted)
                {
                    return;
                }

                for (int i = 0; i < activeSounds.Count; i++)
                {
                    SoundCue cue = activeSounds[i];
                    if (cue.Name == _soundName)
                    {
                        cue.Pause();
                    }
                }

                throw new KeyNotFoundException(
                    "The sound cue " + _soundName + " does not exist.");
            }
        }

        /// <summary>
        /// Resume a paused sound. If sound was not paused, it will play from the beginning.
        /// </summary>
        /// <param name="_soundName">Name of the sound cue.</param>
        public void ResumeSound(String _soundName)
        {
            lock (this)
            {
                if (muted)
                {
                    return;
                }

                for (int i = 0; i < activeSounds.Count; i++)
                {
                    SoundCue cue = activeSounds[i];
                    if (cue.Name == _soundName)
                    {
                        cue.Unpause();
                    }
                }

                throw new KeyNotFoundException(
                    "The sound cue " + _soundName + " does not exist.");
            }
        }

        /// <summary>
        /// Stop a sound.
        /// </summary>
        /// <param name="sound">Sound cue to stop.</param>
        public void StopSound(String _soundName)
        {
            lock (this)
            {
                if (muted)
                {
                    return;
                }

                for (int i = 0; i < activeSounds.Count; i++)
                {
                    SoundCue cue = activeSounds[i];
                    if (cue.Name == _soundName)
                    {
                        cue.Stop();
                    }
                }

                throw new KeyNotFoundException(
                    "The sound cue " + _soundName + " does not exist.");
            }
        }

        /// <summary>
        /// Stop playback of all sounds.
        /// </summary>
        public void StopAllSound()
        {
            lock (this)
            {
                foreach (SoundCue sound in activeSounds)
                {
                    sound.Stop();
                }

                activeSounds.Clear();
            }
        }

        #endregion

        #region Volume Controls
        /// <summary>
        /// Sets the volume for a specific category of sounds
        /// </summary>
        public void SetVolume(String _category, float _volume)
        {
            lock (this)
            {
                audioEngine.GetCategory(_category).SetVolume(_volume);
            }
        }

        /// <summary>
        /// Mutes all sound, preventing further calls from doing anything.
        /// </summary>
        public void Mute()
        {
            StopAllSound();

            lock (this)
            {
                muted = true;
            }
        }

        /// <summary>
        /// Unmute the sound, allowing sounds to be played.
        /// </summary>
        public void Unmute()
        {
            lock (this)
            {
                muted = false;
            }
        }

        #endregion

        #region Information Methods
        /*
        /// <summary>
        /// Gets all the sounds for a given category
        /// </summary>
        public Audio.Utils.xAudioCategory GetPlayableSounds(String _category)
        {
            Audio.Utils.xAudioCategory category = new Utils.xAudioCategory();
            AudioCategory audioCategory = audioEngine.GetCategory(_category);

            category.Name = audioCategory.Name;
            category.SoundList = new List<string>();

            return new Utils.xAudioCategory();
        }

        /// <summary>
        /// Gets all playable sounds
        /// </summary>
        public Audio.Utils.xAudioCategory[] GetAllPlayableSounds()
        {
            return null;
        }
        */
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            foreach (SoundCue sound in activeSounds)
            {
                sound.Dispose();
            }

            activeSounds.Clear();
            audioEngine.Dispose();
        }

        #endregion
    }
}
