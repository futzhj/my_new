using System;

namespace Spine3_8_95;

public class PathConstraintPositionTimeline : CurveTimeline
{
	public const int ENTRIES = 2;

	protected const int PREV_TIME = -2;

	protected const int PREV_VALUE = -1;

	protected const int VALUE = 1;

	internal int pathConstraintIndex;

	internal float[] frames;

	public override int PropertyId => 184549376 + pathConstraintIndex;

	public int PathConstraintIndex
	{
		get
		{
			return pathConstraintIndex;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("index must be >= 0.");
			}
			pathConstraintIndex = value;
		}
	}

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

	public PathConstraintPositionTimeline(int frameCount)
		: base(frameCount)
	{
		frames = new float[frameCount * 2];
	}

	public void SetFrame(int frameIndex, float time, float position)
	{
		frameIndex *= 2;
		frames[frameIndex] = time;
		frames[frameIndex + 1] = position;
	}

	public override void Apply(Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend, MixDirection direction)
	{
		PathConstraint pathConstraint = skeleton.pathConstraints.Items[pathConstraintIndex];
		if (!pathConstraint.active)
		{
			return;
		}
		float[] array = frames;
		if (time < array[0])
		{
			switch (blend)
			{
			case MixBlend.Setup:
				pathConstraint.position = pathConstraint.data.position;
				break;
			case MixBlend.First:
				pathConstraint.position += (pathConstraint.data.position - pathConstraint.position) * alpha;
				break;
			}
			return;
		}
		float num;
		if (time >= array[array.Length - 2])
		{
			num = array[array.Length + -1];
		}
		else
		{
			int num2 = Animation.BinarySearch(array, time, 2);
			num = array[num2 + -1];
			float num3 = array[num2];
			float curvePercent = GetCurvePercent(num2 / 2 - 1, 1f - (time - num3) / (array[num2 + -2] - num3));
			num += (array[num2 + 1] - num) * curvePercent;
		}
		if (blend == MixBlend.Setup)
		{
			pathConstraint.position = pathConstraint.data.position + (num - pathConstraint.data.position) * alpha;
		}
		else
		{
			pathConstraint.position += (num - pathConstraint.position) * alpha;
		}
	}
}
