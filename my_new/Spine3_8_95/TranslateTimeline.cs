using System;

namespace Spine3_8_95;

public class TranslateTimeline : CurveTimeline, IBoneTimeline
{
	public const int ENTRIES = 3;

	protected const int PREV_TIME = -3;

	protected const int PREV_X = -2;

	protected const int PREV_Y = -1;

	protected const int X = 1;

	protected const int Y = 2;

	internal int boneIndex;

	internal float[] frames;

	public override int PropertyId => 16777216 + boneIndex;

	public int BoneIndex
	{
		get
		{
			return boneIndex;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("index must be >= 0.");
			}
			boneIndex = value;
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

	public TranslateTimeline(int frameCount)
		: base(frameCount)
	{
		frames = new float[frameCount * 3];
	}

	public void SetFrame(int frameIndex, float time, float x, float y)
	{
		frameIndex *= 3;
		frames[frameIndex] = time;
		frames[frameIndex + 1] = x;
		frames[frameIndex + 2] = y;
	}

	public override void Apply(Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend, MixDirection direction)
	{
		Bone bone = skeleton.bones.Items[boneIndex];
		if (!bone.active)
		{
			return;
		}
		float[] array = frames;
		if (time < array[0])
		{
			switch (blend)
			{
			case MixBlend.Setup:
				bone.x = bone.data.x;
				bone.y = bone.data.y;
				break;
			case MixBlend.First:
				bone.x += (bone.data.x - bone.x) * alpha;
				bone.y += (bone.data.y - bone.y) * alpha;
				break;
			}
			return;
		}
		float num;
		float num2;
		if (time >= array[array.Length - 3])
		{
			num = array[array.Length + -2];
			num2 = array[array.Length + -1];
		}
		else
		{
			int num3 = Animation.BinarySearch(array, time, 3);
			num = array[num3 + -2];
			num2 = array[num3 + -1];
			float num4 = array[num3];
			float curvePercent = GetCurvePercent(num3 / 3 - 1, 1f - (time - num4) / (array[num3 + -3] - num4));
			num += (array[num3 + 1] - num) * curvePercent;
			num2 += (array[num3 + 2] - num2) * curvePercent;
		}
		switch (blend)
		{
		case MixBlend.Setup:
			bone.x = bone.data.x + num * alpha;
			bone.y = bone.data.y + num2 * alpha;
			break;
		case MixBlend.First:
		case MixBlend.Replace:
			bone.x += (bone.data.x + num - bone.x) * alpha;
			bone.y += (bone.data.y + num2 - bone.y) * alpha;
			break;
		case MixBlend.Add:
			bone.x += num * alpha;
			bone.y += num2 * alpha;
			break;
		}
	}
}
