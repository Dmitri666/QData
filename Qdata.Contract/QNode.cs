using System;

namespace Qdata.Contract
{
    [Serializable]
   
    public class QNode
    {

        public NodeType Type { get; set; }

        public QNode Operand { get; set; }

        public QNode Argument { get; set; }

        public object Value { get; set; }
    }
}
