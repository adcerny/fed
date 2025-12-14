using Fed.Core.ValueTypes;
using System;

namespace Fed.Core.Entities
{
    public class Holiday
    {
        public Holiday(int id, string name, DateTime date, DateTime dateAdded)
        {
            Id = id;
            Name = name;
            Date = date.Date;
            DateAdded = dateAdded;
        }

        public int Id { get; }
        public string Name { get; set; }
        public Date Date { get; set; }
        public DateTime DateAdded { get; }
    }
}