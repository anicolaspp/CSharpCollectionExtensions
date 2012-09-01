using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nikos.Extensions.Collections;

namespace Nikos.Extensions.Types
{
    public static class Types
    {
        /// <summary>
        /// Get the base class type of this class and implemented interfaces
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<Type> BaseTypes<T>(this T source)
        {
            var types = new Queue<Type>();
            types.Enqueue(source.GetType());

            while (types.Count > 0)
            {
                var current = types.Dequeue();
                yield return current;

                types.Enqueue(current.BaseType);
                types.AddRange(current.GetInterfaces());
            }
        }

        ///<summary>
        /// Compare the item with a IComparable by func result
        ///</summary>
        ///<param name="source">Item</param>
        ///<param name="obj">A IComparable</param>
        ///<param name="func">Function to convert type of T to ICompareble</param>
        ///<typeparam name="T">The type to compare with the ICompareble</typeparam>
        ///<returns>The value of comparison</returns>
        public static int CompareTo<T>(this T source, IComparable obj, Func<T, IComparable> func)
        {
            var t = func(source);
            return t.CompareTo(obj);
        }

        ///<summary>
        /// Compare two diferent objects by some inside property
        ///</summary>
        ///<param name="source">First object to compare</param>
        ///<param name="obj">Second object to compare</param>
        ///<param name="func1">Function to convert type of T to ICompareble</param>
        ///<param name="func2">Function to convert type of K to ICompareble</param>
        ///<typeparam name="T">The type to compare with the ICompareble</typeparam>
        ///<typeparam name="K">The type to compare with the ICompareble</typeparam>
        ///<returns>The value of comparison</returns>
        public static int CompareTo<T, K>(this T source, K obj, Func<T, IComparable> func1, Func<K, IComparable> func2)
        {
            return func1(source).CompareTo(func2(obj));
        }
    }
}