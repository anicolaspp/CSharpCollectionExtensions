using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nikos.Extensions.Collections
{
    public static class Collections
    {
        #region Private

        private static List<T[]> Split<T>(T[] data, int index)
        {
            var result = new List<T>[2];
            result[0] = new List<T>();
            result[1] = new List<T>();

            for (int i = 0; i < index; i++)
                result[0].Add(data[i]);
            for (int i = index + 1; i < data.Length; i++)
                result[1].Add(data[i]);

            return new List<T[]> { result[0].ToArray(), result[1].ToArray() };
        }
       
		private static T[] Merge<T>(T[] x, T[] y, Comparison<T> comparison)
        {
            int i = 0, j = 0;
            var result = new List<T>();

            while (result.Count < x.Length + y.Length)
            {
                if (comparison(x[i], y[j]) <= 0)
                    result.Add(x[i++]);
                else
                    result.Add(y[j++]);
            }
            return result.ToArray();
        }
        
		private static int[] Compute_Prefix_Function<T>(T[] data, Comparison<T> comparison)
        {
            int m = data.Length;
            int[] pi = new int[m];
            pi[0] = 0;
            int k = 0;

            for (int i = 1; i < m; i++)
            {
                while (k > 0 && comparison(data[k + 1], data[i]) != 0)
                    k = pi[k];
                if (comparison(data[i], data[k + 1]) == 0)
                    k = k + 1;
                pi[i] = k;
            }

            return pi;
        }

		// Trying to reset the enumerator
        private static bool TryReset(IEnumerator source)
        {
            try
            {
                source.Reset();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
     
		private static void _QuickSort<T>(T[] data, Comparison<T> comparison, int desde, int hasta)
        {
            int i = desde, j = hasta;
            var med = data[(i + j) / 2];
            do
            {
                while (comparison(data[i], med) < 0) i++;
                while (comparison(data[j], med) > 0) j--;
                if (i <= j)
                {
                    var temp = data[i];
                    data[i] = data[j];
                    data[j] = temp;
                    i++;
                    j--;
                }

            } while (i < j);
            if (i < hasta) _QuickSort(data, comparison, i, hasta);
            if (desde < j) _QuickSort(data, comparison, desde, j);
        }
        
		private static T[] _MergeSort<T>(T[] data, Comparison<T> comparison, int desde, int hasta)
        {
            if (data.Length < 2)
                return data;

            var list = Split(data, (desde + hasta) / 2);
            var x = list[0];
            var y = list[1];

            x = _MergeSort(x, comparison, 0, x.Length - 1);
            y = _MergeSort(y, comparison, 0, y.Length - 1);

            return Merge(x, y, comparison);
        }
        
		private static int[] _KMP<T>(T[] data, T[] frag, Comparison<T> comparison)
        {
            var result = new List<int>();

            int n = frag.Length;
            int m = data.Length;
            int[] pi = Compute_Prefix_Function(data, comparison);

            int q = 0;
            for (int i = 0; i < n; i++)
            {
                while (q > 0 && comparison(data[q + 1], frag[i]) != 0)
                    q = pi[q];
                if (comparison(data[q + 1], frag[i]) == 0)
                    q = q + 1;
                if (q == m)
                {
                    result.Add(i - m);
                    q = pi[q];
                }
            }

            return result.ToArray();
        }

        #endregion Private

        /// <summary>
        /// Convert a IEnumerator to IEnumerable
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="source">Enumerator</param>
        /// <returns>Enumerable</returns>
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> source)
        {
            TryReset(source);
            while (source.MoveNext())
                yield return source.Current;
        }

        /// <summary>
        /// Convert a IEnumerator to IEnumerable
        /// </summary>
        /// <param name="source">Enumerator</param>
        /// <returns>Enumerable</returns>
        public static IEnumerable ToEnumerable(this IEnumerator source)
        {
            TryReset(source);
            while (source.MoveNext())
                yield return source.Current;
        }

        /// <summary>
        /// Add a list of elements into Stack
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dest"></param>
        /// <param name="source"></param>
        public static void AddRange<T>(this Stack<T> dest, IEnumerable<T> source)
        {
            foreach (var item in source)
                dest.Push(item);
        }

        /// <summary>
        /// Add a list of elements into Queue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dest"></param>
        /// <param name="source"></param>
        public static void AddRange<T>(this Queue<T> dest, IEnumerable<T> source)
        {
            foreach (var item in source)
                dest.Enqueue(item);
        }

        /// <summary>
        /// Ordena utilizando el metod quick sort
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">Elementos a ordenar</param>
        /// <param name="comparison">Compara dos elementos de la coleccion</param>
        /// <returns>Los elementos ordenados</returns>
        public static IEnumerable<T> QuickSort<T>(this IEnumerable<T> source, Comparison<T> comparison)
        {
            return QuickSort(source, comparison, 0, source.Count() - 1);
        }

        /// <summary>
        ///  Ordena utilizando el metod quick sort basandose en que los elementos son IComparables
        /// </summary>
        /// <typeparam name="T">Elementos a ordenar</typeparam>
        /// <param name="source">Los elementos ordenados</param>
        /// <returns></returns>
        public static IEnumerable<T> QuickSort<T>(this IEnumerable<T> source) where T : IComparable
        {
            return source.QuickSort((x, y) => x.CompareTo(y));
        }

        /// <summary>
        /// Ordena utilizando el metodo quick sort
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">Elementos a ordenar</param>
        /// <param name="comparison">Compara dos elementos de la coleccion</param>
        /// <param name="Inicio">Inicio de donde se va a ordenar</param>
        /// <param name="Final">Final de donde se va a ordenar</param>
        /// <returns>Los elementos ordenados</returns>
        public static IEnumerable<T> QuickSort<T>(this IEnumerable<T> source, Comparison<T> comparison, int Inicio, int Final)
        {
            if (comparison == null)
                throw new ArgumentNullException("The comparison function can't be null");

            T[] data = source.ToArray();
            if (data.Length > 1)
                _QuickSort(data, comparison, Inicio, Final);
            return data;
        }

        /// <summary>
        /// Sort the elemets of a IEnumerable using the MergeSort Method; this one is stable
        /// </summary>
        /// <typeparam name="T">The Generic parameter</typeparam>
        /// <param name="source">Elemetents to sort</param>
        /// <param name="comparison">Function to compare the items of source</param>
        /// <returns>A sorted IEnumerable</returns>
        public static IEnumerable<T> MergeSort<T>(this IEnumerable<T> source, Comparison<T> comparison)
        {
            return MergeSort(source, comparison, 0, source.Count() - 1);
        }

        /// <summary>
        /// Sort the elemets of a IEnumerable using the MergeSort Method; this one is stable
        /// </summary>
        /// <typeparam name="T">The Generic parameter</typeparam>
        /// <param name="source">Elemetents to sort</param>
        /// <returns>A sorted IEnumerable</returns>
        public static IEnumerable<T> MergeSort<T>(this IEnumerable<T> source) where T : IComparable
        {
            return source.MergeSort((x, y) => x.CompareTo(y));
        }

        /// <summary>
        /// Sort the elemets of a IEnumerable using the MergeSort Method; this one is stable
        /// </summary>
        /// <typeparam name="T">The Generic paramete</typeparam>
        /// <param name="source">Elemetents to sort</param>
        /// <param name="comparison">Function to compare the items of source</param>
        /// <param name="Inicio">The begin index</param>
        /// <param name="Final">The end index</param>
        /// <returns> sorted IEnumerable</returns>
        public static IEnumerable<T> MergeSort<T>(this IEnumerable<T> source, Comparison<T> comparison, int Inicio, int Final)
        {
            if (comparison == null)
                throw new ArgumentNullException("comparison");

            return source.Count() > 1 ? _MergeSort(source.ToArray(), comparison, Inicio, Final) : source;
        }

        /// <summary>
        /// Remove a serie of items from a ICollection
        /// </summary>
        /// <typeparam name="T">The generic parameter</typeparam>
        /// <param name="source">A ICollection</param>
        /// <param name="items">Items to remove</param>
        /// <returns>A array of boolean values that contain true if the item was remove; false in other case</returns>
        public static bool[] Remove<T>(this ICollection<T> source, params T[] items)
        {
            bool[] result = new bool[items.Length];

            for (int i = 0; i < items.Length; i++)
                result[i] = source.Remove(items[i]);

            return result;
        }

        /// <summary>
        /// Apply de algoritm KMP to search into the patron a specific secuence
        /// </summary>
        /// <typeparam name="T">Generic parameters</typeparam>
        /// <param name="source">The Patron</param>
        /// <param name="secuence">Secuence to search</param>
        /// <param name="comparison">A delegate to compare the items of T types</param>
        /// <returns>Return the index where secuence was found</returns>
        public static int[] Kmp<T>(this IEnumerable<T> source, IEnumerable<T> secuence, Comparison<T> comparison)
        {
            //aplicando KMP...
            return _KMP(source.ToArray(), secuence.ToArray(), comparison);
        }

        public static int[] Kmp<T>(this IEnumerable<T> source, IEnumerable<T> secuence) where T : IComparable
        {
            return source.Kmp(secuence, (x, y) => x.CompareTo(y));
        }

        ///<summary>
        /// Split a IEnumerable by diferente positions
        ///</summary>
        ///<param name="source">A IEnumerable for split</param>
        ///<param name="indexs">Indexs for split a source</param>
        ///<typeparam name="T">The generic parameter</typeparam>
        ///<returns></returns>
        public static IEnumerable<T[]> Split<T>(this IEnumerable<T> source, params int[] indexs)
        {
            if (indexs.Length == 0)
                return new List<T[]> { source.ToArray() };

            T[] data = source.ToArray();
            var x = new List<int>(indexs);
            var result = new List<T[]>();

            var a = new List<T>();

            for (int i = 0; i < data.Length; i++)
            {
                a.Add(data[i]);
                if (i == x[0])
                {
                    result.Add(a.ToArray());
                    x.RemoveAt(0);
                    a.Clear();
                }
            }

            return result;
        }

        /// <summary>
        /// Convert a IEnumerable of char to String than represent
        /// </summary>
        /// <param name="source">IEnumerable of char</param>
        /// <returns>A String</returns>
        public static string C_ToString(this IEnumerable<char> source)
        {
            return new string(source.ToArray());
        }

        /// <summary>
        /// Get the subsecuence whith specific indexs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static IEnumerable<T> SubSec<T>(this IEnumerable<T> source, int start, int end)
        {
            var x = source.ToArray();

            if (start < 0 || start >= x.Length || end >= x.Length || start > end)
                throw new InvalidOperationException("Index");

            for (int i = start; i <= end; i++)
                yield return x[i];
        }
    }
}