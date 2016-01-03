using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using EmitMapper;
using EmitMapper.MappingConfiguration;
using ModelHelper.Helper;
using ModelHelper.Interface;

namespace ModelHelper.Extensions
{
    public static class DataTransferExtension
    {
        /// <summary>
        /// 將來源集合中的物件所有屬性值填入目的物件中，傳回一新目的物件集合
        /// </summary>
        /// <typeparam name="TSource">來源物件類別</typeparam>
        /// <typeparam name="TTarget">目的物件類別</typeparam>
        /// <param name="sources">來源物件集合</param>
        /// <param name="ignoreNames">略過填入目的物件屬性名列表</param>
        /// <returns></returns>
        public static IEnumerable<TTarget> Inject<TSource, TTarget>(this IEnumerable<TSource> sources,
            params string[] ignoreNames)
            where TTarget : new()
        {
            var mapConfig = new DefaultMapConfig().ShallowMap().IgnoreMembers<TSource, TTarget>(ignoreNames);
            var mapper = ObjectMapperManager.DefaultInstance.GetMapper<TSource, TTarget>(mapConfig);
            return sources.Select(mapper.Map);
        }
        /// <summary>
        /// 將來源物件所有屬性值填入目的物件中，傳回一新目的物件
        /// </summary>
        /// <typeparam name="TSource">來源物件類別</typeparam>
        /// <typeparam name="TTarget">目的物件類別</typeparam>
        /// <param name="model">來源物件</param>
        /// <param name="ignoreNames">略過填入目的物件屬性名列表</param>
        /// <returns></returns>
        public static TTarget Inject<TSource, TTarget>(this TSource model,
            params string[] ignoreNames)
            where TTarget : new()
        {
            var target = new TTarget();
            model.Inject(target, ignoreNames);
            return target;
        }

        /// <summary>
        /// 將來源物件所有屬性值填入指定目的物件中
        /// </summary>
        /// <typeparam name="TSource">來源物件類別</typeparam>
        /// <typeparam name="TTarget">目的物件類別</typeparam>
        /// <param name="source">來源物件</param>
        /// <param name="target">目的物件</param>
        /// <param name="ignoreNames">略過填入目的物件屬性名列表</param>
        public static void Inject<TSource, TTarget>(this TSource source, TTarget target,
            params string[] ignoreNames)
        {
            source.Inject(null, null, target, ignoreNames);
        }

        /// <summary>
        /// 將來源物件所有屬性值填入指定目的物件中
        /// </summary>
        /// <typeparam name="TSource">來源物件類別</typeparam>
        /// <typeparam name="TTarget">目的物件類別</typeparam>
        /// <param name="source">來源物件</param>
        /// <param name="sourcePrefix">來源屬性前綴字</param>
        /// <param name="targetPrefix">目的屬性前綴字</param>
        /// <param name="target">目的物件</param>
        /// <param name="ignoreNames">略過填入目的物件屬性名列表</param>
        public static void Inject<TSource, TTarget>(this TSource source, string sourcePrefix, string targetPrefix,
            TTarget target,
            params string[] ignoreNames)
        {
            var model = target as EntityObject;
            if (model != null && ignoreNames.Length == 0)
            {
                ignoreNames = model.EntityKey.EntityKeyValues.Select(v => v.Key).ToArray();
            }
            var mapConfig = new DefaultMapConfig();
            if (ignoreNames.Length > 0)
                mapConfig.IgnoreMembers<TSource, TTarget>(ignoreNames);
            if (sourcePrefix != null || targetPrefix != null)
            {
                if (sourcePrefix == null)
                    sourcePrefix = "";
                if (targetPrefix == null)
                    targetPrefix = "";
                mapConfig.MatchMembers((m1, m2) => targetPrefix + m1 == sourcePrefix + m2);
            }
            var mapper = ObjectMapperManager.DefaultInstance.GetMapper<TSource, TTarget>(mapConfig);
            mapper.Map(source, target);
        }

#region 進階功能
        /// <summary>
        /// 將來源集合中的物件所有屬性值填入目的物件中，傳回一新目的物件集合
        /// </summary>
        /// <typeparam name="TSource">來源物件類別</typeparam>
        /// <typeparam name="TTarget">目的物件類別</typeparam>
        /// <param name="sources">來源物件集合</param>
        /// <param name="customConvert"></param>
        /// <returns></returns>
        public static IList<TTarget> Inject<TSource, TTarget>(this IEnumerable<TSource> sources,
            Action<TSource, TTarget> customConvert)
            where TTarget : new()
        {
            return sources.Select(source => source.Inject(customConvert)).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="customConvert"></param>
        /// <returns></returns>
        public static TTarget Inject<TSource, TTarget>(this TSource source, Action<TSource, TTarget> customConvert)
            where TTarget : new()
        {
            var target = source.Inject<TSource, TTarget>();
            customConvert(source, target);
            return target;
        }

        public static TEntity Sync2Entity<TRepository, TViewModel, TEntity>(this TViewModel viewModel,
            string viewModelPrefix,
            params string[] ignoreNames)
            where TViewModel : class
            where TEntity : class
            where TRepository : IRepository<TEntity>, new()
        {
            var repository = new TRepository();
            return viewModel.Sync2Entity(viewModelPrefix, repository, ignoreNames);
        }

        public static TEntity Sync2Entity<TViewModel, TEntity>(this TViewModel viewModel, string viewModelPrefix,
            IRepository<TEntity> repository,
            params string[] ignoreNames)
            where TViewModel : class
            where TEntity : class
        {
            var expression = repository.GetEqualKeyFunc(ViewModelHelper.GetKeyValue<TViewModel>(viewModel));
            var target = repository.Get(expression);
            var properties = typeof (TViewModel).GetProperties();
            var ignoreNameList = new List<string>(ignoreNames);
            foreach (var propertyInfo in properties)
            {
                var attribute = Attribute.GetCustomAttribute(propertyInfo, typeof (EditableAttribute)) ??
                                Attribute.GetCustomAttribute(propertyInfo, typeof(ScaffoldColumnAttribute));
                if(attribute != null)
                    ignoreNameList.Add(propertyInfo.Name);
            }
            viewModel.Inject(viewModelPrefix, "", target, ignoreNameList.ToArray());
            return target;
        }

        public static TEntity Sync2Entity<TRepository, TViewModel, TEntity>(this TViewModel viewModel,
            params string[] ignoreNames)
            where TViewModel : class
            where TEntity : class
            where TRepository : IRepository<TEntity>, new()
        {
            var repository = new TRepository();
            return viewModel.Sync2Entity("", repository, ignoreNames);
        }

        public static TEntity Sync2Entity<TViewModel, TEntity>(this TViewModel viewModel,
            IRepository<TEntity> repository,
            params string[] ignoreNames)
            where TViewModel : class
            where TEntity : class
        {
            return viewModel.Sync2Entity("", repository, ignoreNames);
        }

        public static TEntity Update2Entity<TViewModel, TEntity>(this TViewModel viewModel,
            IRepository<TEntity> repository,
            params string[] ignoreNames)
            where TViewModel : class
            where TEntity : class
        {
            var entity = viewModel.Sync2Entity("", repository, ignoreNames);
            repository.Update(entity);
            return entity;
        }
#endregion
    }
}