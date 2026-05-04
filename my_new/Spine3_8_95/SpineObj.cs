using System.Collections.Generic;

namespace Spine3_8_95;

internal class SpineObj(string atlas, string skel, bool package = false)
{
	protected ExposedList<Animation> m_listAnimation;

	protected ExposedList<Skin> m_listSkin;

	protected List<string> m_animationName;

	protected Skeleton m_skeleton;

	protected AnimationState m_state;

	protected Atlas m_atlas;

	protected SkeletonData m_skeletonData;

	protected AnimationStateData m_stateData;

	protected SkeletonBinary m_binary;

	protected SkeletonJson m_json;

	protected float m_Scale_x = 1f;

	protected float m_Scale_y = 1f;

	public bool show = true;

	public bool active = true;

	public void setScale(float scale_x, float scale_y)
	{
		m_Scale_x = scale_x;
		m_Scale_y = scale_y;
	}

	public int BonesCount()
	{
		return m_skeleton.bones.Items.Length;
	}

	public int VertexCount()
	{
		int num = 0;
		foreach (Slot slot in m_skeleton.Slots)
		{
			if (slot.Attachment is MeshAttachment meshAttachment)
			{
				num += meshAttachment.Vertices.Length / 3;
			}
		}
		return num;
	}

	public int TriangleCount()
	{
		int num = 0;
		foreach (Slot slot in m_skeleton.Slots)
		{
			if (slot.Attachment is MeshAttachment meshAttachment)
			{
				num += meshAttachment.Triangles.Length / 3;
			}
		}
		return num;
	}

	public void MoveTo(int x, int y)
	{
		m_skeleton.x = x;
		m_skeleton.y = y;
	}

	public void onUpdate(float speed)
	{
		if (active)
		{
			m_state.Update(speed / 1000f);
			m_state.Apply(m_skeleton);
			m_state.TimeScale = 1f;
			m_skeleton.ScaleY = m_Scale_x;
			m_skeleton.ScaleX = m_Scale_y;
			m_skeleton.UpdateWorldTransform();
		}
	}

	public void onDraw(SkeletonRenderer skeletonRenderer)
	{
		if (show)
		{
			skeletonRenderer.Draw(m_skeleton);
		}
	}
}
