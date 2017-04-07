using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QData.Querable.DataService
{
    [Serializable]
    public class Page
    {
        public object Data { get; set; }

        public int Total { get; set; }
    }
}
