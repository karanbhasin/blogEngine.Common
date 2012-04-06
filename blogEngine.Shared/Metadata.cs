using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace blogEngine.Shared {
    [AttributeUsage(AttributeTargets.All)]
    public class MetaData : System.Attribute {
        private string _value = null;
        public MetaData(string value) {
            _value = value;
        }

        public string Value {
            get { return _value; }
        }
    }
}
