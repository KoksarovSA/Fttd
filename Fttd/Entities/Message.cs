using System;

namespace Fttd.Entities
{
    internal class Message
    {
        public string FromEmployee { get; set; }
        public string WhereEmployee { get; set; }
        public string Mess { get; set; }
        public DateTime TimeMess { get; set; }

        public Message()
        {
        }

        public Message(string fromEmployee, string whereEmployee, string mess, string timeMess)
        {
            FromEmployee = fromEmployee ?? throw new ArgumentNullException(nameof(fromEmployee));
            WhereEmployee = whereEmployee ?? throw new ArgumentNullException(nameof(whereEmployee));
            Mess = mess ?? throw new ArgumentNullException(nameof(mess));
            if (timeMess != "" && timeMess != null) { TimeMess = DateTime.Parse(timeMess); }
        }

        public override string ToString()
        {
            string message = Convert.ToString(TimeMess) + " | " + FromEmployee + " -> " + WhereEmployee + ": " + Mess;
            return message;
        }
    }
}
