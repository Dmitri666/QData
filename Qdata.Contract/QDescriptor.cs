using System.Collections.Generic;

namespace Qdata.Contract
{
    public class QDescriptor
    {
        public QDescriptor()
        {
            this.Include = new List<QNode>();
        }
        public QNode Root { get; set; }

        public List<QNode> Include { get; set; }

    }
}
