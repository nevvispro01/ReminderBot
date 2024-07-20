using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Globalization;
using Telegram.Bot;

namespace ReminderBot
{
    public class DataReminder
    {
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public string IdReminder { get; set; } = "";
        public long ChatId { get; set; }
        public string Text { get; set; } = "";
        public string Time { get; set; } = "";
    }


    public class DataBase
    {
        private static MongoClient client;
        private static IMongoDatabase database;
        private static IMongoCollection<DataReminder> collection;

        public DataBase()
        {
            ConnectionDataBase();
        }

        public List<DataReminder> CheckReminder(ITelegramBotClient botClient)
        {
            var resultList = new List<DataReminder>();
            foreach (var reminder in collection.Find("{}").ToList()){
                var time = DateTime.ParseExact(reminder.Time, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                if (DateTime.Compare(time, DateTime.Now) > 0)
                {
                    resultList.Add(reminder);
                }
                else
                {
                    var filter = new BsonDocument { { "IdReminder", reminder.IdReminder } };
                    collection.DeleteOne(filter);
                    botClient.SendTextMessageAsync(
                    reminder.ChatId,
                    "К велечайшему сожалению наш сервис был аварийно выключен в момент вашего напоминания: " + 
                    reminder.Text + 
                    ". Приносим свои глубочайшие извенения."
            );
                }
            }
            return resultList;
        }

        private static void ConnectionDataBase()
        {
            client = new MongoClient("mongodb://localhost:27017");
            database = client.GetDatabase("Reminder");
            collection = database.GetCollection<DataReminder>("reminderList");
        }

        public string? AddReminder(long chatId, string textReminder, string time)
        {
            var data = new DataReminder { IdReminder = Guid.NewGuid().ToString(), ChatId = chatId, Text = textReminder, Time = time };
            
            collection.InsertOne(data);
            return data.IdReminder.ToString();
        }

        public async Task RemoveReminder(string idReminder)
        {
            var filter = new BsonDocument { { "IdReminder", idReminder }};
            await collection.DeleteOneAsync(filter);
        }

    }
}
