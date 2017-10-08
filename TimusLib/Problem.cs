using System;

namespace TimusLib
{
    public class Problem
    {
        public readonly string Id;
        public DateTime SolvedAt;

        public Problem(string problemId)
        {
            Id = problemId;
        }

        private bool Equals(Problem other)
        {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Problem) obj);
        }

        public override int GetHashCode() => Id?.GetHashCode()??0;
    }
}