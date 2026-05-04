namespace Spine3_8_95;

public class IkConstraintData(string name) : ConstraintData(name)
{
	internal ExposedList<BoneData> bones = new ExposedList<BoneData>();

	internal BoneData target;

	internal int bendDirection = 1;

	internal bool compress;

	internal bool stretch;

	internal bool uniform;

	internal float mix = 1f;

	internal float softness;

	public ExposedList<BoneData> Bones => bones;

	public BoneData Target
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

	public bool Uniform
	{
		get
		{
			return uniform;
		}
		set
		{
			uniform = value;
		}
	}
}
