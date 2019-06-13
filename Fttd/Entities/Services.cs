using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fttd.Entities
{
    internal class Services
    {
        public Services()
        {
        }

        public Services(string nameServ, string dirServ, string note, string dateAddServ)
        {
            NameServ = nameServ ?? throw new ArgumentNullException(nameof(nameServ));
            DirServ = dirServ ?? throw new ArgumentNullException(nameof(dirServ));
            Note = note ?? throw new ArgumentNullException(nameof(note));
            DateAddServ = DateTime.Parse(dateAddServ);
        }

        public string NameServ { get; set; }
        public string DirServ { get; set; }
        public string Note { get; set; }
        public DateTime DateAddServ { get; set; }

        public override string ToString()
        {
            string description = "Название служебной: " + NameServ + "\nДиректория служебной: " + DirServ + "\nПримечание: " + Note + "\nДата добавления: " + Convert.ToString(DateAddServ);
            return description;
        }

    }
}
