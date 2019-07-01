using System;
using System.Collections.ObjectModel;

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
        if (timeMess != "" && timeMess != null)
        {
            TimeMess = DateTime.Parse(timeMess);
        }
    }

    public override string ToString()
    {
        string message = Convert.ToString(TimeMess) + " | " + FromEmployee + " -> " + WhereEmployee + ": " + Mess;
        return message;
    }

    public static Collection<Message> GetMessage()
    {
        var result = new Collection<Message>();
        Dbaccess dbaccess = new Dbaccess();
        dbaccess.Dbselect("SELECT [FromEmployee], [WhereEmployee], [DateTime], [Message] FROM [chat]");
        for (int i = 0; i < dbaccess.Querydata.Count; i++)
        {
            string[] vs = dbaccess.Querydata[i];
            result.Add(new Message(vs[0], vs[1], vs[3], vs[2]));
        }
        return result;
    }
}
}
