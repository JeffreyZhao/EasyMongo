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
            //Property(n => n.Abstract).DefaultValue("");
            //Property(n => n.UserID);
            //Property(n => n.CategoryID);
            //Property(n => n.Tags);
            //Property(n => n.CreateTime).Processor<LocalTimeProcessor>();
            //Property(n => n.UpdateTime).Processor<LocalTimeProcessor>();
            //Property(n => n.ContentTypes);
            //Property(n => n.CommentCount);
            //Property(n => n.SaveAsCount);
            //Property(n => n.SearchText).ChangeWith(n => n.Title).ChangeWith(n => n.Content);
            //Property(n => n.HasAttachments);
            //Property(n => n.SourceUrl).DefaultValue("");
            //Property(n => n.Encrypted).DefaultValue(false);
            //Property(n => n.PasswordReminder).DefaultValue("");
            //Property(n => n.Password).DefaultValue("");
            //Property(n => n.ResourceInlineStatus).DefaultValue(NoteResourceInlineStatus.Original);
            Property(n => n.Version).Version();
        }
    }
}
