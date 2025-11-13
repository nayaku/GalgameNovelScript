using System.Collections.Generic;

namespace GalgameNovelScript
{
    public class ActivationRecord
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public Dictionary<string, object> Members { get; set; } = new Dictionary<string, object>();
        // 作用域链
        public ActivationRecord EnclosingScope { get; set; }
        public ActivationRecord(string name, int level, ActivationRecord? enclosingScope = null)
        {
            Name = name;
            Level = level;
            EnclosingScope = enclosingScope;
        }
        public void AddMember(string name, object value)
        {
            Members[name] = value;
        }
        public object? GetMember(string name)
        {
            if (Members.ContainsKey(name))
                return Members[name];
            else if (EnclosingScope != null)
                return EnclosingScope.GetMember(name);
            else
                return null;
        }
        public override string ToString()
        {
            var lines = $"Name:{Name},Level:{Level}";
            if (Members.Count > 0)
            {
                lines += "\nMembers:";
                foreach (var item in Members)
                {
                    lines += $"\n\t{item.Key}:{item.Value}";
                }
            }
            return lines;
        }
    }
}
