using System;
using Qdata.Contract;

namespace QData.Client
{
    [Serializable]
    public class ProjectionRequest
    {
        public QNode SearchDescriptor { get; set; }

        public QNode ProjectionDescriptor { get; set; }
    }
}
