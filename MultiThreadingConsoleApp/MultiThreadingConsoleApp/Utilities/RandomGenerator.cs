using System;

namespace MultiThreadingConsoleApp
{
    public static class RandomGenerator {
        public static Random r = new Random();
        public static int Generate(int min, int max) {
            return r.Next(min,max);
        }

        public static int Generate(int max)
        {
            return r.Next(max);
        }
    }
}
