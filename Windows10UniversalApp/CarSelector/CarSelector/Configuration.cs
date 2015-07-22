using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSelector
{
    class Configuration
    {
        public string LastMake { get; set; }
        public string LastModel { get; set; }
        public string LastYear { get; set; }
        public string LastScenarioControlIndex { get; set; }
        public bool LastIsAlphabetical { get; set; }
        public Type LastScenario { get; set; }
    }
}
