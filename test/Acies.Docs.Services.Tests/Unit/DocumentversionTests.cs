using Acies.Docs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Acies.Docs.Services.Tests
{
    public class DocumentversionTests
    {
        //[Fact]
        //public void Status_SumPending()
        //{
        //    var dv = new DocumentVersion();

        //    dv.Output = new List<DocumentOutput>
        //        {
        //        new DocumentOutput
        //        {
        //            Status=Status.Pending,
        //        },
        //        new DocumentOutput
        //        {
        //            Status=Status.Processing,
        //        },
        //    };

        //    var status = dv.Status;

        //    Assert.Equal(Status.Pending, status);
        //}

        //[Fact]
        //public void Status_SumProcessing()
        //{
        //    var dv = new DocumentVersion();

        //    dv.Output = new List<DocumentOutput>
        //        {
        //        new DocumentOutput
        //        {
        //            Status=Status.Succeeded,
        //        },
        //        new DocumentOutput
        //        {
        //            Status=Status.Processing,
        //        },
        //    };

        //    var status = dv.Status;

        //    Assert.Equal(Status.Processing, status);
        //}

        //[Fact]
        //public void Status_SumProcessing2()
        //{
        //    var dv = new DocumentVersion();

        //    dv.Output = new List<DocumentOutput>
        //        {
        //        new DocumentOutput
        //        {
        //            Status=Status.Succeeded,
        //        },
        //        new DocumentOutput
        //        {
        //            Status=Status.Failed,
        //        },
        //                        new DocumentOutput
        //        {
        //            Status=Status.Processing,
        //        },
        //    };

        //    var status = dv.Status;

        //    Assert.Equal(Status.Processing, status);
        //}

        //[Fact]
        //public void Status_SumFailed()
        //{
        //    var dv = new DocumentVersion();

        //    dv.Output = new List<DocumentOutput>
        //        {
        //        new DocumentOutput
        //        {
        //            Status=Status.Succeeded,
        //        },
        //        new DocumentOutput
        //        {
        //            Status=Status.Failed,
        //        },
        //        new DocumentOutput
        //        {
        //            Status=Status.Succeeded,
        //        },
        //    };

        //    var status = dv.Status;

        //    Assert.Equal(Status.Failed, status);
        //}

        //[Fact]
        //public void Status_SumUnknown()
        //{
        //    var dv = new DocumentVersion();

        //    dv.Output = new List<DocumentOutput>
        //        {
        //        new DocumentOutput
        //        {
        //            Status=Status.Unknown,
        //        },
        //        new DocumentOutput
        //        {
        //            Status=Status.Failed,
        //        },
        //        new DocumentOutput
        //        {
        //            Status=Status.Succeeded,
        //        },
        //    };

        //    var status = dv.Status;

        //    Assert.Equal(Status.Unknown, status);
        //}
    }
}
