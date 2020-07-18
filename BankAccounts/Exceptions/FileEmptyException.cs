using System;

namespace BankAccounts.Exceptions
{
    public class FileEmptyException : Exception
    {
        public FileEmptyException()
        {

        }

        public FileEmptyException(string message) : base(message)
        {

        }

        public FileEmptyException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
