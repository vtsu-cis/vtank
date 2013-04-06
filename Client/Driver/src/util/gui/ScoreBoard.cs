using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using VTankObject;
using Client.src.service;
using Client.src.states.gamestate;
using Microsoft.Xna.Framework;

namespace Client.src.util.game
{

    /// <summary>
    /// Displays a Scoreboard for use in the VTank game. Thread-safe.
    /// </summary>
    public class ScoreBoard
    {
        #region Structs
        public struct TeamStats
        {
            public int kills;
            public int assists;
            public int deaths;
            public int objectives;
            public int roundWins;
        }

        public struct Score
        {
            public Color Color;
            public Statistics Statistics;

            public Score(Color _color, Statistics _stats)
            {
                Color = _color;
                Statistics = _stats;
            }

            /// <summary>
            /// Sorts by kills, deaths, assists, and finally name
            /// </summary>
            /// <param name="val"></param>
            /// <returns></returns>
            public int CompareTo(Score val)
            {
                if (this.Statistics.kills > val.Statistics.kills)
                    return -1;
                else if (this.Statistics.kills < val.Statistics.kills)
                    return 1;
                else
                {
                    if (this.Statistics.deaths > val.Statistics.deaths)
                        return 1;
                    else if (this.Statistics.deaths < val.Statistics.deaths)
                        return -1;
                    else
                    {
                        if (this.Statistics.assists > val.Statistics.assists)
                            return -1;
                        else if (this.Statistics.assists < val.Statistics.assists)
                            return 1;
                        else
                        {
                            return this.Statistics.tankName.CompareTo(val.Statistics.tankName);
                        }
                    }
                }
            }
            
        }
        #endregion

        #region Members
        private readonly string pathToPromotedImage = "textures\\misc\\Scoreboard\\promotedmessage";
        private readonly string pathToBlueTeamWins = "textures\\misc\\Scoreboard\\blueteamwins";
        private readonly string pathToRedTeamWins = "textures\\misc\\Scoreboard\\redteamwins";
        private float roundWinnerOpacity = 2.0f;
        private string roundWinningTeam;
        private bool drawingRoundWinner = false;

        private Dictionary<string, int> initialRanks;
        private Dictionary<string, int> ranks;
        private List<string> playersWhoRankedUp;
        //private Texture2D rankUpColoring;
        private float rankupMessageOpacity = 1.0f;
        private bool localPlayerRankedUp = false;
        Color customBlue = new Color();

        private GameMode mode;
        private Dictionary<String, Score> team1;
        private Dictionary<String, Score> team2;
        private TeamStats team1stats;
        private TeamStats team2stats;
        private string[] header;
        private string[] serverInfo;
        private static readonly string[] serverInfoHeader = new String[] { 
            "Server Name", "Game Mode", "Map Name" };
        private static readonly string[] team1Header = new String[] { "Team 1" };
        private static readonly string[] team2Header = new String[] { "Team 2" };
        private PlayerManager players;
        private Rectangle rect;
        private Texture2D line;
        private Color neutralTeamColor = Color.LightGreen;

        public bool Enabled = false;
        public Color BackColor = new Color(Color.Black, 200);

        const int x_offset = 10; // Offset of the 'x' coordinate for drawing the scoreboard values.
        #endregion

        #region Properties

        /// <summary>
        /// Returns whether the scoreboard is drawing the round winner.
        /// </summary>
        public bool DrawingRoundWinner
        {
            get { return drawingRoundWinner; }
        }

        #endregion

        #region Initializing

        /// <summary>
        /// Scoreboard is created with values unique to mode being played 
        /// </summary>
        /// <param name="_mode">The current game mode being played.</param>
        /// <param name="_players">The list of current players.</param>
        public ScoreBoard(GameMode _mode, PlayerManager _players)
        {
            players = _players;
            mode = _mode;
            Rectangle viewport = ServiceManager.Game.GraphicsDevice.Viewport.TitleSafeArea;
            rect = new Rectangle();
            rect.Width = (int)(viewport.Width * .70f);
            rect.Height = (int)(viewport.Height * .67f);
            rect.X = (viewport.Width/2) - (rect.Width/2);
            rect.Y = viewport.Height / 2 - rect.Height / 2;
            Initialize(_mode);
            InitScoreBoard();
            Refresh();
        }

        /// <summary>
        /// Sets up the scoreboard according to the type of game being played
        /// </summary>
        /// <param name="gameMode">The current game mode being played.</param>
        private void Initialize(GameMode gameMode)
        {
            //ranks = new Dictionary<string, int>();
            //initialRanks = new Dictionary<string, int>();
            playersWhoRankedUp = new List<string>();

            RefreshRanks(); //Load ranks for current rank tracking
            initialRanks = LoadRanks(this.initialRanks);
            LoadHeader(gameMode);
            team1 = new Dictionary<string, Score>();
            team1stats = new TeamStats();
            team1stats.assists = 0;
            team1stats.deaths = 0;
            team1stats.kills = 0;
            team1stats.objectives = 0;
            team1stats.roundWins = 0;

            customBlue.R = 48;
            customBlue.G = 75;
            customBlue.B = 255;
            customBlue.A = 255;

            if (gameMode != GameMode.DEATHMATCH)
            {
                team2 = new Dictionary<string, Score>();
                team2stats = new TeamStats();
                team2stats.assists = 0;
                team2stats.deaths = 0;
                team2stats.kills = 0;
                team2stats.objectives = 0;
                team2stats.roundWins = 0;
            }
       
            line = new Texture2D(ServiceManager.Game.GraphicsDevice, 1, 1);
            line.SetData<Color>(new Color[] { Color.White });
        }

        /// <summary>
        /// Populates the header with the appropriate labels
        /// </summary>
        /// <param name="_mode">The current game mode being played.</param>
        private void LoadHeader(GameMode _mode)
        {
            List<string> serverInfoList = new List<string>();
            serverInfoList.Add(ServiceManager.CurrentServer.Name);
            
            switch (_mode)
            {
                case GameMode.DEATHMATCH:
                    header = new String[] { "Name", "Rank", "Kills", "Deaths", "Assists" };
                    serverInfoList.Add("Deathmatch");
                    break;
                case GameMode.TEAMDEATHMATCH:
                    header = new String[] { "Name", "Rank", "Kills", "Deaths", "Assists" };
                    serverInfoList.Add("Team Deathmatch");
                    break;
                case GameMode.CAPTURETHEFLAG:
                    header = new String[] { "Name", "Rank", "Kills", "Deaths", "Assists", "Captures" };
                    serverInfoList.Add("Capture the Flag");
                    break;
                case GameMode.CAPTURETHEBASE:
                    header = new String[] { "Name", "Rank", "Kills", "Deaths", "Assists", "Captures" };
                    serverInfoList.Add("Capture the Base");
                    break;
                default:
                    header = new String[] { "Name", "Rank", "Kills", "Deaths", "Assists" };
                    serverInfoList.Add("<unknown>");
                    break;
            }

            String mapname = ServiceManager.Theater.GetCurrentMapName();
            serverInfoList.Add(mapname.Substring(0, mapname.IndexOf(".vtmap")));

            serverInfo = serverInfoList.ToArray();
        }
        #endregion

        #region Draw and Draw Helpers
        /// <summary>
        /// Draws this component
        /// </summary>
        public void Draw()
        {
            if (mode == GameMode.CAPTURETHEFLAG || mode == GameMode.CAPTURETHEBASE)
            {
                this.DrawObjectivesCounter();

                if (((GamePlayState)ServiceManager.StateManager.CurrentState).Rotating == true)
                {
                    this.DrawVictoryMessage();
                }

                if (mode == GameMode.CAPTURETHEBASE && this.drawingRoundWinner == true)
                {
                    this.DrawRoundWinner();
                }
            }

            if (Enabled)
            {
                int height = (int)ServiceManager.Game.Font.MeasureString("55").Y;


                ServiceManager.Game.Batch.Begin();
                ServiceManager.Game.Batch.Draw(ServiceManager.Resources.GetTexture2D(
                    "textures\\misc\\Scoreboard\\scoreback"), rect, BackColor);

                int height_modifier = 1;
                Rectangle serverInfoHeaderRect = new Rectangle(
                    rect.X + x_offset, rect.Y + (height * height_modifier), rect.Width, height);
                ++height_modifier;

                Rectangle serverInfoRect = new Rectangle(rect.X + x_offset, rect.Y + (height * height_modifier), rect.Width, rect.Height);
                ++height_modifier;

                const int width_offset = -20;
                Rectangle separatorLineRect = new Rectangle(
                    rect.X + x_offset, rect.Y + (height * height_modifier), rect.Width + width_offset, height);
                ++height_modifier;

                DrawStrings(serverInfoHeader, serverInfoHeaderRect, Color.White, true);
                DrawStrings(serverInfo, serverInfoRect, Color.White, true);
                DrawLine(separatorLineRect, Color.White);
                
                if (mode != GameMode.DEATHMATCH)
                {
                    // There are teams: Draw the 'Team 1' header.
                    Rectangle team1HeaderRect = new Rectangle(
                        rect.X + x_offset, rect.Y + (height * height_modifier), rect.Width, height);
                    DrawStrings(team1Header, team1HeaderRect, Color.Red, false);
                    ++height_modifier;
                }

                Rectangle headerRect = new Rectangle(
                    rect.X + x_offset, rect.Y + (height * height_modifier), rect.Width, height);
                ++height_modifier;

                DrawStrings(header, headerRect, Color.White, false);
                DrawSortedTeam(team1, ref height_modifier, height, rect, team1stats);

                //foreach (String name in team1.Keys)
                //{
                //    DrawStrings(GetValues(name), new Rectangle(rect.X + 10, rect.Y + (h * i++), rect.Width, h), GetPlayerByName(name).Color, false);
                //}

                if (team2 != null)
                {
                    // Draw the second team.
                    ++height_modifier;

                    Rectangle spacerRect = new Rectangle(
                        rect.X + x_offset, rect.Y + (height * height_modifier), rect.Width, height);
                    DrawSpacer(spacerRect, 10);
                    
                    ++height_modifier;

                    Rectangle team2HeaderRect = new Rectangle(
                        rect.X + x_offset, rect.Y + (height * height_modifier), rect.Width, height);
                    ++height_modifier;

                    Rectangle headerTeam2Rect = new Rectangle(rect.X + 10, rect.Y + (height * height_modifier), rect.Width, height);
                    ++height_modifier;

                    DrawStrings(team2Header, team2HeaderRect, customBlue, false);
                    DrawStrings(header, headerTeam2Rect, Color.White, false);
                    
                    DrawSortedTeam(team2, ref height_modifier, height, rect, team2stats);                        
                }

                if (this.localPlayerRankedUp == true && this.rankupMessageOpacity > 0)
                {
                    this.HandleLocalPlayerPromotion();
                }

                ServiceManager.Game.Batch.End();
            }
        }

        /// <summary>
        /// Draw a 'COLOR Team Wins' message at the end of a map.
        /// </summary>
        private void DrawVictoryMessage()
        {
            if (mode == GameMode.CAPTURETHEFLAG)
            {
                if (team1stats.objectives > team2stats.objectives)
                {
                    this.DrawTeamWins(ServiceManager.Resources.GetTexture2D(pathToRedTeamWins));
                }
                else if (team2stats.objectives > team1stats.objectives)
                {
                    this.DrawTeamWins(ServiceManager.Resources.GetTexture2D(pathToBlueTeamWins));
                }
                else
                {
                    //TODO: Tie game image
                }
            }
            else
            {
                if (team1stats.roundWins > team2stats.roundWins)
                {
                    this.DrawTeamWins(ServiceManager.Resources.GetTexture2D(pathToRedTeamWins));
                }
                else if (team2stats.roundWins > team1stats.roundWins)
                {
                    this.DrawTeamWins(ServiceManager.Resources.GetTexture2D(pathToBlueTeamWins));
                }
                else
                {
                    //TODO: Tie game image
                }

            }
        }

        /// <summary>
        /// Draw two small icons at the top with the special objectives won.
        /// </summary>
        private void DrawObjectivesCounter()
        {
            string redTeamImage;
            string blueTeamImage;
            string blueObjectiveCount;
            string redObjectiveCount;

            if (mode == GameMode.CAPTURETHEFLAG)
            {
                redTeamImage = "textures\\misc\\Scoreboard\\red_flag_icon";
                blueTeamImage = "textures\\misc\\Scoreboard\\blue_flag_icon";

                blueObjectiveCount = team1stats.objectives.ToString();
                redObjectiveCount = team2stats.objectives.ToString();
            }
            else if (mode == GameMode.CAPTURETHEBASE)
            {
                redTeamImage = "textures\\misc\\Scoreboard\\red_base_icon";
                blueTeamImage = "textures\\misc\\Scoreboard\\blue_base_icon";
                blueObjectiveCount = team1stats.roundWins.ToString();
                redObjectiveCount = team2stats.roundWins.ToString();
            }
            else
            {
                return;
            }

            ServiceManager.Game.Batch.Begin();

            ServiceManager.Game.Batch.Draw(ServiceManager.Resources.GetTexture2D(redTeamImage), Vector2.UnitX * 380, Color.White);
            ServiceManager.Game.Batch.DrawString(ServiceManager.Game.Font, blueObjectiveCount,
                                                    Vector2.UnitX * 402, Color.White);

            ServiceManager.Game.Batch.Draw(ServiceManager.Resources.GetTexture2D(blueTeamImage), Vector2.UnitX * 450, Color.White);
            ServiceManager.Game.Batch.DrawString(ServiceManager.Game.Font, redObjectiveCount,
                                                    Vector2.UnitX * 472, Color.White);

            ServiceManager.Game.Batch.End();
        }

        /// <summary>
        /// Draws a dictionary based on sorted values
        /// </summary>
        /// <param name="team1"></param>
        /// <param name="i"></param>
        /// <param name="h"></param>
        /// <param name="rect"></param>
        private void DrawSortedTeam(Dictionary<string, Score> team, ref int height_modifier, int height, Rectangle rect, TeamStats stats)
        {
            List<KeyValuePair<string, Score>> myList = new List<KeyValuePair<string, Score>>(team);

            myList.Sort((firstPair, nextPair) =>
            {
                return firstPair.Value.CompareTo(nextPair.Value);
            });

            Rectangle playerRect = new Rectangle(rect.X + x_offset, rect.Y + (height * height_modifier), rect.Width, height);
            foreach (KeyValuePair<string, Score> s in myList)
            {
                if (!team1.ContainsKey(s.Key) && !team2.ContainsKey(s.Key))
                    continue;

                if (playersWhoRankedUp.Count != 0)
                {
                    if (playersWhoRankedUp.Contains(s.Key))
                        DrawStrings(GetValues(s.Key), playerRect, Color.Yellow, false);
                    else
                        DrawStrings(GetValues(s.Key), playerRect, GetPlayerByName(s.Key).Color, false);
                }
                else
                {
                    DrawStrings(GetValues(s.Key), playerRect, GetPlayerByName(s.Key).Color, false);
                }

                ++height_modifier;
                playerRect.Y = rect.Y + (height * height_modifier);
            }

            if (team2 != null)
            {
                string[] totalTeamStats;

                if (mode == GameMode.TEAMDEATHMATCH)
                {
                    totalTeamStats = new string [] {"Total", 
                                                       " ", 
                                                       stats.kills.ToString(), 
                                                       stats.deaths.ToString(),
                                                       stats.assists.ToString()};
                }
                else
                {
                    totalTeamStats = new string [] {"Total", 
                                                   " ", 
                                                   stats.kills.ToString(), 
                                                   stats.deaths.ToString(),
                                                   stats.assists.ToString(),
                                                   stats.objectives.ToString()};
                }

                DrawStrings(totalTeamStats, playerRect, Color.White, false);                
            }
        }

        private void DrawTeamWins(Texture2D texture)
        {
            int scoreboardTopCenterX = rect.X + rect.Width/2;
            int scoreboardTopY = rect.Y;

            int xOffset = texture.Width / 2;
            int yOffset = texture.Height;
            int yPadding = 10; // 10 pixels between scoreboard and team wins message

            Vector2 position = new Vector2( (scoreboardTopCenterX-xOffset), 
                                            (scoreboardTopY - yOffset - yPadding));

            ServiceManager.Game.Batch.Begin();
            ServiceManager.Game.Batch.Draw(texture, position, Color.White);
            ServiceManager.Game.Batch.End();
        }

        /// <summary>
        /// Draws a line
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        private void DrawLine(Rectangle rect, Color color)
        {
            const int fixed_height = 1;
            int old_height = rect.Height;
            rect.Height = fixed_height;
            ServiceManager.Game.Batch.Draw(line, rect, color);
            rect.Height = old_height;
        }

        /// <summary>
        /// Draws a blank line for spacing.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        private void DrawSpacer(Rectangle rect, int height)
        {
            int old_height = rect.Height;
            rect.Height = height;
            ServiceManager.Game.Batch.Draw(line, rect, Color.TransparentWhite);
            rect.Height = old_height;
        }

        /// <summary>
        /// Parses a given player's statistics and returns the 
        /// relevant values based on the game mode.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private String[] GetValues(String name)
        {
            string rankAbbreviation = "";

            if (!ranks.ContainsKey(name))
                RefreshRanks();

            if (ranks.ContainsKey(name))
            {
                int rank = ranks[name];

                rankAbbreviation = RankLoader.GetRank(rank).Abbreviation;
            }

            
            if (mode == GameMode.TEAMDEATHMATCH)
            {
                if (team1.ContainsKey(name))
                {
                    Score player = team1[name];
                    return new String[] { name, rankAbbreviation,
                        player.Statistics.kills.ToString(), 
                        player.Statistics.deaths.ToString(), 
                        player.Statistics.assists.ToString()
                    };
                }
                else if (team2.ContainsKey(name))
                {
                    Score player = team2[name];
                    return new String[] { name, rankAbbreviation,
                        player.Statistics.kills.ToString(), 
                        player.Statistics.deaths.ToString(), 
                        player.Statistics.assists.ToString()
                    };
                }
            }
            else if (mode == GameMode.CAPTURETHEFLAG)
            {
                if (team1.ContainsKey(name))
                {
                    Score player = team1[name];
                    return new String[] { name, rankAbbreviation,
                        player.Statistics.kills.ToString(), 
                        player.Statistics.deaths.ToString(), 
                        player.Statistics.assists.ToString(),
                        player.Statistics.objectivesCaptured.ToString()
                    };
                }
                else if (team2.ContainsKey(name))
                {
                    Score player = team2[name];
                    return new String[] { name, rankAbbreviation,
                        player.Statistics.kills.ToString(), 
                        player.Statistics.deaths.ToString(), 
                        player.Statistics.assists.ToString(),
                        player.Statistics.objectivesCaptured.ToString()
                    };
                }
            }
            else if (mode == GameMode.CAPTURETHEBASE)
            {
                if (team1.ContainsKey(name))
                {
                    Score player = team1[name];
                    return new String[] { name, rankAbbreviation,
                        player.Statistics.kills.ToString(), 
                        player.Statistics.deaths.ToString(), 
                        player.Statistics.assists.ToString(),
                        player.Statistics.objectivesCaptured.ToString()
                    };
                }
                else if (team2.ContainsKey(name))
                {
                    Score player = team2[name];
                    return new String[] { name, rankAbbreviation,
                        player.Statistics.kills.ToString(), 
                        player.Statistics.deaths.ToString(), 
                        player.Statistics.assists.ToString(),
                        player.Statistics.objectivesCaptured.ToString()
                    };
                }
            }
            else //DeathMatch or default values
            {
                if (team1.ContainsKey(name))
                {
                    Score player = team1[name];
                    return new String[] { name, rankAbbreviation,
                        player.Statistics.kills.ToString(), 
                        player.Statistics.deaths.ToString(), 
                        player.Statistics.assists.ToString()
                    };
                }
            }

            return new String[] { name, "<error>", };
        }

        /// <summary>
        /// Draws an array of Strings in a formatted way. 
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        /// <param name="isInfo"></param>
        public void DrawStrings(String[] strings, Rectangle rect, Color color, bool isInfo)
        {
            if (isInfo)
            {
                float width;

                width = ServiceManager.Game.Font.MeasureString(strings[0]).X;
                ServiceManager.Game.Batch.DrawString(ServiceManager.Game.Font, strings[0], new Vector2(rect.X, rect.Y), color);

                width = ServiceManager.Game.Font.MeasureString(strings[1]).X;
                ServiceManager.Game.Batch.DrawString(ServiceManager.Game.Font, strings[1], new Vector2(rect.X + (rect.Width/2) - (width/2), rect.Y), color);

                width = ServiceManager.Game.Font.MeasureString(strings[2]).X;
                ServiceManager.Game.Batch.DrawString(ServiceManager.Game.Font, strings[2], new Vector2(rect.X + rect.Width - width - 20, rect.Y), color);
            }
            else
            {
                float spacing = (float)rect.Width / (float)strings.Length;

                ServiceManager.Game.Batch.DrawString(ServiceManager.Game.Font, strings[0], new Vector2(rect.X, rect.Y), color);
                for (int i = 1; i < strings.Length; i++)
                {
                    ServiceManager.Game.Batch.DrawString(ServiceManager.Game.Font, strings[i], new Vector2(rect.X + (spacing * i) + spacing / 4f, rect.Y), color);
                }
            }
        }

        /// <summary>
        /// Draw the promotion message.
        /// </summary>
        /// <param name="opacity">The opacity to use when drawing it.</param>
        public void DrawPromotionMessage(float opacity)
        {
            Texture2D promotedImage = ServiceManager.Resources.GetTexture2D(pathToPromotedImage);
            int xOffset = promotedImage.Width / 2;
            int yOffset = promotedImage.Height / 2;

            int centerXCoordinate = ServiceManager.Game.GraphicsDevice.Viewport.Width / 2;
            int centerYCoordinate = ServiceManager.Game.GraphicsDevice.Viewport.Height / 2;

            Vector2 imageCoordinates = new Vector2(centerXCoordinate - xOffset, centerYCoordinate - yOffset);
            
            Color imageColor = Color.White;
            imageColor.A = (byte)(opacity*255);

            ServiceManager.Game.Batch.Draw(promotedImage, imageCoordinates, imageColor);
         }

        /// <summary>
        /// Draw the winner of a given round.
        /// </summary>
        public void DrawRoundWinner()
        {
            if (roundWinnerOpacity <= .2)
            {
                this.drawingRoundWinner = false;
                this.roundWinnerOpacity = 2.0f;
                return;
            }

            Texture2D roundWinnerImage = ServiceManager.Resources.GetTexture2D("textures\\misc\\Scoreboard\\"
                + roundWinningTeam + "roundwinner");
            int xOffset = roundWinnerImage.Width / 2;
            int yOffset = roundWinnerImage.Height / 2;

            int centerXCoordinate = ServiceManager.Game.GraphicsDevice.Viewport.Width / 2;
            int centerYCoordinate = ServiceManager.Game.GraphicsDevice.Viewport.Height / 2;

            Vector2 imageCoordinates = new Vector2(centerXCoordinate - xOffset, centerYCoordinate - yOffset);

            Color imageColor = Color.White;
            this.roundWinnerOpacity -= (float)ServiceManager.Game.DeltaTime;
            imageColor.A = (byte)((roundWinnerOpacity / 2.0f) * 255);
            ServiceManager.Game.Batch.Begin();
            ServiceManager.Game.Batch.Draw(roundWinnerImage, imageCoordinates, imageColor);
            ServiceManager.Game.Batch.End();
        }

        #endregion       

        #region Helpers
        /// <summary>
        /// A no parameter version of loadranks, for usage on the current ranks (not initial).
        /// </summary>
        private void RefreshRanks()
        {
            this.ranks = this.LoadRanks(this.ranks);
        }

        /// <summary>
        /// Get the ranks of the current game's players from Echelon and stores them in a
        /// dictionary of tank names and ranks.
        /// </summary>
        /// <param name="_ranks">The dictionary to be updated.</param>
        /// <returns>The dictionary with updated tank/rank pairs.</returns>
        private Dictionary<string, int> LoadRanks(Dictionary<string, int> _ranks)
        {
            _ranks = new Dictionary<string, int>();
            List<string> tankNames = new List<string>();
            foreach (PlayerTank tank in players.Values)
            {
                tankNames.Add(tank.Name);
            }

            string[] tankNameArray = tankNames.ToArray();

            int[] tankRanks = ServiceManager.Echelon.GetRanksOfTanks(tankNameArray);

            for (int count = 0; count < tankRanks.Length; count++)
            {
                _ranks.Add(tankNameArray[count], tankRanks[count]);
            }

            return _ranks;
        }

        /// <summary>
        /// Update the collection of players who've ranked up, for display on the
        /// scoreboard.
        /// </summary>
        public void CheckForRankUps()
        {
            this.RefreshRanks();

            foreach (string tankName in ranks.Keys)
            {
                if (!initialRanks.ContainsKey(tankName))
                    continue;

                if (ranks[tankName] != initialRanks[tankName])
                {
                    playersWhoRankedUp.Add(tankName);
                    if (tankName == players.GetLocalPlayer().Name)
                        this.localPlayerRankedUp = true;

                }
            }
        }

        /// <summary>
        /// Handles the local player's promotion message.
        /// </summary>
        private void HandleLocalPlayerPromotion()
        {
            if (this.rankupMessageOpacity > 0)
            {
                this.rankupMessageOpacity -= (float)ServiceManager.Game.DeltaTime / 4;
                this.DrawPromotionMessage(rankupMessageOpacity);
            }
        }

        /// <summary>
        /// Refreshes the scores with a new player list.
        /// </summary>
        public void Refresh()
        {
            bool refreshRanks = false;
            if (ServiceManager.Theater == null)
                return;

            foreach (PlayerTank temp in players.Values)
            {
                if (PlayerExists(temp.Name))
                {
                    continue;
                }

                refreshRanks = true;
                if (temp.Team == GameSession.Alliance.RED)
                {
                    team1.Add(temp.Name, new Score(Color.Red, new Statistics(0, 0, 0, 0, 0, 0, temp.Name)));
                }
                else if (temp.Team == GameSession.Alliance.BLUE)
                {
                    team2.Add(temp.Name, new Score(customBlue, new Statistics(0, 0, 0, 0, 0, 0, temp.Name)));
                }
                else
                {
                    team1.Add(temp.Name, new Score(neutralTeamColor, new Statistics(0, 0, 0, 0, 0, 0, temp.Name)));
                }
            }

            if (refreshRanks == true)
            {
                RefreshRanks();

                //Add an initial rank for any new players.
                foreach (string tankname in ranks.Keys)
                {
                    if (!initialRanks.ContainsKey(tankname))
                    {
                        initialRanks.Add(tankname, ranks[tankname]);
                    }
                }
            }
        }

        /// <summary>
        /// Remove a player from the scoreboard.
        /// </summary>
        /// <param name="name"></param>
        public void RemovePlayer(string name)
        {
            if (team1.ContainsKey(name))
            {
                team1.Remove(name);
            }
            else if (team2 != null && team2.ContainsKey(name))
            {
                team2.Remove(name);
            }
        }

        /// <summary>
        /// Add a player to the scoreboard who has no alliance.
        /// </summary>
        /// <param name="name">Name of the player.</param>
        public void AddPlayer(string name)
        {
            AddPlayer(name, GameSession.Alliance.NONE);
        }

        /// <summary>
        /// Add a player to the scoreboard.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="team"></param>
        public void AddPlayer(string name, GameSession.Alliance team)
        {
            if (team == GameSession.Alliance.BLUE)
            {
                team2[name] = new Score(customBlue, new Statistics(0, 0, 0, 0, 0, 0, name));
            }
            else if (team == GameSession.Alliance.RED)
            {
                team1[name] = new Score(Color.Red, new Statistics(0, 0, 0, 0, 0, 0, name));
            }
            else
            {
                team1[name] = new Score(neutralTeamColor, new Statistics(0, 0, 0, 0, 0, 0, name));
            }
        }

        /// <summary>
        /// Initialize the scoreboard.
        /// </summary>
        public void InitScoreBoard()
        {
            if (ServiceManager.Theater == null)
                return;

           Statistics[] stats = ServiceManager.Theater.GetScoreboard();
           GameSession.ScoreboardTotals totals = new GameSession.ScoreboardTotals();
           
           if (mode == GameMode.CAPTURETHEFLAG)
              totals = ServiceManager.Theater.GetTeamStats();

            if (team1 != null) team1.Clear();
            if (team2 != null) team2.Clear();

            foreach (Statistics stat in stats)
            {
                PlayerTank tank = players.GetPlayerByName(stat.tankName);                    

                if (tank != null)
                {
                    if (tank.Team == GameSession.Alliance.RED && !team1.ContainsKey(stat.tankName))
                    {
                        team1.Add(stat.tankName, new Score(Color.Red, stat));
                        team1stats.kills += stat.kills;
                        team1stats.assists += stat.assists;
                        team1stats.deaths += stat.deaths;
                        team1stats.objectives += stat.objectivesCaptured;
                        team1stats.roundWins += stat.objectivesCompleted;
                    }
                    else if (tank.Team == GameSession.Alliance.BLUE && !team2.ContainsKey(stat.tankName))
                    {
                        team2.Add(stat.tankName, new Score(customBlue, stat));
                        team2stats.kills += stat.kills;
                        team2stats.assists += stat.assists;
                        team2stats.deaths += stat.deaths;
                        team2stats.objectives += stat.objectivesCaptured;
                        team2stats.roundWins += stat.objectivesCompleted;
                    }
                    else if (tank.Team == GameSession.Alliance.NONE && !team1.ContainsKey(stat.tankName))
                    {
                        team1.Add(stat.tankName, new Score(neutralTeamColor, stat));
                    }
                }
            }

            if (totals != null)
            {
                team1stats.kills = totals.killsBlue;
                team1stats.objectives = totals.capturesBlue;
                team2stats.kills = totals.killsRed;
                team2stats.objectives = totals.capturesRed;
            }
        }
        #endregion

        #region Adding Methods

        /// <summary>
        /// Returns a Score struct representing player. 
        /// Will return a blank score if player is not found.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Score GetPlayerByName(String name)
        {
            if (team1.ContainsKey(name))
            {
                return team1[name];
            }

            if (team2 != null)
            {
                if (team2.ContainsKey(name))
                {
                    return team2[name];
                }
            }

            Score _score = new Score();
            _score.Statistics = null;

            return _score;
        }

        /// <summary>
        /// Check if a player exists with the scoreboard.
        /// </summary>
        /// <param name="name">Name of the player.</param>
        /// <returns>True if the player exists; false otherwise.</returns>
        public bool PlayerExists(String name)
        {
            if (team1.ContainsKey(name))
            {
                return true;
            }

            if (team2 != null)
            {
                return team2.ContainsKey(name);
            }

            return false;
        }

        /// <summary>
        /// Credit a player with a kill
        /// </summary>
        /// <param name="playerName"></param>
        public void AddKill(String playerName)
        {
            if ( GetPlayerByName(playerName).Statistics != null)
                GetPlayerByName(playerName).Statistics.kills++;

            if (team2 != null)
            {
                if (team1.ContainsKey(playerName))
                    team1stats.kills++;
                else if (team2.ContainsKey(playerName))
                    team2stats.kills++;
            }
        }

        /// <summary>
        /// Credit a player with a death
        /// </summary>
        /// <param name="playerName"></param>
        public void AddDeath(String playerName)
        {
            if (GetPlayerByName(playerName).Statistics != null)
                GetPlayerByName(playerName).Statistics.deaths++;

            if (team2 != null)
            {
                if (team1.ContainsKey(playerName))
                    team1stats.deaths++;
                else if (team2.ContainsKey(playerName))
                    team2stats.deaths++;
            }
        }

        /// <summary>
        /// Credit a player with an assist
        /// </summary>
        /// <param name="playerName"></param>
        public void AddAssist(String playerName)
        {
            if ( GetPlayerByName(playerName).Statistics != null)
                GetPlayerByName(playerName).Statistics.assists++;

            if (team2 != null)
            {
                if (team1.ContainsKey(playerName))
                    team1stats.assists++;
                else if (team2.ContainsKey(playerName))
                    team2stats.assists++;
            }
        }

        /// <summary>
        /// Credit a player with a certain amount of captured objectives
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="amount"></param>
        public void AddObjCapture(String playerName, int amount)
        {
            if ( GetPlayerByName(playerName).Statistics != null)
                GetPlayerByName(playerName).Statistics.objectivesCaptured += amount;

            if (team2 != null)
            {
                if (team1.ContainsKey(playerName))
                    team1stats.objectives++;
                else if (team2.ContainsKey(playerName))
                    team2stats.objectives++;
            }
        }

        /// <summary>
        /// Credit a player with a certain amount of completed objectives
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="amount"></param>
        private void AddObjComplete(String playerName, int amount)
        {
            if ( GetPlayerByName(playerName).Statistics != null)
                GetPlayerByName(playerName).Statistics.objectivesCompleted += amount;

        }

        /// <summary>
        /// Add a round victory to a given team's score.
        /// </summary>
        /// <param name="team">The team to award a round win to.</param>
        public void AddRoundWin(GameSession.Alliance team)
        {
            if (team == GameSession.Alliance.NONE)
                return;

            //Red is 1, Blue is 2
            if (team == GameSession.Alliance.RED)
            {
                team1stats.roundWins++;
            }
            else if (team == GameSession.Alliance.BLUE)
            {
                team2stats.roundWins++;
            }

            drawingRoundWinner = true;
            roundWinningTeam = team.ToString().ToLower();
        }
        #endregion
    }
}
