using Zu.TypeScript.TsTypes;

namespace Zu.TypeScript.TsParser
{
    public static class Factory
    {
        public static INode SkipPartiallyEmittedExpressions(INode node)
        {
            while (node is { Kind: SyntaxKind.PartiallyEmittedExpression })
            {
                node = ((PartiallyEmittedExpression)node).Expression;
            }

            return node;
        }
    }
}