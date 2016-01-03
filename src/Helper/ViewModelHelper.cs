using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Geekors.MvcInfra.Helper
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
    }
}