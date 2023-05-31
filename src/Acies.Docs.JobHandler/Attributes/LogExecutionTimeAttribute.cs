using System.Diagnostics;
using PostSharp.Aspects;
using PostSharp.Serialization;

namespace Acies.Docs.JobHandler.Attributes;

[PSerializable]
[AttributeUsage(AttributeTargets.Method)]
public sealed class LogExecutionTimeAttribute : OnMethodBoundaryAspect
{
    private static readonly Stopwatch Timer = new();      

    public override void OnEntry(MethodExecutionArgs args)
    {
        Timer.Start();
    }

    public override void OnExit(MethodExecutionArgs args)
    {     
        Console.WriteLine($"[{args.Method.Name}] took {Timer.ElapsedMilliseconds} ms.");
        
        Timer.Reset();
    }
}