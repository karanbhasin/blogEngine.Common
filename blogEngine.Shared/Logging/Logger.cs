using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace blogEngine.Shared.Logging {
    public class Logger {

        private static ILoggingService _loggerService;
        public static ILoggingService LoggingService {
            get {
                if (_loggerService == null) {
                    _loggerService = new LoggingService();
                }
                return _loggerService;
            }
            set {
                _loggerService = value;
            }
        }

        private const string DATABASE_LOGSINK = "database";
        private const string MSMQ_LOGSINK = "msmq";
        public static void Write(Log log) {

            switch (Foundation.Common.Util.ConfigUtil.GetAppSetting("Logging.Destination").ToLower()) {
                case Logger.DATABASE_LOGSINK:
                    LoggingService.Save(log);
                    break;
                case Logger.MSMQ_LOGSINK:
                    string destinationQ = Foundation.Common.Util.ConfigUtil.GetAppSetting("Logging.MSMQ.Queue.Path");

                    // publish the request to the appropriate message queue
                    System.Messaging.MessageQueue mq = new System.Messaging.MessageQueue(destinationQ);

                    System.Messaging.Message msg = new System.Messaging.Message();
                    msg.Formatter = new System.Messaging.XmlMessageFormatter(new Type[] { typeof(blogEngine.Shared.Logging.Log) });
                    msg.Body = log;
                    msg.Priority = System.Messaging.MessagePriority.Normal;
                    msg.Recoverable = true;
                    msg.Label = log.LogType.GetType().Name;
                    mq.Send(msg);
                    break;
                default:
                    LoggingService.Save(log);
                    break;
            }
        }
    }
}
