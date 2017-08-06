using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDate.ExpressionProvider.Test
{
    public class PersonDto
    {
        public PersonDto()
        {
            this.Mitarbeiters = new List<PersonDto>();
        }
        public string Vorname { get; set; }

        public string Nachname { get; set; }

        public List<PersonDto> Mitarbeiters { get; set; }

        public PersonDto Leiter { get; set; }
    }
}
