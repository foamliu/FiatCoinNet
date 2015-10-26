using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FiatCoinNet.Stress
{
    public class Stress
    {
        public const int Clients = 32;
        public const int DurationMinute = 8 * 60;

        ///------------------------------------------------------------------------------
        /// Private Fields
        ///------------------------------------------------------------------------------
        #region Private Static Fields
        private static long SuccessCount;
        private static long TotalLatency;
        private static long TimeoutCount;
        private static long ExceptionCount;
        #endregion

        public static string DoWork(Action action)
        {
            Reset();

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            Task[] tasks = new Task[Clients];

            for (var i = 0; i < Clients; i++)
            {
                tasks[i] = Task.Run(() => DoWorkSingleThread(action, token), token);
            }

            Thread.Sleep(DurationMinute * 60 * 1000);
            tokenSource.Cancel();
            Task.WaitAll(tasks);

            // output result
            var result = new TestResult()
            {
                RequestsPerSecond = 1.0 * SuccessCount / (DurationMinute * 60.0),
                AverageLatency = 1.0 * TotalLatency / SuccessCount,
                TimeoutsPerMinute = 1.0 * TimeoutCount / DurationMinute,
                ExceptionsPerMinute = 1.0 * ExceptionCount / DurationMinute
            };

            return JsonConvert.SerializeObject(result);
        }

        private static void DoWorkSingleThread(Action action, CancellationToken token)
        {
            while (true)
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    action();
                    Interlocked.Increment(ref SuccessCount);
                }
                catch (TimeoutException)
                {
                    Interlocked.Increment(ref TimeoutCount);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref ExceptionCount);
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    stopwatch.Stop();
                }
                Interlocked.Add(ref TotalLatency, stopwatch.ElapsedMilliseconds);

                if (token.IsCancellationRequested)
                    break;
            }
        }

        private static void Reset()
        {
            SuccessCount = 0;
            TotalLatency = 0;
            TimeoutCount = 0;
            ExceptionCount = 0;
        }
    }

    public class TestResult
    {
        [JsonProperty(PropertyName = "requests_per_second")]
        public double RequestsPerSecond { get; set; }

        [JsonProperty(PropertyName = "average_latency")]
        public double AverageLatency { get; set; } //ms

        [JsonProperty(PropertyName = "timeouts_per_minute")]
        public double TimeoutsPerMinute { get; set; }

        [JsonProperty(PropertyName = "exceptions_per_minute")]
        public double ExceptionsPerMinute { get; set; }
    }
}
