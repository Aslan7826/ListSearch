using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ListSearch
{
    public class ListService
    {
        /// <summary>
        /// 將列表分頁
        /// </summary>
        /// <param name="page"></param>
        public IQueryable<T> ToPage<T>(IQueryable<T> source, int pageIndex, int pageSize)
        {
            return source.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
        /// <summary>
        /// 列表排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">來源列表</param>
        /// <param name="orderName">要排序的欄位</param>
        /// <param name="isDesc">順序 true: 降序 false:升序</param>
        /// <returns></returns>
        public IQueryable<T> ToOrder<T>(IQueryable<T> source, string orderName, bool isDesc) where T : class, new()
        {
            if (source != null)
            {
                Type entityType = typeof(T);
                var thisField = entityType.GetProperty(orderName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (thisField is null || (thisField.PropertyType.IsGenericType && thisField.PropertyType.GetGenericTypeDefinition() == (typeof(IEnumerable<>))))
                {
                    return source;
                }
                //針對欄位不管傳入大小寫，
                var param = Expression.Parameter(entityType, "o");
                var member = Expression.Property(param, orderName);
                var lambda = Expression.Lambda<Func<T, object>>(Expression.Convert(member, typeof(object)), param);
                //因為是泛型Mehtod要呼叫MakeGenericMethod決定泛型型別
                source = isDesc ? source.OrderByDescending(lambda)
                                : source.OrderBy(lambda);
            }
            return source;
        }
        /// <summary>
        /// 列表文字搜尋
        /// </summary>
        /// <param name="searchStringInfo">搜尋文字，使用空白做區隔</param>
        /// <param name="noSearchProperty">要忽略的屬性</param>
        /// <param name="recursiveMax">最大遞迴次數</param>
        /// <returns></returns>
        public IQueryable<T> SearchString<T>(IQueryable<T> source, string searchString, string keywordSeparator = " ", List<string> noSearchProperty = null, int recursiveMax = 1) where T : class, new()
        {
            if (source != null && !string.IsNullOrEmpty(searchString))
            {
                var sql = source.Expression.ToString();
                Regex regex = new Regex("^[A-Za-z]+$");
                var seachFile = sql.Split(',', '=', ' ').Distinct().Where(o => regex.IsMatch(o)).ToList();
                var searchs = searchString.ToLower().Split(keywordSeparator).Distinct().ToList();
                var expression = GetSelectExpression(typeof(T), seachFile, searchs, noSearchProperty, recursiveMax);
                if (expression != null)
                {
                    source = source.Where(expression as Expression<Func<T, bool>>);
                }
            }
            return source;
        }

        /// <summary>
        /// 遞迴反射查詢
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="searchs"></param>
        /// <param name="noSearchProperty"></param>
        /// <param name="recursiveMax">最大遞迴次數</param>
        /// <param name="recursiveN">程式內判斷遞迴圈數</param>
        /// <returns></returns>
        private Expression GetSelectExpression(Type entityType, List<string> hasProperty, List<string> searchs, List<string> noSearchProperty = null, int recursiveMax = 1, int recursiveN = 0)
        {
            var needFindField = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance).AsEnumerable();
            if (hasProperty.HasValues())
            {
                needFindField = needFindField.Where(o => hasProperty.Contains(o.Name));
            }
            if (noSearchProperty != null)
            {
                needFindField = needFindField.Where(o => !noSearchProperty.Contains(o.Name));
            }

            List<Expression> AllExpressionCalls = new List<Expression>();
            var entity = Expression.Parameter(entityType, $"o{recursiveN}");
            foreach (var property in needFindField)
            {
                Expression expression = null;
                var member = Expression.Property(entity, property);
                var propertyType = property.PropertyType;
                var genericType = propertyType.GenericTypeArguments;
                if (propertyType.IsValueTypeOrString())//數值型
                {
                    expression = member.ToStringEx(propertyType).ToLowerEx().Contains(searchs);
                }
                else if (genericType.Count() == 0 && !propertyType.IsArray) //class
                {
                    List<Expression> classExpression = new List<Expression>();
                    //todo 遞迴 body
                    foreach (var proprty in propertyType.GetProperties())
                    {
                        var classMember = Expression.Property(member, proprty);
                        if (property.PropertyType.IsValueTypeOrString())
                        {
                            var classex = classMember.ToStringEx(property.PropertyType).ToLowerEx().Contains(searchs);
                            classExpression.Add(classex);
                        }
                    }
                    if (classExpression.Count > 0)
                    {
                        expression = classExpression.ComposeOrElse();
                    }

                }
                else if (recursiveMax > recursiveN) //enum
                {
                    Expression lambda = null;
                    //列表型
                    var enumOfType = genericType.FirstOrDefault() ?? propertyType.GetElementType();
                    if (enumOfType.IsValueTypeOrString()) //數值型
                    {
                        var theEntity = Expression.Parameter(enumOfType, $"o{recursiveN + 1}");
                        var ans = theEntity.ToStringEx(enumOfType).Contains(searchs);
                        lambda = Expression.Lambda(ans, theEntity);
                    }
                    else     //class型 做遞迴
                    {
                        lambda = GetSelectExpression(enumOfType, hasProperty, searchs, noSearchProperty, recursiveMax, recursiveN + 1);
                    }
                    expression = member.AnyEx(enumOfType, (LambdaExpression)lambda);

                }
                if (expression != null)
                {
                    if (!(propertyType.IsValueType && genericType.Length == 0)) //非純數值
                    {
                        expression = Expression.AndAlso(member.IsNotNull(), expression);
                    }
                    AllExpressionCalls.Add(expression);
                }
            }
            if (AllExpressionCalls.Count > 0)
            {
                var ans = AllExpressionCalls.ComposeOrElse();
                return Expression.Lambda(ans, entity);
            }
            return null;
        }
    }
}
