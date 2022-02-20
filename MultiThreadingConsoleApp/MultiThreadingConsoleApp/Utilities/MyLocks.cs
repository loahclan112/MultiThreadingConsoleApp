namespace MultiThreadingConsoleApp
{
    public static class MyLocks {
        public static object lockConsoleObject = new object();
        public static object lockInfectedCountObject = new object();
        public static object lockIsDoneMovementObject = new object();
        public static object lockMainObject = new object();
    }
}
