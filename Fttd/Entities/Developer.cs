using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fttd
{
    class Developer
    {
        public Developer()
        {
        }

        public Developer(string developerName)
        {
            DeveloperName = developerName ?? throw new ArgumentNullException(nameof(developerName));
        }

        public string DeveloperName { get; set; }

        public override string ToString()
        {
            return DeveloperName;
        }
    }
}
