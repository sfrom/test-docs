using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acies.Docs.Models.Exceptions
{
    public class UnsupportedOutputTypeException : Exception
    {
        public UnsupportedOutputTypeException(string? message) : base("Unsupported output type " + message)
        {
        }
    }
}
