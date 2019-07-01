using Fttd.Entities;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;


namespace Fttd
{
    internal static class State
    {
        internal static bool stateTreeView;
        internal static Employees employee;

        internal static Collection<Detail> detailColl = new Collection<Detail>();
        internal static Collection<TaskDet> taskColl = new Collection<TaskDet>();
        internal static Collection<Project> projectColl = new Collection<Project>();
        internal static Collection<Developer> developerColl = new Collection<Developer>();
        internal static Collection<Inventory> inventoryColl = new Collection<Inventory>();
        internal static Collection<Device> deviceColl = new Collection<Device>();
        internal static Collection<Graphics> graphicsColl = new Collection<Graphics>();
        internal static Collection<Services> servicesColl = new Collection<Services>();
        internal static Collection<Message> messageColl = new Collection<Message>();
        internal static Collection<Employees> employeeColl = new Collection<Employees>();

        public static void UpdateEmployee()
        {
            Dbaccess dbaccess = new Dbaccess();
            dbaccess.Dbselect("SELECT [Firstname], [Lastname], [Patronymic], [Shortname], [Ip], [Position], [Access], [Tabel] FROM [employees] WHERE [Ip] = '" + Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString() + "'");
            for (int i = 0; i < dbaccess.Querydata.Count; i++)
            {
                string[] vs = dbaccess.Querydata[i];
                employee = new Employees(vs[0], vs[1], vs[2], vs[3], vs[4], vs[5], Convert.ToInt16(vs[6]), Convert.ToInt16(vs[7]));
            }
            dbaccess.Dbselect("SELECT [Firstname], [Lastname], [Patronymic], [Shortname], [Ip], [Position], [Access], [Tabel] FROM [employees]");
            for (int i = 0; i < dbaccess.Querydata.Count; i++)
            {
                string[] vs = dbaccess.Querydata[i];
                employeeColl.Add(new Employees(vs[0], vs[1], vs[2], vs[3], vs[4], vs[5], Convert.ToInt16(vs[6]), Convert.ToInt16(vs[7])));
            }
        }

        public static bool UpdateMessageColl()
        {
            bool result;
            int count = messageColl.Count;
            Dbaccess dbaccess2 = new Dbaccess();
            dbaccess2.Db2select("SELECT [FromEmployee], [WhereEmployee], [DateTime], [Message] FROM [chat] WHERE [FromEmployee] = '" + employee.ShortName + "' OR [FromEmployee] = 'Всем' OR [WhereEmployee] = '" + employee.ShortName + "'");
            if (count != dbaccess2.Querydata.Count)
            {
                messageColl.Clear();
                for (int i = 0; i < dbaccess2.Querydata.Count; i++)
                {
                    string[] vs = dbaccess2.Querydata[i];
                    messageColl.Add(new Message(vs[0], vs[1], vs[3], vs[2]));
                }
                result = true;
            }
            else result = false;
            return result;
        }

        public static void UpdateDataTreeView()
        {
            Dbaccess dbaccess = new Dbaccess();
            dbaccess.Dbselect("SELECT [detail_index], [detail_name], [inventory], [razrabotal], [project], [number_task], [data_add] FROM [detail_db] ORDER BY [detail_name]");
            detailColl.Clear();
            developerColl.Clear();
            for (int i = 0; i < dbaccess.Querydata.Count; i++)
            {
                string[] vs = dbaccess.Querydata[i];
                detailColl.Add(new Detail(vs[2], vs[0], vs[1], vs[4], vs[3], vs[5], vs[6]));
                developerColl.Add(new Developer(vs[3]));
                inventoryColl.Add(new Inventory(Convert.ToDouble(vs[2])));
            }
            developerColl.Distinct();
            developerColl.GroupBy(developerColl => developerColl.DeveloperName);
            inventoryColl.GroupBy(inventoryColl => inventoryColl.InventoryNom);
            dbaccess.Dbselect("SELECT [task], [project], [dir], [note], [iscurrent], [datein], [dateout], [leading] FROM [task] ORDER BY [task]");
            taskColl.Clear();
            for (int i = 0; i < dbaccess.Querydata.Count; i++)
            {
                string[] vs = dbaccess.Querydata[i];
                taskColl.Add(new TaskDet(vs[0], vs[1], vs[2], vs[3], vs[4], vs[5], vs[6], vs[7]));
            }
            dbaccess.Dbselect("SELECT [project] FROM [project] ORDER BY [project]");
            projectColl.Clear();
            for (int i = 0; i < dbaccess.Querydata.Count; i++)
            {
                string[] vs = dbaccess.Querydata[i];
                projectColl.Add(new Project(vs[0]));
            }
            dbaccess.Dbselect("SELECT [indexdev], [namedev], [razrab], [data_add] FROM [device] ORDER BY [indexdev]");
            deviceColl.Clear();
            for (int i = 0; i < dbaccess.Querydata.Count; i++)
            {
                string[] vs = dbaccess.Querydata[i];
                deviceColl.Add(new Device(vs[1], vs[0], vs[2], vs[3]));
            }
            dbaccess.Dbselect("SELECT [namegrap], [project], [dir], [data_add] FROM [graphics] ORDER BY [namegrap]");
            graphicsColl.Clear();
            for (int i = 0; i < dbaccess.Querydata.Count; i++)
            {
                string[] vs = dbaccess.Querydata[i];
                graphicsColl.Add(new Graphics(vs[0], vs[1], vs[2], vs[3]));
            }
            dbaccess.Dbselect("SELECT [nameserv], [dir], [note], [data_add] FROM [service] ORDER BY [nameserv]");
            servicesColl.Clear();
            for (int i = 0; i < dbaccess.Querydata.Count; i++)
            {
                string[] vs = dbaccess.Querydata[i];
                servicesColl.Add(new Services(vs[0], vs[1], vs[2], vs[3]));
            }
        }

        /// <summary>
        /// Метод бэкапит базу данных через 6 дней  
        /// </summary>
        public static void BackupFTTDDB()
        {
            if (Math.Abs(DateTime.Now.Day - Convert.ToInt32(Param_in.GetFTTDBackup())) > 6)
            {
                Directory.CreateDirectory(Directory.GetParent(Param_in.DirDb).ToString() + "\\backup");
                if (!File.Exists(Directory.GetParent(Param_in.DirDb).ToString() + "\\backup\\backup_from_" + DateTime.Now.ToString("dd.MM.yyyy") + "_" + new DirectoryInfo(Param_in.DirDb).Name)) File.Copy(Param_in.DirDb, Directory.GetParent(Param_in.DirDb).ToString() + "\\backup\\backup_from_" + DateTime.Now.ToString("dd.MM.yyyy") + "_" + new DirectoryInfo(Param_in.DirDb).Name);
                Param_in.SetFTTDBackup(Convert.ToString(DateTime.Now.Day));
            }
        }
    }
}
