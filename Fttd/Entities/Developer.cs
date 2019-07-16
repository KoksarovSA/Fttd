using System;

namespace Fttd.Entities
{
    internal class Developer : IEquatable<Developer>
    {
        public Developer()
        {
        }

        public Developer(string developerName)
        {
            DeveloperName = developerName ?? throw new ArgumentNullException(nameof(developerName));
        }

        public string DeveloperName { get; set; }

        public bool Equals(Developer other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            return DeveloperName.Equals(other.DeveloperName);
        }

        public override int GetHashCode()
        {
            int hashDeveloper = DeveloperName == null ? 0 : DeveloperName.GetHashCode();
            return hashDeveloper;
        }
        public override string ToString()
        {
            return DeveloperName;
        }
    }
}
