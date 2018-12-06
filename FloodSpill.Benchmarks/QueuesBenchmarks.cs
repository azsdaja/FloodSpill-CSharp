using BenchmarkDotNet.Attributes;
using FloodSpill.Queues;

namespace FloodSpill.Benchmarks
{
	[ShortRunJob, MarkdownExporter, AsciiDocExporter, HtmlExporter, CsvExporter, RPlotExporter, RankColumn, MemoryDiagnoser
	]
	public class QueuesBenchmarks
	{
		private int[] _toAddX;
		private int[] _toAddY;

		[GlobalSetup]
		public void Setup()
		{
			_toAddX = new int[Elements];
			_toAddY = new int[Elements];

			for (int i = 0; i < Elements; i++)
			{
				_toAddX[i] = (i*i/4 - i*23 + 35) % 600;
				_toAddY[i] = (-i*i/2 + i*3 - 3245) % 500;
			}
		}

		[Params(10, 100, 1000, 10000)]
		public int Elements;
		[Benchmark, MemoryDiagnoser]
		public void TestQueue()
		{
			var queue = new FifoPositionQueue();

			for (int i = 0; i < Elements; i++)
			{
				queue.Enqueue(_toAddX[i], _toAddY[i]);
			}
		}

		[Benchmark, MemoryDiagnoser]
		public void TestStack()
		{
			var stack = new LifoPositionQueue();

			for (int i = 0; i < Elements; i++)
			{
				stack.Enqueue(_toAddX[i], _toAddY[i]);
			}
		}

		[Benchmark, MemoryDiagnoser]
		public void TestPriorityQueue()
		{
			var priorityQueue = new PriorityPositionQueue((first, second) => (first.X + first.Y).CompareTo(second.X + second.Y));

			for (int i = 0; i < Elements; i++)
			{
				priorityQueue.Enqueue(_toAddX[i], _toAddY[i]);
			}
		}

	}
}