using System;
using System.Text;
using System.Globalization;
using System.Management.Instrumentation;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Xml.Serialization;

namespace blogEngine.Shared.Logging {
    public partial class Log {
        public string AdditionalDetail { get; set; }
    }
}