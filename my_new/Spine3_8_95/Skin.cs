using System;
using System.Collections.Generic;
using Spine.Collections;

namespace Spine3_8_95;

public class Skin
{
	public struct SkinEntry(int slotIndex, string name, Attachment attachment)
	{
		private readonly int slotIndex = slotIndex;

		private readonly string name = name;

		private readonly Attachment attachment = attachment;

		internal readonly int hashCode = name.GetHashCode() + slotIndex * 37;

		public int SlotIndex => slotIndex;

		public string Name => name;

		public Attachment Attachment => attachment;
	}

	private class SkinEntryComparer : IEqualityComparer<SkinEntry>
	{
		internal static readonly SkinEntryComparer Instance = new SkinEntryComparer();

		bool IEqualityComparer<SkinEntry>.Equals(SkinEntry e1, SkinEntry e2)
		{
			if (e1.SlotIndex != e2.SlotIndex)
			{
				return false;
			}
			if (!string.Equals(e1.Name, e2.Name, StringComparison.Ordinal))
			{
				return false;
			}
			return true;
		}

		int IEqualityComparer<SkinEntry>.GetHashCode(SkinEntry e)
		{
			return e.Name.GetHashCode() + e.SlotIndex * 37;
		}
	}

	internal string name;

	private OrderedDictionary<SkinEntry, Attachment> attachments = new OrderedDictionary<SkinEntry, Attachment>(SkinEntryComparer.Instance);

	internal readonly ExposedList<BoneData> bones = new ExposedList<BoneData>();

	internal readonly ExposedList<ConstraintData> constraints = new ExposedList<ConstraintData>();

	public string Name => name;

	public OrderedDictionary<SkinEntry, Attachment> Attachments => attachments;

	public ExposedList<BoneData> Bones => bones;

	public ExposedList<ConstraintData> Constraints => constraints;

	public Skin(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name", "name cannot be null.");
		}
		this.name = name;
	}

	public void SetAttachment(int slotIndex, string name, Attachment attachment)
	{
		if (attachment == null)
		{
			throw new ArgumentNullException("attachment", "attachment cannot be null.");
		}
		if (slotIndex < 0)
		{
			throw new ArgumentNullException("slotIndex", "slotIndex must be >= 0.");
		}
		attachments[new SkinEntry(slotIndex, name, attachment)] = attachment;
	}

	public void AddSkin(Skin skin)
	{
		foreach (BoneData bone in skin.bones)
		{
			if (!bones.Contains(bone))
			{
				bones.Add(bone);
			}
		}
		foreach (ConstraintData constraint in skin.constraints)
		{
			if (!constraints.Contains(constraint))
			{
				constraints.Add(constraint);
			}
		}
		foreach (SkinEntry key in skin.attachments.Keys)
		{
			SetAttachment(key.SlotIndex, key.Name, key.Attachment);
		}
	}

	public void CopySkin(Skin skin)
	{
		foreach (BoneData bone in skin.bones)
		{
			if (!bones.Contains(bone))
			{
				bones.Add(bone);
			}
		}
		foreach (ConstraintData constraint in skin.constraints)
		{
			if (!constraints.Contains(constraint))
			{
				constraints.Add(constraint);
			}
		}
		foreach (SkinEntry key in skin.attachments.Keys)
		{
			if (key.Attachment is MeshAttachment)
			{
				SetAttachment(key.SlotIndex, key.Name, (key.Attachment != null) ? ((MeshAttachment)key.Attachment).NewLinkedMesh() : null);
			}
			else
			{
				SetAttachment(key.SlotIndex, key.Name, (key.Attachment != null) ? key.Attachment.Copy() : null);
			}
		}
	}

	public Attachment GetAttachment(int slotIndex, string name)
	{
		SkinEntry key = new SkinEntry(slotIndex, name, null);
		Attachment value = null;
		if (!attachments.TryGetValue(key, out value))
		{
			return null;
		}
		return value;
	}

	public void RemoveAttachment(int slotIndex, string name)
	{
		if (slotIndex < 0)
		{
			throw new ArgumentOutOfRangeException("slotIndex", "slotIndex must be >= 0");
		}
		SkinEntry key = new SkinEntry(slotIndex, name, null);
		attachments.Remove(key);
	}

	public ICollection<SkinEntry> GetAttachments()
	{
		return attachments.Keys;
	}

	public void GetAttachments(int slotIndex, List<SkinEntry> attachments)
	{
		foreach (SkinEntry key in this.attachments.Keys)
		{
			if (key.SlotIndex == slotIndex)
			{
				attachments.Add(key);
			}
		}
	}

	public void Clear()
	{
		attachments.Clear();
		bones.Clear();
		constraints.Clear();
	}

	public override string ToString()
	{
		return name;
	}

	internal void AttachAll(Skeleton skeleton, Skin oldSkin)
	{
		foreach (SkinEntry key in oldSkin.attachments.Keys)
		{
			int slotIndex = key.SlotIndex;
			Slot slot = skeleton.slots.Items[slotIndex];
			if (slot.Attachment == key.Attachment)
			{
				Attachment attachment = GetAttachment(slotIndex, key.Name);
				if (attachment != null)
				{
					slot.Attachment = attachment;
				}
			}
		}
	}
}
