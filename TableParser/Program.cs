using System;
using System.Collections.Generic;
using System.Threading;

namespace TableParser
{
    public class Program
    {
        public static void Main()
        {
            //var renderer = new TableRenderer<User>(new[] { "User Id", "Name", "Birthdate", "Location" }, user => user.Id, user => user.Name, user => user.DateOfBirth, user => string.Format("{0:00.00} : {1:00.00}", user.Location.Lat, user.Location.Lon));
            var renderer = new TableRenderer<User>();

            var rnd = new Random();

            while (true)
            {
                var users = new List<User>();
                var max = rnd.Next(5, 10);
                for (var x = 0; x <= max; x++)
                {
                    var user =
                        new User
                        {
                            Id = Guid.NewGuid(),
                            DateOfBirth = DateTimeOffset.Now.AddDays(rnd.Next(-6000, -3000)),
                            Location = new Location
                            {
                                Lat = rnd.Next(0, 90),
                                Lon = rnd.Next(-180, 180)
                            },
                            Name = "Test User"
                        };
                    users.Add(user);
                }
                renderer.Content = users;
                Thread.Sleep(1000);
            }
        }
    }

    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public Location Location { get; set; }
    }

    public class Location
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
}