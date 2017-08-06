using System;

namespace Qdata.Contract
{
    public class EnumResolver 
    {
        public static NodeType ResolveNodeType(object value)
        {
            NodeType nodeType;
            if (value is long)
            {
                nodeType = (NodeType)Convert.ToInt16(value);
            }
            else
            {
                Enum.TryParse(Convert.ToString(value), out nodeType);
            }

            return nodeType;
        }

        public static NodeGroup ResolveNodeGroup(NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.And:
                case NodeType.Or:
                case NodeType.Equal:
                case NodeType.GreaterThan:
                case NodeType.GreaterThanOrEqual:
                case NodeType.LessThan:
                case NodeType.LessThanOrEqual:
                case NodeType.NotEqual:
                    return NodeGroup.Binary;
                case NodeType.Where:
                case NodeType.Select:
                case NodeType.Any:
                case NodeType.Count:
                case NodeType.OrderBy:
                case NodeType.OrderByDescending:
                case NodeType.ToString:
                case NodeType.QueryString:
                case NodeType.Contains:
                case NodeType.In:
                case NodeType.NotIn:
                case NodeType.StartsWith:
                case NodeType.EndsWith:
                case NodeType.Take:
                case NodeType.Skip:
                    return NodeGroup.Method;
                case NodeType.Constant:
                    return NodeGroup.Constant;
                case NodeType.Querable:
                    return NodeGroup.Querable;
                case NodeType.Member:
                    return NodeGroup.Member;
                default:
                    throw new NotImplementedException(nodeType.ToString());

            }
        }


    }
}
