using System.Collections.Generic;

namespace Qdata.Json.Contract
{
    using QData.Common;
    public class QDescriptor<TM> where TM : IModelEntity
    {
        public QDescriptor()
        {
            this.Include = new List<QNode>();
        }
        public QNode Root { get; set; }

        public List<QNode> Include { get; set; }

    }
}
