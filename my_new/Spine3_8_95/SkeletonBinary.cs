using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Spine3_8_95;

public class SkeletonBinary
{
	internal class Vertices
	{
		public int[] bones;

		public float[] vertices;
	}

	internal class SkeletonInput
	{
		private byte[] chars = new byte[32];

		private byte[] bytesBigEndian = new byte[4];

		internal ExposedList<string> strings;

		private Stream input;

		public SkeletonInput(Stream input)
		{
			this.input = input;
		}

		public byte ReadByte()
		{
			return (byte)input.ReadByte();
		}

		public sbyte ReadSByte()
		{
			int num = input.ReadByte();
			if (num == -1)
			{
				throw new EndOfStreamException();
			}
			return (sbyte)num;
		}

		public bool ReadBoolean()
		{
			return input.ReadByte() != 0;
		}

		public float ReadFloat()
		{
			input.Read(bytesBigEndian, 0, 4);
			chars[3] = bytesBigEndian[0];
			chars[2] = bytesBigEndian[1];
			chars[1] = bytesBigEndian[2];
			chars[0] = bytesBigEndian[3];
			return BitConverter.ToSingle(chars, 0);
		}

		public int ReadInt()
		{
			input.Read(bytesBigEndian, 0, 4);
			return (bytesBigEndian[0] << 24) + (bytesBigEndian[1] << 16) + (bytesBigEndian[2] << 8) + bytesBigEndian[3];
		}

		public int ReadInt(bool optimizePositive)
		{
			int num = input.ReadByte();
			int num2 = num & 0x7F;
			if ((num & 0x80) != 0)
			{
				num = input.ReadByte();
				num2 |= (num & 0x7F) << 7;
				if ((num & 0x80) != 0)
				{
					num = input.ReadByte();
					num2 |= (num & 0x7F) << 14;
					if ((num & 0x80) != 0)
					{
						num = input.ReadByte();
						num2 |= (num & 0x7F) << 21;
						if ((num & 0x80) != 0)
						{
							num2 |= (input.ReadByte() & 0x7F) << 28;
						}
					}
				}
			}
			if (!optimizePositive)
			{
				return (num2 >> 1) ^ -(num2 & 1);
			}
			return num2;
		}

		public string ReadString()
		{
			int num = ReadInt(optimizePositive: true);
			switch (num)
			{
			case 0:
				return null;
			case 1:
				return "";
			default:
			{
				num--;
				byte[] array = chars;
				if (array.Length < num)
				{
					array = new byte[num];
				}
				ReadFully(array, 0, num);
				return Encoding.UTF8.GetString(array, 0, num);
			}
			}
		}

		public string ReadStringRef()
		{
			int num = ReadInt(optimizePositive: true);
			if (num != 0)
			{
				return strings.Items[num - 1];
			}
			return null;
		}

		public void ReadFully(byte[] buffer, int offset, int length)
		{
			while (length > 0)
			{
				int num = input.Read(buffer, offset, length);
				if (num <= 0)
				{
					throw new EndOfStreamException();
				}
				offset += num;
				length -= num;
			}
		}

		public string GetVersionString()
		{
			try
			{
				int num = ReadInt(optimizePositive: true);
				if (num > 1)
				{
					input.Position += num - 1;
				}
				num = ReadInt(optimizePositive: true);
				if (num > 1)
				{
					num--;
					byte[] array = new byte[num];
					ReadFully(array, 0, num);
					return Encoding.UTF8.GetString(array, 0, num);
				}
				throw new ArgumentException("Stream does not contain a valid binary Skeleton Data.", "input");
			}
			catch (Exception ex)
			{
				throw new ArgumentException("Stream does not contain a valid binary Skeleton Data.\n" + ex, "input");
			}
		}
	}

	public const int BONE_ROTATE = 0;

	public const int BONE_TRANSLATE = 1;

	public const int BONE_SCALE = 2;

	public const int BONE_SHEAR = 3;

	public const int SLOT_ATTACHMENT = 0;

	public const int SLOT_COLOR = 1;

	public const int SLOT_TWO_COLOR = 2;

	public const int PATH_POSITION = 0;

	public const int PATH_SPACING = 1;

	public const int PATH_MIX = 2;

	public const int CURVE_LINEAR = 0;

	public const int CURVE_STEPPED = 1;

	public const int CURVE_BEZIER = 2;

	private AttachmentLoader attachmentLoader;

	private List<SkeletonJson.LinkedMesh> linkedMeshes = new List<SkeletonJson.LinkedMesh>();

	public static readonly TransformMode[] TransformModeValues = new TransformMode[5]
	{
		TransformMode.Normal,
		TransformMode.OnlyTranslation,
		TransformMode.NoRotationOrReflection,
		TransformMode.NoScale,
		TransformMode.NoScaleOrReflection
	};

	public float Scale { get; set; }

	public SkeletonBinary(params Atlas[] atlasArray)
		: this(new AtlasAttachmentLoader(atlasArray))
	{
	}

	public SkeletonBinary(AttachmentLoader attachmentLoader)
	{
		if (attachmentLoader == null)
		{
			throw new ArgumentNullException("attachmentLoader");
		}
		this.attachmentLoader = attachmentLoader;
		Scale = 1f;
	}

	public SkeletonData ReadSkeletonData(string path)
	{
		using FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		SkeletonData skeletonData = ReadSkeletonData(file);
		skeletonData.name = Path.GetFileNameWithoutExtension(path);
		return skeletonData;
	}

	public static string GetVersionString(Stream file)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		return new SkeletonInput(file).GetVersionString();
	}

	public SkeletonData ReadSkeletonData(Stream file)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		float scale = Scale;
		SkeletonData skeletonData = new SkeletonData();
		SkeletonInput skeletonInput = new SkeletonInput(file);
		skeletonData.hash = skeletonInput.ReadString();
		if (skeletonData.hash.Length == 0)
		{
			skeletonData.hash = null;
		}
		skeletonData.version = skeletonInput.ReadString();
		if (skeletonData.version.Length == 0)
		{
			skeletonData.version = null;
		}
		if ("3.8.75" == skeletonData.version)
		{
			throw new Exception("Unsupported skeleton data, please export with a newer version of Spine.");
		}
		skeletonData.x = skeletonInput.ReadFloat();
		skeletonData.y = skeletonInput.ReadFloat();
		skeletonData.width = skeletonInput.ReadFloat();
		skeletonData.height = skeletonInput.ReadFloat();
		bool flag = skeletonInput.ReadBoolean();
		if (flag)
		{
			skeletonData.fps = skeletonInput.ReadFloat();
			skeletonData.imagesPath = skeletonInput.ReadString();
			if (string.IsNullOrEmpty(skeletonData.imagesPath))
			{
				skeletonData.imagesPath = null;
			}
			skeletonData.audioPath = skeletonInput.ReadString();
			if (string.IsNullOrEmpty(skeletonData.audioPath))
			{
				skeletonData.audioPath = null;
			}
		}
		int num;
		skeletonInput.strings = new ExposedList<string>(num = skeletonInput.ReadInt(optimizePositive: true));
		object[] items = skeletonInput.strings.Resize(num).Items;
		object[] array = items;
		for (int i = 0; i < num; i++)
		{
			array[i] = skeletonInput.ReadString();
		}
		items = skeletonData.bones.Resize(num = skeletonInput.ReadInt(optimizePositive: true)).Items;
		array = items;
		for (int j = 0; j < num; j++)
		{
			string name = skeletonInput.ReadString();
			BoneData parent = ((j == 0) ? null : skeletonData.bones.Items[skeletonInput.ReadInt(optimizePositive: true)]);
			BoneData boneData = new BoneData(j, name, parent);
			boneData.rotation = skeletonInput.ReadFloat();
			boneData.x = skeletonInput.ReadFloat() * scale;
			boneData.y = skeletonInput.ReadFloat() * scale;
			boneData.scaleX = skeletonInput.ReadFloat();
			boneData.scaleY = skeletonInput.ReadFloat();
			boneData.shearX = skeletonInput.ReadFloat();
			boneData.shearY = skeletonInput.ReadFloat();
			boneData.length = skeletonInput.ReadFloat() * scale;
			boneData.transformMode = TransformModeValues[skeletonInput.ReadInt(optimizePositive: true)];
			boneData.skinRequired = skeletonInput.ReadBoolean();
			if (flag)
			{
				skeletonInput.ReadInt();
			}
			array[j] = boneData;
		}
		items = skeletonData.slots.Resize(num = skeletonInput.ReadInt(optimizePositive: true)).Items;
		array = items;
		for (int k = 0; k < num; k++)
		{
			string name2 = skeletonInput.ReadString();
			BoneData boneData2 = skeletonData.bones.Items[skeletonInput.ReadInt(optimizePositive: true)];
			SlotData slotData = new SlotData(k, name2, boneData2);
			int num2 = skeletonInput.ReadInt();
			slotData.r = (float)((num2 & 0xFF000000u) >> 24) / 255f;
			slotData.g = (float)((num2 & 0xFF0000) >> 16) / 255f;
			slotData.b = (float)((num2 & 0xFF00) >> 8) / 255f;
			slotData.a = (float)(num2 & 0xFF) / 255f;
			int num3 = skeletonInput.ReadInt();
			if (num3 != -1)
			{
				slotData.hasSecondColor = true;
				slotData.r2 = (float)((num3 & 0xFF0000) >> 16) / 255f;
				slotData.g2 = (float)((num3 & 0xFF00) >> 8) / 255f;
				slotData.b2 = (float)(num3 & 0xFF) / 255f;
			}
			slotData.attachmentName = skeletonInput.ReadStringRef();
			slotData.blendMode = (BlendMode)skeletonInput.ReadInt(optimizePositive: true);
			array[k] = slotData;
		}
		items = skeletonData.ikConstraints.Resize(num = skeletonInput.ReadInt(optimizePositive: true)).Items;
		array = items;
		for (int l = 0; l < num; l++)
		{
			IkConstraintData ikConstraintData = new IkConstraintData(skeletonInput.ReadString());
			ikConstraintData.order = skeletonInput.ReadInt(optimizePositive: true);
			ikConstraintData.skinRequired = skeletonInput.ReadBoolean();
			int num4;
			items = ikConstraintData.bones.Resize(num4 = skeletonInput.ReadInt(optimizePositive: true)).Items;
			object[] array2 = items;
			for (int m = 0; m < num4; m++)
			{
				array2[m] = skeletonData.bones.Items[skeletonInput.ReadInt(optimizePositive: true)];
			}
			ikConstraintData.target = skeletonData.bones.Items[skeletonInput.ReadInt(optimizePositive: true)];
			ikConstraintData.mix = skeletonInput.ReadFloat();
			ikConstraintData.softness = skeletonInput.ReadFloat() * scale;
			ikConstraintData.bendDirection = skeletonInput.ReadSByte();
			ikConstraintData.compress = skeletonInput.ReadBoolean();
			ikConstraintData.stretch = skeletonInput.ReadBoolean();
			ikConstraintData.uniform = skeletonInput.ReadBoolean();
			array[l] = ikConstraintData;
		}
		items = skeletonData.transformConstraints.Resize(num = skeletonInput.ReadInt(optimizePositive: true)).Items;
		array = items;
		for (int n = 0; n < num; n++)
		{
			TransformConstraintData transformConstraintData = new TransformConstraintData(skeletonInput.ReadString());
			transformConstraintData.order = skeletonInput.ReadInt(optimizePositive: true);
			transformConstraintData.skinRequired = skeletonInput.ReadBoolean();
			int num5;
			items = transformConstraintData.bones.Resize(num5 = skeletonInput.ReadInt(optimizePositive: true)).Items;
			object[] array3 = items;
			for (int num6 = 0; num6 < num5; num6++)
			{
				array3[num6] = skeletonData.bones.Items[skeletonInput.ReadInt(optimizePositive: true)];
			}
			transformConstraintData.target = skeletonData.bones.Items[skeletonInput.ReadInt(optimizePositive: true)];
			transformConstraintData.local = skeletonInput.ReadBoolean();
			transformConstraintData.relative = skeletonInput.ReadBoolean();
			transformConstraintData.offsetRotation = skeletonInput.ReadFloat();
			transformConstraintData.offsetX = skeletonInput.ReadFloat() * scale;
			transformConstraintData.offsetY = skeletonInput.ReadFloat() * scale;
			transformConstraintData.offsetScaleX = skeletonInput.ReadFloat();
			transformConstraintData.offsetScaleY = skeletonInput.ReadFloat();
			transformConstraintData.offsetShearY = skeletonInput.ReadFloat();
			transformConstraintData.rotateMix = skeletonInput.ReadFloat();
			transformConstraintData.translateMix = skeletonInput.ReadFloat();
			transformConstraintData.scaleMix = skeletonInput.ReadFloat();
			transformConstraintData.shearMix = skeletonInput.ReadFloat();
			array[n] = transformConstraintData;
		}
		items = skeletonData.pathConstraints.Resize(num = skeletonInput.ReadInt(optimizePositive: true)).Items;
		array = items;
		for (int num7 = 0; num7 < num; num7++)
		{
			PathConstraintData pathConstraintData = new PathConstraintData(skeletonInput.ReadString());
			pathConstraintData.order = skeletonInput.ReadInt(optimizePositive: true);
			pathConstraintData.skinRequired = skeletonInput.ReadBoolean();
			int num8;
			items = pathConstraintData.bones.Resize(num8 = skeletonInput.ReadInt(optimizePositive: true)).Items;
			object[] array4 = items;
			for (int num9 = 0; num9 < num8; num9++)
			{
				array4[num9] = skeletonData.bones.Items[skeletonInput.ReadInt(optimizePositive: true)];
			}
			pathConstraintData.target = skeletonData.slots.Items[skeletonInput.ReadInt(optimizePositive: true)];
			pathConstraintData.positionMode = (PositionMode)Enum.GetValues(typeof(PositionMode)).GetValue(skeletonInput.ReadInt(optimizePositive: true));
			pathConstraintData.spacingMode = (SpacingMode)Enum.GetValues(typeof(SpacingMode)).GetValue(skeletonInput.ReadInt(optimizePositive: true));
			pathConstraintData.rotateMode = (RotateMode)Enum.GetValues(typeof(RotateMode)).GetValue(skeletonInput.ReadInt(optimizePositive: true));
			pathConstraintData.offsetRotation = skeletonInput.ReadFloat();
			pathConstraintData.position = skeletonInput.ReadFloat();
			if (pathConstraintData.positionMode == PositionMode.Fixed)
			{
				pathConstraintData.position *= scale;
			}
			pathConstraintData.spacing = skeletonInput.ReadFloat();
			if (pathConstraintData.spacingMode == SpacingMode.Length || pathConstraintData.spacingMode == SpacingMode.Fixed)
			{
				pathConstraintData.spacing *= scale;
			}
			pathConstraintData.rotateMix = skeletonInput.ReadFloat();
			pathConstraintData.translateMix = skeletonInput.ReadFloat();
			array[num7] = pathConstraintData;
		}
		Skin skin = ReadSkin(skeletonInput, skeletonData, defaultSkin: true, flag);
		if (skin != null)
		{
			skeletonData.defaultSkin = skin;
			skeletonData.skins.Add(skin);
		}
		int num10 = skeletonData.skins.Count;
		items = skeletonData.skins.Resize(num = num10 + skeletonInput.ReadInt(optimizePositive: true)).Items;
		array = items;
		for (; num10 < num; num10++)
		{
			array[num10] = ReadSkin(skeletonInput, skeletonData, defaultSkin: false, flag);
		}
		num = linkedMeshes.Count;
		for (int num11 = 0; num11 < num; num11++)
		{
			SkeletonJson.LinkedMesh linkedMesh = linkedMeshes[num11];
			Attachment attachment = (((linkedMesh.skin == null) ? skeletonData.DefaultSkin : skeletonData.FindSkin(linkedMesh.skin)) ?? throw new Exception("Skin not found: " + linkedMesh.skin)).GetAttachment(linkedMesh.slotIndex, linkedMesh.parent);
			if (attachment == null)
			{
				throw new Exception("Parent mesh not found: " + linkedMesh.parent);
			}
			linkedMesh.mesh.DeformAttachment = (linkedMesh.inheritDeform ? ((VertexAttachment)attachment) : linkedMesh.mesh);
			linkedMesh.mesh.ParentMesh = (MeshAttachment)attachment;
			linkedMesh.mesh.UpdateUVs();
		}
		linkedMeshes.Clear();
		items = skeletonData.events.Resize(num = skeletonInput.ReadInt(optimizePositive: true)).Items;
		array = items;
		for (int num12 = 0; num12 < num; num12++)
		{
			EventData eventData = new EventData(skeletonInput.ReadStringRef());
			eventData.Int = skeletonInput.ReadInt(optimizePositive: false);
			eventData.Float = skeletonInput.ReadFloat();
			eventData.String = skeletonInput.ReadString();
			eventData.AudioPath = skeletonInput.ReadString();
			if (eventData.AudioPath != null)
			{
				eventData.Volume = skeletonInput.ReadFloat();
				eventData.Balance = skeletonInput.ReadFloat();
			}
			array[num12] = eventData;
		}
		items = skeletonData.animations.Resize(num = skeletonInput.ReadInt(optimizePositive: true)).Items;
		array = items;
		for (int num13 = 0; num13 < num; num13++)
		{
			array[num13] = ReadAnimation(skeletonInput.ReadString(), skeletonInput, skeletonData);
		}
		return skeletonData;
	}

	private Skin ReadSkin(SkeletonInput input, SkeletonData skeletonData, bool defaultSkin, bool nonessential)
	{
		int num;
		Skin skin;
		if (defaultSkin)
		{
			num = input.ReadInt(optimizePositive: true);
			if (num == 0)
			{
				return null;
			}
			skin = new Skin("default");
		}
		else
		{
			skin = new Skin(input.ReadStringRef());
			object[] items = skin.bones.Resize(input.ReadInt(optimizePositive: true)).Items;
			object[] array = items;
			int i = 0;
			for (int count = skin.bones.Count; i < count; i++)
			{
				array[i] = skeletonData.bones.Items[input.ReadInt(optimizePositive: true)];
			}
			int j = 0;
			for (int num2 = input.ReadInt(optimizePositive: true); j < num2; j++)
			{
				skin.constraints.Add(skeletonData.ikConstraints.Items[input.ReadInt(optimizePositive: true)]);
			}
			int k = 0;
			for (int num3 = input.ReadInt(optimizePositive: true); k < num3; k++)
			{
				skin.constraints.Add(skeletonData.transformConstraints.Items[input.ReadInt(optimizePositive: true)]);
			}
			int l = 0;
			for (int num4 = input.ReadInt(optimizePositive: true); l < num4; l++)
			{
				skin.constraints.Add(skeletonData.pathConstraints.Items[input.ReadInt(optimizePositive: true)]);
			}
			skin.constraints.TrimExcess();
			num = input.ReadInt(optimizePositive: true);
		}
		for (int m = 0; m < num; m++)
		{
			int slotIndex = input.ReadInt(optimizePositive: true);
			int n = 0;
			for (int num5 = input.ReadInt(optimizePositive: true); n < num5; n++)
			{
				string text = input.ReadStringRef();
				Attachment attachment = ReadAttachment(input, skeletonData, skin, slotIndex, text, nonessential);
				if (attachment != null)
				{
					skin.SetAttachment(slotIndex, text, attachment);
				}
			}
		}
		return skin;
	}

	private Attachment ReadAttachment(SkeletonInput input, SkeletonData skeletonData, Skin skin, int slotIndex, string attachmentName, bool nonessential)
	{
		float scale = Scale;
		string text = input.ReadStringRef();
		if (text == null)
		{
			text = attachmentName;
		}
		switch ((AttachmentType)input.ReadByte())
		{
		case AttachmentType.Region:
		{
			string text2 = input.ReadStringRef();
			float rotation = input.ReadFloat();
			float num3 = input.ReadFloat();
			float num4 = input.ReadFloat();
			float scaleX = input.ReadFloat();
			float scaleY = input.ReadFloat();
			float num5 = input.ReadFloat();
			float num6 = input.ReadFloat();
			int num7 = input.ReadInt();
			if (text2 == null)
			{
				text2 = text;
			}
			RegionAttachment regionAttachment = attachmentLoader.NewRegionAttachment(skin, text, text2);
			if (regionAttachment == null)
			{
				return null;
			}
			regionAttachment.Path = text2;
			regionAttachment.x = num3 * scale;
			regionAttachment.y = num4 * scale;
			regionAttachment.scaleX = scaleX;
			regionAttachment.scaleY = scaleY;
			regionAttachment.rotation = rotation;
			regionAttachment.width = num5 * scale;
			regionAttachment.height = num6 * scale;
			regionAttachment.r = (float)((num7 & 0xFF000000u) >> 24) / 255f;
			regionAttachment.g = (float)((num7 & 0xFF0000) >> 16) / 255f;
			regionAttachment.b = (float)((num7 & 0xFF00) >> 8) / 255f;
			regionAttachment.a = (float)(num7 & 0xFF) / 255f;
			regionAttachment.UpdateOffset();
			return regionAttachment;
		}
		case AttachmentType.Boundingbox:
		{
			int num20 = input.ReadInt(optimizePositive: true);
			Vertices vertices4 = ReadVertices(input, num20);
			if (nonessential)
			{
				input.ReadInt();
			}
			BoundingBoxAttachment boundingBoxAttachment = attachmentLoader.NewBoundingBoxAttachment(skin, text);
			if (boundingBoxAttachment == null)
			{
				return null;
			}
			boundingBoxAttachment.worldVerticesLength = num20 << 1;
			boundingBoxAttachment.vertices = vertices4.vertices;
			boundingBoxAttachment.bones = vertices4.bones;
			return boundingBoxAttachment;
		}
		case AttachmentType.Mesh:
		{
			string text3 = input.ReadStringRef();
			int num8 = input.ReadInt();
			int num9 = input.ReadInt(optimizePositive: true);
			float[] regionUVs = ReadFloatArray(input, num9 << 1, 1f);
			int[] triangles = ReadShortArray(input);
			Vertices vertices2 = ReadVertices(input, num9);
			int num10 = input.ReadInt(optimizePositive: true);
			int[] edges = null;
			float num11 = 0f;
			float num12 = 0f;
			if (nonessential)
			{
				edges = ReadShortArray(input);
				num11 = input.ReadFloat();
				num12 = input.ReadFloat();
			}
			if (text3 == null)
			{
				text3 = text;
			}
			MeshAttachment meshAttachment = attachmentLoader.NewMeshAttachment(skin, text, text3);
			if (meshAttachment == null)
			{
				return null;
			}
			meshAttachment.Path = text3;
			meshAttachment.r = (float)((num8 & 0xFF000000u) >> 24) / 255f;
			meshAttachment.g = (float)((num8 & 0xFF0000) >> 16) / 255f;
			meshAttachment.b = (float)((num8 & 0xFF00) >> 8) / 255f;
			meshAttachment.a = (float)(num8 & 0xFF) / 255f;
			meshAttachment.bones = vertices2.bones;
			meshAttachment.vertices = vertices2.vertices;
			meshAttachment.WorldVerticesLength = num9 << 1;
			meshAttachment.triangles = triangles;
			meshAttachment.regionUVs = regionUVs;
			meshAttachment.UpdateUVs();
			meshAttachment.HullLength = num10 << 1;
			if (nonessential)
			{
				meshAttachment.Edges = edges;
				meshAttachment.Width = num11 * scale;
				meshAttachment.Height = num12 * scale;
			}
			return meshAttachment;
		}
		case AttachmentType.Linkedmesh:
		{
			string text4 = input.ReadStringRef();
			int num13 = input.ReadInt();
			string skin2 = input.ReadStringRef();
			string parent = input.ReadStringRef();
			bool inheritDeform = input.ReadBoolean();
			float num14 = 0f;
			float num15 = 0f;
			if (nonessential)
			{
				num14 = input.ReadFloat();
				num15 = input.ReadFloat();
			}
			if (text4 == null)
			{
				text4 = text;
			}
			MeshAttachment meshAttachment2 = attachmentLoader.NewMeshAttachment(skin, text, text4);
			if (meshAttachment2 == null)
			{
				return null;
			}
			meshAttachment2.Path = text4;
			meshAttachment2.r = (float)((num13 & 0xFF000000u) >> 24) / 255f;
			meshAttachment2.g = (float)((num13 & 0xFF0000) >> 16) / 255f;
			meshAttachment2.b = (float)((num13 & 0xFF00) >> 8) / 255f;
			meshAttachment2.a = (float)(num13 & 0xFF) / 255f;
			if (nonessential)
			{
				meshAttachment2.Width = num14 * scale;
				meshAttachment2.Height = num15 * scale;
			}
			linkedMeshes.Add(new SkeletonJson.LinkedMesh(meshAttachment2, skin2, slotIndex, parent, inheritDeform));
			return meshAttachment2;
		}
		case AttachmentType.Path:
		{
			bool closed = input.ReadBoolean();
			bool constantSpeed = input.ReadBoolean();
			int num16 = input.ReadInt(optimizePositive: true);
			Vertices vertices3 = ReadVertices(input, num16);
			float[] array = new float[num16 / 3];
			int i = 0;
			for (int num17 = array.Length; i < num17; i++)
			{
				array[i] = input.ReadFloat() * scale;
			}
			if (nonessential)
			{
				input.ReadInt();
			}
			PathAttachment pathAttachment = attachmentLoader.NewPathAttachment(skin, text);
			if (pathAttachment == null)
			{
				return null;
			}
			pathAttachment.closed = closed;
			pathAttachment.constantSpeed = constantSpeed;
			pathAttachment.worldVerticesLength = num16 << 1;
			pathAttachment.vertices = vertices3.vertices;
			pathAttachment.bones = vertices3.bones;
			pathAttachment.lengths = array;
			return pathAttachment;
		}
		case AttachmentType.Point:
		{
			float rotation2 = input.ReadFloat();
			float num18 = input.ReadFloat();
			float num19 = input.ReadFloat();
			if (nonessential)
			{
				input.ReadInt();
			}
			PointAttachment pointAttachment = attachmentLoader.NewPointAttachment(skin, text);
			if (pointAttachment == null)
			{
				return null;
			}
			pointAttachment.x = num18 * scale;
			pointAttachment.y = num19 * scale;
			pointAttachment.rotation = rotation2;
			return pointAttachment;
		}
		case AttachmentType.Clipping:
		{
			int num = input.ReadInt(optimizePositive: true);
			int num2 = input.ReadInt(optimizePositive: true);
			Vertices vertices = ReadVertices(input, num2);
			if (nonessential)
			{
				input.ReadInt();
			}
			ClippingAttachment clippingAttachment = attachmentLoader.NewClippingAttachment(skin, text);
			if (clippingAttachment == null)
			{
				return null;
			}
			clippingAttachment.EndSlot = skeletonData.slots.Items[num];
			clippingAttachment.worldVerticesLength = num2 << 1;
			clippingAttachment.vertices = vertices.vertices;
			clippingAttachment.bones = vertices.bones;
			return clippingAttachment;
		}
		default:
			return null;
		}
	}

	private Vertices ReadVertices(SkeletonInput input, int vertexCount)
	{
		float scale = Scale;
		int num = vertexCount << 1;
		Vertices vertices = new Vertices();
		if (!input.ReadBoolean())
		{
			vertices.vertices = ReadFloatArray(input, num, scale);
			return vertices;
		}
		ExposedList<float> exposedList = new ExposedList<float>(num * 3 * 3);
		ExposedList<int> exposedList2 = new ExposedList<int>(num * 3);
		for (int i = 0; i < vertexCount; i++)
		{
			int num2 = input.ReadInt(optimizePositive: true);
			exposedList2.Add(num2);
			for (int j = 0; j < num2; j++)
			{
				exposedList2.Add(input.ReadInt(optimizePositive: true));
				exposedList.Add(input.ReadFloat() * scale);
				exposedList.Add(input.ReadFloat() * scale);
				exposedList.Add(input.ReadFloat());
			}
		}
		vertices.vertices = exposedList.ToArray();
		vertices.bones = exposedList2.ToArray();
		return vertices;
	}

	private float[] ReadFloatArray(SkeletonInput input, int n, float scale)
	{
		float[] array = new float[n];
		if (scale == 1f)
		{
			for (int i = 0; i < n; i++)
			{
				array[i] = input.ReadFloat();
			}
		}
		else
		{
			for (int j = 0; j < n; j++)
			{
				array[j] = input.ReadFloat() * scale;
			}
		}
		return array;
	}

	private int[] ReadShortArray(SkeletonInput input)
	{
		int num = input.ReadInt(optimizePositive: true);
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = (input.ReadByte() << 8) | input.ReadByte();
		}
		return array;
	}

	private Animation ReadAnimation(string name, SkeletonInput input, SkeletonData skeletonData)
	{
		ExposedList<Timeline> exposedList = new ExposedList<Timeline>(32);
		float scale = Scale;
		float num = 0f;
		int i = 0;
		for (int num2 = input.ReadInt(optimizePositive: true); i < num2; i++)
		{
			int slotIndex = input.ReadInt(optimizePositive: true);
			int j = 0;
			for (int num3 = input.ReadInt(optimizePositive: true); j < num3; j++)
			{
				int num4 = input.ReadByte();
				int num5 = input.ReadInt(optimizePositive: true);
				switch (num4)
				{
				case 0:
				{
					AttachmentTimeline attachmentTimeline = new AttachmentTimeline(num5);
					attachmentTimeline.slotIndex = slotIndex;
					for (int m = 0; m < num5; m++)
					{
						attachmentTimeline.SetFrame(m, input.ReadFloat(), input.ReadStringRef());
					}
					exposedList.Add(attachmentTimeline);
					num = Math.Max(num, attachmentTimeline.frames[num5 - 1]);
					break;
				}
				case 1:
				{
					ColorTimeline colorTimeline = new ColorTimeline(num5);
					colorTimeline.slotIndex = slotIndex;
					for (int l = 0; l < num5; l++)
					{
						float time2 = input.ReadFloat();
						int num8 = input.ReadInt();
						float r3 = (float)((num8 & 0xFF000000u) >> 24) / 255f;
						float g3 = (float)((num8 & 0xFF0000) >> 16) / 255f;
						float b3 = (float)((num8 & 0xFF00) >> 8) / 255f;
						float a2 = (float)(num8 & 0xFF) / 255f;
						colorTimeline.SetFrame(l, time2, r3, g3, b3, a2);
						if (l < num5 - 1)
						{
							ReadCurve(input, l, colorTimeline);
						}
					}
					exposedList.Add(colorTimeline);
					num = Math.Max(num, colorTimeline.frames[(num5 - 1) * 5]);
					break;
				}
				case 2:
				{
					TwoColorTimeline twoColorTimeline = new TwoColorTimeline(num5);
					twoColorTimeline.slotIndex = slotIndex;
					for (int k = 0; k < num5; k++)
					{
						float time = input.ReadFloat();
						int num6 = input.ReadInt();
						float r = (float)((num6 & 0xFF000000u) >> 24) / 255f;
						float g = (float)((num6 & 0xFF0000) >> 16) / 255f;
						float b = (float)((num6 & 0xFF00) >> 8) / 255f;
						float a = (float)(num6 & 0xFF) / 255f;
						int num7 = input.ReadInt();
						float r2 = (float)((num7 & 0xFF0000) >> 16) / 255f;
						float g2 = (float)((num7 & 0xFF00) >> 8) / 255f;
						float b2 = (float)(num7 & 0xFF) / 255f;
						twoColorTimeline.SetFrame(k, time, r, g, b, a, r2, g2, b2);
						if (k < num5 - 1)
						{
							ReadCurve(input, k, twoColorTimeline);
						}
					}
					exposedList.Add(twoColorTimeline);
					num = Math.Max(num, twoColorTimeline.frames[(num5 - 1) * 8]);
					break;
				}
				}
			}
		}
		int n = 0;
		for (int num9 = input.ReadInt(optimizePositive: true); n < num9; n++)
		{
			int boneIndex = input.ReadInt(optimizePositive: true);
			int num10 = 0;
			for (int num11 = input.ReadInt(optimizePositive: true); num10 < num11; num10++)
			{
				int num12 = input.ReadByte();
				int num13 = input.ReadInt(optimizePositive: true);
				switch (num12)
				{
				case 0:
				{
					RotateTimeline rotateTimeline = new RotateTimeline(num13);
					rotateTimeline.boneIndex = boneIndex;
					for (int num16 = 0; num16 < num13; num16++)
					{
						rotateTimeline.SetFrame(num16, input.ReadFloat(), input.ReadFloat());
						if (num16 < num13 - 1)
						{
							ReadCurve(input, num16, rotateTimeline);
						}
					}
					exposedList.Add(rotateTimeline);
					num = Math.Max(num, rotateTimeline.frames[(num13 - 1) * 2]);
					break;
				}
				case 1:
				case 2:
				case 3:
				{
					float num14 = 1f;
					TranslateTimeline translateTimeline;
					switch (num12)
					{
					case 2:
						translateTimeline = new ScaleTimeline(num13);
						break;
					case 3:
						translateTimeline = new ShearTimeline(num13);
						break;
					default:
						translateTimeline = new TranslateTimeline(num13);
						num14 = scale;
						break;
					}
					translateTimeline.boneIndex = boneIndex;
					for (int num15 = 0; num15 < num13; num15++)
					{
						translateTimeline.SetFrame(num15, input.ReadFloat(), input.ReadFloat() * num14, input.ReadFloat() * num14);
						if (num15 < num13 - 1)
						{
							ReadCurve(input, num15, translateTimeline);
						}
					}
					exposedList.Add(translateTimeline);
					num = Math.Max(num, translateTimeline.frames[(num13 - 1) * 3]);
					break;
				}
				}
			}
		}
		int num17 = 0;
		for (int num18 = input.ReadInt(optimizePositive: true); num17 < num18; num17++)
		{
			int ikConstraintIndex = input.ReadInt(optimizePositive: true);
			int num19 = input.ReadInt(optimizePositive: true);
			IkConstraintTimeline ikConstraintTimeline = new IkConstraintTimeline(num19)
			{
				ikConstraintIndex = ikConstraintIndex
			};
			for (int num20 = 0; num20 < num19; num20++)
			{
				ikConstraintTimeline.SetFrame(num20, input.ReadFloat(), input.ReadFloat(), input.ReadFloat() * scale, input.ReadSByte(), input.ReadBoolean(), input.ReadBoolean());
				if (num20 < num19 - 1)
				{
					ReadCurve(input, num20, ikConstraintTimeline);
				}
			}
			exposedList.Add(ikConstraintTimeline);
			num = Math.Max(num, ikConstraintTimeline.frames[(num19 - 1) * 6]);
		}
		int num21 = 0;
		for (int num22 = input.ReadInt(optimizePositive: true); num21 < num22; num21++)
		{
			int transformConstraintIndex = input.ReadInt(optimizePositive: true);
			int num23 = input.ReadInt(optimizePositive: true);
			TransformConstraintTimeline transformConstraintTimeline = new TransformConstraintTimeline(num23);
			transformConstraintTimeline.transformConstraintIndex = transformConstraintIndex;
			for (int num24 = 0; num24 < num23; num24++)
			{
				transformConstraintTimeline.SetFrame(num24, input.ReadFloat(), input.ReadFloat(), input.ReadFloat(), input.ReadFloat(), input.ReadFloat());
				if (num24 < num23 - 1)
				{
					ReadCurve(input, num24, transformConstraintTimeline);
				}
			}
			exposedList.Add(transformConstraintTimeline);
			num = Math.Max(num, transformConstraintTimeline.frames[(num23 - 1) * 5]);
		}
		int num25 = 0;
		for (int num26 = input.ReadInt(optimizePositive: true); num25 < num26; num25++)
		{
			int num27 = input.ReadInt(optimizePositive: true);
			PathConstraintData pathConstraintData = skeletonData.pathConstraints.Items[num27];
			int num28 = 0;
			for (int num29 = input.ReadInt(optimizePositive: true); num28 < num29; num28++)
			{
				int num30 = input.ReadSByte();
				int num31 = input.ReadInt(optimizePositive: true);
				switch (num30)
				{
				case 0:
				case 1:
				{
					float num33 = 1f;
					PathConstraintPositionTimeline pathConstraintPositionTimeline;
					if (num30 == 1)
					{
						pathConstraintPositionTimeline = new PathConstraintSpacingTimeline(num31);
						if (pathConstraintData.spacingMode == SpacingMode.Length || pathConstraintData.spacingMode == SpacingMode.Fixed)
						{
							num33 = scale;
						}
					}
					else
					{
						pathConstraintPositionTimeline = new PathConstraintPositionTimeline(num31);
						if (pathConstraintData.positionMode == PositionMode.Fixed)
						{
							num33 = scale;
						}
					}
					pathConstraintPositionTimeline.pathConstraintIndex = num27;
					for (int num34 = 0; num34 < num31; num34++)
					{
						pathConstraintPositionTimeline.SetFrame(num34, input.ReadFloat(), input.ReadFloat() * num33);
						if (num34 < num31 - 1)
						{
							ReadCurve(input, num34, pathConstraintPositionTimeline);
						}
					}
					exposedList.Add(pathConstraintPositionTimeline);
					num = Math.Max(num, pathConstraintPositionTimeline.frames[(num31 - 1) * 2]);
					break;
				}
				case 2:
				{
					PathConstraintMixTimeline pathConstraintMixTimeline = new PathConstraintMixTimeline(num31);
					pathConstraintMixTimeline.pathConstraintIndex = num27;
					for (int num32 = 0; num32 < num31; num32++)
					{
						pathConstraintMixTimeline.SetFrame(num32, input.ReadFloat(), input.ReadFloat(), input.ReadFloat());
						if (num32 < num31 - 1)
						{
							ReadCurve(input, num32, pathConstraintMixTimeline);
						}
					}
					exposedList.Add(pathConstraintMixTimeline);
					num = Math.Max(num, pathConstraintMixTimeline.frames[(num31 - 1) * 3]);
					break;
				}
				}
			}
		}
		int num35 = 0;
		for (int num36 = input.ReadInt(optimizePositive: true); num35 < num36; num35++)
		{
			Skin skin = skeletonData.skins.Items[input.ReadInt(optimizePositive: true)];
			int num37 = 0;
			for (int num38 = input.ReadInt(optimizePositive: true); num37 < num38; num37++)
			{
				int slotIndex2 = input.ReadInt(optimizePositive: true);
				int num39 = 0;
				for (int num40 = input.ReadInt(optimizePositive: true); num39 < num40; num39++)
				{
					VertexAttachment vertexAttachment = (VertexAttachment)skin.GetAttachment(slotIndex2, input.ReadStringRef());
					bool flag = vertexAttachment.bones != null;
					float[] vertices = vertexAttachment.vertices;
					int num41 = (flag ? (vertices.Length / 3 * 2) : vertices.Length);
					int num42 = input.ReadInt(optimizePositive: true);
					DeformTimeline deformTimeline = new DeformTimeline(num42);
					deformTimeline.slotIndex = slotIndex2;
					deformTimeline.attachment = vertexAttachment;
					for (int num43 = 0; num43 < num42; num43++)
					{
						float time3 = input.ReadFloat();
						int num44 = input.ReadInt(optimizePositive: true);
						float[] array;
						if (num44 == 0)
						{
							array = (flag ? new float[num41] : vertices);
						}
						else
						{
							array = new float[num41];
							int num45 = input.ReadInt(optimizePositive: true);
							num44 += num45;
							if (scale == 1f)
							{
								for (int num46 = num45; num46 < num44; num46++)
								{
									array[num46] = input.ReadFloat();
								}
							}
							else
							{
								for (int num47 = num45; num47 < num44; num47++)
								{
									array[num47] = input.ReadFloat() * scale;
								}
							}
							if (!flag)
							{
								int num48 = 0;
								for (int num49 = array.Length; num48 < num49; num48++)
								{
									array[num48] += vertices[num48];
								}
							}
						}
						deformTimeline.SetFrame(num43, time3, array);
						if (num43 < num42 - 1)
						{
							ReadCurve(input, num43, deformTimeline);
						}
					}
					exposedList.Add(deformTimeline);
					num = Math.Max(num, deformTimeline.frames[num42 - 1]);
				}
			}
		}
		int num50 = input.ReadInt(optimizePositive: true);
		if (num50 > 0)
		{
			DrawOrderTimeline drawOrderTimeline = new DrawOrderTimeline(num50);
			int count = skeletonData.slots.Count;
			for (int num51 = 0; num51 < num50; num51++)
			{
				float time4 = input.ReadFloat();
				int num52 = input.ReadInt(optimizePositive: true);
				int[] array2 = new int[count];
				for (int num53 = count - 1; num53 >= 0; num53--)
				{
					array2[num53] = -1;
				}
				int[] array3 = new int[count - num52];
				int num54 = 0;
				int num55 = 0;
				for (int num56 = 0; num56 < num52; num56++)
				{
					int num57 = input.ReadInt(optimizePositive: true);
					while (num54 != num57)
					{
						array3[num55++] = num54++;
					}
					array2[num54 + input.ReadInt(optimizePositive: true)] = num54++;
				}
				while (num54 < count)
				{
					array3[num55++] = num54++;
				}
				for (int num58 = count - 1; num58 >= 0; num58--)
				{
					if (array2[num58] == -1)
					{
						array2[num58] = array3[--num55];
					}
				}
				drawOrderTimeline.SetFrame(num51, time4, array2);
			}
			exposedList.Add(drawOrderTimeline);
			num = Math.Max(num, drawOrderTimeline.frames[num50 - 1]);
		}
		int num59 = input.ReadInt(optimizePositive: true);
		if (num59 > 0)
		{
			EventTimeline eventTimeline = new EventTimeline(num59);
			for (int num60 = 0; num60 < num59; num60++)
			{
				float time5 = input.ReadFloat();
				EventData eventData = skeletonData.events.Items[input.ReadInt(optimizePositive: true)];
				Event obj = new Event(time5, eventData)
				{
					Int = input.ReadInt(optimizePositive: false),
					Float = input.ReadFloat(),
					String = (input.ReadBoolean() ? input.ReadString() : eventData.String)
				};
				if (obj.data.AudioPath != null)
				{
					obj.volume = input.ReadFloat();
					obj.balance = input.ReadFloat();
				}
				eventTimeline.SetFrame(num60, obj);
			}
			exposedList.Add(eventTimeline);
			num = Math.Max(num, eventTimeline.frames[num59 - 1]);
		}
		exposedList.TrimExcess();
		return new Animation(name, exposedList, num);
	}

	private void ReadCurve(SkeletonInput input, int frameIndex, CurveTimeline timeline)
	{
		switch (input.ReadByte())
		{
		case 1:
			timeline.SetStepped(frameIndex);
			break;
		case 2:
			timeline.SetCurve(frameIndex, input.ReadFloat(), input.ReadFloat(), input.ReadFloat(), input.ReadFloat());
			break;
		}
	}
}
