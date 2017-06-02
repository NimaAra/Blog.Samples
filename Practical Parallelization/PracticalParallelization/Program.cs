namespace PracticalParallelization
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Easy.Common;

    internal class Program
    {
        private const uint SanityCountToMatch = 4230497;
        private const uint TopCount = 100;
        private static readonly FileInfo InputFile = new FileInfo(@"F:\Stuff\IMDB\plot.list");
        private static readonly char[] Separators = { ' ', '.', ',' };
        private static readonly HashSet<char> InvalidCharacters = new HashSet<char>(new[] { '®', '"', '\'', '!', '(', ')', '{', '}', '<', '>', '|', '?', '-', '_', '&' });
        private static readonly HashSet<string> StopWords = new HashSet<string>(new[] { "a", "about", "above", "above", "across", "after", "afterwards", "again", "against", "all", "almost", "alone", "along", "already", "also", "although", "always", "am", "among", "amongst", "amoungst", "amount", "an", "and", "another", "any", "anyhow", "anyone", "anything", "anyway", "anywhere", "are", "around", "as", "at", "back", "be", "became", "because", "become", "becomes", "becoming", "been", "before", "beforehand", "behind", "being", "below", "beside", "besides", "between", "beyond", "bill", "both", "bottom", "but", "by", "call", "can", "cannot", "cant", "co", "con", "could", "couldnt", "cry", "de", "describe", "detail", "do", "done", "down", "due", "during", "each", "eg", "eight", "either", "eleven", "else", "elsewhere", "empty", "enough", "etc", "even", "ever", "every", "everyone", "everything", "everywhere", "except", "few", "fifteen", "fify", "fill", "find", "fire", "first", "five", "for", "former", "formerly", "forty", "found", "four", "from", "front", "full", "further", "get", "give", "go", "had", "has", "hasnt", "have", "he", "hence", "her", "here", "hereafter", "hereby", "herein", "hereupon", "hers", "herself", "him", "himself", "his", "how", "however", "hundred", "ie", "if", "in", "inc", "indeed", "interest", "into", "is", "it", "its", "itself", "keep", "last", "latter", "latterly", "least", "less", "ltd", "made", "many", "may", "me", "meanwhile", "might", "mill", "mine", "more", "moreover", "most", "mostly", "move", "much", "must", "my", "myself", "name", "namely", "neither", "never", "nevertheless", "next", "nine", "no", "nobody", "none", "noone", "nor", "not", "nothing", "now", "nowhere", "of", "off", "often", "on", "once", "one", "only", "onto", "or", "other", "others", "otherwise", "our", "ours", "ourselves", "out", "over", "own", "part", "per", "perhaps", "please", "put", "rather", "re", "same", "see", "seem", "seemed", "seeming", "seems", "serious", "several", "she", "should", "show", "side", "since", "sincere", "six", "sixty", "so", "some", "somehow", "someone", "something", "sometime", "sometimes", "somewhere", "still", "such", "system", "take", "ten", "than", "that", "the", "their", "them", "themselves", "then", "thence", "there", "thereafter", "thereby", "therefore", "therein", "thereupon", "these", "they", "thickv", "thin", "third", "this", "those", "though", "three", "through", "throughout", "thru", "thus", "to", "together", "too", "top", "toward", "towards", "twelve", "twenty", "two", "un", "under", "until", "up", "upon", "us", "very", "via", "was", "we", "well", "were", "what", "whatever", "when", "whence", "whenever", "where", "whereafter", "whereas", "whereby", "wherein", "whereupon", "wherever", "whether", "which", "while", "whither", "who", "whoever", "whole", "whom", "whose", "why", "will", "with", "within", "without", "would", "yet", "you", "your", "yours", "yourself", "yourselves", "the", "pl:", "mv:", "by:", "Anonymous", "he's", "it's", "she's", "i'm" }, StringComparer.InvariantCultureIgnoreCase);

        private static void Main()
        {
            Console.Write("Starting ");
            var stopWatch = Stopwatch.StartNew();
            IDictionary<string, uint> words = null;

            words = GetTopWordsSequential();
            words = GetTopWordsSequentialLINQ();
            words = GetTopWordsPLINQNaive();
            words = GetTopWordsPLINQ();
            words = GetTopWordsPLINQMapReduce();
            words = GetTopWordsParallelForEachMapReduce();
            words = GetTopWordsParallelForEachConcurrentDictionary();
            words = GetTopWordsProducerConsumer();
            words = GetTopWordsProducerConsumerEasier();
            words = GetTopWordsDataFlow();

            stopWatch.Stop();

            var sanityCount = (uint)words.Sum(pair => pair.Value);
            if (SanityCountToMatch == sanityCount)
            {
                using (var process = Process.GetCurrentProcess())
                {
                    Console.WriteLine("Execution time: {0}\r\nGen-0: {1}, Gen-1: {2}, Gen-2: {3}\r\nPeak WrkSet: {4}",
                        stopWatch.Elapsed.ToString(),
                        GC.CollectionCount(0).ToString(),
                        GC.CollectionCount(1).ToString(),
                        GC.CollectionCount(2).ToString(),
                        process.PeakWorkingSet64.ToString("n0"));
                }
            }
            else
            {
                Console.WriteLine("Invalid results, expected a sanity count of: {0} but got: {1}",
                    SanityCountToMatch.ToString("n0"), sanityCount.ToString("n0"));
            }
        }

        private static IDictionary<string, uint> GetTopWordsDataFlow()
        {
            Console.WriteLine(nameof(GetTopWordsDataFlow) + "...");

            const int WorkerCount = 12;
            const int BoundedCapacity = 10000;
            var result = new ConcurrentDictionary<string, uint>(StringComparer.InvariantCultureIgnoreCase);

            var bufferBlock = new BufferBlock<string>(
                new DataflowBlockOptions { BoundedCapacity = BoundedCapacity });
            
            var splitLineToWordsBlock = new TransformManyBlock<string, string>(
                line => line.Split(Separators, StringSplitOptions.RemoveEmptyEntries),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 1, 
                    BoundedCapacity = BoundedCapacity
                });
            
            var batchWordsBlock = new BatchBlock<string>(5000);
            
            var trackWordsOccurrencBlock = new ActionBlock<string[]>(words =>
            {
                foreach (var word in words)
                {
                    if (!IsValidWord(word)) { continue; }
                    result.AddOrUpdate(word, 1, (key, oldVal) => oldVal + 1);
                }
            }, 
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = WorkerCount });

            var defaultLinkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            bufferBlock.LinkTo(splitLineToWordsBlock, defaultLinkOptions);
            splitLineToWordsBlock.LinkTo(batchWordsBlock, defaultLinkOptions);
            batchWordsBlock.LinkTo(trackWordsOccurrencBlock, defaultLinkOptions);

            // Begin producing
            foreach (var line in File.ReadLines(InputFile.FullName))
            {
                bufferBlock.SendAsync(line).Wait();
            }

            bufferBlock.Complete();
            // End of producing

            // Wait for workers to finish their work
            trackWordsOccurrencBlock.Completion.Wait();

            return result
                .OrderByDescending(kv => kv.Value)
                .Take((int)TopCount)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private static IDictionary<string, uint> GetTopWordsProducerConsumerEasier()
        {
            Console.WriteLine(nameof(GetTopWordsProducerConsumerEasier) + "...");

            const int WorkerCount = 12;
            const int BoundedCapacity = 10000;
            var result = new ConcurrentDictionary<string, uint>(StringComparer.InvariantCultureIgnoreCase);

            // Declare the worker
            Action<string> work = line =>
            {
                foreach (var word in line.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!IsValidWord(word)) { continue; }
                    result.AddOrUpdate(word, 1, (key, oldVal) => oldVal + 1);
                }
            };

            // Setup the queue
            var pcq = new ProducerConsumerQueue<string>(work, WorkerCount, BoundedCapacity);

            // Begin producing
            foreach (var line in File.ReadLines(InputFile.FullName))
            {
                pcq.Add(line);
            }
            pcq.CompleteAdding();
            // End of producing

            // Wait for workers to finish their work
            pcq.Completion.Wait();

            return result
                .OrderByDescending(kv => kv.Value)
                .Take((int)TopCount)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private static IDictionary<string, uint> GetTopWordsProducerConsumer()
        {
            Console.WriteLine(nameof(GetTopWordsProducerConsumer) + "...");

            const int WorkerCount = 12;
            const int BoundedCapacity = 10000;
            var result = new ConcurrentDictionary<string, uint>(StringComparer.InvariantCultureIgnoreCase);

            // Setup the queue
            var blockingCollection = new BlockingCollection<string>(BoundedCapacity);

            // Declare the worker
            Action work = () =>
            {
                foreach (var line in blockingCollection.GetConsumingEnumerable())
                {
                    foreach (var word in line.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (!IsValidWord(word)) { continue; }
                        result.AddOrUpdate(word, 1, (key, oldVal) => oldVal + 1);
                    }
                }
            };

            // Start the workers
            var tasks = Enumerable.Range(1, WorkerCount).Select(n => Task.Factory.StartNew(work, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default))
                .ToArray();

            // Begin producing
            foreach (var line in File.ReadLines(InputFile.FullName))
            {
                blockingCollection.Add(line);
            }
            blockingCollection.CompleteAdding();
            // End of producing

            // Wait for workers to finish their work
            Task.WaitAll(tasks);

            return result
                .OrderByDescending(kv => kv.Value)
                .Take((int)TopCount)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private static IDictionary<string, uint> GetTopWordsParallelForEachMapReduce()
        {
            Console.WriteLine(nameof(GetTopWordsParallelForEachMapReduce) + "...");

            var result = new Dictionary<string, uint>(StringComparer.InvariantCultureIgnoreCase);
            Parallel.ForEach(
                File.ReadLines(InputFile.FullName),
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                () => new Dictionary<string, uint>(StringComparer.InvariantCultureIgnoreCase),
                (line, state, index, localDic) =>
                {
                    foreach (var word in line.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (!IsValidWord(word)) { continue; }
                        TrackWordsOccurrence(localDic, word);
                    }
                    return localDic;
                },
                localDic =>
                {
                    lock (result)
                    {
                        foreach (var pair in localDic)
                        {
                            var key = pair.Key;
                            if (result.ContainsKey(key))
                            {
                                result[key] += pair.Value;
                            }
                            else
                            {
                                result[key] = pair.Value;
                            }
                        }
                    }
                }
            );

            return result
                .OrderByDescending(kv => kv.Value)
                .Take((int)TopCount)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private static IDictionary<string, uint> GetTopWordsParallelForEachConcurrentDictionary()
        {
            Console.WriteLine(nameof(GetTopWordsParallelForEachConcurrentDictionary) + "...");

            var result = new ConcurrentDictionary<string, uint>(StringComparer.InvariantCultureIgnoreCase);
            Parallel.ForEach(
                File.ReadLines(InputFile.FullName),
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                (line, state, index) =>
                {
                    foreach (var word in line.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (!IsValidWord(word)) { continue; }
                        result.AddOrUpdate(word, 1, (key, oldVal) => oldVal + 1);
                    }
                }
            );

            return result
                .OrderByDescending(kv => kv.Value)
                .Take((int)TopCount)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private static IDictionary<string, uint> GetTopWordsPLINQMapReduce()
        {
            Console.WriteLine(nameof(GetTopWordsPLINQMapReduce) + "...");

            return File.ReadLines(InputFile.FullName)
                .AsParallel()
                .Aggregate(
                    () => new Dictionary<string, uint>(StringComparer.InvariantCultureIgnoreCase), //#1
                    (localDic, line) => //#2
                    {
                        foreach (var word in line.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (!IsValidWord(word)) { continue; }
                            TrackWordsOccurrence(localDic, word);
                        }
                        return localDic;
                    },
                    (finalResult, localDic) => //#3
                    {
                        foreach (var pair in localDic)
                        {
                            var key = pair.Key;
                            if (finalResult.ContainsKey(key))
                            {
                                finalResult[key] += pair.Value;
                            }
                            else
                            {
                                finalResult[key] = pair.Value;
                            }
                        }
                        return finalResult;
                    },
                    finalResult => finalResult //#4
                        .OrderByDescending(kv => kv.Value)
                        .Take((int)TopCount)
                        .ToDictionary(kv => kv.Key, kv => kv.Value)
                );
        }

        private static IDictionary<string, uint> GetTopWordsPLINQ()
        {
            Console.WriteLine(nameof(GetTopWordsPLINQ) + "...");

            return File.ReadLines(InputFile.FullName)
                .AsParallel()
                .SelectMany(l => l.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                .Where(IsValidWord)
                .ToLookup(x => x, StringComparer.InvariantCultureIgnoreCase)
                .AsParallel()
                .Select(x => new { Word = x.Key, Count = (uint)x.Count() })
                .OrderByDescending(kv => kv.Count)
                .Take((int)TopCount)
                .ToDictionary(kv => kv.Word, kv => kv.Count);
        }

        private static IDictionary<string, uint> GetTopWordsPLINQNaive()
        {
            Console.WriteLine(nameof(GetTopWordsPLINQNaive) + "...");

            var words = File.ReadLines(InputFile.FullName)
                .AsParallel()
                .SelectMany(l => l.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                .Where(IsValidWord);

            var result = new Dictionary<string, uint>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var word in words)
            {
                if (!IsValidWord(word)) { continue; }
                TrackWordsOccurrence(result, word);
            }

            return result
                .OrderByDescending(kv => kv.Value)
                .Take((int)TopCount)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private static IDictionary<string, uint> GetTopWordsSequentialLINQ()
        {
            Console.WriteLine(nameof(GetTopWordsSequentialLINQ) + "...");

            return File.ReadLines(InputFile.FullName)
                .SelectMany(l => l.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                .Where(IsValidWord)
                .ToLookup(x => x, StringComparer.InvariantCultureIgnoreCase)
                .Select(x => new { Word = x.Key, Count = (uint)x.Count() })
                .OrderByDescending(kv => kv.Count)
                .Take((int)TopCount)
                .ToDictionary(kv => kv.Word, kv => kv.Count);
        }

        private static IDictionary<string, uint> GetTopWordsSequential()
        {
            Console.WriteLine(nameof(GetTopWordsSequential) + "...");

            var result = new Dictionary<string, uint>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var line in File.ReadLines(InputFile.FullName))
            {
                foreach (var word in line.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!IsValidWord(word)) { continue; }
                    TrackWordsOccurrence(result, word);
                }
            }

            return result
                .OrderByDescending(kv => kv.Value)
                .Take((int)TopCount)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        private static void TrackWordsOccurrence(IDictionary<string, uint> wordCounts, string word)
        {
            if (wordCounts.TryGetValue(word, out uint count))
            {
                wordCounts[word] = count + 1;
            }
            else
            {
                wordCounts[word] = 1;
            }
        }

        private static bool IsValidWord(string word) => !InvalidCharacters.Contains(word[0]) && !StopWords.Contains(word);
    }
}
