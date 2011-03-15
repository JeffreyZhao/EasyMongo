using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyMongo.Mapping;

namespace SimpleUsages.Mapping
{
    public class NoteMap : EntityMap<Note>
    {
        public NoteMap()
        {
            Collection("Notes");

            Property(n => n.NoteID).Identity();
            Property(n => n.Title);
            Property(n => n.Content).DefaultValue("");
            Property(n => n.UserID);
        }
    }
}
