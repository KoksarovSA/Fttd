using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fttd
{
    internal class TaskDet
    {
        public TaskDet()
        {
        }

        public TaskDet(string taskName)
        {
            TaskName = taskName ?? throw new ArgumentNullException(nameof(taskName));
        }

        public TaskDet(string taskName, string projectTaskName)
        {
            TaskName = taskName ?? throw new ArgumentNullException(nameof(taskName));
            ProjectTaskName = projectTaskName ?? throw new ArgumentNullException(nameof(projectTaskName));
        }

        public TaskDet(string taskName, string projectTaskName, string taskDir, string taskNote, string taskIsCurrent, string taskDateIn, string taskDateOut)
        {
            TaskName = taskName ?? throw new ArgumentNullException(nameof(taskName));
            ProjectTaskName = projectTaskName ?? throw new ArgumentNullException(nameof(projectTaskName));
            TaskDir = taskDir ?? throw new ArgumentNullException(nameof(taskDir));
            TaskNote = taskNote ?? throw new ArgumentNullException(nameof(taskNote));
            TaskIsCurrent = taskIsCurrent ?? throw new ArgumentNullException(nameof(taskIsCurrent));
            if (taskDateIn!="") { TaskDateIn = DateTime.Parse(taskDateIn); }
            if (taskDateOut != "") { TaskDateOut = DateTime.Parse(taskDateOut); }           
        }

        public string TaskName { get; set; }
        public string ProjectTaskName { get; set; }
        public string TaskDir { get; set; }
        public string TaskNote { get; set; }
        public string TaskIsCurrent { get; set; }
        public DateTime TaskDateIn { get; set; }
        public DateTime TaskDateOut { get; set; }
    }
}
