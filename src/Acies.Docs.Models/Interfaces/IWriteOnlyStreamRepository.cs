using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acies.Docs.Models
{
    public interface IWriteOnlyStreamRepository
    {
        Task WriteAsync(Stream stream, string key);
    }




}
