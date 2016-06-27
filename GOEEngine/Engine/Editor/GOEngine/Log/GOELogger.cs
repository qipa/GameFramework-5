using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
        enum LogFile
    {
        Gui,
        Res,
        Global
    }
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class Logger
	{				
		private static FileStream outStream = null;
		private static StreamWriter writer = null;

		private static bool mbLogToFile = false;

        private static Dictionary<string, GOELog> _logDic = new Dictionary<string, GOELog>();
        static public GOELog GetFile(object keyenum)
        {
            string key = keyenum.GetType().Name;
            return GetFile(key);
        }

        static public GOELog GetFile(string key)
        {
            GOELog log;
            if (_logDic.TryGetValue(key, out log))
                return log;
            log = new GOELog(key);
            _logDic.Add(key, log);
            return log;
        }

		public static void Start (bool bLogToFile)
		{ 
			mbLogToFile = bLogToFile;
			if (mbLogToFile)
			{
				CreateOutLog ();		
			}
			
			GOELog.mAllLog = new GOELog();
			
			GOELog.mAllLog.Name = "All";
        
		}

		public static void Shutdown ()
		{ 
			if ( !mbLogToFile )
			{
				return;
			}
            				
			if (writer != null)
				writer.Write ("end log ---------------------------\n");

			if (writer != null)
			{
				writer.Close ();
				writer = null;
			}
			if (outStream != null)
			{
				outStream.Close ();
				outStream = null;
			}
		}
        
		private static void CreateOutLog ()
		{
            if ( Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer )
            {
                System.DateTime dt = System.DateTime.Now;
                string name = "Fire-" + dt.Year + "-" + dt.Month + "-" + dt.Day
                        + "-" + dt.Hour + "-" + dt.Minute + "-" + dt.Second + ".log";
                outStream = new FileStream( name, FileMode.Create );

                writer = new StreamWriter( outStream );
            }
		}

        static public void LogMsg( string text )
        {
            if ( writer != null )
            {
                writer.Write( text );
                writer.Flush();
            }
        }
	}
}


