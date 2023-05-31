using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acies.Docs.Models
{
    public interface INotifyService
    {
        Task Notify<T>(T data, OutputTypes output);
    }
}