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
                if (field.charFirst <= position && field.charLast <= position)
                {
                    return field;
                }
            }

            return null;
        }
    }
}
