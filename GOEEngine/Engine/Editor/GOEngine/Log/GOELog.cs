
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace GOEngine.Implement
{
    public class GOELog : IGOELog
    {
        private string mName;
        public static GOELog mAllLog = null;
        public static Action<string> OnLogMsg;
        
        public GOELog()
        {

        }
        public GOELog(string name)
        {
            mName = name;
        }
        public string Name
        {
            set { mName = value; }
        }
        
        private void _LogMsg( string msg )
        {
#if UNITY_EDITOR

#else
            
            msg = TimeHead + "\n" + msg;
#endif
            mAllLog._LogAllMsg( msg );
        }
        
        private void _LogAllMsg( string msg )
        {
            if ( this == mAllLog )
            {		
                if ( Application.platform == RuntimePlatform.WindowsEditor )
                {
                    UnityEngine.Debug.Log( msg);
                    Logger.LogMsg( msg );
                }
                if (OnLogMsg != null)
                    OnLogMsg(msg);
            }
        }
        
        public void LogError( string msg )
        {
            this._LogMsg(  "error:\n" + mName + ": " + msg);
        }
        
        public void LogWarning( string msg )
        {
            this._LogMsg( "warning:\n" + mName + ": " + msg );		
        }

        
        public void LogMsg( string msg )
        {
            msg  = "normal:\n" + mName + ": " + msg;
            
            this._LogMsg( msg );
        }
        
        public void LogException( System.Exception ex )
        {
            string msg = "exception:\n" + mName + ": "
                + "type: " + ex.GetType () + "\n"
                + "msg: " + ex.Message + "\n"
                + "stack:\n" + ex.StackTrace;

#if UNITY_EDITOR				
            if (ex.InnerException != null)
            {
                msg += "\ninner exception:\n" + "type: " + ex.InnerException.GetType ()
                + "\n" + "msg: " + ex.InnerException.Message + "\n"
                + "stack:\n" + ex.InnerException.StackTrace;
            }
#endif
            this._LogMsg(msg );
        }

        private string TimeString
        {
            get
            {
                DateTime dt = DateTime.Now;
                
                string msg = " (" + dt.Month + "/" + dt.Day + "-" + dt.Hour + ":" + dt.Minute + ":" + dt.Second
#if UNITY_EDITOR				
                    + "-" + dt.Millisecond
#endif
                    + ")\n";
                
                return msg;
            }
        }
        
        private string TimeHead
        {
            get
            {
                DateTime dt = DateTime.Now;
                string msg = dt.Year + "-" + dt.Month + "-" + dt.Day + " " + dt.Hour + ":" + dt.Minute + ":" + dt.Second;
                return msg;
            }
        }
    }
}

