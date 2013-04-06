using System;
using System.Collections.Generic;
using System.Text;

namespace Network.Util
{
    /// <summary>
    /// GameServerInfo wraps information about a game server and abstracts the process of
    /// creating temporary proxies for the purpose of pinging.
    /// </summary>
    public class GameServerInfo
    {
        #region Members
        private Pinger pinger;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of this game server. The name has nothing to do with the host name,
        /// it's just a name assigned to that game server.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the host name of the game server.
        /// </summary>
        public string Host
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the port number that the game server listens for clients on.
        /// </summary>
        public int Port
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether or not this game server uses a Glacier2 router.
        /// </summary>
        public bool UseGlacier2
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the maximum number of players allowed on this game server.
        /// </summary>
        public int PlayerLimit
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current number of players on the game server.
        /// </summary>
        public int NumberOfPlayers
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether or not this server is an approved one.
        /// </summary>
        public bool Approved
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets which map this server is playing on.
        /// </summary>
        public string CurrentMap
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current mode being played.
        /// </summary>
        public VTankObject.GameMode CurrentGameMode
        {
            get;
            private set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Construct a GameServerInfo object from a VTankObject.ServerInfo object. It uses a
        /// communicator to create a proxy pointing to the game server.
        /// </summary>
        /// <param name="communicator">Pre-initialized communicator.</param>
        /// <param name="server">Server information.</param>
        public GameServerInfo(Ice.Communicator communicator, VTankObject.ServerInfo server)
        {
            Name = server.name;
            Host = server.host;
            Port = server.port;
            UseGlacier2 = server.usingGlacier2;
            PlayerLimit = server.playerLimit;
            NumberOfPlayers = server.players;
            Approved = server.approved;
            CurrentMap = server.currentMap;
            CurrentGameMode = server.gameMode;

            Ice.ObjectPrx proxy = null;
            if (server.usingGlacier2)
            {
                proxy = communicator.stringToProxy(String.Format(
                    "Theatre/router:tcp -h {0} -p {1} -t 5000", server.host, server.port));
            }
            else
            {
                proxy = communicator.stringToProxy(String.Format(
                    "GameSessionFactory:tcp -h {0} -p {1} -t 5000", server.host, server.port));
            }

            // Ping once to make sure that the proxy is a valid one.
            // If it isn't, it throws Ice.ConnectionRefusedException, which is handled elsewhere.
            proxy.ice_ping();
            pinger = new Pinger(proxy);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get the average latency from the client machine to the game server.
        /// </summary>
        /// <returns>Latency in milliseconds.</returns>
        public double GetAverageLatency()
        {
            return pinger.AverageLatency;
        }

        /// <summary>
        /// Get the average latency from the client machine to the game server formatted in a
        /// way that's easy to understand for users.
        /// </summary>
        /// <returns>Latency formatted like: "0 ms"</returns>
        public string GetFormattedAverageLatency()
        {
            return pinger.GetFormattedAverageLatency();
        }
        #endregion
    }
}
