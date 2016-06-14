using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDI_Utilities
{
    class idocSegment
    {
        public String name { get; set; }
        public String level { get; set; }
        public String status { get; set; }
        public String loopMin { get; set; }
        public String loopMax { get; set; }
        //the string is the field name
        public List<idocField> fields = new List<idocField>();

        public idocField getFieldInPosition(int position)
        {
            foreach (idocField field in fields)
            {
                if (field.charFirst <= (position+1) && field.charLast >= (position+1))
                {
                    return field;
                }
            }

            return null;
        }

        public override String ToString()
        {
            String output = "-SEGMENT-" + Environment.NewLine;
            output += "NAME: " + name + Environment.NewLine;
            output += "LEVEL: " + level + Environment.NewLine;
            output += "STATUS: " + status + Environment.NewLine;
            output += "LOOP_MIN: " + loopMin + Environment.NewLine;
            output += "LOOP_MAX: " + loopMax + Environment.NewLine;
            output += "KNOWN FIELDS: " + fields.Count + Environment.NewLine;
            return output;
        }
    }
}
