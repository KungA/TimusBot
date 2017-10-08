using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TimusLib;
using File = System.IO.File;
using User = TimusLib.User;

namespace TimusBot
{
    public class Bot
    {
        private readonly ILog log;
        private readonly TimusClient timus;
        private readonly TelegramBotClient bot;
        private readonly ChatId chatId;

        public Bot()
        {
            log = Logger.Log;
            timus = new TimusClient();

            var telegramKey = ConfigurationManager.AppSettings["TelegramKey"];
            chatId = ConfigurationManager.AppSettings["ChatId"];
            bot = new TelegramBotClient(telegramKey);
        }

        public async Task Run()
        {
            var users = GetUsers();
            
            while (true)
            {
                try
                {
                    await MakeIteration(users);
                }
                catch (Exception e)
                {
                    log.Error(e);
                }
                
                log.Info("USERS: " + string.Join(", ", users));
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        private async Task MakeIteration(List<User> users)
        {
            foreach (var user in users)
            {
                var newSolvedProblems = timus.GetLatestSolvedProblems(user)
                    .Where(x => x.SolvedAt > user.LastSolverProblemTime)
                    .ToList();

                UpdateUser(user, newSolvedProblems);

                foreach (var task in newSolvedProblems.Distinct())
                {
                    var action = user.Name.StartsWith("Аня") ? "сдала" : "сдал";
                    var message = $"{user.Name} {action} задачу {task.Id} [{timus.GetTaskName(task.Id)}](http://acm.timus.ru/problem.aspx?num={task.Id}) ";

                    var solved = user.SolvedCount;
                    if (solved % 10 == 0)
                        message += $"\nУже {solved} 👍";

                    message += "\n" + WhoSolved(users, task.Id);
                    await bot.SendTextMessageAsync(chatId, message, ParseMode.Markdown);
                    log.Info($"SENT {message} to {chatId}");


                    if (solved == 1000)
                        await bot.SendStickerAsync(chatId, new FileToSend("BQADAgADFQADkZ2hB9unroIFwbsCAg")); //ого
                    else if (solved % 100 == 0)
                        await bot.SendStickerAsync(chatId, new FileToSend("BQADAgADrgAD41AwAAENKAshbD_oQwI")); //ничосе
                }
            }
        }

        private void UpdateUser(User user, List<Problem> solvedTasks)
        {
            user.SolvedCount = timus.GetSolvedCount(user.Id);
            if (solvedTasks.Any())
                user.LastSolverProblemTime = solvedTasks.Max(x => x.SolvedAt);
        }

        private string WhoSolved(List<User> users, string task)
        {
            var result = "";
            foreach (var user in users)
            {
                if (timus.HasAc(task, user.Id))
                    result += user.Name.Substring(user.Name.Length - 2);
            }
            if (result.Length == 2)
                result = "First AC!";
            return result;
        }

        private List<User> GetUsers()
        {
            var users = new List<User>();
            var text = File.ReadAllText(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "timus_users.txt"));

            var lines = text.Split('\n');
            foreach (var line in lines)
            {
                var tokens = line.Split(' ');
                var user = new User
                {
                    Id = tokens[1],
                    Name = tokens[0],
                    LastSolverProblemTime = DateTime.Now.AddMinutes(-2),
                    SolvedCount = timus.GetSolvedCount(tokens[1])
                };
                users.Add(user);
            }

            return users;
        }
    }
}