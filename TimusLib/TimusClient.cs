using System;
using System.Collections.Generic;
using System.Linq;

namespace TimusLib
{
    public class TimusClient
    {
        public List<Problem> GetLatestSolvedProblems(User user)
        {
            var result = new List<Problem>();

            var text = WebHelper.DownloadPage($@"http://acm.timus.ru/textstatus.aspx?author={user.Id}&status=accepted&count=100");
            var lines = text.Split('\n').Skip(1).ToList();

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                var tokens = line.Split('\t');
                var time = DateTime.Parse(tokens[1]);
                var task = tokens[4];

                result.Add(new Problem(task)
                {
                    SolvedAt = time
                });
            }

            return result;
        }

        public string GetTaskName(string taskId)
        {
            var text = WebHelper.DownloadPage($"http://acm.timus.ru/problem.aspx?num={taskId}&locale=ru");

            int l = text.IndexOf("<TITLE>", StringComparison.Ordinal);
            int r = text.IndexOf("@ Timus Online Judge</TITLE>", StringComparison.Ordinal);
            var name = text.Substring(l + 7 + 6, r - l - 8 - 6);
            return name;
        }

        public int GetSolvedCount(string userId)
        {
            var text = WebHelper.DownloadPage($"http://acm.timus.ru/author.aspx?id={userId}&locale=ru");
            int l = text.IndexOf("Решено задач", StringComparison.Ordinal);
            int r = text.IndexOf(" из", l, StringComparison.Ordinal);
            var count = text.Substring(l + 48, r - l - 48);
            return int.Parse(count);
        }

        public bool HasAc(string taskId, string userId)
        {
            var text = WebHelper.DownloadPage($"http://acm.timus.ru/textstatus.aspx?space=1&num={taskId}&author={userId}&status=accepted");
            var lines = text.Split('\n');
            return lines.Length > 2;
        }
    }
}