using System;
using Newtonsoft.Json;

namespace Qdata.Contract
{
    [Serializable]
   
    public class QNode
    {

        public NodeType Type { get; set; }

        public QNode Left { get; set; }

        public QNode Right { get; set; }

        [JsonConverter(typeof(EnumResolver))]
        public object Value { get; set; }
    }
}
