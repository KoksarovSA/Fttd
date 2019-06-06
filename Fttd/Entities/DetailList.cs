using Fttd.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fttd
{
    internal static class DetailList
    {
        public static Collection<Detail> detailColl = new Collection<Detail>();
        public static Collection<TaskDet> taskColl = new Collection<TaskDet>();
        public static Collection<Project> projectColl = new Collection<Project>();
        public static Collection<Developer> developerColl = new Collection<Developer>();
        public static Collection<Inventory> inventoryColl = new Collection<Inventory>();
        public static Collection<Device> deviceColl = new Collection<Device>();
        public static Collection<Graphics> graphicsColl = new Collection<Graphics>();
        public static Collection<Services> servicesColl = new Collection<Services>();

    }
}
