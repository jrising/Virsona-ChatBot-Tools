using System;
using System.Collections.Generic;
using System.Text;

namespace InOutTools
{
    public class UserException : Exception
    {
        public UserException(string message)
            : base(message)
        {
            Unilog.Exception(this, typeof(UserException), message);
        }

        public UserException(string message, Exception innerException)
            : base(message, innerException)
        {
            Unilog.Exception(innerException, typeof(UserException), message);
        }
    }
}
