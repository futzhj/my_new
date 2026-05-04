using System;

namespace Spine3_8_95;

public class DrawOrderTimeline : Timeline
{
	internal float[] frames;

	private int[][] drawOrders;

	public int PropertyId => 134217728;

	public int FrameCount => frames.Length;

	public float[] Frames
	{
		get
		{
			return frames;
		}
		set
		{
			frames = value;
		}
	}

	public int[][] DrawOrders
	{
		get
		{
			return drawOrders;
		}
		set
		{
			drawOrders = value;
		}
	}

	public DrawOrderTimeline(int frameCount)
	{
		frames = new float[frameCount];
		drawOrders = new int[frameCount][];
	}

	public void SetFrame(int frameIndex, float time, int[] drawOrder)
	{
		frames[frameIndex] = time;
		drawOrders[frameIndex] = drawOrder;
	}

	public void Apply(Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend, MixDirection direction)
	{
		ExposedList<Slot> drawOrder = skeleton.drawOrder;
		ExposedList<Slot> slots = skeleton.slots;
		if (direction == MixDirection.Out)
		{
			if (blend == MixBlend.Setup)
			{
				Array.Copy(slots.Items, 0, drawOrder.Items, 0, slots.Count);
			}
			return;
		}
		float[] array = frames;
		if (time < array[0])
		{
			if (blend == MixBlend.Setup || blend == MixBlend.First)
			{
				Array.Copy(slots.Items, 0, drawOrder.Items, 0, slots.Count);
			}
			return;
		}
		int num = ((!(time >= array[array.Length - 1])) ? (Animation.BinarySearch(array, time) - 1) : (array.Length - 1));
		int[] array2 = drawOrders[num];
		if (array2 == null)
		{
			Array.Copy(slots.Items, 0, drawOrder.Items, 0, slots.Count);
			return;
		}
		Slot[] items = drawOrder.Items;
		Slot[] items2 = slots.Items;
		int i = 0;
		for (int num2 = array2.Length; i < num2; i++)
		{
			items[i] = items2[array2[i]];
		}
	}
}
