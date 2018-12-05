using System.Linq;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Loggers;

namespace FloodSpiller.Benchmarks
{
	public class BenchmarkRunner
	{
		private class CustomConfig : ManualConfig
		{
			public CustomConfig()
			{
				Add(MemoryDiagnoser.Default);
				Add(ConsoleLogger.Default);
				Add(DefaultColumnProviders.Instance);
			}
		}

		public static void Main(string[] args)
		{
			var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<FloodSpillerBenchmarks>(new CustomConfig());
			//var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<QueuesBenchmarks>(new CustomConfig());
		}
	}
}