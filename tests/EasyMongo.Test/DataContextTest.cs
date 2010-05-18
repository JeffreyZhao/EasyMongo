using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using MongoDB.Driver;
using Moq;
using EasyMongo.Mapping;

namespace EasyMongo.Test
{
    public class DataContextTest
    {
        public class User
        {
            public int UserID { get; set; }
            public string Name {get;set;}
            public bool IsZhao { get { return this.Name.StartsWith("Zhao"); } }
        }

        public class UserMap : EntityMap<User>
        {
            public UserMap()
            {
                Collection("Users");

                Property(u => u.UserID).Identity();
                Property(u => u.Name).DefaultValue("Anonymous");
                Property(u => u.IsZhao).Name("Zhao").ChangeWith(u => u.Name);
            }
        }

        public static MappingSource GetMappingSource()
        {
            return new MappingSource(
                new[] { new UserMap() }.Cast<IEntityMap>().Select(m => m.ToDescriptor()));
        }

        [Fact]
        public void AddSingleEntity()
        {
            var user = new User
            {
                UserID = 1,
                Name = "Zhao Jie"
            };

            var mockColl = new Mock<IMongoCollection>();
            mockColl
                .Setup(c => c.Insert(It.Is<IEnumerable<Document>>(docs => docs != null)))
                .Callback<IEnumerable<Document>>(documents =>
                {
                    var d = documents.Single();
                    Assert.Equal(3, d.Count);
                    Assert.Equal(user.UserID, d["_id"]);
                    Assert.Equal(user.Name, d["Name"]);
                    Assert.Equal(user.IsZhao, d["Zhao"]);
                });

            var mockDatabase = new Mock<IMongoDatabase>();
            mockDatabase.Setup(d => d.Open()).Verifiable();
            mockDatabase.Setup(d => d["Users"]).Returns(mockColl.Object);

            var db = new DataContext(mockDatabase.Object, GetMappingSource(), true);
            db.InsertOnSubmit(user);
            db.SubmitChanges();

            mockDatabase.Verify();
        }
    }
}
