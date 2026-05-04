namespace Spine3_8_95;

public class PathConstraintSpacingTimeline : PathConstraintPositionTimeline
{
	public override int PropertyId => 201326592 + pathConstraintIndex;

	public PathConstraintSpacingTimeline(int frameCount)
		: base(frameCount)
	{
	}

	public override void Apply(Skeleton skeleton, float lastTime, float time, ExposedList<Event> events, float alpha, MixBlend blend, MixDirection direction)
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
				pathConstraint.spacing = pathConstraint.data.spacing;
				break;
			case MixBlend.First:
				pathConstraint.spacing += (pathConstraint.data.spacing - pathConstraint.spacing) * alpha;
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
			pathConstraint.spacing = pathConstraint.data.spacing + (num - pathConstraint.data.spacing) * alpha;
		}
		else
		{
			pathConstraint.spacing += (num - pathConstraint.spacing) * alpha;
		}
	}
}
