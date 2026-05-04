using System;

namespace Spine3_8_95;

public class ScaleTimeline : TranslateTimeline, IBoneTimeline
{
	public override int PropertyId => 33554432 + boneIndex;

	public ScaleTimeline(int frameCount)
		: base(frameCount)
	{
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
				bone.scaleX = bone.data.scaleX;
				bone.scaleY = bone.data.scaleY;
				break;
			case MixBlend.First:
				bone.scaleX += (bone.data.scaleX - bone.scaleX) * alpha;
				bone.scaleY += (bone.data.scaleY - bone.scaleY) * alpha;
				break;
			}
			return;
		}
		float num;
		float num2;
		if (time >= array[array.Length - 3])
		{
			num = array[array.Length + -2] * bone.data.scaleX;
			num2 = array[array.Length + -1] * bone.data.scaleY;
		}
		else
		{
			int num3 = Animation.BinarySearch(array, time, 3);
			num = array[num3 + -2];
			num2 = array[num3 + -1];
			float num4 = array[num3];
			float curvePercent = GetCurvePercent(num3 / 3 - 1, 1f - (time - num4) / (array[num3 + -3] - num4));
			num = (num + (array[num3 + 1] - num) * curvePercent) * bone.data.scaleX;
			num2 = (num2 + (array[num3 + 2] - num2) * curvePercent) * bone.data.scaleY;
		}
		if (alpha == 1f)
		{
			if (blend == MixBlend.Add)
			{
				bone.scaleX += num - bone.data.scaleX;
				bone.scaleY += num2 - bone.data.scaleY;
			}
			else
			{
				bone.scaleX = num;
				bone.scaleY = num2;
			}
		}
		else if (direction == MixDirection.Out)
		{
			switch (blend)
			{
			case MixBlend.Setup:
			{
				float scaleX = bone.data.scaleX;
				float scaleY = bone.data.scaleY;
				bone.scaleX = scaleX + (Math.Abs(num) * (float)Math.Sign(scaleX) - scaleX) * alpha;
				bone.scaleY = scaleY + (Math.Abs(num2) * (float)Math.Sign(scaleY) - scaleY) * alpha;
				break;
			}
			case MixBlend.First:
			case MixBlend.Replace:
			{
				float scaleX = bone.scaleX;
				float scaleY = bone.scaleY;
				bone.scaleX = scaleX + (Math.Abs(num) * (float)Math.Sign(scaleX) - scaleX) * alpha;
				bone.scaleY = scaleY + (Math.Abs(num2) * (float)Math.Sign(scaleY) - scaleY) * alpha;
				break;
			}
			case MixBlend.Add:
			{
				float scaleX = bone.scaleX;
				float scaleY = bone.scaleY;
				bone.scaleX = scaleX + (Math.Abs(num) * (float)Math.Sign(scaleX) - bone.data.scaleX) * alpha;
				bone.scaleY = scaleY + (Math.Abs(num2) * (float)Math.Sign(scaleY) - bone.data.scaleY) * alpha;
				break;
			}
			}
		}
		else
		{
			switch (blend)
			{
			case MixBlend.Setup:
			{
				float scaleX = Math.Abs(bone.data.scaleX) * (float)Math.Sign(num);
				float scaleY = Math.Abs(bone.data.scaleY) * (float)Math.Sign(num2);
				bone.scaleX = scaleX + (num - scaleX) * alpha;
				bone.scaleY = scaleY + (num2 - scaleY) * alpha;
				break;
			}
			case MixBlend.First:
			case MixBlend.Replace:
			{
				float scaleX = Math.Abs(bone.scaleX) * (float)Math.Sign(num);
				float scaleY = Math.Abs(bone.scaleY) * (float)Math.Sign(num2);
				bone.scaleX = scaleX + (num - scaleX) * alpha;
				bone.scaleY = scaleY + (num2 - scaleY) * alpha;
				break;
			}
			case MixBlend.Add:
			{
				float scaleX = Math.Sign(num);
				float scaleY = Math.Sign(num2);
				bone.scaleX = Math.Abs(bone.scaleX) * scaleX + (num - Math.Abs(bone.data.scaleX) * scaleX) * alpha;
				bone.scaleY = Math.Abs(bone.scaleY) * scaleY + (num2 - Math.Abs(bone.data.scaleY) * scaleY) * alpha;
				break;
			}
			}
		}
	}
}
