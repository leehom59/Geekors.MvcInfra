using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using ModelHelper.Infrastructure;

namespace ModelHelper.Helper
{
    public static class ViewModelHelper
    {
        public static Expression<Func<T, object>>[] GetKeys<T>()
        {
            var modelKeys = new List<Expression<Func<T, object>>>();
            var properties = typeof (T).GetProperties();
            var parameter = Expression.Parameter(typeof (T));
            foreach (var propertyInfo in properties)
            {
                var attribute = Attribute.GetCustomAttribute(propertyInfo, typeof (KeyAttribute));

                if (attribute != null) // This property has a KeyAttribute
                {
                    Expression expr = Expression.Property(parameter, propertyInfo);
                    if (propertyInfo.PropertyType.IsValueType)
                        expr = Expression.Convert(expr, typeof (object));
                    var lambda = Expression.Lambda<Func<T, object>>(expr,
                        parameter);
                    modelKeys.Add(lambda);
                }
            }
            return modelKeys.ToArray();
        }

        public static Expression<Func<T, bool>> GetEqualKeyFunc<T>(object value)
        {
            var properties = typeof (T).GetProperties();
            var parameter = Expression.Parameter(typeof (T));
            foreach (var propertyInfo in properties)
            {
                var attribute = Attribute.GetCustomAttribute(propertyInfo, typeof (KeyAttribute));

                if (attribute != null) // This property has a KeyAttribute
                {
                    //x.ColumnName
                    var left = Expression.Property(parameter, propertyInfo);
                    //value (Constant Value)
                    var right = propertyInfo.PropertyType == typeof(string) && propertyInfo.PropertyType != value.GetType()
                        ? Expression.Constant(value.ToString())
                        : Expression.Constant(value);
                    //x.ColumnName == value
                    var filter = Expression.Equal(left, right);
                    //x => x.ColumnName == value
                    return Expression.Lambda<Func<T, bool>>(filter, parameter);
                }
            }
            throw new Exception("沒有指定 Key Attribute!");
        }

        public static object GetKeyValue<T>(object obj)
        {
            var properties = typeof (T).GetProperties();
            foreach (var propertyInfo in properties)
            {
                var attribute = Attribute.GetCustomAttribute(propertyInfo, typeof (KeyAttribute));

                if (attribute != null) // This property has a KeyAttribute
                {
                    var keyValue = propertyInfo.GetValue(obj);
                    return keyValue;
                }
            }
            throw new Exception("沒有指定 Key Attribute!");
        }
        public static void SetForeignKeyDataByEnum<T>(this ControllerBase controller, string fieldName,
            params T[] excludeEnumFields)
        {
            Array enumValues = typeof(T).GetEnumValues();
            IList<ForeignKeyItem> foreignKeyList = new List<ForeignKeyItem>();
            Type valueType = Enum.GetUnderlyingType(typeof(T));
            IEnumerable<string> excludeEnumName = new List<string>();
            if (excludeEnumFields != null)
                excludeEnumName = excludeEnumFields.Select(o => typeof(T).GetEnumName(o));
            foreach (var userType in enumValues)
            {
                int value;
                if (valueType == typeof(byte))
                    value = (byte)userType;
                else if (valueType == typeof(short))
                    value = (short)userType;
                else
                    value = (int)userType;
                string enumFieldName = typeof(T).GetEnumName(userType);
                if (excludeEnumName.Contains(enumFieldName))
                    continue;
                var fi = typeof(T).GetField(enumFieldName, BindingFlags.Public | BindingFlags.Static);
                if (fi != null)
                {
                    var displayAttribute =
                        (DisplayAttribute)fi.GetCustomAttributes(typeof(DisplayAttribute), false).SingleOrDefault();
                    if (displayAttribute != null)
                        enumFieldName = displayAttribute.GetName();
                }
                if (!string.IsNullOrEmpty(enumFieldName))
                    foreignKeyList.Add(new ForeignKeyItem(value.ToString(), enumFieldName));
            }
            controller.ViewData[string.Format("{0}_Data", fieldName)] = new SelectList(foreignKeyList, "Value", "Text");
        }

        public static void SetForeignKeyData<TViewModel>(this ControllerBase controller, string fieldName,
                                                         IEnumerable<TViewModel> data, string displayField = null, string keyField = "Id")
            where TViewModel : class
        {
            if (displayField == null)
                displayField =
                    ((DisplayColumnAttribute)
                     typeof(TViewModel).GetCustomAttributes(typeof(DisplayColumnAttribute), false).First())
                        .DisplayColumn;
            controller.ViewData[string.Format("{0}_Data", fieldName)] = data.ToSelectList(displayField, keyField);
        }

        public static SelectList GetForeignKeyData(this HtmlHelper htmlHelper, string fieldName)
        {
            return (SelectList)htmlHelper.ViewData[string.Format("{0}_Data", fieldName)];
        }

    }
}