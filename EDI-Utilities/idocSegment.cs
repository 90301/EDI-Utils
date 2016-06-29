using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDI_Utilities
{
    class IdocSegment
    {
        public String name { get; set; }
        public String level { get; set; }
        public String status { get; set; }
        public String loopMin { get; set; }
        public String loopMax { get; set; }
        //the string is the field name
        public List<IdocField> fields = new List<IdocField>();

        public IdocField getFieldInPosition(int position)
        {
            foreach (IdocField field in fields)
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

        public IdocField findField(string idocField)
        {
            foreach (IdocField field in fields)
            {
                if (field.name.Contains(idocField))
                {
                    return field;
                }
            }
            return null;
        }
    }
}
