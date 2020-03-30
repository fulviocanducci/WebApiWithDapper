using System.Collections.Generic;

namespace WebApiWithDapper.Models
{
    public class People
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        public List<Phone> Phones { get; set; }
    }

    public class Phone
    {
        public int Id { get; set; }
        public int PeopleId { get; set; }
        public People People { get; set; }
        public string Ddd { get; set; }
        public string Number { get; set; }
    }

    public class Address
    {
        public int PeopleId { get; set; }
        public People People { get; set; }
        public string Street { get; set; }
        public string Number { get; set; }
    }
}
