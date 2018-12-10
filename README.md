![logo](https://github.com/azsdaja/FloodSpill-CSharp/blob/master/icon48x48.png)

# FloodSpill — a free multi-purpose flood-fill algorithm for C#

### What can you do with it? ###
* run a **flood-fill in two-dimensional space**,
* pass your own **conditions** for visiting positions and for stopping the flood,
* pass your own **callbacks** that will be executed for visited positions,
* use **LIFO, FIFO or priority queue** for deciding in what order positions should be visited,
* decide if you allow **diagonal neighbourhood** of positions or not,
* use **scanline fill** to **double up execution speed**.

### It is:
* fast and memory efficient,
* easy to use,
* elastic,
* compatible with .NET Standard 1.6+ and .NET Framework 3.5+.

It can for example be used in games (roguelikes, RTS, RPG) to calculate so called **influence maps**, **scent maps**, **Dijkstra maps** et cætera.

---

### Usage example

```csharp
var wallMatrix = new bool[6, 5]; // setting up some obstacles for flood
wallMatrix[2, 0] = wallMatrix[2, 1] = wallMatrix[2, 2] 
	= wallMatrix[3, 0] = wallMatrix[3, 1] = wallMatrix[3, 2] = true;

Predicate<int, int> positionQualifier = (x, y) => wallMatrix[x, y] == false;

var floodParameters = new FloodParameters(startX: 0, startY: 0)
{
	Qualifier = positionQualifier
};
var markMatrix = new int[6, 5];

new FloodSpiller().SpillFlood(floodParameters, markMatrix);
```

Code above fills `markMatrix` with numbers indicating in how many steps the starting position is reachable:

![presentation](https://github.com/azsdaja/FloodSpill-CSharp/blob/master/flood_presentation.gif)

---

### More advanced example

``` csharp
private int[,] _positionMarkMatrix;

public void BucketFillImage(int floodStartX, int floodStartY, Color replacedColor, Color targetColor)
{
	var floodSpiller = new FloodSpiller();
	var floodParameters = new FloodParameters(floodStartX, floodStartY)
	{
		BoundsRestriction = new FloodBounds(_imageSizeX, _imageSizeY),
		NeighbourhoodType = NeighbourhoodType.Four,
		Qualifier = (x, y) => GetColor(x, y) == replacedColor,
		NeighbourProcessor = (x, y, mark) => SetColor(x, y, targetColor),
		ProcessStartAsFirstNeighbour = true
	};

	floodSpiller.SpillFlood(floodParameters, _positionMarkMatrix);
}
```

**For more instructions and code examples see [**Getting started**](https://github.com/azsdaja/FloodSpill-CSharp/wiki/Home) section in wiki.**

---

### Performance report measured with [BenchmarkDotNet](https://benchmarkdotnet.org)
(with checking for wall presence by accessing a bool[,] matrix; measured on a good 2016 laptop with Intel i7-6700HQ)

| Area size |       Walls blocking flood | Mode |          Mean execution time |    Allocated memory |
|--------- |--------------------- |-------------- |--------------:|--------------:|
|       **20x20** |                 **No walls (open area)** |         **Normal** |      **33 µs** | < 1kB|
|       **20x20** |                 **No walls (open area)** |          **Scanline** |      **15 µs** |  < 1kB|
|       **20x20** | **Sparse pillars (11% of area)** |         **Normal** |      **33 µs** |  < 1kB |
|       **20x20** | **Sparse pillars (11% of area)** |          **Scanline** |      **23 µs** |  < 1kB|
|       **20x20** | **Circular walls (50% of area)** |         **Normal** |      **20 µs** |  < 1kB|
|       **20x20** | **Circular walls (50% of area)** |          **Scanline** |      **10 µs** |  < 1kB|
|      **200x200** |                 **No walls (open area)** |         **Normal** |   **3,458 µs** | 16 kB|
|      **200x200** |                 **No walls (open area)** |          **Scanline** |   **1,158 µs** |  < 1kB|
|      **200x200** | **Sparse pillars (11% of area)** |         **Normal** |   **3,072 µs** | 16 kB|
|      **200x200** | **Sparse pillars (11% of area)** |          **Scanline** |   **2,430 µs** | 8 kB|
|      **200x200** | **Circular walls (50% of area)** |         **Normal** |   **2,031 µs** | 8 kB|
|      **200x200** | **Circular walls (50% of area)** |          **Scanline** |     **879 µs** |  < 1kB|
|     **2000x2000** |                 **No walls (open area)** |         **Normal** | **371,000 µs** | 131 kB|
|     **2000x2000** |                 **No walls (open area)** |          **Scanline** | **117,000 µs** | < 1kB|
|     **2000x2000** | **Sparse pillars (11% of area)** |         **Normal** | **328,000 µs** | 131 kB|
|     **2000x2000** | **Sparse pillars (11% of area)** |          **Scanline** | **262,670 µs** | 66 kB|
|     **2000x2000** | **Circular walls (50% of area)** |         **Normal** | **216,312 µs** | 66 kB|
|     **2000x2000** | **Circular walls (50% of area)** |          **Scanline** |  **88,618 µs** | 8 kB|

