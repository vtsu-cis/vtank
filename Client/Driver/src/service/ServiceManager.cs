namespace Client.src.service
{
    using System;
    using Client.src.service.services;
    using Network.Main;
    using Network.Game;
    using Client.src.callbacks;
    using Network.Util;
    using Client.src.config;
    using Client.src.util;
    using Audio;
using Audio.Music;
    using System.Threading;

    /// <summary>
    /// The Service tracks all in-game services; that is, objects that are globally
    /// tracked and can be accessed from anywhere. 
    /// </summary>
    public static class ServiceManager
    {
        #region Constants
        public static readonly string ECHELON_HOST = "155.42.94.219";
        public static readonly int ECHELON_PORT = 4063;
        #endregion

        #region Properties (Services)
        /// <summary>
        /// Gets the object through which all gameplay takes place. While most actual
        /// actions don't take place through this game object, it does hold vital
        /// objects and configurations such as the graphics device manager and the font.
        /// </summary>
        public static VTank Game
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the resource manager to load resources.
        /// </summary>
        public static ResourceManager Resources
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the audio manager used to manage and play songs.
        /// </summary>
        public static xAudio AudioManager
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the MP3 Player, used to play music in the background.
        /// </summary>
        public static MP3Player MP3Player
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the object that controls which state the game is on.
        /// </summary>
        public static StateManager StateManager
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Gets the object which communicates with Echelon.
        /// Use ConnectToEchelon first.
        /// </summary>
        public static MasterCommunicator Echelon
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the object which communicates with Theater.
        /// Use ConnectToTheater first.
        /// </summary>
        public static GameCommunicator Theater
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the current server instance.
        /// </summary>
        public static GameServerInfo CurrentServer
        {
            get;
            set;
        }

        /// <summary>
        /// A shortcut method to getting the active scene.
        /// </summary>
        public static Renderer.SceneTools.Scene Scene
        {
            get
            {
                return Game.Renderer.ActiveScene;
            }
        }

        /// <summary>
        /// Gets the logger, which may be used to quickly log a message to a file.
        /// </summary>
        public static Client.src.service.services.Logger Logger
        {
            get;
            private set;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Stop all services and dispose of them. This should be called once before the game
        /// exits. It can be called repeatedly to destroy re-created resources.
        /// </summary>
        public static void StopAllServices()
        {
            try
            {
                Game.Dispose();
            }
            catch (Exception) { }
            finally
            {
                Game = null;
            }

            try
            {
                Resources.Dispose();
            }
            catch (Exception) { }
            finally
            {
                Resources = null;
            }

            StateManager = null;
            try
            {
                Echelon.Dispose();
            }
            catch (Exception) { }
            finally
            {
                Echelon = null;
            }

            try
            {
                Theater.Dispose();
            }
            catch (Exception) { }
            finally
            {
                Theater = null;
            }

            try
            {
                AudioManager.Dispose();
            }
            catch (Exception) { }
            finally 
            {
                AudioManager = null;
            }

            try
            {
                MP3Player.Dispose();
            }
            catch (Exception) { }
            finally
            {
                MP3Player = null;
            }

            try
            {
                Logger.Close();
            }
            catch (Exception) { }
            finally
            {
                Logger = null;
            }
        }

        /// <summary>
        /// Start the game and run it, causing it to block until the game exits.
        /// Fails if the game has already been started.
        /// </summary>
        public static void StartGame()
        {
            if (Game == null)
            {
                Game = new VTank();
                Game.Run();
            }
            else
            {
                throw new Exception("The game has already been started.");
            }
        }

        /// <summary>
        /// Create the manager which tracks resources. Fails if it has already been
        /// created.
        /// </summary>
        public static void CreateResourceManager()
        {
            if (Resources == null)
            {
                Resources = new ResourceManager();
            }
            else
            {
                throw new Exception("The resource manager has already been created.");
            }
        }

        /// <summary>
        /// Create the manager which creates and plays sound cues. Fails if it has already
        /// been created.
        /// </summary>
        public static void CreateAudioManager()
        {
            if (AudioManager == null)
            {
                AudioManager = new xAudio(
                    "Content\\audio\\VTankAudio.xgs",
                    "Content\\audio\\Wave Bank.xwb",
                    "Content\\audio\\Sound Bank.xsb");
            }
            else
            {
                throw new Exception("The audio manager has already been created.");
            }
        }

        /// <summary>
        /// Create the manager which allows the programmer to play MP3 files. Fails if it has
        /// already been created.
        /// </summary>
        public static bool CreateMP3Player()
        {
            if (MP3Player == null)
            {
                const string DEFAULT_MUSIC_DIRECTORY = "Music";
                try
                {
                    MP3Player = new MP3Player(DEFAULT_MUSIC_DIRECTORY);
                    return true;
                }
                catch (Exception ex)
                {
                    // There are sometimes a LoaderLock exception thrown due to incompatibilities with 
                    // DirectX and VS2008 in Debug mode. This will catch that error and ignore it.
                    Console.Error.WriteLine(ex);
                    return false;
                }
            }
            else
            {
                throw new Exception("The MP3 player has already been created.");
            }
        }

        /// <summary>
        /// Create a new state manager. Fails if one already exists.
        /// </summary>
        public static void CreateStateManager()
        {
            if (StateManager == null)
            {
                StateManager = new StateManager();
            }
            else
            {
                throw new Exception("The state manager has already been created.");
            }
        }

        /// <summary>
        /// Create the Echelon service. Once created, it attempts to open the connection
        /// by calling Echelon.Connect(). If unsuccessful, it will throw an exception and
        /// null out the communicator.
        /// </summary>
        /// <param name="host">Host to connect to.</param>
        /// <param name="port">Port that the server listens on.</param>
        public static void ConnectToEchelon()
        {
            if (Echelon == null)
            {
                try
                {
                    Echelon = new MasterCommunicator(ECHELON_HOST, ECHELON_PORT);
                    Echelon.ConfigFile = "config.client";
                    if (!Echelon.Connect())
                    {
                        Echelon.Disconnect();
                        Echelon = null;

                        throw new Exception("Unable to connect to " + ECHELON_HOST + ".");
                    }
                }
                catch (Exception)
                {
                    Echelon = null;
                    throw;
                }
            }
            else
            {
                throw new Exception("The communicator has already been created.");
            }
        }

        public delegate void ConnectCallback(bool result);
        /// <summary>
        /// Connect to Echelon asynchronously.
        /// </summary>
        /// <param name="callback"></param>
        public static void ConnectToEchelonAsync(ConnectCallback callback)
        {
            if (callback == null || Echelon != null)
            {
                ConnectToEchelon();
            }
            else
            {
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        ConnectToEchelon();
                        callback(true);
                    }
                    catch (Exception)
                    {
                        callback(false);
                    }
                });
                thread.Start();
            }
        }

        /// <summary>
        /// Disconnect from Echelon, enabling ConnectToEchelon to be called again.
        /// </summary>
        public static void DestroyEchelonCommunicator()
        {
            if (Echelon != null)
            {
                Echelon.Disconnect();
                Echelon = null;
            }
        }

        /// <summary>
        /// Open a connection to an instance of Theater. Uses a timeout of 10 seconds.
        /// </summary>
        /// <param name="server">Game server to join.</param>
        public static void ConnectToTheater(GameServerInfo server)
        {
            ConnectToTheater(server, 10000);
        }

        /// <summary>
        /// Open a connection to an instance of Theater.
        /// </summary>
        /// <param name="server">Game server to join.</param>
        /// <param name="timeout">Timeout in milliseconds.</param>
        public static void ConnectToTheater(GameServerInfo server, long timeout)
        {
            if (Echelon == null)
            {
                throw new Exception("You must connect to Echelon first.");
            }

            if (Theater == null)
            {
                try
                {
                    Theater = new GameCommunicator(
                        server.Host, server.Port, server.UseGlacier2, timeout);
                    if (!Theater.Connect())
                    {
                        Theater.Disconnect();
                        Theater = null;

                        throw new Exception("Unable to connect to " + server.Host + ".");
                    }
                }
                catch (Exception)
                {
                    Theater = null;
                    throw;
                }
            }
            else
            {
                throw new Exception("The communicator has already been created.");
            }
        }

        /// <summary>
        /// Disconnect from Theater, enabling ConnectToTheater to be called again.
        /// </summary>
        public static void DestroyTheaterCommunicator()
        {
            if (Theater != null)
            {
                Theater.Disconnect();
                Theater.Dispose();
                Theater = null;
            }
        }

        /// <summary>
        /// Create a logger using the default log filename ("error.log").
        /// </summary>
        /// <returns>True if the logger was successfully created; false otherwise.</returns>
        public static bool CreateLogger()
        {
            const string DEFAULT_LOG_FILENAME = @"error.log";
            return CreateLogger(DEFAULT_LOG_FILENAME);
        }

        /// <summary>
        /// Create a logger using the given filename.
        /// </summary>
        /// <param name="filename">Name of the log file.</param>
        /// <returns>True if the logger was successfully created; false otherwise.</returns>
        public static bool CreateLogger(string filename)
        {
            if (Logger != null)
            {
                throw new InvalidOperationException(
                    "Cannot create two instances of the logger.");
            }

            try
            {
                Logger = new Client.src.service.services.Logger(filename);
                Logger.Info("Started client up at {0}", DateTime.Now);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[ERROR] Cannot log messages: {0}", ex);
            }

            return false;
        }
        #endregion
    }
}
