using System;

namespace Fttd
{
    internal class Project
    {
        public Project()
        {
        }

        public Project(string projectName)
        {
            ProjectName = projectName ?? throw new ArgumentNullException(nameof(projectName));
        }

        public string ProjectName { get; set; }

        public override string ToString()
        {
            return ProjectName;
        }
    }
}
