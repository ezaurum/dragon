#if UNITY_EDITOR || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_STANDALONE_WIN
using System;
using UnityEngine;

namespace Dragon 
{
	public static class LoggerManager
	{
		private static UnityLogger _unityLogger = new UnityLogger();
		public static ILogger GetLoggerManager(Type type)
		{
			return _unityLogger;
		}
	}

	public class UnityLogger : ILogger
	{
		public void Debug(string message,  params object[] args)
		{
			if ( Debug.isDebugBuild)
			{
				Debug.Log(message, args);
			}
		}
		
		public void Error(string message,  params object[] args)
		{
			if ( Debug.isDebugBuild)
			{
				Debug.LogError(message, args);
			}
		}
		
		public void Info(string message,  params object[] args)
		{
			if ( Debug.isDebugBuild)
			{
				Debug.Log(message, args);
			}
		}
		
		public void Fatal(string message,  params object[] args)
		{
			if ( Debug.isDebugBuild)
			{
				Debug.LogException(message, args);
			}
		}
		
		public void Warn(string message,  params object[] args)
		{
			if ( Debug.isDebugBuild)
			{
				Debug.LogWarning(message, args);
			}
		}

        public void Debug(string message)
        {
            Debug.Log(message);
        }

        public void Error(string message)
        {
            Debug.LogError(message);
        }

        public void Info(string message)
        {
             Debug.Log(message);
        }

        public void Fatal(string message)
        {
             Debug.LogException(message);
        }

        public void Warn(string message)
        {
            Debug.LogWarning(message);
        }

	}
}

#else
using System;
using System.IO;
using log4net;
using log4net.Config;

namespace Dragon
{
    public static class LoggerManager
    {
        private static bool _initialized;
        public static ILogger GetLogger(Type type)
        {
            if (!_initialized)
            {
                BasicConfigurator.Configure();
                _initialized = true;
            }
            return new CommonLogger(LogManager.GetLogger(type));
        }

        public static void XmlConfigure(FileInfo configurationXmlFile)
        {
            XmlConfigurator.Configure(configurationXmlFile);
            _initialized = true;
        }
        
    }
   
    public class CommonLogger : ILogger
    {
        private readonly ILog _log;
        public CommonLogger(ILog getLogger)
        {
            _log = getLogger;
        }

        public void Debug(string message, params object[] args)
        {
            if (_log.IsDebugEnabled)
            {
                _log.DebugFormat(message, args);
            }
        }

        public void Error(string message, params object[] args)
        {
            if (_log.IsErrorEnabled)
            {
                _log.ErrorFormat(message, args);
            }
        }

        public void Info(string message, params object[] args)
        {
            if (_log.IsInfoEnabled)
            {
                _log.InfoFormat(message, args);
            }
        }

        public void Fatal(string message, params  object[] args)
        {
            if (_log.IsFatalEnabled)
            {
                _log.FatalFormat(message, args);
            }
        }

        public void Warn(string message, params object[] args)
        {
            if (_log.IsWarnEnabled)
            {
                _log.WarnFormat(message, args);
            }
        }

        public void Debug(string message)
        {
            _log.Debug(message);
        }

        public void Error(string message)
        {
            _log.Error(message);
        }

        public void Info(string message)
        {
            _log.Info(message);
        }

        public void Fatal(string message)
        {
            _log.Fatal(message);
        }

        public void Warn(string message)
        {
            _log.Warn(message);
        }
    }
}

#endif

namespace Dragon
{
    public interface ILogger
    {
        void Debug(string message, params object[] args);
        void Error(string message, params object[] args);
        void Info(string message, params object[] args);
        void Fatal(string message, params object[] args);
        void Warn(string message, params object[] args);
        void Debug(string message);
        void Error(string message);
        void Info(string message);
        void Fatal(string message);
        void Warn(string message);
    }
}