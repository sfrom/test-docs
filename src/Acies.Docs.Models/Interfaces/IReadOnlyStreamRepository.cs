using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acies.Docs.Models
{
    public interface IReadOnlyStreamRepository
    {
        Task<Stream> GetStreamAsync(string key);
    }
}