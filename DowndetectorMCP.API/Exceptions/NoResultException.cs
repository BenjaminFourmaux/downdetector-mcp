using System;
using System.Collections.Generic;
using System.Text;

namespace DowndetectorMCP.API.Exceptions
{
    public class NoResultException : Exception
    {
        public NoResultException(string searchWord) : base($"INFO: No result found for service '{searchWord}'")
        {
        }
    }
}
