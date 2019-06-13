using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fttd
{
    class Inventory
    {
        public Inventory()
        {
        }

        public Inventory(double inventoryNom)
        {
            InventoryNom = inventoryNom;
        }

        public double InventoryNom { get; set; }

        public override string ToString()
        {
            return Convert.ToString(InventoryNom);
        }
    }
}
