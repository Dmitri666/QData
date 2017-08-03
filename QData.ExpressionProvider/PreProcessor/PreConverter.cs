using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QData.ExpressionProvider.PreProcessor
{
    using Qdata.Contract;

    public class PreConverter
    {
        public QNode Prepare(QNode node)
        {
            var convertedNode = new MemberNodeExpander().ConvertNode(node);
            convertedNode = new QueryStringPreConverter().ConvertNode(convertedNode);
            return convertedNode;
        }

         
    }
}
