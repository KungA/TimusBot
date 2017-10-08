using System;

namespace TimusLib
{
    public class Submit
    {
        public string SubmitId;
        public string ProblemId;
        public DateTime Timestamp;

        public static Submit Parse(string str)
        {
            var tokens = str.Split('\t');

            return new Submit
            {
                Timestamp = DateTime.Parse(tokens[1]),
                ProblemId = tokens[4]
            };
        }
    }
}