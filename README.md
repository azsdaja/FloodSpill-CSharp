# FloodSpiller-CSharp — an open-source multi-purpose flood-filling algorithm for C#

### What can you do with it? ###
* run a flood-fill in two-dimensional space
* pass your own conditions for visiting positions and for stopping the flood
* pass your own functions that will be executed for visited positions
* use LIFO, FIFO or priority queue for deciding in what order positions should be processed
* use scanline fill to double up execution speed
* reuse matrix used for marking visited positions to save memory

### It is:
* fast
* easy to use
* elastic
* running on .NET Framework 3.5+

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
