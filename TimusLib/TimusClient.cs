using System;
using System.Collections.Generic;
using System.Linq;

namespace TimusLib
{
    public class TimusClient
    {
        public List<Submit> GetSubmits(string userId, string problemId = null)
        {
            var uri = $@"http://timus.online/textstatus.aspx?space=1&author={userId}&status=accepted&count=100";
            if (problemId != null)
                uri += $"&num={problemId}";

            var text = WebHelper.DownloadPage(uri);
            var lines = text.Split(new [] {'\n'}, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList();

            return lines.Select(Submit.Parse).ToList();
        }

        public bool HasAc(string userId, string problemId)
        {
            return GetSubmits(userId, problemId).Any();
        }

        public string GetProblemName(string problemId)
        {
            var text = WebHelper.DownloadPage($"http://timus.online/problem.aspx?num={problemId}&locale=ru");

            int l = text.IndexOf("<TITLE>", StringComparison.Ordinal);
            int r = text.IndexOf("@ Timus Online Judge</TITLE>", StringComparison.Ordinal);
            var name = text.Substring(l + 7 + 6, r - l - 8 - 6);
            return name;
        }

        public int GetSolvedCount(string userId)
        {
            var text = WebHelper.DownloadPage($"http://timus.online/author.aspx?id={userId}&locale=ru");
            int l = text.IndexOf("Решено задач", StringComparison.Ordinal);
            int r = text.IndexOf(" из", l, StringComparison.Ordinal);
            var count = text.Substring(l + 48, r - l - 48);
            return int.Parse(count);
        }
    }
}