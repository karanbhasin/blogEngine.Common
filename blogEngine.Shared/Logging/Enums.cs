using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace blogEngine.Shared.Logging.Enums {
    public enum LogType {
        /// <summary>
        /// Default.
        /// </summary>
        Unspecified = 1,

        /// <summary>
        /// Informational.
        /// </summary>
        Information = 2,

        /// <summary>
        /// Non-critical error/warning.
        /// </summary>
        Warning = 3,

        /// <summary>
        /// Critical error.
        /// </summary>
        Error = 4,

        /// <summary>
        /// Performance Stats
        /// </summary>
        Stats= 5
    }
}
