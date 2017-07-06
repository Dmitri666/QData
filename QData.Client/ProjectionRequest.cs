using System;
using Qdata.Contract;

namespace QData.Client
{
    [Serializable]
    public class ProjectionRequest
    {
        public QDescriptor SearchDescriptor { get; set; }

        public QDescriptor ProjectionDescriptor { get; set; }
    }
}
