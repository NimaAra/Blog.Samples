namespace IDGenerator.Core
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    internal static class Program
    {
        private enum Scenario
        {
            Unsafe = 0,
            WithLocking,
            ThreadLocal,
            ThreadStatic,
            NewBuffer,
            SpinLock
        }
        
        private const int ITERATION_COUNT = 1_000_000_000;

        static void Main()
        {
            // var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<Benchmarker>();
            RunScenario(Scenario.ThreadLocal, 12);
        }

        private static void RunScenario(Scenario scenario, int threadCount)
        {
            Console.WriteLine("Executing Scenario: '{0}' with: '{1}' threads over: '{2:N0}' iterations.", 
                scenario, 
                threadCount, 
                ITERATION_COUNT);
            
            Func<string> idGenerator;
            switch (scenario)
            {
                case Scenario.Unsafe:
                    idGenerator = IDGeneratorService.GetNextIDUnsafe;
                    break;
                case Scenario.WithLocking:
                    idGenerator = IDGeneratorService.GetNextIDWithLocking;
                    break;
                case Scenario.ThreadLocal:
                    idGenerator = IDGeneratorService.GetNextIDThreadLocal;
                    break;
                case Scenario.ThreadStatic:
                    idGenerator = IDGeneratorService.GetNextIDThreadStatic;
                    break;
                case Scenario.NewBuffer:
                    idGenerator = IDGeneratorService.GetNextIDNewBuffer;
                    break;
                case Scenario.SpinLock:
                    idGenerator = IDGeneratorService.GetNextIDSpinLock;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null);
            }

            var sw = Stopwatch.StartNew();

            ParallelEnumerable
                .Range(1, ITERATION_COUNT)
                .WithDegreeOfParallelism(threadCount)
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .ForAll(_ => idGenerator());

            sw.Stop();
            using (var process = Process.GetCurrentProcess())
            {
                Console.WriteLine("  - Execution time: {0}\r\n  - Gen-0: {1}, Gen-1: {2}, Gen-2: {3}\r\n  - Peak WrkSet: {4:N0}",
                    sw.Elapsed.ToString(),
                    GC.CollectionCount(0),
                    GC.CollectionCount(1),
                    GC.CollectionCount(2), 
                    process.PeakWorkingSet64);
            }
        }
    }
}
