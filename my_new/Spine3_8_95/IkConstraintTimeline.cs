using System;

namespace Spine3_8_95;

public class IkConstraintTimeline : CurveTimeline
{
	public const int ENTRIES = 6;

	private const int PREV_TIME = -6;

	private const int PREV_MIX = -5;

	private const int PREV_SOFTNESS = -4;

	private const int PREV_BEND_DIRECTION = -3;

	private const int PREV_COMPRESS = -2;

	private const int PREV_STRETCH = -1;

	private const int MIX = 1;

	private const int SOFTNESS = 2;

	private const int BEND_DIRECTION = 3;

	private const int COMPRESS = 4;

	private const int STRETCH = 5;

	internal int ikConstraintIndex;

	internal float[] frames;

	public override int PropertyId => 150994944 + ikConstraintIndex;

	public int IkConstraintIndex
	{
		get
		{
			return ikConstraintIndex;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("index must be >= 0.");
			}
			ikConstraintIndex = value;
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

	public IkConstraintTimeline(int frameCount)
		: base(frameCount)
	{
		frames = new float[frameCount * 6];
	}

	public void SetFrame(int frameIndex, float time, float mix, float softness, int bendDirection, bool compress, bool stretch)
	{
		frameIndex *= 6;
		frames[frameIndex] = time;
		frames[frameIndex + 1] = mix;
		frames[frameIndex + 2] = softness;
		frames[frameIndex + 3] = bendDirection;
		frames[frameIndex + 4] = (compress ? 1 : 0);
		frames[frameIndex + 5] = (stretch ? 1 : 0);
	}

	public override void Apply(Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend, MixDirection direction)
	{
		IkConstraint ikConstraint = skeleton.ikConstraints.Items[ikConstraintIndex];
		if (!ikConstraint.active)
		{
			return;
		}
		float[] array = frames;
		if (time < array[0])
		{
			switch (blend)
			{
			case MixBlend.Setup:
				ikConstraint.mix = ikConstraint.data.mix;
				ikConstraint.softness = ikConstraint.data.softness;
				ikConstraint.bendDirection = ikConstraint.data.bendDirection;
				ikConstraint.compress = ikConstraint.data.compress;
				ikConstraint.stretch = ikConstraint.data.stretch;
				break;
			case MixBlend.First:
				ikConstraint.mix += (ikConstraint.data.mix - ikConstraint.mix) * alpha;
				ikConstraint.softness += (ikConstraint.data.softness - ikConstraint.softness) * alpha;
				ikConstraint.bendDirection = ikConstraint.data.bendDirection;
				ikConstraint.compress = ikConstraint.data.compress;
				ikConstraint.stretch = ikConstraint.data.stretch;
				break;
			}
			return;
		}
		if (time >= array[array.Length - 6])
		{
			if (blend == MixBlend.Setup)
			{
				ikConstraint.mix = ikConstraint.data.mix + (array[array.Length + -5] - ikConstraint.data.mix) * alpha;
				ikConstraint.softness = ikConstraint.data.softness + (array[array.Length + -4] - ikConstraint.data.softness) * alpha;
				if (direction == MixDirection.Out)
				{
					ikConstraint.bendDirection = ikConstraint.data.bendDirection;
					ikConstraint.compress = ikConstraint.data.compress;
					ikConstraint.stretch = ikConstraint.data.stretch;
				}
				else
				{
					ikConstraint.bendDirection = (int)array[array.Length + -3];
					ikConstraint.compress = array[array.Length + -2] != 0f;
					ikConstraint.stretch = array[array.Length + -1] != 0f;
				}
			}
			else
			{
				ikConstraint.mix += (array[array.Length + -5] - ikConstraint.mix) * alpha;
				ikConstraint.softness += (array[array.Length + -4] - ikConstraint.softness) * alpha;
				if (direction == MixDirection.In)
				{
					ikConstraint.bendDirection = (int)array[array.Length + -3];
					ikConstraint.compress = array[array.Length + -2] != 0f;
					ikConstraint.stretch = array[array.Length + -1] != 0f;
				}
			}
			return;
		}
		int num = Animation.BinarySearch(array, time, 6);
		float num2 = array[num + -5];
		float num3 = array[num + -4];
		float num4 = array[num];
		float curvePercent = GetCurvePercent(num / 6 - 1, 1f - (time - num4) / (array[num + -6] - num4));
		if (blend == MixBlend.Setup)
		{
			ikConstraint.mix = ikConstraint.data.mix + (num2 + (array[num + 1] - num2) * curvePercent - ikConstraint.data.mix) * alpha;
			ikConstraint.softness = ikConstraint.data.softness + (num3 + (array[num + 2] - num3) * curvePercent - ikConstraint.data.softness) * alpha;
			if (direction == MixDirection.Out)
			{
				ikConstraint.bendDirection = ikConstraint.data.bendDirection;
				ikConstraint.compress = ikConstraint.data.compress;
				ikConstraint.stretch = ikConstraint.data.stretch;
			}
			else
			{
				ikConstraint.bendDirection = (int)array[num + -3];
				ikConstraint.compress = array[num + -2] != 0f;
				ikConstraint.stretch = array[num + -1] != 0f;
			}
		}
		else
		{
			ikConstraint.mix += (num2 + (array[num + 1] - num2) * curvePercent - ikConstraint.mix) * alpha;
			ikConstraint.softness += (num3 + (array[num + 2] - num3) * curvePercent - ikConstraint.softness) * alpha;
			if (direction == MixDirection.In)
			{
				ikConstraint.bendDirection = (int)array[num + -3];
				ikConstraint.compress = array[num + -2] != 0f;
				ikConstraint.stretch = array[num + -1] != 0f;
			}
		}
	}
}
