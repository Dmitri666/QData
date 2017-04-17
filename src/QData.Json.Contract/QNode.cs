using QData.Common;

namespace Qdata.Json.Contract
{
    
    public class QNode
    {
        
        public NodeType Type { get; set; }
        
        public QNode Left { get; set; }
        
        public QNode Right { get; set; }
        
        public object Value { get; set; }
    }
}
