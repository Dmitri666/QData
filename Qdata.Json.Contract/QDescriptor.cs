﻿using System.Collections.Generic;

namespace Qdata.Json.Contract
{
    using QData.Common;
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
