using System;

namespace Spine3_8_95;

public class IkConstraint : IUpdatable
{
	internal IkConstraintData data;

	internal ExposedList<Bone> bones = new ExposedList<Bone>();

	internal Bone target;

	internal int bendDirection;

	internal bool compress;

	internal bool stretch;

	internal float mix = 1f;

	internal float softness;

	internal bool active;

	public ExposedList<Bone> Bones => bones;

	public Bone Target
	{
		get
		{
			return target;
		}
		set
		{
			target = value;
		}
	}

	public float Mix
	{
		get
		{
			return mix;
		}
		set
		{
			mix = value;
		}
	}

	public float Softness
	{
		get
		{
			return softness;
		}
		set
		{
			softness = value;
		}
	}

	public int BendDirection
	{
		get
		{
			return bendDirection;
		}
		set
		{
			bendDirection = value;
		}
	}

	public bool Compress
	{
		get
		{
			return compress;
		}
		set
		{
			compress = value;
		}
	}

	public bool Stretch
	{
		get
		{
			return stretch;
		}
		set
		{
			stretch = value;
		}
	}

	public bool Active => active;

	public IkConstraintData Data => data;

	public IkConstraint(IkConstraintData data, Skeleton skeleton)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data", "data cannot be null.");
		}
		if (skeleton == null)
		{
			throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
		}
		this.data = data;
		mix = data.mix;
		softness = data.softness;
		bendDirection = data.bendDirection;
		compress = data.compress;
		stretch = data.stretch;
		bones = new ExposedList<Bone>(data.bones.Count);
		foreach (BoneData bone in data.bones)
		{
			bones.Add(skeleton.FindBone(bone.name));
		}
		target = skeleton.FindBone(data.target.name);
	}

	public IkConstraint(IkConstraint constraint, Skeleton skeleton)
	{
		if (constraint == null)
		{
			throw new ArgumentNullException("constraint cannot be null.");
		}
		if (skeleton == null)
		{
			throw new ArgumentNullException("skeleton cannot be null.");
		}
		data = constraint.data;
		bones = new ExposedList<Bone>(constraint.Bones.Count);
		foreach (Bone bone in constraint.Bones)
		{
			bones.Add(skeleton.Bones.Items[bone.data.index]);
		}
		target = skeleton.Bones.Items[constraint.target.data.index];
		mix = constraint.mix;
		softness = constraint.softness;
		bendDirection = constraint.bendDirection;
		compress = constraint.compress;
		stretch = constraint.stretch;
	}

	public void Apply()
	{
		Update();
	}

	public void Update()
	{
		Bone bone = target;
		ExposedList<Bone> exposedList = bones;
		switch (exposedList.Count)
		{
		case 1:
			Apply(exposedList.Items[0], bone.worldX, bone.worldY, compress, stretch, data.uniform, mix);
			break;
		case 2:
			Apply(exposedList.Items[0], exposedList.Items[1], bone.worldX, bone.worldY, bendDirection, stretch, softness, mix);
			break;
		}
	}

	public override string ToString()
	{
		return data.name;
	}

	public static void Apply(Bone bone, float targetX, float targetY, bool compress, bool stretch, bool uniform, float alpha)
	{
		if (!bone.appliedValid)
		{
			bone.UpdateAppliedTransform();
		}
		Bone parent = bone.parent;
		float a = parent.a;
		float num = parent.b;
		float c = parent.c;
		float num2 = parent.d;
		float num3 = 0f - bone.ashearX - bone.arotation;
		float num4 = 0f;
		float num5 = 0f;
		TransformMode transformMode = bone.data.transformMode;
		if (transformMode != TransformMode.NoRotationOrReflection)
		{
			if (transformMode == TransformMode.OnlyTranslation)
			{
				num4 = targetX - bone.worldX;
				num5 = targetY - bone.worldY;
				goto IL_0134;
			}
		}
		else
		{
			float num6 = Math.Abs(a * num2 - num * c) / (a * a + c * c);
			float num7 = a / bone.skeleton.ScaleX;
			num = (0f - c / bone.skeleton.ScaleY) * num6 * bone.skeleton.ScaleX;
			num2 = num7 * num6 * bone.skeleton.ScaleY;
			num3 += (float)Math.Atan2(c, a) * (180f / (float)Math.PI);
		}
		float num8 = targetX - parent.worldX;
		float num9 = targetY - parent.worldY;
		float num10 = a * num2 - num * c;
		num4 = (num8 * num2 - num9 * num) / num10 - bone.ax;
		num5 = (num9 * a - num8 * c) / num10 - bone.ay;
		goto IL_0134;
		IL_0134:
		num3 += (float)Math.Atan2(num5, num4) * (180f / (float)Math.PI);
		if (bone.ascaleX < 0f)
		{
			num3 += 180f;
		}
		if (num3 > 180f)
		{
			num3 -= 360f;
		}
		else if (num3 < -180f)
		{
			num3 += 360f;
		}
		float num11 = bone.ascaleX;
		float num12 = bone.ascaleY;
		if (compress || stretch)
		{
			switch (bone.data.transformMode)
			{
			case TransformMode.NoScale:
				num4 = targetX - bone.worldX;
				num5 = targetY - bone.worldY;
				break;
			case TransformMode.NoScaleOrReflection:
				num4 = targetX - bone.worldX;
				num5 = targetY - bone.worldY;
				break;
			}
			float num13 = bone.data.length * num11;
			float num14 = (float)Math.Sqrt(num4 * num4 + num5 * num5);
			if ((compress && num14 < num13) || (stretch && num14 > num13 && num13 > 0.0001f))
			{
				float num15 = (num14 / num13 - 1f) * alpha + 1f;
				num11 *= num15;
				if (uniform)
				{
					num12 *= num15;
				}
			}
		}
		bone.UpdateWorldTransform(bone.ax, bone.ay, bone.arotation + num3 * alpha, num11, num12, bone.ashearX, bone.ashearY);
	}

	public static void Apply(Bone parent, Bone child, float targetX, float targetY, int bendDir, bool stretch, float softness, float alpha)
	{
		if (alpha == 0f)
		{
			child.UpdateWorldTransform();
			return;
		}
		if (!parent.appliedValid)
		{
			parent.UpdateAppliedTransform();
		}
		if (!child.appliedValid)
		{
			child.UpdateAppliedTransform();
		}
		float ax = parent.ax;
		float ay = parent.ay;
		float num = parent.ascaleX;
		float num2 = num;
		float num3 = parent.ascaleY;
		float num4 = child.ascaleX;
		int num5;
		int num6;
		if (num < 0f)
		{
			num = 0f - num;
			num5 = 180;
			num6 = -1;
		}
		else
		{
			num5 = 0;
			num6 = 1;
		}
		if (num3 < 0f)
		{
			num3 = 0f - num3;
			num6 = -num6;
		}
		int num7;
		if (num4 < 0f)
		{
			num4 = 0f - num4;
			num7 = 180;
		}
		else
		{
			num7 = 0;
		}
		float ax2 = child.ax;
		float a = parent.a;
		float b = parent.b;
		float c = parent.c;
		float d = parent.d;
		bool flag = Math.Abs(num - num3) <= 0.0001f;
		float num8;
		float num9;
		float num10;
		if (!flag)
		{
			num8 = 0f;
			num9 = a * ax2 + parent.worldX;
			num10 = c * ax2 + parent.worldY;
		}
		else
		{
			num8 = child.ay;
			num9 = a * ax2 + b * num8 + parent.worldX;
			num10 = c * ax2 + d * num8 + parent.worldY;
		}
		Bone parent2 = parent.parent;
		a = parent2.a;
		b = parent2.b;
		c = parent2.c;
		d = parent2.d;
		float num11 = 1f / (a * d - b * c);
		float num12 = num9 - parent2.worldX;
		float num13 = num10 - parent2.worldY;
		float num14 = (num12 * d - num13 * b) * num11 - ax;
		float num15 = (num13 * a - num12 * c) * num11 - ay;
		float num16 = (float)Math.Sqrt(num14 * num14 + num15 * num15);
		float num17 = child.data.length * num4;
		if (num16 < 0.0001f)
		{
			Apply(parent, targetX, targetY, compress: false, stretch, uniform: false, alpha);
			child.UpdateWorldTransform(ax2, num8, 0f, child.ascaleX, child.ascaleY, child.ashearX, child.ashearY);
			return;
		}
		num12 = targetX - parent2.worldX;
		num13 = targetY - parent2.worldY;
		float num18 = (num12 * d - num13 * b) * num11 - ax;
		float num19 = (num13 * a - num12 * c) * num11 - ay;
		float num20 = num18 * num18 + num19 * num19;
		if (softness != 0f)
		{
			softness *= num * (num4 + 1f) / 2f;
			float num21 = (float)Math.Sqrt(num20);
			float num22 = num21 - num16 - num17 * num + softness;
			if (num22 > 0f)
			{
				float num23 = Math.Min(1f, num22 / (softness * 2f)) - 1f;
				num23 = (num22 - softness * (1f - num23 * num23)) / num21;
				num18 -= num23 * num18;
				num19 -= num23 * num19;
				num20 = num18 * num18 + num19 * num19;
			}
		}
		float num26;
		float num25;
		if (flag)
		{
			num17 *= num;
			float num24 = (num20 - num16 * num16 - num17 * num17) / (2f * num16 * num17);
			if (num24 < -1f)
			{
				num24 = -1f;
			}
			else if (num24 > 1f)
			{
				num24 = 1f;
				if (stretch)
				{
					num2 *= ((float)Math.Sqrt(num20) / (num16 + num17) - 1f) * alpha + 1f;
				}
			}
			num25 = (float)Math.Acos(num24) * (float)bendDir;
			a = num16 + num17 * num24;
			b = num17 * (float)Math.Sin(num25);
			num26 = (float)Math.Atan2(num19 * a - num18 * b, num18 * a + num19 * b);
		}
		else
		{
			a = num * num17;
			b = num3 * num17;
			float num27 = a * a;
			float num28 = b * b;
			float num29 = (float)Math.Atan2(num19, num18);
			c = num28 * num16 * num16 + num27 * num20 - num27 * num28;
			float num30 = -2f * num28 * num16;
			float num31 = num28 - num27;
			d = num30 * num30 - 4f * num31 * c;
			if (d >= 0f)
			{
				float num32 = (float)Math.Sqrt(d);
				if (num30 < 0f)
				{
					num32 = 0f - num32;
				}
				num32 = (0f - (num30 + num32)) / 2f;
				float num33 = num32 / num31;
				float num34 = c / num32;
				float num35 = ((Math.Abs(num33) < Math.Abs(num34)) ? num33 : num34);
				if (num35 * num35 <= num20)
				{
					num13 = (float)Math.Sqrt(num20 - num35 * num35) * (float)bendDir;
					num26 = num29 - (float)Math.Atan2(num13, num35);
					num25 = (float)Math.Atan2(num13 / num3, (num35 - num16) / num);
					goto IL_05b6;
				}
			}
			float num36 = (float)Math.PI;
			float num37 = num16 - a;
			float num38 = num37 * num37;
			float num39 = 0f;
			float num40 = 0f;
			float num41 = num16 + a;
			float num42 = num41 * num41;
			float num43 = 0f;
			c = (0f - a) * num16 / (num27 - num28);
			if (c >= -1f && c <= 1f)
			{
				c = (float)Math.Acos(c);
				num12 = a * (float)Math.Cos(c) + num16;
				num13 = b * (float)Math.Sin(c);
				d = num12 * num12 + num13 * num13;
				if (d < num38)
				{
					num36 = c;
					num38 = d;
					num37 = num12;
					num39 = num13;
				}
				if (d > num42)
				{
					num40 = c;
					num42 = d;
					num41 = num12;
					num43 = num13;
				}
			}
			if (num20 <= (num38 + num42) / 2f)
			{
				num26 = num29 - (float)Math.Atan2(num39 * (float)bendDir, num37);
				num25 = num36 * (float)bendDir;
			}
			else
			{
				num26 = num29 - (float)Math.Atan2(num43 * (float)bendDir, num41);
				num25 = num40 * (float)bendDir;
			}
		}
		goto IL_05b6;
		IL_05b6:
		float num44 = (float)Math.Atan2(num8, ax2) * (float)num6;
		float arotation = parent.arotation;
		num26 = (num26 - num44) * (180f / (float)Math.PI) + (float)num5 - arotation;
		if (num26 > 180f)
		{
			num26 -= 360f;
		}
		else if (num26 < -180f)
		{
			num26 += 360f;
		}
		parent.UpdateWorldTransform(ax, ay, arotation + num26 * alpha, num2, parent.ascaleY, 0f, 0f);
		arotation = child.arotation;
		num25 = ((num25 + num44) * (180f / (float)Math.PI) - child.ashearX) * (float)num6 + (float)num7 - arotation;
		if (num25 > 180f)
		{
			num25 -= 360f;
		}
		else if (num25 < -180f)
		{
			num25 += 360f;
		}
		child.UpdateWorldTransform(ax2, num8, arotation + num25 * alpha, child.ascaleX, child.ascaleY, child.ashearX, child.ashearY);
	}
}
