using System;

namespace TimusLib
{
    public class User
    {
        public string Name;
        public string Id;

        public DateTime LastSolvedProblemTime;
        public int SolvedCount { get; set; }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(SolvedCount)}: {SolvedCount}";
        }
    }
}