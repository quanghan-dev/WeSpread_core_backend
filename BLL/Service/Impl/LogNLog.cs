using NLog;

namespace BLL.Service.Impl
{
    public class LogNLog : ILogger
    {
        private static NLog.ILogger logger = LogManager.GetCurrentClassLogger();

        public LogNLog()
        {
        }

        public void Debug(string message)
        {
            logger.Debug(message);
        }

        public void Error(string message)
        {
            logger.Error(message);
        }

        public void Information(string message)
        {
            logger.Info(message);
        }

        public void Warning(string message)
        {
            logger.Warn(message);
        }
    }
}
