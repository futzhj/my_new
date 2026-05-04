namespace Spine3_8_95;

public class BoundingBoxAttachment : VertexAttachment
{
	public BoundingBoxAttachment(string name)
		: base(name)
	{
	}

	public override Attachment Copy()
	{
		BoundingBoxAttachment boundingBoxAttachment = new BoundingBoxAttachment(base.Name);
		CopyTo(boundingBoxAttachment);
		return boundingBoxAttachment;
	}
}
