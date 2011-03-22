using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using SimpleUsages.Mapping;
using EasyMongo;

namespace SimpleUsages
{
    class Program
    {
        private static void Go(long userId)
        {
            var settings = new MongoServerSettings
            {
                Server = new MongoServerAddress("127.0.0.1", 27017),
                SafeMode = SafeMode.False
            };
            var mongoServer = new MongoServer(settings);

            var database = mongoServer.GetDatabase("Test");

            var map = new NoteMap();
            var collection = new EntityCollection<Note>(database, map.GetDescriptor(), true);

            var note = new Note
            {
                NoteID = "1",
                Title = "This is a book.",
                Content = "Oh yeah",
                UserID = 123321L
            };
            // collection.InsertOnSubmit(note);
            // collection.SubmitChanges();
            // var data = collection.SelectTo(n => new { n.NoteID, n.UserID });
            collection.Log = Console.Out;
            var a = 4;
            collection.Update(
                n => new Note { },
                n => true);
        }

        static void Main(string[] args)
        {
            Go(20L);

            Console.WriteLine("Finished");
            Console.ReadLine();
        }
    }
}
