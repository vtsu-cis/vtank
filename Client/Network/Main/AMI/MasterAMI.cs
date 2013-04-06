using Main;
using System;
using Network.Util;

namespace Network.Main.AMI
{
    #region Login_Callback
    /// <summary>
    /// Implements the AMI_SessionFactory_VTankLogin callback methods.
    /// </summary>
    public class Login_Callback : AMI_SessionFactory_VTankLogin
    {
        private ActionFinished<MainSessionPrx> callback;

        public Login_Callback(ActionFinished<MainSessionPrx> _callback)
        {
            callback = _callback;
        }

        public override void ice_response(MainSessionPrx ret__)
        {
            if (callback != null)
            {
                Result<MainSessionPrx> result = new Result<MainSessionPrx>(ret__);
                callback(result);
            }
        }

        public override void ice_exception(Ice.Exception ex)
        {
            if (callback != null)
            {
                Result<MainSessionPrx> result = new Result<MainSessionPrx>(ex);
                callback(result);
            }
        }

        
    }
    #endregion

    #region Disconnect_Callback
    /// <summary>
    /// Implements the AMI_MainSession_Disconnect callback methods.
    /// </summary>
    public class Disconnect_Callback : AMI_MainSession_Disconnect
    {
        public Disconnect_Callback()
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

    #region RouterDestroySession_Callback
    /// <summary>
    /// Implements the AMI_MainSession_Disconnect callback methods.
    /// </summary>
    public class RouterDestroySession_Callback : Glacier2.AMI_Router_destroySession
    {
        public RouterDestroySession_Callback()
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

    #region KeepAlive_Callback
    /// <summary>
    /// Implements the AMI_MainSession_KeepAlive callback methods.
    /// </summary>
    public class KeepAlive_Callback : AMI_MainSession_KeepAlive
    {
        private ActionFinished<NoReturnValue> callback;

        public KeepAlive_Callback(ActionFinished<NoReturnValue> _callback)
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

    #region GetTankList_Callback
    /// <summary>
    /// Implements the AMI_MainSession_KeepAlive callback methods.
    /// </summary>
    public class GetTankList_Callback : AMI_MainSession_GetTankList
    {
        private ActionFinished<VTankObject.TankAttributes[]> callback;

        public GetTankList_Callback(ActionFinished<VTankObject.TankAttributes[]> _callback)
        {
            callback = _callback;
        }

        public override void ice_response(VTankObject.TankAttributes[] ret__)
        {
            if (callback != null)
            {
                callback(new Result<VTankObject.TankAttributes[]>(ret__));
            }
        }
        public override void ice_exception(Ice.Exception ex)
        {
            if (callback != null)
            {
                callback(new Result<VTankObject.TankAttributes[]>(ex));
            }
        }
    }
    #endregion

    #region CreateTank_Callback
    /// <summary>
    /// Implements the AMI_MainSession_KeepAlive callback methods.
    /// </summary>
    public class CreateTank_Callback : AMI_MainSession_CreateTank
    {
        private ActionFinished<bool> callback;

        public CreateTank_Callback(ActionFinished<bool> _callback)
        {
            callback = _callback;
        }

        public override void ice_response(bool ret__)
        {
            if (callback != null)
            {
                callback(new Result<bool>(ret__));
            }
        }
        public override void ice_exception(Ice.Exception ex)
        {
            if (callback != null)
            {
                callback(new Result<bool>(ex));
            }
        }
    }
    #endregion

    #region UpdateTank_Callback
    /// <summary>
    /// Implements the AMI_MainSession_KeepAlive callback methods.
    /// </summary>
    public class UpdateTank_Callback : AMI_MainSession_UpdateTank
    {
        private ActionFinished<bool> callback;

        public UpdateTank_Callback(ActionFinished<bool> _callback)
        {
            callback = _callback;
        }

        public override void ice_response(bool ret__)
        {
            if (callback != null)
            {
                callback(new Result<bool>(ret__));
            }
        }
        public override void ice_exception(Ice.Exception ex)
        {
            if (callback != null)
            {
                callback(new Result<bool>(ex));
            }
        }
    }
    #endregion

    #region DeleteTank_Callback
    /// <summary>
    /// Implements the AMI_MainSession_KeepAlive callback methods.
    /// </summary>
    public class DeleteTank_Callback : AMI_MainSession_DeleteTank
    {
        private ActionFinished<bool> callback;

        public DeleteTank_Callback(ActionFinished<bool> _callback)
        {
            callback = _callback;
        }

        public override void ice_response(bool ret__)
        {
            if (callback != null)
            {
                callback(new Result<bool>(ret__));
            }
        }
        public override void ice_exception(Ice.Exception ex)
        {
            if (callback != null)
            {
                callback(new Result<bool>(ex));
            }
        }
    }
    #endregion

    #region GetRank_Callback
    /// <summary>
    /// Implements the AMI_MainSession_GetRank callback methods.
    /// </summary>
    public class GetRank_Callback : AMI_MainSession_GetRank
    {
        private ActionFinished<int> callback;

        public GetRank_Callback(ActionFinished<int> _callback)
        {
            callback = _callback;
        }

        public override void ice_response(int ret__)
        {
            if (callback != null)
            {
                callback(new Result<int>(ret__));
            }
        }
        public override void ice_exception(Ice.Exception ex)
        {
            if (callback != null)
            {
                callback(new Result<int>(ex));
            }
        }
    }
    #endregion

    #region DownloadMap_Callback
    /// <summary>
    /// Implements the AMI_MainSession_DownloadMap callback methods.
    /// </summary>
    public class DownloadMap_Callback : AMI_MainSession_DownloadMap
    {
        private ActionFinished<VTankObject.Map> callback;
        private bool done;

        #region DownloadMap_Callback Properties
        /// <summary>
        /// Check to see if the download has finished.
        /// </summary>
        public bool Finished
        {
            get
            {
                lock (this)
                {
                    return done;
                }
            }

            private set
            {
                lock (this)
                {
                    done = value;
                }
            }
        }

        /// <summary>
        /// Gets the downloaded map, if one exists.
        /// </summary>
        public VTankObject.Map Map
        {
            get;
            private set;
        }
        #endregion

        public DownloadMap_Callback(ActionFinished<VTankObject.Map> _callback)
        {
            callback = _callback;
        }

        public override void ice_response(VTankObject.Map ret__)
        {
            Map = ret__;

            if (callback != null)
            {
                callback(new Result<VTankObject.Map>(ret__));
            }
        }

        public override void ice_exception(Ice.Exception ex)
        {
            if (callback != null)
            {
                callback(new Result<VTankObject.Map>(ex));
            }
        }
    }
    #endregion
}
