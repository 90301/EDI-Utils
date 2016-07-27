using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDI_Utilities
{
    class IdocRealLine
    {
        public String segmentName;
        public String displaySegName;//[X] appended if already existing.
        public List<String> fields = new List<string>();
        public IdocSegment segmentInfo;
        public String fullLine;
    }
}
