using System;

namespace GalgameNovelScript
{
    public class NodeVisitor
    {
        public virtual object Visit(AST node)
        {
            var method = GetType().GetMethod("Visit" + node.GetType().Name);
            if (method != null)
            {
                return method.Invoke(this, new object[] { node });
            }
            else
            {
                throw new Exception("No visit" + node.GetType().Name + " method defined");
            }
        }

    }
}
