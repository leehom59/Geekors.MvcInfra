using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;

namespace Geekors.MvcInfra.Helper
{
    public static class DataHelper
    {
        private static object GetUnderlyingTypeValue(Type enumType, object enumValue)
        {
            return Convert.ChangeType(enumValue, Enum.GetUnderlyingType(enumType));
        }

        public static IOrderedDictionary GetEnumNamesAndValues(Type enumType)
        {
            var orderedDictionary = new OrderedDictionary();
            foreach (
                var enumEntry in
                    Enum.GetValues(enumType).OfType<object>().Select(e => new EnumEntry
                    {
                        Name = Enum.GetName(enumType, e),
                        UnderlyingValue =
                            GetUnderlyingTypeValue(enumType, e)
                    }).OrderBy((e => e.UnderlyingValue)))
                orderedDictionary.Add(enumEntry.Name, enumEntry.UnderlyingValue.ToString());
            return orderedDictionary;
        }

        public static SelectList ToSelectList<TViewModel>(this IEnumerable<TViewModel> items, string displayField)
            where TViewModel : class
        {
            return new SelectList(items.ToList(), "Id", displayField);
        }

        private struct EnumEntry
        {
            public string Name { get; set; }
            public object UnderlyingValue { get; set; }
        }
    }
}