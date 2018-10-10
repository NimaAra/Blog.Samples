namespace IDGenerator.Core
{
    using BenchmarkDotNet.Attributes;

    [ClrJob(true), CoreJob]
    [LegacyJitX64Job, RyuJitX64Job]
    [MemoryDiagnoser, GcServer(true), GcForce]
    public class Benchmarker
    {
        [Benchmark(Baseline = true, Description = "Unsafe")]
        public static string GetIDUnsafe() => IDGeneratorService.GetNextIDUnsafe();

        [Benchmark(Description = "NewBuffer")]
        public static string GetIDNewBuffer() => IDGeneratorService.GetNextIDNewBuffer();

        [Benchmark(Description = "WithLocking")]
        public static string GetIDWithLocking() => IDGeneratorService.GetNextIDWithLocking();

        [Benchmark(Description = "ThreadLocal")]
        public static string GetIDThreadLocal() => IDGeneratorService.GetNextIDThreadLocal();

        [Benchmark(Description = "ThreadStatic")]
        public static string GetIDThreadStatic() => IDGeneratorService.GetNextIDThreadStatic();
        
        [Benchmark(Description = "SpinLock")]
        public static string GetIDSpinLock() => IDGeneratorService.GetNextIDSpinLock();
    }
}