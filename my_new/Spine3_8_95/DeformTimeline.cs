using System;

namespace Spine3_8_95;

public class DeformTimeline : CurveTimeline, ISlotTimeline
{
	internal int slotIndex;

	internal VertexAttachment attachment;

	internal float[] frames;

	internal float[][] frameVertices;

	public override int PropertyId => 805306368 + attachment.id + slotIndex;

	public int SlotIndex
	{
		get
		{
			return slotIndex;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("index must be >= 0.");
			}
			slotIndex = value;
		}
	}

	public VertexAttachment Attachment
	{
		get
		{
			return attachment;
		}
		set
		{
			attachment = value;
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

	public float[][] Vertices
	{
		get
		{
			return frameVertices;
		}
		set
		{
			frameVertices = value;
		}
	}

	public DeformTimeline(int frameCount)
		: base(frameCount)
	{
		frames = new float[frameCount];
		frameVertices = new float[frameCount][];
	}

	public void SetFrame(int frameIndex, float time, float[] vertices)
	{
		frames[frameIndex] = time;
		frameVertices[frameIndex] = vertices;
	}

	public override void Apply(Skeleton skeleton, float lastTime, float time, ExposedList<Event> firedEvents, float alpha, MixBlend blend, MixDirection direction)
	{
		Slot slot = skeleton.slots.Items[slotIndex];
		if (!slot.bone.active || !(slot.attachment is VertexAttachment vertexAttachment) || vertexAttachment.DeformAttachment != attachment)
		{
			return;
		}
		ExposedList<float> deform = slot.Deform;
		if (deform.Count == 0)
		{
			blend = MixBlend.Setup;
		}
		float[][] array = frameVertices;
		int num = array[0].Length;
		float[] array2 = frames;
		float[] items;
		if (time < array2[0])
		{
			switch (blend)
			{
			case MixBlend.Setup:
				deform.Clear();
				break;
			case MixBlend.First:
				if (alpha == 1f)
				{
					deform.Clear();
					break;
				}
				if (deform.Capacity < num)
				{
					deform.Capacity = num;
				}
				deform.Count = num;
				items = deform.Items;
				if (vertexAttachment.bones == null)
				{
					float[] vertices = vertexAttachment.vertices;
					for (int i = 0; i < num; i++)
					{
						items[i] += (vertices[i] - items[i]) * alpha;
					}
				}
				else
				{
					alpha = 1f - alpha;
					for (int j = 0; j < num; j++)
					{
						items[j] *= alpha;
					}
				}
				break;
			}
			return;
		}
		if (deform.Capacity < num)
		{
			deform.Capacity = num;
		}
		deform.Count = num;
		items = deform.Items;
		if (time >= array2[array2.Length - 1])
		{
			float[] array3 = array[array2.Length - 1];
			if (alpha == 1f)
			{
				if (blend == MixBlend.Add)
				{
					if (vertexAttachment.bones == null)
					{
						float[] vertices2 = vertexAttachment.vertices;
						for (int k = 0; k < num; k++)
						{
							items[k] += array3[k] - vertices2[k];
						}
					}
					else
					{
						for (int l = 0; l < num; l++)
						{
							items[l] += array3[l];
						}
					}
				}
				else
				{
					Array.Copy(array3, 0, items, 0, num);
				}
				return;
			}
			switch (blend)
			{
			case MixBlend.Setup:
				if (vertexAttachment.bones == null)
				{
					float[] vertices4 = vertexAttachment.vertices;
					for (int num2 = 0; num2 < num; num2++)
					{
						float num3 = vertices4[num2];
						items[num2] = num3 + (array3[num2] - num3) * alpha;
					}
				}
				else
				{
					for (int num4 = 0; num4 < num; num4++)
					{
						items[num4] = array3[num4] * alpha;
					}
				}
				break;
			case MixBlend.First:
			case MixBlend.Replace:
			{
				for (int num5 = 0; num5 < num; num5++)
				{
					items[num5] += (array3[num5] - items[num5]) * alpha;
				}
				break;
			}
			case MixBlend.Add:
				if (vertexAttachment.bones == null)
				{
					float[] vertices3 = vertexAttachment.vertices;
					for (int m = 0; m < num; m++)
					{
						items[m] += (array3[m] - vertices3[m]) * alpha;
					}
				}
				else
				{
					for (int n = 0; n < num; n++)
					{
						items[n] += array3[n] * alpha;
					}
				}
				break;
			}
			return;
		}
		int num6 = Animation.BinarySearch(array2, time);
		float[] array4 = array[num6 - 1];
		float[] array5 = array[num6];
		float num7 = array2[num6];
		float curvePercent = GetCurvePercent(num6 - 1, 1f - (time - num7) / (array2[num6 - 1] - num7));
		if (alpha == 1f)
		{
			if (blend == MixBlend.Add)
			{
				if (vertexAttachment.bones == null)
				{
					float[] vertices5 = vertexAttachment.vertices;
					for (int num8 = 0; num8 < num; num8++)
					{
						float num9 = array4[num8];
						items[num8] += num9 + (array5[num8] - num9) * curvePercent - vertices5[num8];
					}
				}
				else
				{
					for (int num10 = 0; num10 < num; num10++)
					{
						float num11 = array4[num10];
						items[num10] += num11 + (array5[num10] - num11) * curvePercent;
					}
				}
			}
			else
			{
				for (int num12 = 0; num12 < num; num12++)
				{
					float num13 = array4[num12];
					items[num12] = num13 + (array5[num12] - num13) * curvePercent;
				}
			}
			return;
		}
		switch (blend)
		{
		case MixBlend.Setup:
			if (vertexAttachment.bones == null)
			{
				float[] vertices7 = vertexAttachment.vertices;
				for (int num18 = 0; num18 < num; num18++)
				{
					float num19 = array4[num18];
					float num20 = vertices7[num18];
					items[num18] = num20 + (num19 + (array5[num18] - num19) * curvePercent - num20) * alpha;
				}
			}
			else
			{
				for (int num21 = 0; num21 < num; num21++)
				{
					float num22 = array4[num21];
					items[num21] = (num22 + (array5[num21] - num22) * curvePercent) * alpha;
				}
			}
			break;
		case MixBlend.First:
		case MixBlend.Replace:
		{
			for (int num23 = 0; num23 < num; num23++)
			{
				float num24 = array4[num23];
				items[num23] += (num24 + (array5[num23] - num24) * curvePercent - items[num23]) * alpha;
			}
			break;
		}
		case MixBlend.Add:
			if (vertexAttachment.bones == null)
			{
				float[] vertices6 = vertexAttachment.vertices;
				for (int num14 = 0; num14 < num; num14++)
				{
					float num15 = array4[num14];
					items[num14] += (num15 + (array5[num14] - num15) * curvePercent - vertices6[num14]) * alpha;
				}
			}
			else
			{
				for (int num16 = 0; num16 < num; num16++)
				{
					float num17 = array4[num16];
					items[num16] += (num17 + (array5[num16] - num17) * curvePercent) * alpha;
				}
			}
			break;
		}
	}
}
