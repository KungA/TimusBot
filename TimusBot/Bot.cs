using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
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
                var submits = timus.GetSubmits(user.Id)
                    .Where(x => x.Timestamp > user.LastSolvedProblemTime)
                    .ToList();

                UpdateUser(user, submits);

                foreach (var submit in submits)
                {
                    var problemSubmits = timus.GetSubmits(user.Id, submit.ProblemId);
                    if (problemSubmits.Any(s => s.Timestamp < submit.Timestamp))
                        continue;

                    await SendMessage(users, user, submit);
                }

                SaveBotState(users);
            }
        }

        private async Task SendMessage(List<User> users, User user, Submit submit)
        {
            var action = user.Name.StartsWith("Аня") ? "сдала" : "сдал";
            var message = $"{user.Name} {action} задачу {submit.ProblemId} [{timus.GetProblemName(submit.ProblemId)}](http://timus.online/problem.aspx?num={submit.ProblemId}) ";

            var solved = user.SolvedCount;
            if (solved % 10 == 0)
                message += $"\nУже {solved} 👍";

            message += "\n" + WhoSolved(users, submit.ProblemId);
            await bot.SendTextMessageAsync(chatId, message, ParseMode.Markdown);
            log.Info($"SENT {message} to {chatId}");

            if (solved == 1000)
                await bot.SendStickerAsync(chatId, new FileToSend("BQADAgADFQADkZ2hB9unroIFwbsCAg")); //ого
            else if (solved % 100 == 0)
                await bot.SendStickerAsync(chatId, new FileToSend("BQADAgADrgAD41AwAAENKAshbD_oQwI")); //ничосе
        }

        private void UpdateUser(User user, List<Submit> submits)
        {
            user.SolvedCount = timus.GetSolvedCount(user.Id);
            if (submits.Any())
                user.LastSolvedProblemTime = submits.Max(x => x.Timestamp);
        }

        private string WhoSolved(List<User> users, string problemId)
        {
            var result = "";
            foreach (var user in users)
            {
                if (timus.HasAc(user.Id, problemId))
                    result += user.Name.Substring(user.Name.Length - 2);
            }
            if (result.Length == 2)
                result = "First AC!";
            return result;
        }

        private List<User> GetUsers()
        {
            var state = GetBotState();
            if (state.Any())
                return state;

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
                    LastSolvedProblemTime = DateTime.Now.AddMinutes(-2),
                    SolvedCount = timus.GetSolvedCount(tokens[1])
                };
                users.Add(user);
            }

            return users;
        }

        private void SaveBotState(List<User> users)
        {
            var json = JsonConvert.SerializeObject(users, Formatting.Indented);
            File.WriteAllText(StateFileName(), json);
        }

        private List<User> GetBotState()
        {
            try
            {
                var json = File.ReadAllText(StateFileName());
                return JsonConvert.DeserializeObject<List<User>>(json);
            }
            catch (Exception)
            {
                return new List<User>();
            }
        }

        private string StateFileName()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TimusBotState.json");
        }
    }
}