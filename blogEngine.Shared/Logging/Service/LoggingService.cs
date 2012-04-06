using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.Data;

namespace blogEngine.Shared.Logging {
    public interface ILoggingService {
        void Save(Log log);
    }

    public class LoggingService : ILoggingService {
        public void Save(Log log) {
            using (LoggingEntities context = new LoggingEntities()) {
                if (log.LogID == 0) {
                    context.Logs.AddObject(log);
                    if (!string.IsNullOrEmpty(log.AdditionalDetail)) {
                        context.LogDetails.AddObject(new LogDetail() { Detail = log.AdditionalDetail });
                    }
                } else {
                    EntityKey key = default(EntityKey);
                    object originalItem = null;
                    key = context.CreateEntityKey("Logs", log);
                    // Get the original item based on the entity key from the context 
                    // or from the database. 
                    if (context.TryGetObjectByKey(key, out originalItem)) {
                        // Call the ApplyCurrentValues method to apply changes 
                        // from the updated item to the original version. 
                        context.ApplyCurrentValues(key.EntitySetName, log);
                    }
                }
                context.SaveChanges();
            }
        }
    }
}
