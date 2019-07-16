using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fttd.Entities
{
    internal class TaskStatus
    {
        public TaskStatus()
        {
        }

        public TaskStatus(string task, string detail, string employee, string status, string problem, string solution, string data)
        {
            Task = task ?? throw new ArgumentNullException(nameof(task));
            Detail = detail ?? throw new ArgumentNullException(nameof(detail));
            Employee = employee ?? throw new ArgumentNullException(nameof(employee));
            Status = status ?? throw new ArgumentNullException(nameof(status));
            Problem = problem ?? throw new ArgumentNullException(nameof(problem));
            Solution = solution ?? throw new ArgumentNullException(nameof(solution));
            Data = DateTime.Parse(data);
        }

        public string Task { get; private set; }
        public string Detail { get; private set; }
        public string Employee { get; private set; }
        public string Status { get; private set; }
        public string Problem { get; private set; }
        public string Solution { get; private set; }
        public DateTime Data { get; private set; }
    }
}
