using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSelector
{
    public class Make
    {
        public string MakeName { get; set; }
        public string MakeImageSource { get; set; }
        public string MakeRate { get; set; }
        public string Text { get; set; }
        public override string ToString()
        {
            return Text;
        }
    }
}
