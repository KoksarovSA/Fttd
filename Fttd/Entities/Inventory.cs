using System;

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
