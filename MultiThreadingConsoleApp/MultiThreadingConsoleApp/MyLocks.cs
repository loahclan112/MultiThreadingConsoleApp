namespace MultiThreadingConsoleApp
{
    public static class MyLocks {

        public static object lockConsoleObject = new object();
        public static object lockInfectedCountObject = new object();
        public static object lockUpdateMapObject = new object();
        public static object lockThreadIdIncrease = new object();


    }


}
