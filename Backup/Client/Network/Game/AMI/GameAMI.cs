using IGame;
using GameSession;
using System;
using Network.Util;

namespace Network.Game.AMI
{
    #region GameRouterDestroySession_Callback
    /// <summary>
    /// Implements the Glacier2.AMI_Router_destroySession callback methods.
    /// </summary>
    internal class GameRouterDestroySession_Callback : Glacier2.AMI_Router_destroySession
    {
        public GameRouterDestroySession_Callback()
        {
        }

        public override void ice_response()
        {
        }

        public override void ice_exception(Ice.Exception ex)
        {
        }
    }
    #endregion

    #region Destroy_Callback
    /// <summary>
    /// Implements the Glacier2.AMI_Session_destroy callback methods.
    /// </summary>
    internal class Destroy_Callback : Glacier2.AMI_Session_destroy
    {
        public Destroy_Callback()
        {
        }

        public override void ice_response()
        {
        }

        public override void ice_exception(Ice.Exception ex)
        {
        }
    }
    #endregion

    #region GameKeepAlive_Callback
    /// <summary>
    /// Implements the AMI_MainSession_KeepAlive callback methods.
    /// </summary>
    internal class GameKeepAlive_Callback : AMI_GameInfo_KeepAlive
    {
        private ActionFinished<NoReturnValue> callback;

        public GameKeepAlive_Callback(ActionFinished<NoReturnValue> _callback)
        {
            callback = _callback;
        }

        public override void ice_response() 
        {
            if (callback != null)
            {
                callback(new Result<NoReturnValue>());
            }
        }
        public override void ice_exception(Ice.Exception ex)
        {
            if (callback != null)
            {
                Result<NoReturnValue> result = new Result<NoReturnValue>(ex);
                callback(result);
            }
        }
    }
    #endregion

    #region Move_Callback
    /// <summary>
    /// Implements the AMI_GameInfo_Move callback methods.
    /// </summary>
    internal class Move_Callback : AMI_GameInfo_Move
    {
        private ActionFinished<NoReturnValue> callback;

        public Move_Callback(ActionFinished<NoReturnValue> _callback)
        {
            callback = _callback;
        }

        public override void ice_response() 
        {
            if (callback != null)
            {
                callback(new Result<NoReturnValue>());
            }
        }
        public override void ice_exception(Ice.Exception ex)
        {
            if (callback != null)
            {
                callback(new Result<NoReturnValue>(ex));
            }
        }
    }
    #endregion

    #region Rotate_Callback
    /// <summary>
    /// Implements the AMI_GameInfo_Rotate callback methods.
    /// </summary>
    internal class Rotate_Callback : AMI_GameInfo_Rotate
    {
        private ActionFinished<NoReturnValue> callback;

        public Rotate_Callback(ActionFinished<NoReturnValue> _callback)
        {
            callback = _callback;
        }

        public override void ice_response() 
        {
            if (callback != null)
            {
                callback(new Result<NoReturnValue>());
            }
        }
        public override void ice_exception(Ice.Exception ex)
        {
            if (callback != null)
            {
                callback(new Result<NoReturnValue>(ex));
            }
        }
    }
    #endregion

    #region Fire_Callback
    /// <summary>
    /// Implements the AMI_GameInfo_Rotate callback methods.
    /// </summary>
    internal class Fire_Callback : AMI_GameInfo_Fire
    {
        private ActionFinished<NoReturnValue> callback;

        public Fire_Callback(ActionFinished<NoReturnValue> _callback)
        {
            callback = _callback;
        }

        public override void ice_response() 
        {
            if (callback != null)
            {
                callback(new Result<NoReturnValue>());
            }
        }
        public override void ice_exception(Ice.Exception ex)
        {
            if (callback != null)
            {
                callback(new Result<NoReturnValue>(ex));
            }
        }
    }
    #endregion

    #region Chat_Callback
    /// <summary>
    /// Implements the AMI_GameInfo_SendMessage callback methods.
    /// </summary>
    internal class Chat_Callback : AMI_GameInfo_SendMessage
    {
        private ActionFinished<NoReturnValue> callback;

        public Chat_Callback(ActionFinished<NoReturnValue> _callback)
        {
            callback = _callback;
        }

        public override void ice_response()
        {
            if (callback != null)
            {
                callback(new Result<NoReturnValue>());
            }
        }
        public override void ice_exception(Ice.Exception ex)
        {
            if (callback != null)
            {
                callback(new Result<NoReturnValue>(ex));
            }
        }
    }
    #endregion
}
