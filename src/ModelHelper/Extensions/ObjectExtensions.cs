using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelHelper.Extensions
{
    public static class ObjectExtensions
    {
        public static IDictionary<string, object> ToDictionary(this object o)
        {
            return o.GetType().GetProperties()
                .Select(n => n.Name)
                .ToDictionary(k => k, k => o.GetType().GetProperty(k).GetValue(o, null));
        }

        public static string ToUnsortedList(this object data)
        {
            if (data is IEnumerable)
            {
                var ul = new StringBuilder("<ul>\n");
                var hasItems = false;
                foreach (var m in (IEnumerable) data)
                {
                    hasItems = true;
                    ul.Append("<li>").Append(m).AppendLine("</li>");
                }
                if (!hasItems)
                    return string.Empty;
                return ul.AppendLine("</ul>").ToString();
            }
            return string.Empty;
        }
    }
}