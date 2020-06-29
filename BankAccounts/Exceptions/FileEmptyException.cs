using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
