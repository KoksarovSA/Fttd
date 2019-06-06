using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fttd
{
    internal class Detail
    {
        public double Inventory { get; set;}
        public string Index { get; set; }
        public string DetailName { get; set; }
        public string Project { get; set; }
        public string Developer { get; set; }
        public string Task { get; set; }
        public string DataAdd { get; set; }

        public Detail()
        {
        }

        public Detail(string inventory, string index, string detailName, string project, string developer,string task, string dataAdd)
        {
            Inventory = Convert.ToDouble(inventory);
            Index = index ?? throw new ArgumentNullException(nameof(index));
            DetailName = detailName ?? throw new ArgumentNullException(nameof(detailName));
            Project = project ?? throw new ArgumentNullException(nameof(project));
            Developer = developer ?? throw new ArgumentNullException(nameof(developer));
            Task = task ?? throw new ArgumentNullException(nameof(task));
            DataAdd = dataAdd ?? throw new ArgumentNullException(nameof(dataAdd));
        }
    }
}
