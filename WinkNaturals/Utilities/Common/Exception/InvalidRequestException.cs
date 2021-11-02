using System;
namespace WinkNaturals.Utilities.Common.Exception
{
    public class InvalidRequestException : SystemException
    {
        public InvalidRequestException()
        {
        }
        public InvalidRequestException(string message)
            : base(message)
        {
        }

        public InvalidRequestException(string message, SystemException inner)
        : base(message, inner)
        {
        }
    }
}