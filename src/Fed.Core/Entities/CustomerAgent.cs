using System;
using System.Text.RegularExpressions;

namespace Fed.Core.Entities
{
    public class CustomerAgent
    {
        public CustomerAgent(Guid id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }

        public CustomerAgent()
        {
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Initials { get { return Regex.Replace(Name, @"[^A-Z]", ""); } }
    }
}
