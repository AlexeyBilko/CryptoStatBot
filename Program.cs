using CryptoTradingBot_MVP.Models;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CryptoTradingBot_MVP
{
    class Program
    {
        const string BOT_TOKEN = "5104208334:AAFqLUj7ydjfCzZ7bccYlSl2Eiws9QpR_vs";

        static void Main(string[] args)
        {
            var telegramBotClient_ = new TelegramBotClient(BOT_TOKEN);
            //telegramBotClient_.StartReceiving();
            GlobalConfiguration.Configuration.UseMemoryStorage();
            while (true)
            {
                try
                {
                    GetMessages().Wait();
                }
                catch (Exception e) { Console.WriteLine(e); }
            }
        }

        public static async Task GetMessages()
        {
            TelegramBotClient telegramBotClient = new TelegramBotClient(BOT_TOKEN);
            int offset = 0;
            int timeout = 0;
            try
            {
                await telegramBotClient.SetWebhookAsync("");
                APICoinmarketcapData api = new APICoinmarketcapData();
                while (true)
                {
                    var backgroud = new BackgroundJobServer();
                    var updates = await telegramBotClient.GetUpdatesAsync(offset, timeout);
                    foreach (var update in updates)
                    {
                        var message = update.Message;
                        Console.WriteLine(message.Chat.Username + " -> " + message.Text);
                        using (UsersContext db = new UsersContext())
                        {
                            try
                            {
                                db.Users.Add(new Models.User()
                                {
                                    ChatId = (int)message.Chat.Id,
                                    DateJoined = DateTime.Now,
                                    FirstName = message.Chat.FirstName,
                                    LastName = message.Chat.LastName,
                                    UserName = message.Chat.Username
                                });
                                db.SaveChanges();
                                Console.WriteLine("User added");
                            }
                            catch (Exception)
                            {
                                //Console.WriteLine($"User \"{message.Chat.Username}\" already added or some error occured");
                            }
                        }
                        switch (message.Text.Substring(0, message.Text.IndexOf(' ') == -1 ? message.Text.Length : message.Text.IndexOf(' ')))
                        {
                            case "/info":
                                break;
                            case "/settimer":
                                RecurringJob.AddOrUpdate("alert_" + message.Chat.Username, () => SendAlert(telegramBotClient, message), Cron.Minutely);
                                break;
                            case "/removetimer":
                                RecurringJob.RemoveIfExists("alert_" + message.Chat.Username);
                                break;
                            case "/start":
                                await telegramBotClient.SendTextMessageAsync(message.Chat.Id,"Hi! Welcome to CryptoStatBot\n\n\n" +
                                                                                             "To get current price of some cryptocurrency just write it symbol" +
                                                                                             "(for Bitcoin it is btc, for Ethereum - eth, etc.)\n\n" +
                                                                                             "For all availabe commands write /info");
                                break;
                            default:
                                Console.WriteLine($"User \"{message.Chat.Username}\" sended -> \"{message.Text}\"");
                                var answer = api.Start(message.Text.ToString());
                                if (answer != "")
                                {
                                    await telegramBotClient.SendTextMessageAsync(message.Chat.Id, $"Current price of {message.Text.ToUpper()} -> $" + answer + $"\n\nCurrect time: {DateTime.Now}");
                                }
                                else
                                {
                                    await telegramBotClient.SendTextMessageAsync(message.Chat.Id, "Command unknown (/info for all available)");
                                }
                                break;
                        }
                        offset = update.Id + 1;
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e); }
        }

        public static void SendAlert(TelegramBotClient t, Telegram.Bot.Types.Message message)
        {
            t.SendTextMessageAsync(message.Chat.Id, "hi");
            Console.WriteLine("Alert sended");
        }
    }
}




//////
//public static Task<List<Mark>> GetAllOwners()
//{
//    const string URL_GRAPHQL = "https://kpiweb-lab3.herokuapp.com/v1/graphql";
//    IGraphQLClient client = new GraphQLHttpClient(URL_GRAPHQL, new NewtonsoftJsonSerializer());
//    var owners = GetAllOwners();
//    var query = new GraphQLRequest
//    {
//        Query = @"
//        query MyQuery {
//          marks_marks {
//            id
//            subject
//            mark
//          }
//        }"
//    };
//    var response = client.SendQueryAsync<ResponseOwnerCollectionType>(query);
//    return response;
//}
