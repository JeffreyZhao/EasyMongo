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

            //var note = new Note
            //{
            //    NoteID = "1",
            //    Title = "This is a book.",
            //    Content = "Oh yeah",
            //};
            //collection.InsertOnSubmit(note);

            collection.Log = Console.Out;
            var note = collection.Where(n => n.UserID == userId).ToList().FirstOrDefault();
            note.Title = "This is a book!";

            try
            {
                collection.SubmitChanges();
            }
            catch (ChangeConflictException<Note> ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        static void Main(string[] args)
        {
            Go(20L);

            Console.WriteLine("Finished");
            Console.ReadLine();
        }
    }
}
