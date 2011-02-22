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
        static void Main(string[] args)
        {
            var settings = new MongoServerSettings
            {
                Server = new MongoServerAddress("127.0.0.1", 27017)
            };
            var mongoServer = new MongoServer(settings);
            // mongoServer.Connect();
            
            var database = mongoServer.GetDatabase("Partition_1");

            var map = new NoteMap();
            var collection = new EntityCollection<Note>(database, map.GetDescriptor(), true);

            Console.WriteLine(collection.Count(n => n.ContentTypes.Contains(NoteContentTypes.Image)));

            //var note = collection.Get(n => n.Types.Contains(NoteTypes.Image));
            //note.Title = "是啊是啊";
            //note.Types = NoteTypes.Image;
            //note.Priority = NotePriority.Normal;
            //note.Tags.Add("World");
            //collection.SubmitChanges();

            //collection.DeleteOnSubmit(note);
            //collection.SubmitChanges();

            Console.WriteLine("Finished");
            Console.ReadLine();
        }
    }
}
