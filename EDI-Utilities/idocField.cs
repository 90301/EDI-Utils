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
    }
}
