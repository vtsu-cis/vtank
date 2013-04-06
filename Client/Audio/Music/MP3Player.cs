using Microsoft.DirectX;
using Microsoft.DirectX.AudioVideoPlayback;

namespace Audio.Music
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// MP3Player is responsible for loading, looping through, and playing music. The user
    /// only has to offer MP3Player a directory to look for music from, or a single song
    /// to play.
    /// </summary>
    public class MP3Player
    {
        #region Members
        private double lastMusicPosition;
        private string audioDirectory;
        private string currentSongTitle;
        private string currentSongPath;
        private bool isPlaying;
        private Microsoft.DirectX.AudioVideoPlayback.Audio currentSong;
        private List<string> playlist;
        private int playlistPosition;
        private bool playPlaylist;
        private int volume;
        private bool playNextSong;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether or not to automatically
        /// play the next track in a playlist.
        /// </summary>
        public bool AutoPlayPlaylist
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the volume of the music.
        /// </summary>
        public int Volume
        {
            get
            {
                return volume;
            }

            set
            {
                if (value > 0)
                {
                    throw new ArgumentException(
                        "Volume cannot be greater than 0 (it's negative based).", "Volume");
                }

                if (isPlaying)
                {
                    currentSong.Volume = value;
                }

                volume = value;
            }
        }

        /// <summary>
        /// Internal property for dealing with whether the next song in a playlist should be played.
        /// </summary>
        private bool PlayNextSong
        {
            get
            {
                lock (this)
                {
                    return playNextSong;
                }
            }

            set
            {
                lock (this)
                {
                    playNextSong = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the directory to look for audio.
        /// </summary>
        public string AudioDirectory
        {
            get
            {
                return audioDirectory;
            }

            set
            {
                audioDirectory = value;
            }
        }

        /// <summary>
        /// Loop the currently playing song.
        /// </summary>
        public bool LoopCurrentSong
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs the MP3Player, but does not start playing anything yet.
        /// </summary>
        /// <param name="audioDirectory">Directory on the local hard drive to look
        /// for music from.</param>
        public MP3Player(string audioDirectory)
        {
            this.audioDirectory = audioDirectory;
            lastMusicPosition = 0;
            isPlaying = false;
            currentSong = null;
            playlist = new List<string>();
            playlistPosition = 0;
            playPlaylist = false;
            volume = -1000;
            AutoPlayPlaylist = true;
            LoopCurrentSong = false;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Play the given song.
        /// </summary>
        /// <param name="songName">Full path to the song, or simply the song name.</param>
        /// <returns>True if the file could be found and has started playing; false otherwise.</returns>
        public bool Play(string songName)
        {
            return Play(songName, false);
        }

        /// <summary>
        /// Play the given song.
        /// </summary>
        /// <param name="songName"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
        public bool Play(string songName, bool loop)
        {
            LoopCurrentSong = loop;

            string finalPath;
            songName = songName.Replace("/", @"\");
            if (songName.Contains(@"\"))
            {
                currentSongTitle = songName.Substring(songName.LastIndexOf('\\') + 1);
                finalPath = songName;
            }
            else
            {
                currentSongTitle = songName;
                finalPath = Path.Combine(audioDirectory, songName);
            }

            currentSongPath = finalPath;

            try
            {
                currentSong = new Microsoft.DirectX.AudioVideoPlayback.Audio(finalPath);
                currentSong.Play();
                currentSong.Volume = Volume;
                currentSong.Ending += new EventHandler(OnSongEnding);

                isPlaying = true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (DirectXException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Event handler for when the song ends.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSongEnding(object sender, EventArgs e)
        {
            if (playPlaylist && AutoPlayPlaylist)
            {
                PlayNextSong = true;
            }
        }

        /// <summary>
        /// Do regular update checks, such as if a song has stopped, it should play the next song.
        /// </summary>
        public void Update()
        {
            if (currentSong != null && LoopCurrentSong && currentSong.Duration - currentSong.CurrentPosition < 0.01)
            {
                currentSong.CurrentPosition = 0;
            }
            
            if (PlayNextSong)
            {
                Next();

                PlayNextSong = false;
            }
        }

        /// <summary>
        /// Play all songs on the playlist. If no playlist exists currently, a new one
        /// is created through GeneratePlaylist().
        /// </summary>
        public bool PlayPlaylist()
        {
            if (playlist.Count == 0)
            {
                GeneratePlaylist();
                if (playlist.Count == 0)
                    return false;
            }

            playlistPosition = -1;
            playPlaylist = true;
            Next();

            return true;
        }

        /// <summary>
        /// Move to the next song in a playlist.
        /// </summary>
        public void Next()
        {
            if (!playPlaylist)
            {
                PlayPlaylist();
                return;
            }

            Stop();

            ++playlistPosition;
            if (playlistPosition >= playlist.Count)
                playlistPosition = 0;

            Play(playlist[playlistPosition]);
        }

        /// <summary>
        /// Move to the previous song in a playlist.
        /// </summary>
        public void Previous()
        {
            if (!playPlaylist)
            {
                if (playlist.Count == 0)
                {
                    GeneratePlaylist();
                    if (playlist.Count == 0)
                    {
                        throw new InvalidOperationException("No songs in playlist.");
                    }
                }

                playPlaylist = true;
            }

            Stop();

            --playlistPosition;
            if (playlistPosition <= -1)
                playlistPosition = playlist.Count - 1;

            Play(playlist[playlistPosition]);
        }

        /// <summary>
        /// Play a random song on the list.
        /// </summary>
        public void Random()
        {
            if (!playPlaylist)
            {
                if (playlist.Count == 0)
                {
                    GeneratePlaylist();
                    if (playlist.Count == 0)
                    {
                        throw new InvalidOperationException("No songs in playlist.");
                    }
                }

                playPlaylist = true;
            }

            Stop();

            Random random = new Random();
            int nextPosition = random.Next(0, playlist.Count - 1);
            playlistPosition = nextPosition;

            Play(playlist[playlistPosition]);
        }

        /// <summary>
        /// Stop music that is currently playing and dispose of the song.
        /// Does nothing if no music is playing.
        /// </summary>
        public void Stop()
        {
            if (isPlaying)
            {
                currentSong.Stop();
                currentSong.Dispose();
                currentSong = null;
                isPlaying = false;
                lastMusicPosition = 0;
            }
        }

        /// <summary>
        /// Pause the current song, if it is playing. Does nothing if it is not.
        /// </summary>
        public void Pause()
        {
            if (isPlaying)
            {
                lastMusicPosition = currentSong.CurrentPosition;
                currentSong.Pause();
            }
        }

        /// <summary>
        /// Resume the current song, if it is paused. Does nothing if it is
        /// not playing or paused.
        /// </summary>
        public void Resume()
        {
            if (isPlaying && currentSong.State == StateFlags.Paused)
            {
                currentSong.Play();
            }
        }

        /// <summary>
        /// Get the title of the current song, if any.
        /// </summary>
        /// <returns>Current song title, or the string "&lt;None&gt;" if no song is 
        /// playing.</returns>
        public string GetCurrentSongFilename()
        {
            if (isPlaying)
            {
                return currentSongTitle;
            }

            return "<None>";
        }

        /// <summary>
        /// Gets the duration of the current song, if a song is playing.
        /// </summary>
        /// <returns>Current song's duration in seconds, or -1 if a song is
        /// not playing.</returns>
        public double GetCurrentSongDuration()
        {
            if (isPlaying)
            {
                return currentSong.Duration;
            }

            return -1;
        }

        /// <summary>
        /// Generate a playlist from the audioDirectory given from the constructor.
        /// </summary>
        public void GeneratePlaylist()
        {
            const string DEFAULT_EXCLUSIONS = @"official\;.svn\";
            GeneratePlaylist(DEFAULT_EXCLUSIONS);
        }

        /// <summary>
        /// Generate a playlist excluding a list of exclusions.
        /// </summary>
        /// <param name="exclusions">Exclusions, which are formatted like:
        /// "official;nonplaylistmusic"
        /// or:
        /// "official,nonplaylistmusic"</param>
        public void GeneratePlaylist(string exclusions)
        {
            string[] separators = { ";", "," };
            string[] exclusionList = exclusions.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            playlist.Clear();

            DirectoryInfo directory = new DirectoryInfo(audioDirectory);
            FileInfo[] mp3Files = directory.GetFiles("*.mp3", SearchOption.AllDirectories);
            foreach (FileInfo file in mp3Files)
            {
                string filename = file.FullName;
                bool includeFile = true;
                foreach (string exclusion in exclusionList)
                {
                    if (filename.Contains(exclusion))
                    {
                        includeFile = false;
                        break;
                    }
                }

                if (includeFile)
                {
                    playlist.Add(file.FullName);
                }
            }
        }
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes the MP3 player, making it unusable by cleaning up all resources.
        /// </summary>
        public void Dispose()
        {
            Stop();
            playlist.Clear();
        }

        #endregion
    }
}
