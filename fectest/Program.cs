using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace fectest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            List<List<byte>> buf = new List<List<byte>>()
            {
                new List<byte>(){},
                new List<byte>(){},
                new List<byte>(){ }
            };

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 10000; j++)
                {
                    buf[i].Add((byte)i);
                }
            }
        }
    }
}