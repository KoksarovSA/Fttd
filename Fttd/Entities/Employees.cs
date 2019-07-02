using System;

namespace Fttd.Entities
{
    internal class Employees
    {
        public Employees()
        {
        }

        public Employees(string firstName, string lastName, string patronimic, string shortName, string ip, string position, int access, int tabel)
        {
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            Patronimic = patronimic ?? throw new ArgumentNullException(nameof(patronimic));
            ShortName = shortName ?? throw new ArgumentNullException(nameof(shortName));
            Ip = ip ?? throw new ArgumentNullException(nameof(ip));
            Position = position ?? throw new ArgumentNullException(nameof(position));
            Access = access;
            Tabel = tabel;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronimic { get; set; }
        public string ShortName { get; set; }
        public string Ip { get; set; }
        public string Position { get; set; }
        public int Access { get; set; }
        public int Tabel { get; set; }
    }
}
