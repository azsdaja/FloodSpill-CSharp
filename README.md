![logo](https://github.com/azsdaja/FloodSpill-CSharp/blob/master/icon48x48.png)

# FloodSpill — an open-source multi-purpose flood-filling algorithm for C#

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
* compatible with .NET Standard 2.0 and .NET Framework 3.5.

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

**For more instructions and code examples see [**Getting started**](https://github.com/azsdaja/FloodSpill-CSharp/wiki/Getting-started) section in wiki.**

---

### Performance report measured with [BenchmarkDotNet](https://benchmarkdotnet.org)
(with checking for wall presence by accessing a bool[,] matrix; measured on a good 2016 laptop with Intel i7-6700HQ)

| Area size |       Walls blocking flood | Mode |          Average execution time |
|--------- |--------------------- |-------------- |--------------:|
|       **20x20** |                 **No walls (open area)** |         **Normal** |      **32 µs** |
|       **20x20** |                 **No walls (open area)** |          **Scanline** |      **14 µs** |
|       **20x20** | **Sparse pillars (11% of area)** |         **Normal** |      **31 µs** |
|       **20x20** | **Sparse pillars (11% of area)** |          **Scanline** |      **25 µs** |
|       **20x20** | **Circular walls (50% of area)** |         **Normal** |      **19 µs** |
|       **20x20** | **Circular walls (50% of area)** |          **Scanline** |      **11 µs** |
|      **200x200** |                 **No walls (open area)** |         **Normal** |   **3,222 µs** |
|      **200x200** |                 **No walls (open area)** |          **Scanline** |   **1,249 µs** |
|      **200x200** | **Sparse pillars (11% of area)** |         **Normal** |   **2,868 µs** | 
|      **200x200** | **Sparse pillars (11% of area)** |          **Scanline** |   **2,624 µs** |
|      **200x200** | **Circular walls (50% of area)** |         **Normal** |   **1,877 µs** |
|      **200x200** | **Circular walls (50% of area)** |          **Scanline** |     **947 µs** |
|     **2000x2000** |                 **No walls (open area)** |         **Normal** | **355,445 µs** |
|     **2000x2000** |                 **No walls (open area)** |          **Scanline** | **124,329 µs** |
|     **2000x2000** | **Sparse pillars (11% of area)** |         **Normal** | **309,879 µs** |
|     **2000x2000** | **Sparse pillars (11% of area)** |          **Scanline** | **267,670 µs** |
|     **2000x2000** | **Circular walls (50% of area)** |         **Normal** | **204,312 µs** |
|     **2000x2000** | **Circular walls (50% of area)** |          **Scanline** |  **92,618 µs** |

