using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDI_Utilities
{
    class idocField
    {
        public String name { get; set; }
        public String text { get; set; }
        public String type { get; set; }
        public int fieldPos { get; set; }
        public int length { get; set; }
        public int charFirst { get; set; }
        public int charLast { get; set; }

        public override string ToString()
        {
            String output = "";
            output += "NAME: " + name + Environment.NewLine;
            output += "TEXT: " + text + Environment.NewLine;
            output += "TYPE: " + type + Environment.NewLine;
            output += "FIELD_POS: " + fieldPos + Environment.NewLine;
            output += "LENGTH: " + length + Environment.NewLine;
            output += "CHAR_FIRST: " + charFirst + Environment.NewLine;
            output += "CHAR_LAST: " + charLast + Environment.NewLine;
            return output;
        }
    }
}
