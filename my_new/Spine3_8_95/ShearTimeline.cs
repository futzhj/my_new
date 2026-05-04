namespace Spine3_8_95;

public class ShearTimeline : TranslateTimeline, IBoneTimeline
{
	public override int PropertyId => 50331648 + boneIndex;

	public ShearTimeline(int frameCount)
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
				bone.shearX = bone.data.shearX;
				bone.shearY = bone.data.shearY;
				break;
			case MixBlend.First:
				bone.shearX += (bone.data.shearX - bone.shearX) * alpha;
				bone.shearY += (bone.data.shearY - bone.shearY) * alpha;
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
			bone.shearX = bone.data.shearX + num * alpha;
			bone.shearY = bone.data.shearY + num2 * alpha;
			break;
		case MixBlend.First:
		case MixBlend.Replace:
			bone.shearX += (bone.data.shearX + num - bone.shearX) * alpha;
			bone.shearY += (bone.data.shearY + num2 - bone.shearY) * alpha;
			break;
		case MixBlend.Add:
			bone.shearX += num * alpha;
			bone.shearY += num2 * alpha;
			break;
		}
	}
}
