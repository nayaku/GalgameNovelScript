using System.Collections.Generic;

namespace GalgameNovelScript
{
    public class CallStack
    {
        public Stack<ActivationRecord> Records { get; set; } = new Stack<ActivationRecord>();
        public void Push(ActivationRecord record)
        {
            Records.Push(record);
        }
        public ActivationRecord Pop()
        {
            return Records.Pop();
        }
        public ActivationRecord Peek()
        {
            return Records.Peek();
        }
        public override string ToString()
        {
            var lines = "";
            foreach (var item in Records)
            {
                lines += item.ToString() + "\n";
            }
            return lines;
        }
    }
}
