namespace QData.MongoDb.Test
{
    using System.Collections.Generic;

    public class Customer
    {
        public Customer()
        {
            this.Childs = new List<Contract>();
        }
        public int Id { get; set; }
        public string Vorname { get; set; }

        public string Nachname { get; set; }

        public int Age { get; set; }

        public IEnumerable<Contract> Childs { get; set; }
    }
}
