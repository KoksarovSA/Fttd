using Fttd.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fttd
{
    internal static class State
    {
        public static bool stateTreeView;

        public static void UpdateDataTreeView()
        {
            Dbaccess dbaccess = new Dbaccess();
            dbaccess.Dbselect("SELECT [detail_index], [detail_name], [inventory], [razrabotal], [project], [number_task], [data_add] FROM [detail_db] ORDER BY [detail_name]");
            DetailList.detailColl.Clear();
            DetailList.developerColl.Clear();
            for (int i = 0; i < dbaccess.Querydata.Count; i++)
            {
                string[] vs = dbaccess.Querydata[i];
                DetailList.detailColl.Add(new Detail(vs[2], vs[0], vs[1], vs[4], vs[3], vs[5], vs[6]));
                DetailList.developerColl.Add(new Developer(vs[3]));
                DetailList.inventoryColl.Add(new Inventory(Convert.ToDouble(vs[2])));
            }
            DetailList.developerColl.Distinct();
            DetailList.developerColl.GroupBy(developerColl => developerColl.DeveloperName);
            DetailList.inventoryColl.GroupBy(inventoryColl => inventoryColl.InventoryNom);

            dbaccess.Dbselect("SELECT [task], [project], [dir], [note], [iscurrent], [datein], [dateout] FROM [task] ORDER BY [task]");
            DetailList.taskColl.Clear();
            for (int i = 0; i < dbaccess.Querydata.Count; i++)
            {
                string[] vs = dbaccess.Querydata[i];
                DetailList.taskColl.Add(new TaskDet(vs[0], vs[1], vs[2], vs[3], vs[4], vs[5], vs[6]));
            }
            dbaccess.Dbselect("SELECT [project] FROM [project] ORDER BY [project]");
            DetailList.projectColl.Clear();
            for (int i = 0; i < dbaccess.Querydata.Count; i++)
            {
                string[] vs = dbaccess.Querydata[i];
                DetailList.projectColl.Add(new Project(vs[0]));
            }
            dbaccess.Dbselect("SELECT [indexdev], [namedev], [razrab], [data_add] FROM [device] ORDER BY [indexdev]");
            DetailList.deviceColl.Clear();
            for (int i = 0; i < dbaccess.Querydata.Count; i++)
            {
                string[] vs = dbaccess.Querydata[i];
                DetailList.deviceColl.Add(new Device(vs[1], vs[0], vs[2], vs[3]));
            }
            dbaccess.Dbselect("SELECT [namegrap], [project], [dir], [data_add] FROM [graphics] ORDER BY [namegrap]");
            DetailList.graphicsColl.Clear();
            for (int i = 0; i < dbaccess.Querydata.Count; i++)
            {
                string[] vs = dbaccess.Querydata[i];
                DetailList.graphicsColl.Add(new Graphics(vs[0], vs[1], vs[2], vs[3]));
            }
            dbaccess.Dbselect("SELECT [nameserv], [dir], [note], [data_add] FROM [service] ORDER BY [nameserv]");
            DetailList.servicesColl.Clear();
            for (int i = 0; i < dbaccess.Querydata.Count; i++)
            {
                string[] vs = dbaccess.Querydata[i];
                DetailList.servicesColl.Add(new Services(vs[0], vs[1], vs[2], vs[3]));
            }
            stateTreeView = false;
        }
    }
}
