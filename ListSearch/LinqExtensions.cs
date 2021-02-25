using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ListSearch
{
    public static class LinqExtensions
    {
        /// <summary>
        /// 判斷列舉項目為空或者有資料 回傳 True:有值 False :無資料或為null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool HasValues<T>(this IEnumerable<T> source)
        {
            return source != null && source.Count() > 0;
        }

        /// <summary>
        /// 判斷此型別是String或一般數值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsValueTypeOrString(this Type type)
        {
            if (type != null)
            {
                return type.IsValueType || type == typeof(string);
            }
            return false;
        }
        public static bool TryGetValue<T>(this IDictionary<object, object> dict, string key, out Nullable<T> model) where T : struct
        {
            var result = false;
            model = null;
            if (dict.TryGetValue(key, out object value))
            {
                model = (T)Convert.ChangeType(value, typeof(T));
                result = true;
            }
            return result;
        }
        private static MethodInfo _ContainsMethod;
        private static MethodInfo MethodContains
        {
            get
            {
                if (_ContainsMethod is null)
                {
                    _ContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                }
                return _ContainsMethod;
            }
        }
        private static MethodInfo _AnyMethod;
        private static MethodInfo MethodAny
        {
            get
            {
                if (_AnyMethod is null)
                {
                    _AnyMethod = typeof(Enumerable).GetMethods().Single(m => m.Name == "Any" && m.GetParameters().Length == 2);
                }
                return _AnyMethod;
            }
        }

        private static MethodInfo _ToLowerMethod;
        private static MethodInfo MethodToLower
        {
            get
            {
                if (_ToLowerMethod is null)
                {
                    _ToLowerMethod = typeof(string).GetMethod("ToLower", new Type[] { });
                }
                return _ToLowerMethod;
            }
        }

        private static MethodInfo _ToStringMethod;
        private static MethodInfo MethodToString
        {
            get
            {
                if (_ToStringMethod is null)
                {
                    _ToStringMethod = typeof(object).GetMethod("ToString");
                }
                return _ToStringMethod;
            }
        }

        /// <summary>
        /// member.Any((type)o=>lambda)
        /// </summary>
        /// <param name="member"></param>
        /// <param name="type"></param>
        /// <param name="lambda"></param>
        /// <returns></returns>
        public static Expression AnyEx(this MemberExpression member, Type type, LambdaExpression lambda)
        {
            if (member is null)
            {
                return null;
            }
            var method = MethodAny.MakeGenericMethod(type);
            var result = Expression.Call(method, member, lambda);
            return result;
        }

        /// <summary>
        /// member.ToString()
        /// </summary>
        /// <param name="type"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Expression ToStringEx(this Expression member, Type type)
        {
            if (member is null)
            {
                return null;
            }
            if (type != typeof(string))
            {
                member = Expression.Call(member, MethodToString);
            }
            return member;
        }
        /// <summary>
        /// member != null
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Expression IsNotNull(this MemberExpression member)
        {
            if (member is null)
            {
                return null;
            }
            var result = Expression.NotEqual(member, Expression.Constant(null));
            return result;
        }
        /// <summary>
        /// 將堆疊的判斷資料使用Or並在一起
        /// </summary>
        /// <param name="exs"></param>
        /// <returns></returns>
        public static Expression ComposeOrElse(this List<Expression> exs)
        {
            if (exs.Count == 0)
            {
                return null;
            }
            var expression = exs[0];
            for (var i = 1; i < exs.Count; i++)
            {
                expression = Expression.OrElse(expression, exs[i]);
            }
            return expression;
        }


        public static Expression ToLowerEx(this Expression member)
        {
            return Expression.Call(member, MethodToLower);
        }

        /// <summary>
        /// Search 轉換
        /// </summary>
        /// <param name="searchs"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        public static Expression Contains(this Expression member, List<string> searchs)
        {
            if (member is null)
            {
                return null;
            }
            var ans = searchs.Select(qt =>
                Expression.Call(member, MethodContains, Expression.Constant(qt, typeof(string)))
            ).ToList<Expression>();
            var result = ans.ComposeOrElse();
            return result;
        }

    }
}