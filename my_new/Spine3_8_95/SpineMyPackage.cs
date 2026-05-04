using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Spine3_8_95;

internal class SpineMyPackage : SpineObj
{
	public SpineMyPackage(string atlas, string skel, bool package)
		: base(atlas, skel, package)
	{
		if (!package)
		{
			m_atlas = new Atlas(atlas, new WPFTextureLoader());
		}
		else
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string name = "my_new." + atlas.Replace("pack://application:,,,/", "").Replace("/", ".");
			using Stream stream = executingAssembly.GetManifestResourceStream(name);
			if (stream != null)
			{
				TextReader reader = new StreamReader(stream);
				string directoryName = Path.GetDirectoryName(atlas.Replace("pack://application:,,,/", ""));
				directoryName = Path.GetDirectoryName(directoryName).Replace("\\", "/");
				directoryName = "pack://application:,,,/" + directoryName;
				m_atlas = new Atlas(reader, directoryName, new WPFTextureLoader());
			}
		}
		if (!package)
		{
			if (Utils.IsBinaryData(skel))
			{
				m_binary = new SkeletonBinary(m_atlas);
				m_binary.Scale = 1f;
				m_skeletonData = m_binary.ReadSkeletonData(skel);
			}
			else
			{
				m_json = new SkeletonJson(m_atlas);
				m_json.Scale = 1f;
				m_skeletonData = m_json.ReadSkeletonData(skel);
			}
		}
		else
		{
			m_binary = new SkeletonBinary(m_atlas);
			m_binary.Scale = 1f;
			Assembly executingAssembly2 = Assembly.GetExecutingAssembly();
			string name2 = "my_new." + skel.Replace("pack://application:,,,/", "").Replace("/", ".");
			using Stream file = executingAssembly2.GetManifestResourceStream(name2);
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
