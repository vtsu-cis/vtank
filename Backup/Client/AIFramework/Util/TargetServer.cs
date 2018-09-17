using System;
using System.Collections.Generic;
using System.Text;

namespace AIFramework.Util
{
    public struct TargetServer
    {
        #region Properties
        /// <summary>
        /// Gets or sets the host name of the target server.
        /// </summary>
        public string Host
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the port number of the target server.
        /// </summary>
        public ushort Port
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public TargetServer(string _host, ushort _port)
            : this()
        {
            Host = _host;
            Port = _port;
        }
        #endregion
    }
}
