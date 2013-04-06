using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using GameForms.src.Forms;
using Client.src.service;
using Client.src.util;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Text;
using Client.src.states.gamestate;
using Client.src.util.game;
using Renderer.SceneTools.Entities;

namespace Client.src.util
{
    /// <summary>
    /// A GameConsole is a command-line utility built into the client. It allows the developer
    /// to execute debug commands and look at game information to see what's happening. It is
    /// also a quick interface for testing a command, such as logging in or performing an
    /// administrative action.
    /// </summary>
    public class GameConsole
    {
        #region Members
        private GameConsoleForm form;
        private bool next = false;
        private StringBuilder buffer;
        private bool needsUpdate = true;
        private Queue<string> history;
        private int historyIndex = -1;
        private static readonly int HistoryLimit = 20;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the position of this console. The position indicates the top-left
        /// corner of the console window.
        /// </summary>
        public Rectangle Frame
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the key which will invoke this frame.
        /// </summary>
        public Keys KeyInvoke
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether this window is visible.
        /// </summary>
        public bool Visible
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the game console is enabled. To be disabled means that it
        /// will ignore invokation attempts.
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// The constructor performs basic form initialization required to use the console.
        /// It starts out hidden.
        /// </summary>
        /// <param name="frame">Frame of the console window; i.e. it's position, and it's
        /// width/height.</param>
        public GameConsole(Rectangle frame)
            : this(frame, Keys.OemTilde)
        {
        }

        /// <summary>
        /// The constructor performs basic form initialization required to use the cosnole.
        /// It starts out hidden.
        /// </summary>
        /// <param name="frame">Frame of the console window; i.e. it's positoin and it's
        /// width/height.</param>
        /// <param name="keyToInvoke">Key which invokes this frame.</param>
        public GameConsole(Rectangle frame, Keys keyToInvoke)
        {
            Frame = frame;
            KeyInvoke = keyToInvoke;
            Visible = false;
            Enabled = true;
            buffer = new StringBuilder("Welcome to the console. Type 'help' for info.\n", 10000);
            history = new Queue<string>(HistoryLimit);

            Initialize();
        }
        #endregion

        #region Initialize, Update, and Draw Methods
        /// <summary>
        /// Perform initializations necessary to run this class.
        /// </summary>
        private void Initialize()
        {
            form = new GameConsoleForm(ServiceManager.Game.Manager, Frame);
            form.Input.KeyPress += new TomShane.Neoforce.Controls.KeyEventHandler(Input_KeyPress);
        }

        /// <summary>
        /// Event handler for the text box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Input_KeyPress(object sender, TomShane.Neoforce.Controls.KeyEventArgs e)
        {
            if (e.Key == KeyInvoke)
            {
                // Eat the invoke.
                form.Input.Text = form.Input.Text.Replace("`", "");
            }
            else if (e.Key == Keys.Enter)
            {
                string text = form.Input.Text.Trim();
                form.Input.Text = "";
                historyIndex = -1;
                ProcessInput(text);
            }
            else if (e.Key == Keys.Up)
            {
                historyIndex--;
                if (historyIndex == -1)
                {
                    form.Input.Text = "";
                    form.Input.CursorPosition = 0;
                }
                else if (historyIndex < -1)
                {
                    historyIndex = history.Count - 1;
                }

                if (historyIndex >= 0 && historyIndex < history.Count)
                {
                    form.Input.Text = history.ToArray()[historyIndex];
                    form.Input.CursorPosition = form.Input.Text.Length;
                }
            }
            else if (e.Key == Keys.Down)
            {
                historyIndex++;
                if (historyIndex > HistoryLimit || historyIndex >= history.Count)
                {
                    historyIndex = -1;
                    form.Input.Text = "";
                    form.Input.CursorPosition = 0;
                }

                if (historyIndex >= 0 && historyIndex < history.Count)
                {
                    form.Input.Text = history.ToArray()[historyIndex];
                    form.Input.CursorPosition = form.Input.Text.Length;
                }
            }
        }

        /// <summary>
        /// Perform logic updates on the console window, which is essentially key processing
        /// and deciding whether or not it should be visible. This method does nothing if the
        /// console is disabled (as determined by the Enabled property).
        /// </summary>
        public void Update()
        {
            if (needsUpdate)
            {
                //form.TextArea.Text = buffer.ToString();
                needsUpdate = false;
            }

            // A quick check to make sure it's not simultaenously disabled and visible.
            if (!Enabled && Visible)
            {
                Visible = false;
            }

            if (!Enabled)
            {
                return;
            }

            if (Keyboard.GetState().IsKeyDown(KeyInvoke))
            {
                next = true;
            }
            else if (next)
            {
                next = false;

                Visible = !Visible;
                if (!Visible)
                {
                    ServiceManager.Game.FormManager.RemoveWindow(form.Window);
                    form.Window.Hide();
                    form.Input.Focused = false;
                }
                else
                {
                    ServiceManager.Game.FormManager.AddWindow(form.Window);
                    form.Window.Show();
                    form.Input.Focused = true;
                }
            }
        }

        /// <summary>
        /// Draw the console to the window. Note that if the console is not visible (as 
        /// determined by the Visible property) this method does nothing.
        /// </summary>
        public void Draw()
        {
            if (!Visible)
            {
                return;
            }
        }
        #endregion

        #region Processing
        /// <summary>
        /// The Commands class is a class that encapsulates all possible commands that can be
        /// executed from the console.
        /// </summary>
        protected class Commands
        {
            public delegate void Printer(string message, params object[] parameters);
            private Printer printer;
            private GameConsole console;

            public Commands(GameConsole _console, Printer _printer)
            {
                console = _console;
                printer = _printer;
            }

            /// <summary>
            /// Print out help about a given command.
            /// </summary>
            /// <param name="parameter">Parameter to help out with.</param>
            public void helptopic(string topic)
            {
                if (topic == "help")
                {
                    printer("help: Print a list of commands.");
                }
                else if (topic == "helptopic")
                {
                    printer("helptopic(topic): Ask for help on a specific topic.");
                }
                else if (topic == "ping")
                {
                    printer("ping(server): {0}",
                        "Get the average ping for either \"echelon\" or \"theater\".");
                }
                else if (topic == "loadmodel")
                {
                    printer("loadmodel(modelname, layer, x, y, z): Load a model and draw it.");
                    printer("  modelname: Path to the model (e.g. tanks\\rhino).");
                    printer("  layer: Layer number (e.g. 1).");
                    printer("  x, y, z: Position to draw the model at.");
                }
                else if (topic == "loadtexture")
                {
                    printer("loadtexture(texture, layer, x, y, z): Load a texture and draw it.");
                    printer("  texture: Path to the textures (e.g. tiles\\cursors\\circle).");
                    printer("  layer: Layer number (e.g. 1).");
                    printer("  x, y, z: Position to draw the texture at.");
                }
                else if (topic == "remove")
                {
                    printer("remove(id): Remove an object by it's integer ID.");
                }
                else if (topic == "removeall")
                {
                    printer("removeall: Remove *all* objects from the renderer.");
                }
                else if (topic == "move")
                {
                    printer("move(id, x, y, z): Move an object to a new position.");
                }
                else if (topic == "translate")
                {
                    printer("translate(id, x, y, z): Translate an object (x, y, z) units.");
                }
                else if (topic == "attach")
                {
                    printer("attach(attacher, attachee): Attach one 3D object to another.");
                }
                else if (topic == "orbit")
                {
                    printer("orbit(orbiter, orbitee, distance, speed): Orbit one object around another.");
                }
                else if (topic == "sampleemitter")
                {
                    printer("Attaches a new SampleParticleEmitter object onto the local player.");
                }

                else if (topic == "tracktocursor")
                {
                    printer("tracktocursor(entity): Makes an entity point at the cursor.");
                }
                else if (topic == "zoom")
                {
                    printer("zoom(zoomlevel): Zoom to the specified pixel value (0-5000).");
                }
                else if (topic == "switchcamera")
                {
                    printer("switchcamera(newcamera): Switch to a new camera view (e.g. 'overhead').");
                }
                else if (topic == "clear")
                {
                    printer("clear: Clear the console of it's text.");
                }
                else if (topic == "exit")
                {
                    printer("exit: Exit the game.");
                }
                else if (topic == "setresolution")
                {
                    printer("setresolution(width, height): Switch to a new resolution immediately.");
                }
                else if (topic == "togglefullscreen")
                {
                    printer("togglefullscreen: Toggle between windowed and full screen mode.");
                }
                else if (topic == "jukebox")
                {
                    printer("jukebox(command, ...): Music player. Type \"jukebox\" to see more.");
                }
                else
                {
                    printer("There is no help topic for \"{0}\".", topic);
                }
            }

            /// <summary>
            /// Print a list of available commands.
            /// </summary>
            public void help()
            {
                printer("Commands:");
                Type type = typeof(Commands);
                MethodInfo[] methods = type.GetMethods();
                foreach (MethodInfo method in methods)
                {
                    if (method.Name == "SetPrinter" || method.Name == "ToString" ||
                        method.Name == "GetType" || method.Name == "Equals" || 
                        method.Name == "GetHashCode")
                    {
                        continue;
                    }

                    System.Text.StringBuilder buffer = new System.Text.StringBuilder();
                    buffer.Append(method.Name);

                    buffer.Append("(");
                    ParameterInfo[] parameters = method.GetParameters();
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        buffer.Append(parameters[i].Name);
                        if (i + 1 < parameters.Length)
                        {
                            buffer.Append(", ");
                        }
                    }
                    buffer.Append(")");

                    printer(buffer.ToString());
                }
            }

            public void help(string with_what)
            {
                helptopic(with_what);
            }

            public void lockcameras()
            {
                ServiceManager.Scene.LockCameras();
            }

            public void resettext()
            {
                State state = ServiceManager.StateManager.CurrentState;
                if (state is GamePlayState)
                {
                    GamePlayState gameState = (GamePlayState)state;
                    foreach (PlayerTank player in gameState.Players.Values)
                    {
                        if (player.NameObject != null)
                            player.NameObject.SetText(player.Name);
                    }
                }
                else
                {
                    printer("You can't do that outside of a game.");
                }
            }

            public void jukebox()
            {
                jukebox("help");
            }

            public void jukebox(string command)
            {
                switch (command)
                {
                    case "play":
                        printer("Usage: jukebox play [song]");
                        break;
                    case "volume":
                        printer("jukebox: volume is at {0}", ServiceManager.MP3Player.Volume);
                        printer("jukebox: use \"jukebox volume [level]\" to change it.");
                        break;
                    case "stop":
                        ServiceManager.MP3Player.Stop();
                        printer("jukebox: stopped.");
                        break;
                    case "next":
                        ServiceManager.MP3Player.Next();
                        printer("jukebox: now playing {0}", ServiceManager.MP3Player.GetCurrentSongFilename());
                        break;
                    case "prev":
                        ServiceManager.MP3Player.Previous();
                        printer("jukebox: now playing {0}", ServiceManager.MP3Player.GetCurrentSongFilename());
                        break;
                    case "rand":
                        ServiceManager.MP3Player.Random();
                        printer("jukebox: now playing {0}", ServiceManager.MP3Player.GetCurrentSongFilename());
                        break;
                    case "pause":
                        ServiceManager.MP3Player.Pause();
                        printer("jukebox: paused.");
                        break;
                    case "resume":
                        ServiceManager.MP3Player.Resume();
                        printer("jukebox: resumed.");
                        break;
                    case "playlist":
                        ServiceManager.MP3Player.GeneratePlaylist();
                        ServiceManager.MP3Player.PlayPlaylist();
                        printer("jukebox: playing playlist.");
                        break;
                    default:
                        printer("Commands: play, stop, next, prev, rand, pause, resume, playlist, volume");
                        break;
                }
            }

            public void jukebox(string command, string param)
            {
                if (command != "play" && command != "volume")
                {
                    printer("jukebox: command {0} does not accept parameters.", command);
                    return;
                }

                if (command == "play")
                {
                    ServiceManager.MP3Player.Play(param);
                    printer("jukebox: now playing {0}", ServiceManager.MP3Player.GetCurrentSongFilename());
                }
                else
                {
                    ServiceManager.MP3Player.Volume = Int32.Parse(param);
                    printer("jukebox: volume set to {0}", param);
                }
            }

            public void setresolution(string res_width, string res_height)
            {
                int width = int.Parse(res_width);
                int height = int.Parse(res_height);

                ServiceManager.Game.GraphicsDeviceManager.PreferredBackBufferWidth = width;
                ServiceManager.Game.GraphicsDeviceManager.PreferredBackBufferHeight = height;
                ServiceManager.Game.GraphicsDeviceManager.ApplyChanges();
            }

            public void togglefullscreen()
            {
                ServiceManager.Game.GraphicsDeviceManager.ToggleFullScreen();
            }

            public void sampleemitter()
            {
                State state = ServiceManager.StateManager.CurrentState;
                if (state is GamePlayState)
                {
                    GamePlayState gameState = (GamePlayState)state;
                    ServiceManager.Scene.AddParticleEmitterAtPosition("Sample",
                        gameState.LocalPlayer.Position);
                }
                else
                {
                    printer("You can't do that outside of a game.");
                }
            }

            public void skin(string skin_name)
            {
                if (String.IsNullOrEmpty(skin_name))
                {
                    printer("Usage: skin [skin name]");
                }
                else
                {
                    State state = ServiceManager.StateManager.CurrentState;
                    if (state is GamePlayState)
                    {
                        GamePlayState gameState = (GamePlayState)state;
                        PlayerTank localPlayer = gameState.LocalPlayer;
                        try
                        {
                            Texture2D skin = ServiceManager.Resources.Load<Texture2D>(
                                String.Format(@"models\tanks\skins\{0}", skin_name));
                            localPlayer.ApplySkin(skin);
                            printer("New skin applied.");
                        }
                        catch (Exception)
                        {
                            printer("That skin does not exist!");
                        }
                    }
                    else
                    {
                        printer("You can't do that outside of a game.");
                    }
                }
            }

            /// <summary>
            /// Get the average ping for a targetted server.
            /// </summary>
            /// <param name="server">Server or server alias to ping.</param>
            public void ping(string server)
            {
                server = server.ToLower();
                if (server == "echelon" || server == "main" || server == "master")
                {
                    if (ServiceManager.Echelon == null)
                    {
                        printer("You are not connected to a master server.");
                    }
                    else
                    {
                        printer("Ping to the master server: {0}",
                            ServiceManager.Echelon.GetFormattedAverageLatency());
                    }
                }
                else if (server == "theater" || server == "game" || server == "theatre")
                {
                    if (ServiceManager.Theater == null)
                    {
                        printer("You are not connected to a game server.");
                    }
                    else
                    {
                        printer("Ping to the game server: {0}",
                            ServiceManager.Theater.GetFormattedAverageLatency());
                    }
                }
                else
                {
                    helptopic("ping");
                }
            }

            /// <summary>
            /// Get the entire player list.
            /// </summary>
            public void playerlist()
            {
                State state = ServiceManager.StateManager.CurrentState;
                if (state is GamePlayState)
                {
                    GamePlayState gameState = (GamePlayState)state;

                    foreach (PlayerTank player in gameState.Players.Values)
                    {
                        printer("#{0}: Name={1} Pos=({2}, {3}, {4}) RenderID={5} TurretRenderID={6}",
                            player.ID, player.Name, player.Position.X, player.Position.Y, player.Position.Z,
                            player.RenderID, player.TurretRenderID);
                    }
                }
                else
                {
                    printer("You can only view the player list in-game!");
                }
            }

            /// <summary>
            /// Load a model and display it in-game.
            /// </summary>
            /// <param name="modelname">Name of the model to load.</param>
            /// <param name="layer">Layer to add the model to.</param>
            /// <param name="x">Position of the model (x).</param>
            /// <param name="y">Position of the model (y).</param>
            /// <param name="z">Position of the model (z).</param>
            public void loadmodel(string modelname, string layer, string x, string y, string z)
            {
                Model model = ServiceManager.Resources.GetModel(modelname);
                int id = ServiceManager.Scene.Add(
                    new Renderer.SceneTools.Entities.Object3(model,
                        new Vector3(float.Parse(x), float.Parse(y), float.Parse(z))), 
                    int.Parse(layer));
                printer("Model {0} added to the renderer as ID #{1}.", modelname, id);
            }

            /// <summary>
            /// Refresh the list of players.
            /// </summary>
            public void refresh()
            {
                State state = ServiceManager.StateManager.CurrentState;
                if (state is GamePlayState)
                {
                    GamePlayState gameState = (GamePlayState)state;
                    gameState.Players.RefreshPlayerList();
                }
                else
                {
                    printer("You are not in a game!");
                }
            }

            /// <summary>
            /// Attempt to remove the given ID from the game's renderer.
            /// </summary>
            /// <param name="id">ID of the asset to remove.</param>
            public void remove(string id)
            {
                ServiceManager.Scene.Delete(int.Parse(id));
                printer("Object with the ID {0} was removed from the renderer.", id);
            }

            /// <summary>
            /// Attempt to remove *all* objects from the renderer.
            /// </summary>
            public void removeall()
            {
                ServiceManager.Scene.ClearAll();
            }

            /// <summary>
            /// Move an object given it's ID.
            /// </summary>
            /// <param name="id">ID of the object to move.</param>
            /// <param name="x">New position of the object (x).</param>
            /// <param name="y">New position of the object (y).</param>
            /// <param name="z">New position of the object (z).</param>
            public void move(string id, string x, string y, string z)
            {
                try
                {
                    ServiceManager.Scene.Access(int.Parse(id)).Position =
                        new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
                    printer("Moved the object with the ID #{0} to position ({1}, {2}, {3})",
                        id, x, y, z);
                }
                catch (FormatException)
                {
                    printer("All values must be integers.");
                }
                catch (Exception)
                {
                    printer("An object with the ID #{0} doesn't exist.", id);
                }
            }

            /// <summary>
            /// Translate an object given it's ID.
            /// </summary>
            /// <param name="id">ID of the object to translate.</param>
            /// <param name="x">Units to move the object (x).</param>
            /// <param name="y">Units to move the object (y).</param>
            /// <param name="z">Units to move the object (z).</param>
            public void translate(string id, string x, string y, string z)
            {
                try
                {
                    ServiceManager.Scene.Access(int.Parse(id)).Translate(
                        new Vector3(float.Parse(x), float.Parse(y), float.Parse(z)));
                    printer("Translated object #{0} ({1}, {2}, {3}) units.",
                        id, x, y, z);
                }
                catch (FormatException)
                {
                    printer("All values must be integers.");
                }
                catch (Exception)
                {
                    printer("An object with the ID #{0} doesn't exist.", id);
                }
            }

            /// <summary>
            /// Attach one 3D object to another 3D object.
            /// </summary>
            /// <param name="attacher">The model who will leech off of the 
            /// atachee.</param>
            /// <param name="attachee">The model to attach to.</param>
            public void attach(string attacher, string attachee)
            {
                try
                {
                    ServiceManager.Scene.Access3D(
                        int.Parse(attacher)).Attach(
                            ServiceManager.Scene.Access3D(
                                int.Parse(attachee)),
                                Constants.TURRET_MOUNT);
                    printer("Attached #{0} to #{1}.", attacher, attachee);
                }
                catch (FormatException)
                {
                    printer("All values must be integers.");
                }
                catch (Exception)
                {
                    printer("An object with the ID #{0} or ID #{1} doesn't exist.", 
                        attacher, attachee);
                }
            }

            /// <summary>
            /// Make one 3D object orbit around another 3D object.
            /// </summary>
            /// <param name="orbiter">Object that will rotate.</param>
            /// <param name="orbitee">Object to rotate around.</param>
            /// <param name="distance">Distance from the original object.</param>
            /// <param name="speed">Speed of the orbit.</param>
            public void orbit(string orbiter, string orbitee, string distance, string speed)
            {
                try
                {
                    ServiceManager.Scene.Access3D(
                        int.Parse(orbiter)).Orbit(
                            ServiceManager.Scene.Access3D(
                                int.Parse(orbitee)),
                                float.Parse(distance),
                                float.Parse(speed)
                        );
                    printer("Orbiting #{0} around #{1}.", orbiter, orbitee);
                }
                catch (FormatException)
                {
                    printer("All values must be integers.");
                }
                catch (Exception)
                {
                    printer("An object with the ID #{0} or ID #{1} doesn't exist.",
                        orbiter, orbitee);
                }
            }

            /// <summary>
            /// Make one 3D object aim at another.
            /// </summary>
            /// <param name="tracker">Object that will do the aiming.</param>
            /// <param name="trackee">Object to be tracked.</param>
            public void track(string tracker, string trackee)
            {
                try
                {
                    ServiceManager.Scene.Access3D(
                        int.Parse(tracker)).TrackTo(
                            ServiceManager.Scene.Access3D(
                                int.Parse(trackee))
                        );
                    printer("Tracking #{0} to #{1}.", tracker, trackee);
                }
                catch (FormatException)
                {
                    printer("All values must be integers.");
                }
                catch (Exception)
                {
                    printer("An object with the ID #{0} or ID #{1} doesn't exist.",
                        tracker, trackee);
                }
            }

            /// <summary>
            /// Switch to a new camera.
            /// </summary>
            /// <param name="newcamera">Camera name to switch to.</param>
            public void switchcamera(string newcamera)
            {
                try
                {
                    ServiceManager.Game.Renderer.ActiveScene.SwitchCamera(newcamera);
                    if (newcamera.Equals("chase"))
                    {
                        ServiceManager.Game.Renderer.ActiveScene.TransparentWalls = true;
                    }
                    else
                    {
                        ServiceManager.Game.Renderer.ActiveScene.TransparentWalls = false;
                    }

                    printer("Using camera {0}.", newcamera);
                }
                catch (Exception)
                {
                    printer("The camera \"{0}\" does not exist.", newcamera);
                }
            }

            /// <summary>
            /// Zoom the camera to a new level.
            /// </summary>
            /// <param name="zoomlevel">Z-index of the camera.</param>
            public void zoom(string zoomlevel)
            {
                Renderer.SceneTools.Entities.Camera camera =
                    ServiceManager.Game.Renderer.ActiveScene.CurrentCamera;

                try
                {
                    float zoom = float.Parse(zoomlevel);
                    camera.Position = new Vector3(camera.Position.X, camera.Position.Y,
                        zoom);

                    printer("Camera zoom level set to {0}.", zoom);
                }
                catch (FormatException)
                {
                    printer("The zoom level must be an integer.");
                }
            }

            /// <summary>
            /// Makes an entity point towards the cursor.
            /// </summary>
            /// <param name="entityID">The renderer id of the entity to track.</param>
            public void tracktocursor(string entityID)
            {
                ServiceManager.Scene.Access3D(int.Parse(entityID)).TrackToCursor();
            }

            /// <summary>
            /// Insert a chat message into the chat window.
            /// </summary>
            /// <param name="message">Message to insert.</param>
            public void say(string message)
            {
                State state = ServiceManager.StateManager.CurrentState;
                if (!(state is GamePlayState))
                {
                    printer("Cannot chat outside of the game.");
                }
                else
                {
                    GamePlayState game = (GamePlayState)state;
                    ServiceManager.Theater.SendChatMessage(message);
                }
            }

            /// <summary>
            /// Clear the console of all of it's text.
            /// </summary>
            public void clear()
            {
                console.Clear();
            }

            /// <summary>
            /// Exit the game.
            /// </summary>
            public void exit()
            {
                ServiceManager.Game.Exit();
            }
        }

        /// <summary>
        /// Process the input from the console and execute an action depending on the request.
        /// </summary>
        /// <param name="message">Message to parse.</param>
        private void ProcessInput(string message)
        {
            if (String.IsNullOrEmpty(message))
            {
                return;
            }

            message = message.Replace('(', ' ')
                             .Replace(')', ' ')
                             .Replace(',', ' ')
                             .Replace(";", "")
                             .Trim();

            List<object> parameters = new List<object>(
                message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            for (int i = 0; i < parameters.Count; i++)
            {
                string param = parameters[i].ToString().Trim();
                // Merge parameters who are wrapped in quotation marks.
                if (param.Contains("\"")) 
                {
                    int index = param.IndexOf('"');
                    int lastIndex = param.LastIndexOf('"');
                    if (lastIndex == index)
                    {
                        param = param.Replace("\"", "");

                        // The closing quotation mark is not within the same parameter.
                        bool found = false;
                        int j = i + 1;
                        // Search for the closing mark.
                        while (j < parameters.Count)
                        {
                            param += " " + parameters[j];
                            if (param.Contains("\""))
                            {
                                found = true;
                                break;
                            }

                            j++;
                        }

                        if (!found)
                        {
                            DebugPrint("Syntax error: No closing quotation mark.");
                            return;
                        }
                        else
                        {
                            // Remove the extra parameters that we just merged.
                            int toRemove = (j - i) + (i + 1);
                            for (int k = i + 1; k < toRemove; k++)
                            {
                                parameters.RemoveAt(i + 1);
                            }
                        }
                    }
                }

                parameters[i] = param.Replace("\"", "");
            }

            string command = parameters[0].ToString().ToLower();
            parameters.RemoveAt(0);

            // Print a preview of what the command looks like in our eyes.
            System.Text.StringBuilder buffer = new System.Text.StringBuilder();
            buffer.Append("> ");
            buffer.Append(command);
            buffer.Append('(');
            for (int i = 0; i < parameters.Count; i++)
            {
                buffer.Append(parameters[i]);
                if (i + 1 < parameters.Count)
                {
                    buffer.Append(", ");
                }
            }
            buffer.Append(");");

            string formattedMessage = buffer.ToString();
            DebugPrint(formattedMessage);
            history.Enqueue(formattedMessage.Substring(2));
            while (history.Count > HistoryLimit)
            {
                history.Dequeue();
            }

            // Attempt to find the method by the given name.
            Commands commands = new Commands(this, DebugPrint);
            MethodInfo[] methods = typeof(Commands).GetMethods();
            MethodInfo method = null;
            foreach (MethodInfo testMethod in methods)
            {
                if (testMethod.Name == command && testMethod.GetParameters().Length == parameters.Count)
                {
                    method = testMethod;
                    break;
                }
            }

            //MethodInfo method = typeof(Commands).GetMethod(command);
            if (method == null)
            {
                DebugPrint(
                    "Command \"{0}\" is not recognized. Type \"help()\" for help.", 
                    command);
                return;
            }

            int numberOfParameters = method.GetParameters().Length;
            if (parameters.Count != numberOfParameters)
            {
                commands.helptopic(command);
                return;
            }

            try
            {
                method.Invoke(commands, parameters.ToArray());
            }
            catch (Exception e)
            {
                DebugPrint("Syntax error: {0}", e.Message);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Print a debug message to the game console. Whether or not the console is visible,
        /// the message is appended to the console's text.
        /// </summary>
        /// <param name="message">Message to print with no formatting.</param>
        public void DebugPrint(string message)
        {
                if (String.IsNullOrEmpty(message))
                {
                    System.ArgumentNullException nullEx =
                            new ArgumentNullException("message", "Arguments may not be null or empty.");
                    System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(nullEx);
                    Console.Error.WriteLine(trace.ToString());
                }
                else
                {
                    form.TextArea.AddString(message);
                    needsUpdate = true;
                }
   
        }

        /// <summary>
        /// Print a debug message to the game console. Whether or not the console is visible,
        /// the message is appended to the console's text.
        /// </summary>
        /// <param name="message">Message to print.</param>
        /// <param name="args">Formatting arguments for the message.</param>
        public void DebugPrint(string message, params object[] args)
        {
            try
            {
                DebugPrint(String.Format(message, args));
            }
            catch { }
        }

        /// <summary>
        /// Clear the console of all of it's text.
        /// </summary>
        public void Clear()
        {
            buffer.Remove(0, buffer.Length);
            form.TextArea.Clear();
           // form.TextArea.Text = "";
        }
        #endregion
    }
}
