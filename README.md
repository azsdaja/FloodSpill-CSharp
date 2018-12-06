# FloodSpiller-CSharp — an open-source multi-purpose flood-filling algorithm for C#

### What can you do with it? ###
* run a **flood-fill in two-dimensional space**
* pass your own **conditions** for visiting positions and for stopping the flood
* pass your own **callbacks** that will be executed for visited positions
* use **LIFO, FIFO or priority queue** for deciding in what order positions should be processed
* use **scanline fill** to **double up execution speed**
* reuse position queue and the matrix used for marking visited positions to **minimize memory allocation**

### It is:
* fast
* easy to use
* elastic
* compatible with .NET Standard 2.0 and .NET Framework 3.5

### Code sample

```
private int[,] _positionMarkMatrix;

public void BucketFillImage(int floodStartX, int floodStartY, Color replacedColor, Color targetColor)
{
	var floodParameters = new FloodParameters(floodStartX, floodStartY)
	{
		BoundsRestriction = new FloodBounds(_imageSizeX, _imageSizeY),
		NeighbourhoodType = NeighbourhoodType.Four,
		Qualifier = (x, y) => GetColor(x, y) == replacedColor,
		NeighbourProcessor = (x, y, mark) => SetColor(x, y, targetColor),
		ProcessStartAsFirstNeighbour = true
	};

	var floodSpiller = new FloodSpiller();
	floodSpiller.SpillFlood(floodParameters, _positionMarkMatrix);
}
```

### Performance report (measured with [BenchmarkDotNet](https://benchmarkdotnet.org))

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

