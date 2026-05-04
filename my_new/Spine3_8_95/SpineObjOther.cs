using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Spine3_8_95;

internal class SpineObjOther : SpineObj
{
	private string getAtlasName(string atlas)
	{
		string text = atlas.Replace("pack://application:,,,/myRes;Component", "");
		text = text.Replace("/", ".");
		return "myRes" + text;
	}

	private string getSkelName(string skel)
	{
		string text = skel.Replace("pack://application:,,,/myRes;Component", "");
		text = text.Replace("/", ".");
		return "myRes" + text;
	}

	public SpineObjOther(string atlas, string skel, bool package)
		: base(atlas, skel, package)
	{
		Assembly assembly = Assembly.Load("myRes");
		assembly.GetManifestResourceNames();
		using (Stream stream = assembly.GetManifestResourceStream(getAtlasName(atlas)))
		{
			if (stream != null)
			{
				TextReader reader = new StreamReader(stream);
				string directoryName = Path.GetDirectoryName(atlas.Replace("pack://application:,,,/", ""));
				directoryName = Path.GetDirectoryName(directoryName).Replace("\\", "/");
				directoryName = "pack://application:,,,/" + directoryName;
				m_atlas = new Atlas(reader, directoryName, new WPFTextureLoaderPackage());
			}
		}
		m_binary = new SkeletonBinary(m_atlas);
		m_binary.Scale = 1f;
		using (Stream file = assembly.GetManifestResourceStream(getSkelName(skel)))
		{
			m_skeletonData = m_binary.ReadSkeletonData(file);
		}
		m_skeleton = new Skeleton(m_skeletonData);
		m_stateData = new AnimationStateData(m_skeleton.Data);
		m_state = new AnimationState(m_stateData);
		m_animationName = new List<string>();
		m_listAnimation = m_state.Data.skeletonData.Animations;
		foreach (Animation item in m_listAnimation)
		{
			m_animationName.Add(item.name);
		}
		Console.WriteLine(m_animationName);
		List<string> list = new List<string>();
		m_listSkin = m_state.Data.skeletonData.Skins;
		foreach (Skin item2 in m_listSkin)
		{
			list.Add(item2.name);
		}
		Console.WriteLine(list);
		m_state.SetAnimation(0, m_state.Data.skeletonData.animations.Items[0].name, loop: true);
	}
}
