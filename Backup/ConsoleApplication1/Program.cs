using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Nikos.Extensions.Collections;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            string patron = "abb76987765acadsghgsgcererty345dfg8675jhfcjhg;lhg.,/'i";

            patron = patron.QuickSort((x, y) =>
            {
                if (char.IsDigit(x) && !char.IsDigit(y))
                    return 1;
                else if (char.IsDigit(y) && !char.IsDigit(x))
                    return -1;
                return x.CompareTo(y);
            }).C_ToString();


            foreach (var c in patron)
            {
                Console.WriteLine(c);  
            }
        }

        private static int A(char x, char y)
        {
            throw new NotImplementedException();
        }
    }
}
