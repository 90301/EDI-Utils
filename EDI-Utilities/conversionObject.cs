﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDI_Utilities
{
    class ConversionObject
    {
        public String x12, idocSeg, idocField, line;

        public String toString()
        {
            String s = "";
            s += "X12: " + x12 + Environment.NewLine;
            s += "IDOC Seg: " + idocSeg + Environment.NewLine;
            s += "IDOC Field: " + idocField + Environment.NewLine;
            s += "Whole Line: " + line + Environment.NewLine;
            return s;
        }

        public String findIdocInfo()
        {
            String s = "idoc info not found";
            


            return s;
        }
    }
}