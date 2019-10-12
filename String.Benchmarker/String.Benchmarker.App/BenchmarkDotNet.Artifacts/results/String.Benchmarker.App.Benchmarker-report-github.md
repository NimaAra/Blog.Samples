``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.14393.3243 (1607/AnniversaryUpdate/Redstone1)
Intel Xeon CPU X5650 2.67GHz, 1 CPU, 12 logical and 6 physical cores
Frequency=2597660 Hz, Resolution=384.9619 ns, Timer=TSC
.NET Core SDK=3.0.100
  [Host] : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT
  Core   : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), 64bit RyuJIT


```
|                      Method |  Job | Runtime |      Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------- |----- |-------- |----------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|      GenerateImplicitConcat |  Clr |     Clr |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|      GenerateImplicitConcat | Core |    Core | 109.98 ns | 0.2335 ns | 0.2184 ns |  1.00 |    0.00 | 0.0166 |     - |     - |     104 B |
|                             |      |         |           |           |           |       |         |        |       |       |           |
|      GenerateExplicitConcat |  Clr |     Clr |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|      GenerateExplicitConcat | Core |    Core | 116.80 ns | 2.3226 ns | 2.3851 ns |  1.00 |    0.00 | 0.0165 |     - |     - |     104 B |
|                             |      |         |           |           |           |       |         |        |       |       |           |
|         GenerateConcatArray |  Clr |     Clr |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|         GenerateConcatArray | Core |    Core |  72.48 ns | 0.2057 ns | 0.1924 ns |  1.00 |    0.00 | 0.0063 |     - |     - |      40 B |
|                             |      |         |           |           |           |       |         |        |       |       |           |
|       GenerateInterpolation |  Clr |     Clr |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|       GenerateInterpolation | Core |    Core | 104.34 ns | 0.4479 ns | 0.4189 ns |  1.00 |    0.00 | 0.0166 |     - |     - |     104 B |
|                             |      |         |           |           |           |       |         |        |       |       |           |
|        GenerateStringFormat |  Clr |     Clr |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|        GenerateStringFormat | Core |    Core | 186.48 ns | 1.8079 ns | 1.5097 ns |  1.00 |    0.00 | 0.0062 |     - |     - |      40 B |
|                             |      |         |           |           |           |       |         |        |       |       |           |
|       GenerateStringBuilder |  Clr |     Clr |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|       GenerateStringBuilder | Core |    Core |  63.25 ns | 0.5997 ns | 0.5610 ns |  1.00 |    0.00 | 0.0063 |     - |     - |      40 B |
|                             |      |         |           |           |           |       |         |        |       |       |           |
| GenerateCachedStringBuilder |  Clr |     Clr |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
| GenerateCachedStringBuilder | Core |    Core |  84.82 ns | 0.2264 ns | 0.2007 ns |  1.00 |    0.00 | 0.0063 |     - |     - |      40 B |
|                             |      |         |           |           |           |       |         |        |       |       |           |
|        GenerateStringCreate |  Clr |     Clr |        NA |        NA |        NA |     ? |       ? |      - |     - |     - |         - |
|        GenerateStringCreate | Core |    Core |  30.86 ns | 0.0923 ns | 0.0864 ns |  1.00 |    0.00 | 0.0063 |     - |     - |      40 B |

Benchmarks with issues:
  Benchmarker.GenerateImplicitConcat: Clr(Runtime=Clr)
  Benchmarker.GenerateExplicitConcat: Clr(Runtime=Clr)
  Benchmarker.GenerateConcatArray: Clr(Runtime=Clr)
  Benchmarker.GenerateInterpolation: Clr(Runtime=Clr)
  Benchmarker.GenerateStringFormat: Clr(Runtime=Clr)
  Benchmarker.GenerateStringBuilder: Clr(Runtime=Clr)
  Benchmarker.GenerateCachedStringBuilder: Clr(Runtime=Clr)
  Benchmarker.GenerateStringCreate: Clr(Runtime=Clr)
