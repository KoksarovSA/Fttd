using System;

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

        public TaskDet(string taskName, string projectTaskName, string taskDir, string taskNote, string taskIsCurrent, string taskDateIn, string taskDateOut, string leading)
        {
            TaskName = taskName ?? throw new ArgumentNullException(nameof(taskName));
            ProjectTaskName = projectTaskName ?? throw new ArgumentNullException(nameof(projectTaskName));
            TaskDir = taskDir ?? throw new ArgumentNullException(nameof(taskDir));
            TaskNote = taskNote ?? throw new ArgumentNullException(nameof(taskNote));
            TaskIsCurrent = Convert.ToBoolean(taskIsCurrent);
            if (taskDateIn != "") { TaskDateIn = DateTime.Parse(taskDateIn); }
            if (taskDateOut != "") { TaskDateOut = DateTime.Parse(taskDateOut); }
            Days = (TaskDateOut - DateTime.Now).Days;
            Leading = leading ?? "Нет";
        }

        public string TaskName { get; set; }
        public string ProjectTaskName { get; set; }
        public string TaskDir { get; set; }
        public string TaskNote { get; set; }
        public bool TaskIsCurrent { get; set; }
        public DateTime TaskDateIn { get; set; }
        public DateTime TaskDateOut { get; set; }
        public string Actuality
        { get
            {
                if (TaskIsCurrent) return "Актуально";
                else return "Не актуально";
            }
        }
        public int Days { get; set; }
        public string Leading { get; set; }

        public override string ToString()
        {
            string description = "Задание: " + TaskName + "\nПроект: " + ProjectTaskName + "\nАктуальность: " + Actuality + "\nОтветственный: " + Leading + "\nДата выдачи: " + TaskDateIn.Date.ToString("dd.MM.yy") + "\nВыполнить до: " + TaskDateOut.Date.ToString("dd.MM.yy");
            return description;
        }
    }
}
