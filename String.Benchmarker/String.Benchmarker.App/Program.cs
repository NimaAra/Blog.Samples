namespace String.Benchmarker.App
{
#if NETCOREAPP
    using System.Buffers;
#endif
    using System.Linq;
    using System.Text;
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Jobs;
    using BenchmarkDotNet.Running;
    using Easy.Common;

    /// <summary>
    ///  dotnet run -c Release -f netcoreapp3.0 --filter Benchmarker
    ///  // don't need the below
    ///  dotnet run -c Release -f netcoreapp3.0 --runtimes netcoreapp3.0 net472
    /// </summary>
    internal class Program
    {
        static void Main(string[] args) => 
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }

    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.NetCoreApp30, baseline: true)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [SimpleJob(RuntimeMoniker.Net48)]
    [SimpleJob(RuntimeMoniker.Net472)]
    public class Benchmarker
    {
        private readonly MatchFilter _matchFilter;

        public Benchmarker() => _matchFilter = new MatchFilter("Foo", "Bar");

        [Benchmark]
        public string GenerateImplicitConcat() => _matchFilter.RenderImplicitConcat();

        [Benchmark]
        public string GenerateExplicitConcat() => _matchFilter.RenderExplicitConcat();

        [Benchmark]
        public string GenerateConcatArray() => _matchFilter.RenderConcatArray();

        [Benchmark]
        public string GenerateInterpolation() => _matchFilter.RenderInterpolation();

        [Benchmark]
        public string GenerateStringFormat() => _matchFilter.RenderStringFormat();

        [Benchmark]
        public string GenerateStringBuilder() => _matchFilter.RenderStringBuilder();

        [Benchmark]
        public string GenerateCachedStringBuilder() => _matchFilter.RenderCachedStringBuilder();

#if NETCOREAPP
        [Benchmark]
        public string GenerateStringCreate() => _matchFilter.RenderStringCreate();
#endif
    }

    public sealed class MatchFilter
    {
        const string DOUBLE_QUOTE = "\"";
        const string COLON = ":";

        private readonly string[] _concatArray = 
        {
            DOUBLE_QUOTE,
            string.Empty,
            COLON,
            string.Empty,
            DOUBLE_QUOTE
        };

        private readonly int _totalStringLength;

        public string Field => _concatArray[1];
        public string Value => _concatArray[3];

        public MatchFilter(string field, string value)
        {
            _concatArray[1] = field;
            _concatArray[3] = value;
            _totalStringLength = _concatArray.Sum(x => x.Length);
        }

        public string RenderImplicitConcat() => DOUBLE_QUOTE + Field + COLON + Value + DOUBLE_QUOTE;

        public string RenderExplicitConcat() => string.Concat(DOUBLE_QUOTE, Field, COLON, Value, DOUBLE_QUOTE);

        public string RenderInterpolation() => $"\"{Field}:{Value}\"";

        public string RenderConcatArray() => string.Concat(_concatArray);

        public string RenderStringFormat() => string.Format("\"{0}:{1}\"", Field, Value);

        static readonly StringBuilder BUILDER = new StringBuilder();
        public string RenderStringBuilder() =>
            BUILDER
                .Clear()
                .Append(DOUBLE_QUOTE)
                .Append(Field)
                .Append(COLON)
                .Append(Value)
                .Append(DOUBLE_QUOTE)
                .ToString();

        public string RenderCachedStringBuilder()
        {
            var builder = StringBuilderCache.Acquire();
            builder
                .Append(DOUBLE_QUOTE)
                .Append(Field)
                .Append(COLON)
                .Append(Value)
                .Append(DOUBLE_QUOTE);
            return StringBuilderCache.GetStringAndRelease(builder);
        }

#if NETCOREAPP
        public string RenderStringCreate() => string.Create(_totalStringLength, this, _writeToStringMemory);

        private static readonly SpanAction<char, MatchFilter> _writeToStringMemory = (span, filter) =>
        {
            var i = 0;
            span[i++] = '"';
            foreach (var c in filter.Field) { span[i++] = c; }
            span[i++] = ':';
            foreach (var c in filter.Value) { span[i++] = c; }
            span[i] = '"';
        };
#endif
    }
}
