// OSPC - Open Software Plagiarism Checker
// Copyright(C) 2015 Arthur Zaczek at the UAS Technikum Wien


// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OSPC
{
    public class TupleList<T1, T2> : List<Tuple<T1, T2>>
    {
        public void Add(T1 item, T2 item2)
        {
            Add(new Tuple<T1, T2>(item, item2));
        }
    }

    public static class Extensions
    {
        /// <summary>
        /// Returns the string with at most "maxLength" characters. If the length is exceeded, suffix is added.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <param name="suffix">Suffix to add when string length exceedes maxLength</param>
        /// <param name="trimStart">cut off first chars</param>
        /// <returns></returns>
        public static string MaxLength(this string str, int maxLength, string suffix = "", bool trimStart = false)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > maxLength)
            {
                if (trimStart)
                    return (suffix ?? "") + str.Substring(str.Length - maxLength, maxLength);
                else
                    return str.Substring(0, maxLength) + suffix;
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// http://code.logos.com/blog/2008/01/nullpropagating_extension_meth.html
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="t"></param>
        /// <param name="fn"></param>
        /// <returns></returns>
        public static U IfNotNull<T, U>(this T t, Func<T, U> fn)
        {
            if (fn == null) throw new ArgumentNullException("fn");
            return t != null ? fn(t) : default(U);
        }

        public static string IfNullOrEmpty(this string str, string def)
        {
            return string.IsNullOrEmpty(str) ? def : str;
        }

        public static string IfNullOrWhiteSpace(this string str, string def)
        {
            return string.IsNullOrWhiteSpace(str) ? def : str;
        }

        public static int MaxIndex<T>(this IEnumerable<T> sequence)
            where T : IComparable<T>
        {
            int maxIndex = -1;
            T maxValue = default(T);

            int index = 0;
            foreach (T value in sequence)
            {
                if (value.CompareTo(maxValue) > 0 || maxIndex == -1)
                {
                    maxIndex = index;
                    maxValue = value;
                }
                index++;
            }
            return maxIndex;
        }

        public static double SlidingAvg(this double[] lst, int idx, int length = 5)
        {
            var sum = 0.0;
            for (int i = idx; i < idx + length && i < lst.Length; i++)
            {
                sum += lst[i];
            }
            return sum / length;
        }

        public static double[] CalcDerv2(this double[] lst)
        {
            if (lst.Length <= 2) return new double[] { };
            var derv_2 = new double[lst.Length - 2];
            for (int i = 0; i < lst.Length - 2; i++)
            {
                derv_2[i] = SlidingAvg(lst, i) - 2 * SlidingAvg(lst, i + 1) + SlidingAvg(lst, i + 2);
            }

            return derv_2;
        }

        /// <summary>
        /// Converts a object to XML.
        /// </summary>
        /// <param name="obj">Any XML Serializable Object.</param>
        /// <param name="s">Output stream</param>
        public static void ToXmlStream(this object obj, Stream s)
        {
            if (obj == null) { throw new ArgumentNullException("obj"); }

            XmlSerializer xml = new XmlSerializer(obj.GetType());
            var sw = new StreamWriter(s);
            xml.Serialize(sw, obj);
            sw.Flush();
        }

        /// <summary>
        /// Converts a XML byte array to a Objekt.
        /// </summary>
        /// <typeparam name="T">Type of the Object.</typeparam>
        /// <param name="stream">Input stream.</param>
        /// <returns>Returns a Object or throws an XML-Exception (see MSDN, XmlSerializer)</returns>
        public static T FromXmlStream<T>(this System.IO.Stream stream)
            where T : new()
        {
            if (stream == null) throw new ArgumentNullException("stream");

            using (var sr = new StreamReader(stream))
            {
                var xml = new XmlSerializer(typeof(T));
                return (T)xml.Deserialize(sr);
            }
        }

        /// <summary>
        /// Foreach Extension Method for IEnumerable.
        /// </summary>
        /// <typeparam name="T">Type of the Objects in the Enumeration.</typeparam>
        /// <param name="lst">Enumeration</param>
        /// <param name="action">Action to perform on each element.</param>
        public static void ForEach<T>(this IEnumerable lst, Action<T> action)
        {
            if (lst == null) { throw new ArgumentNullException("lst"); }
            if (action == null) { throw new ArgumentNullException("action"); }

            foreach (T obj in lst)
            {
                action(obj);
            }
        }

        /// <summary>
        /// Foreach Extension Method for IEnumerable. This Extension does not check if the Enumeration Entry is NULL!
        /// </summary>
        /// <typeparam name="T">Type of the Objects in the Enumeration.</typeparam>
        /// <param name="lst">Enumeration</param>
        /// <param name="action">Action to perform on each element.</param>
        public static void ForEach<T>(this IEnumerable<T> lst, Action<T> action)
        {
            if (lst == null) { throw new ArgumentNullException("lst"); }
            if (action == null) { throw new ArgumentNullException("action"); }

            foreach (T obj in lst)
            {
                action(obj);
            }
        }

        /// <summary>
        /// Foreach Extension Method for IList&lt;>. This Extension does not check if the Enumeration Entry is NULL!
        /// </summary>
        /// <typeparam name="T">Type of the Objects in the Enumeration.</typeparam>
        /// <param name="lst">Enumeration</param>
        /// <param name="action">Action to perform on each element.</param>
        public static void ForEach<T>(this IList<T> lst, Action<T> action)
        {
            if (lst == null) { throw new ArgumentNullException("lst"); }
            if (action == null) { throw new ArgumentNullException("action"); }

            foreach (T obj in lst)
            {
                action(obj);
            }
        }
        /// <summary>
        /// Foreach Extension Method for IQueryable&lt;>. This Extension does not check if the query results contain NULLs!
        /// </summary>
        /// <typeparam name="T">Type of the Objects in the IQueryable.</typeparam>
        /// <param name="lst">IQueryable</param>
        /// <param name="action">Action to perform on each element.</param>
        public static void ForEach<T>(this IQueryable<T> lst, Action<T> action)
        {
            if (lst == null) { throw new ArgumentNullException("lst"); }
            if (action == null) { throw new ArgumentNullException("action"); }

            foreach (T i in lst)
            {
                action(i);
            }
        }

        public static void Add(this ICollection col, object val, bool unique)
        {
            if (col == null) throw new ArgumentNullException("col");

            Type collectionType = col.GetType();
            Type collectionItemType = collectionType.FindElementTypes().Single(t => t != typeof(object));

            if (unique)
            {
                MethodInfo contains = collectionType.FindMethod("Contains", new Type[] { collectionItemType });
                if (contains == null) throw new ArgumentException("Cound not find \"Contains\" method of the given Collection");
                bool result = (bool)contains.Invoke(col, new object[] { val });
                if (result) return;
            }

            MethodInfo add = collectionType.FindMethod("Add", new Type[] { collectionItemType });
            if (add == null) throw new ArgumentException("Cound not find \"Add\" method of the given Collection");
            add.Invoke(col, new object[] { val });
        }

        public static void Remove(this ICollection col, object val)
        {
            if (col == null) throw new ArgumentNullException("col");

            Type collectionType = col.GetType();
            Type collectionItemType = collectionType.FindElementTypes().Single(t => t != typeof(object));

            MethodInfo remove = collectionType.FindMethod("Remove", new Type[] { collectionItemType });
            if (remove == null) throw new ArgumentException("Cound not find \"Remove\" method of the given Collection");
            remove.Invoke(col, new object[] { val });
        }

        /// <summary>
        /// Calls a public method on the given object. Uses Reflection.
        /// </summary>
        /// <typeparam name="TReturn">expected return type</typeparam>
        /// <param name="obj">the object on which to call the method</param>
        /// <param name="methodName">which method to call</param>
        /// <returns>the return value of the method</returns>
        /// <exception cref="ArgumentOutOfRangeException">if the method is not found</exception>
        public static TReturn CallMethod<TReturn>(this object obj, string methodName)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            Type t = obj.GetType();
            MethodInfo mi = null;
            while (mi == null && t != null)
            {
                mi = t.GetMethod(methodName, new Type[] { });
                t = t.BaseType;
            }
            if (mi == null) throw new ArgumentOutOfRangeException("methodName", string.Format("Method {0} was not found in Type {1}", methodName, obj.GetType().FullName));
            return (TReturn)mi.Invoke(obj, new object[] { });
        }

        /// <summary>
        /// Finds a Method with the given method parameter.
        /// </summary>
        /// <param name="type">Type to search in</param>
        /// <param name="methodName">Methodname to search for</param>
        /// <param name="parameterTypes">parameter types to match</param>
        /// <returns>MethodInfo or null if the method was not found</returns>
        public static MethodInfo FindMethod(this Type type, string methodName, Type[] parameterTypes)
        {
            if (type == null) { throw new ArgumentNullException("type"); }

            if (parameterTypes == null)
            {
                MethodInfo mi = type.GetMethod(methodName);
                if (mi != null) return mi;
            }
            else
            {
                MethodInfo[] methods = type.GetMethods();
                foreach (MethodInfo mi in methods)
                {
                    if (mi.Name == methodName)
                    {
                        ParameterInfo[] parameters = mi.GetParameters();
                        if (parameters.Length == parameterTypes.Length)
                        {
                            bool paramSame = true;
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                if (parameters[i].ParameterType != parameterTypes[i])
                                {
                                    paramSame = false;
                                    break;
                                }
                            }

                            if (paramSame) return mi;
                        }
                    }
                }
            }

            // Look in Basetypes
            if (type.BaseType != null)
            {
                MethodInfo mi = type.BaseType.FindMethod(methodName, parameterTypes);
                if (mi != null) return mi;
            }

            // Look in Interfaces
            foreach (Type i in type.GetInterfaces())
            {
                MethodInfo mi = i.FindMethod(methodName, parameterTypes);
                if (mi != null) return mi;
            }

            return null;
        }

        /// <summary>
        /// Finds a Method with the given method parameter.
        /// </summary>
        /// <param name="type">Type to search in</param>
        /// <param name="methodName">Methodname to search for</param>
        /// <param name="typeArguments">type arguments to match</param>
        /// <param name="parameterTypes">parameter types to match</param>
        /// <returns>MethodInfo or null if the method was not found</returns>
        public static MethodInfo FindGenericMethod(this Type type, string methodName, Type[] typeArguments, Type[] parameterTypes)
        {
            return type.FindGenericMethod(false, methodName, typeArguments, parameterTypes);
        }

        /// <summary>
        /// Finds a Method with the given method parameter.
        /// </summary>
        /// <param name="type">Type to search in</param>
        /// <param name="isPrivate">whether or not the method is private</param>
        /// <param name="methodName">Methodname to search for</param>
        /// <param name="typeArguments">type arguments to match</param>
        /// <param name="parameterTypes">parameter types to match</param>
        /// <returns>MethodInfo or null if the method was not found</returns>
        public static MethodInfo FindGenericMethod(this Type type, bool isPrivate, string methodName, Type[] typeArguments, Type[] parameterTypes)
        {
            if (type == null) { throw new ArgumentNullException("type"); }

            if (parameterTypes == null)
            {
                MethodInfo mi = isPrivate
                    ? type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic)
                    : type.GetMethod(methodName);

                if (mi != null)
                {
                    return mi.MakeGenericMethod(typeArguments);
                }
            }
            else
            {
                MethodInfo[] methods = isPrivate
                    ? type.GetMethods(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic)
                    : type.GetMethods();

                foreach (MethodInfo method in methods)
                {
                    if (method.Name == methodName && method.GetGenericArguments().Length == typeArguments.Length)
                    {
                        MethodInfo mi = method.MakeGenericMethod(typeArguments);
                        ParameterInfo[] parameters = mi.GetParameters();

                        if (parameters.Length == parameterTypes.Length)
                        {
                            bool paramSame = true;
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                if (parameters[i].ParameterType != parameterTypes[i])
                                {
                                    paramSame = false;
                                    break;
                                }
                            }

                            if (paramSame) return mi;
                        }
                    }
                }
            }

            // Look in Basetypes
            if (type.BaseType != null)
            {
                MethodInfo mi = type.BaseType.FindGenericMethod(isPrivate, methodName, typeArguments, parameterTypes);
                if (mi != null) return mi;
            }

            // Look in Interfaces
            foreach (Type i in type.GetInterfaces())
            {
                MethodInfo mi = i.FindGenericMethod(isPrivate, methodName, typeArguments, parameterTypes);
                if (mi != null) return mi;
            }

            return null;
        }

        /// <summary>
        /// Finds all implemented IEnumerables, IQueryables and IOrderedQueryables of the given Type
        /// </summary>
        public static IQueryable<Type> FindSequences(this Type seqType)
        {
            if (seqType == null || seqType == typeof(object) || seqType == typeof(string))
                return new Type[] { }.AsQueryable();

            if (seqType.IsArray || seqType == typeof(IEnumerable))
                return new Type[] { typeof(IEnumerable) }.AsQueryable();

            if (seqType.IsArray || seqType == typeof(IQueryable))
                return new Type[] { typeof(IQueryable) }.AsQueryable();

            if (seqType.IsArray || seqType == typeof(IOrderedQueryable))
                return new Type[] { typeof(IOrderedQueryable) }.AsQueryable();

            if (seqType.IsGenericType && seqType.GetGenericArguments().Length == 1 && seqType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return new Type[] { seqType, typeof(IEnumerable) }.AsQueryable();
            }

            if (seqType.IsGenericType && seqType.GetGenericArguments().Length == 1 && seqType.GetGenericTypeDefinition() == typeof(IQueryable<>))
            {
                return new Type[] { seqType, typeof(IQueryable) }.AsQueryable();
            }

            if (seqType.IsGenericType && seqType.GetGenericArguments().Length == 1 && seqType.GetGenericTypeDefinition() == typeof(IOrderedQueryable<>))
            {
                return new Type[] { seqType, typeof(IOrderedQueryable) }.AsQueryable();
            }

            var result = new List<Type>();

            foreach (var iface in (seqType.GetInterfaces() ?? new Type[] { }))
            {
                result.AddRange(FindSequences(iface));
            }

            return FindSequences(seqType.BaseType).Union(result);
        }

        /// <summary>
        /// Finds all element types provided by a specified sequence type.
        /// "Element types" are T for IEnumerable&lt;T&gt; and object for IEnumerable.
        /// </summary>
        public static IQueryable<Type> FindElementTypes(this Type seqType)
        {
            return seqType.FindSequences().Select(t => t.IsGenericType ? t.GetGenericArguments().Single() : typeof(object));
        }
    }
}

