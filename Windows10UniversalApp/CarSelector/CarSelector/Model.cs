using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSelector
{
    public class Model
    {
        public string ModelName { get; set; }
        public string ModelImageSource { get; set; }
        public string ModelRate { get; set; }
        public string Text { get; set; }
        public override string ToString()
        {
            return Text;
        }
    }
}
