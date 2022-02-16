using System;

namespace MultiThreadingConsoleApp
{
    public class InvalidInputException : InvalidCastException
    {
        public string msg;

        public InvalidInputException()
        {
          
        }

        public InvalidInputException(string msg) 
        {
            this.msg = msg;
        }
    }
}
