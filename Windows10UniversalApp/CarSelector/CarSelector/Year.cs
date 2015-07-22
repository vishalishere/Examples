using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSelector
{
    public class Year
    {
        public string YearName { get; set; }
        public string YearImageSource { get; set; }
        public string YearRate { get; set; }
        public string Text { get; set; }
        public override string ToString()
        {
            return Text;
        }
    }
}