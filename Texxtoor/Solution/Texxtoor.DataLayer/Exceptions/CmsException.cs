using System;

namespace Texxtoor.DataModels.Exceptions
{

    public class CmsException : ApplicationException
    {
        public CmsException()
        {
        }

        public CmsException(string message, Exception innerException)
            : base(message,
                   innerException)
        {
        }

        public CmsException(string message)
            : base(message)
        {
        }
    }
}