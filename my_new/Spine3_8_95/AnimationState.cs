using System;
using System.Collections.Generic;
using System.Text;

namespace Spine3_8_95;

public class AnimationState
{
	public delegate void TrackEntryDelegate(TrackEntry trackEntry);

	public delegate void TrackEntryEventDelegate(TrackEntry trackEntry, Event e);

	private static readonly Animation EmptyAnimation = new Animation("<empty>", new ExposedList<Timeline>(), 0f);

	internal const int Subsequent = 0;

	internal const int First = 1;

	internal const int Hold = 2;

	internal const int HoldMix = 3;

	internal const int Setup = 1;

	internal const int Current = 2;

	protected AnimationStateData data;

	private readonly ExposedList<TrackEntry> tracks = new ExposedList<TrackEntry>();

	private readonly ExposedList<Event> events = new ExposedList<Event>();

	private readonly EventQueue queue;

	private readonly HashSet<int> propertyIDs = new HashSet<int>();

	private bool animationsChanged;

	private float timeScale = 1f;

	private int unkeyedState;

	private readonly Pool<TrackEntry> trackEntryPool = new Pool<TrackEntry>();

	public float TimeScale
	{
		get
		{
			return timeScale;
		}
		set
		{
			timeScale = value;
		}
	}

	public AnimationStateData Data
	{
		get
		{
			return data;
		}
		set
		{
			if (data == null)
			{
				throw new ArgumentNullException("data", "data cannot be null.");
			}
			data = value;
		}
	}

	public ExposedList<TrackEntry> Tracks => tracks;

	public event TrackEntryDelegate Start;

	public event TrackEntryDelegate Interrupt;

	public event TrackEntryDelegate End;

	public event TrackEntryDelegate Dispose;

	public event TrackEntryDelegate Complete;

	public event TrackEntryEventDelegate Event;

	internal void OnStart(TrackEntry entry)
	{
		if (this.Start != null)
		{
			this.Start(entry);
		}
	}

	internal void OnInterrupt(TrackEntry entry)
	{
		if (this.Interrupt != null)
		{
			this.Interrupt(entry);
		}
	}

	internal void OnEnd(TrackEntry entry)
	{
		if (this.End != null)
		{
			this.End(entry);
		}
	}

	internal void OnDispose(TrackEntry entry)
	{
		if (this.Dispose != null)
		{
			this.Dispose(entry);
		}
	}

	internal void OnComplete(TrackEntry entry)
	{
		if (this.Complete != null)
		{
			this.Complete(entry);
		}
	}

	internal void OnEvent(TrackEntry entry, Event e)
	{
		if (this.Event != null)
		{
			this.Event(entry, e);
		}
	}

	public void AssignEventSubscribersFrom(AnimationState src)
	{
		this.Event = src.Event;
		this.Start = src.Start;
		this.Interrupt = src.Interrupt;
		this.End = src.End;
		this.Dispose = src.Dispose;
		this.Complete = src.Complete;
	}

	public void AddEventSubscribersFrom(AnimationState src)
	{
		Event += src.Event;
		Start += src.Start;
		Interrupt += src.Interrupt;
		End += src.End;
		Dispose += src.Dispose;
		Complete += src.Complete;
	}

	public AnimationState(AnimationStateData data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data", "data cannot be null.");
		}
		this.data = data;
		queue = new EventQueue(this, delegate
		{
			animationsChanged = true;
		}, trackEntryPool);
	}

	public void Update(float delta)
	{
		delta *= timeScale;
		TrackEntry[] items = tracks.Items;
		int i = 0;
		for (int count = tracks.Count; i < count; i++)
		{
			TrackEntry trackEntry = items[i];
			if (trackEntry == null)
			{
				continue;
			}
			trackEntry.animationLast = trackEntry.nextAnimationLast;
			trackEntry.trackLast = trackEntry.nextTrackLast;
			float num = delta * trackEntry.timeScale;
			if (trackEntry.delay > 0f)
			{
				trackEntry.delay -= num;
				if (trackEntry.delay > 0f)
				{
					continue;
				}
				num = 0f - trackEntry.delay;
				trackEntry.delay = 0f;
			}
			TrackEntry trackEntry2 = trackEntry.next;
			if (trackEntry2 != null)
			{
				float num2 = trackEntry.trackLast - trackEntry2.delay;
				if (num2 >= 0f)
				{
					trackEntry2.delay = 0f;
					trackEntry2.trackTime += ((trackEntry.timeScale == 0f) ? 0f : ((num2 / trackEntry.timeScale + delta) * trackEntry2.timeScale));
					trackEntry.trackTime += num;
					SetCurrent(i, trackEntry2, interrupt: true);
					while (trackEntry2.mixingFrom != null)
					{
						trackEntry2.mixTime += delta;
						trackEntry2 = trackEntry2.mixingFrom;
					}
					continue;
				}
			}
			else if (trackEntry.trackLast >= trackEntry.trackEnd && trackEntry.mixingFrom == null)
			{
				items[i] = null;
				queue.End(trackEntry);
				DisposeNext(trackEntry);
				continue;
			}
			if (trackEntry.mixingFrom != null && UpdateMixingFrom(trackEntry, delta))
			{
				TrackEntry mixingFrom = trackEntry.mixingFrom;
				trackEntry.mixingFrom = null;
				if (mixingFrom != null)
				{
					mixingFrom.mixingTo = null;
				}
				while (mixingFrom != null)
				{
					queue.End(mixingFrom);
					mixingFrom = mixingFrom.mixingFrom;
				}
			}
			trackEntry.trackTime += num;
		}
		queue.Drain();
	}

	private bool UpdateMixingFrom(TrackEntry to, float delta)
	{
		TrackEntry mixingFrom = to.mixingFrom;
		if (mixingFrom == null)
		{
			return true;
		}
		bool result = UpdateMixingFrom(mixingFrom, delta);
		mixingFrom.animationLast = mixingFrom.nextAnimationLast;
		mixingFrom.trackLast = mixingFrom.nextTrackLast;
		if (to.mixTime > 0f && to.mixTime >= to.mixDuration)
		{
			if (mixingFrom.totalAlpha == 0f || to.mixDuration == 0f)
			{
				to.mixingFrom = mixingFrom.mixingFrom;
				if (mixingFrom.mixingFrom != null)
				{
					mixingFrom.mixingFrom.mixingTo = to;
				}
				to.interruptAlpha = mixingFrom.interruptAlpha;
				queue.End(mixingFrom);
			}
			return result;
		}
		mixingFrom.trackTime += delta * mixingFrom.timeScale;
		to.mixTime += delta;
		return false;
	}

	public bool Apply(Skeleton skeleton)
	{
		if (skeleton == null)
		{
			throw new ArgumentNullException("skeleton", "skeleton cannot be null.");
		}
		if (animationsChanged)
		{
			AnimationsChanged();
		}
		ExposedList<Event> exposedList = events;
		bool result = false;
		TrackEntry[] items = tracks.Items;
		int i = 0;
		for (int count = tracks.Count; i < count; i++)
		{
			TrackEntry trackEntry = items[i];
			if (trackEntry == null || trackEntry.delay > 0f)
			{
				continue;
			}
			result = true;
			MixBlend mixBlend = ((i == 0) ? MixBlend.First : trackEntry.mixBlend);
			float num = trackEntry.alpha;
			if (trackEntry.mixingFrom != null)
			{
				num *= ApplyMixingFrom(trackEntry, skeleton, mixBlend);
			}
			else if (trackEntry.trackTime >= trackEntry.trackEnd && trackEntry.next == null)
			{
				num = 0f;
			}
			float animationLast = trackEntry.animationLast;
			float animationTime = trackEntry.AnimationTime;
			int count2 = trackEntry.animation.timelines.Count;
			ExposedList<Timeline> timelines = trackEntry.animation.timelines;
			Timeline[] items2 = timelines.Items;
			if ((i == 0 && num == 1f) || mixBlend == MixBlend.Add)
			{
				for (int j = 0; j < count2; j++)
				{
					Timeline timeline = items2[j];
					if (timeline is AttachmentTimeline)
					{
						ApplyAttachmentTimeline((AttachmentTimeline)timeline, skeleton, animationTime, mixBlend, attachments: true);
					}
					else
					{
						timeline.Apply(skeleton, animationLast, animationTime, exposedList, num, mixBlend, MixDirection.In);
					}
				}
			}
			else
			{
				int[] items3 = trackEntry.timelineMode.Items;
				bool flag = trackEntry.timelinesRotation.Count != count2 << 1;
				if (flag)
				{
					trackEntry.timelinesRotation.Resize(timelines.Count << 1);
				}
				float[] items4 = trackEntry.timelinesRotation.Items;
				for (int k = 0; k < count2; k++)
				{
					Timeline timeline2 = items2[k];
					MixBlend blend = ((items3[k] == 0) ? mixBlend : MixBlend.Setup);
					if (timeline2 is RotateTimeline timeline3)
					{
						ApplyRotateTimeline(timeline3, skeleton, animationTime, num, blend, items4, k << 1, flag);
					}
					else if (timeline2 is AttachmentTimeline)
					{
						ApplyAttachmentTimeline((AttachmentTimeline)timeline2, skeleton, animationTime, mixBlend, attachments: true);
					}
					else
					{
						timeline2.Apply(skeleton, animationLast, animationTime, exposedList, num, blend, MixDirection.In);
					}
				}
			}
			QueueEvents(trackEntry, animationTime);
			exposedList.Clear(clearArray: false);
			trackEntry.nextAnimationLast = animationTime;
			trackEntry.nextTrackLast = trackEntry.trackTime;
		}
		int num2 = unkeyedState + 1;
		Slot[] items5 = skeleton.slots.Items;
		int l = 0;
		for (int count3 = skeleton.slots.Count; l < count3; l++)
		{
			Slot slot = items5[l];
			if (slot.attachmentState == num2)
			{
				string attachmentName = slot.data.attachmentName;
				slot.Attachment = ((attachmentName == null) ? null : skeleton.GetAttachment(slot.data.index, attachmentName));
			}
		}
		unkeyedState += 2;
		queue.Drain();
		return result;
	}

	private float ApplyMixingFrom(TrackEntry to, Skeleton skeleton, MixBlend blend)
	{
		TrackEntry mixingFrom = to.mixingFrom;
		if (mixingFrom.mixingFrom != null)
		{
			ApplyMixingFrom(mixingFrom, skeleton, blend);
		}
		float num;
		if (to.mixDuration == 0f)
		{
			num = 1f;
			if (blend == MixBlend.First)
			{
				blend = MixBlend.Setup;
			}
		}
		else
		{
			num = to.mixTime / to.mixDuration;
			if (num > 1f)
			{
				num = 1f;
			}
			if (blend != MixBlend.First)
			{
				blend = mixingFrom.mixBlend;
			}
		}
		ExposedList<Event> exposedList = ((num < mixingFrom.eventThreshold) ? events : null);
		bool attachments = num < mixingFrom.attachmentThreshold;
		bool flag = num < mixingFrom.drawOrderThreshold;
		float animationLast = mixingFrom.animationLast;
		float animationTime = mixingFrom.AnimationTime;
		ExposedList<Timeline> timelines = mixingFrom.animation.timelines;
		int count = timelines.Count;
		Timeline[] items = timelines.Items;
		float num2 = mixingFrom.alpha * to.interruptAlpha;
		float num3 = num2 * (1f - num);
		if (blend == MixBlend.Add)
		{
			for (int i = 0; i < count; i++)
			{
				items[i].Apply(skeleton, animationLast, animationTime, exposedList, num3, blend, MixDirection.Out);
			}
		}
		else
		{
			int[] items2 = mixingFrom.timelineMode.Items;
			TrackEntry[] items3 = mixingFrom.timelineHoldMix.Items;
			bool flag2 = mixingFrom.timelinesRotation.Count != count << 1;
			if (flag2)
			{
				mixingFrom.timelinesRotation.Resize(timelines.Count << 1);
			}
			float[] items4 = mixingFrom.timelinesRotation.Items;
			mixingFrom.totalAlpha = 0f;
			for (int j = 0; j < count; j++)
			{
				Timeline timeline = items[j];
				MixDirection direction = MixDirection.Out;
				MixBlend mixBlend;
				float num4;
				switch (items2[j])
				{
				case 0:
					if (!flag && timeline is DrawOrderTimeline)
					{
						continue;
					}
					mixBlend = blend;
					num4 = num3;
					break;
				case 1:
					mixBlend = MixBlend.Setup;
					num4 = num3;
					break;
				case 2:
					mixBlend = MixBlend.Setup;
					num4 = num2;
					break;
				default:
				{
					mixBlend = MixBlend.Setup;
					TrackEntry trackEntry = items3[j];
					num4 = num2 * Math.Max(0f, 1f - trackEntry.mixTime / trackEntry.mixDuration);
					break;
				}
				}
				mixingFrom.totalAlpha += num4;
				if (timeline is RotateTimeline timeline2)
				{
					ApplyRotateTimeline(timeline2, skeleton, animationTime, num4, mixBlend, items4, j << 1, flag2);
					continue;
				}
				if (timeline is AttachmentTimeline)
				{
					ApplyAttachmentTimeline((AttachmentTimeline)timeline, skeleton, animationTime, mixBlend, attachments);
					continue;
				}
				if (flag && timeline is DrawOrderTimeline && mixBlend == MixBlend.Setup)
				{
					direction = MixDirection.In;
				}
				timeline.Apply(skeleton, animationLast, animationTime, exposedList, num4, mixBlend, direction);
			}
		}
		if (to.mixDuration > 0f)
		{
			QueueEvents(mixingFrom, animationTime);
		}
		events.Clear(clearArray: false);
		mixingFrom.nextAnimationLast = animationTime;
		mixingFrom.nextTrackLast = mixingFrom.trackTime;
		return num;
	}

	private void ApplyAttachmentTimeline(AttachmentTimeline timeline, Skeleton skeleton, float time, MixBlend blend, bool attachments)
	{
		Slot slot = skeleton.slots.Items[timeline.slotIndex];
		if (!slot.bone.active)
		{
			return;
		}
		float[] frames = timeline.frames;
		if (time < frames[0])
		{
			if (blend == MixBlend.Setup || blend == MixBlend.First)
			{
				SetAttachment(skeleton, slot, slot.data.attachmentName, attachments);
			}
		}
		else
		{
			int num = ((!(time >= frames[frames.Length - 1])) ? (Animation.BinarySearch(frames, time) - 1) : (frames.Length - 1));
			SetAttachment(skeleton, slot, timeline.attachmentNames[num], attachments);
		}
		if (slot.attachmentState <= unkeyedState)
		{
			slot.attachmentState = unkeyedState + 1;
		}
	}

	private void SetAttachment(Skeleton skeleton, Slot slot, string attachmentName, bool attachments)
	{
		slot.Attachment = ((attachmentName == null) ? null : skeleton.GetAttachment(slot.data.index, attachmentName));
		if (attachments)
		{
			slot.attachmentState = unkeyedState + 2;
		}
	}

	private static void ApplyRotateTimeline(RotateTimeline timeline, Skeleton skeleton, float time, float alpha, MixBlend blend, float[] timelinesRotation, int i, bool firstFrame)
	{
		if (firstFrame)
		{
			timelinesRotation[i] = 0f;
		}
		if (alpha == 1f)
		{
			timeline.Apply(skeleton, 0f, time, null, 1f, blend, MixDirection.In);
			return;
		}
		Bone bone = skeleton.bones.Items[timeline.boneIndex];
		if (!bone.active)
		{
			return;
		}
		float[] frames = timeline.frames;
		float num2;
		float num;
		if (time < frames[0])
		{
			switch (blend)
			{
			default:
				return;
			case MixBlend.Setup:
				bone.rotation = bone.data.rotation;
				return;
			case MixBlend.First:
				break;
			}
			num = bone.rotation;
			num2 = bone.data.rotation;
		}
		else
		{
			num = ((blend == MixBlend.Setup) ? bone.data.rotation : bone.rotation);
			if (time >= frames[frames.Length - 2])
			{
				num2 = bone.data.rotation + frames[frames.Length + -1];
			}
			else
			{
				int num3 = Animation.BinarySearch(frames, time, 2);
				float num4 = frames[num3 + -1];
				float num5 = frames[num3];
				float curvePercent = timeline.GetCurvePercent((num3 >> 1) - 1, 1f - (time - num5) / (frames[num3 + -2] - num5));
				num2 = frames[num3 + 1] - num4;
				num2 -= (float)((16384 - (int)(16384.499999999996 - (double)(num2 / 360f))) * 360);
				num2 = num4 + num2 * curvePercent + bone.data.rotation;
				num2 -= (float)((16384 - (int)(16384.499999999996 - (double)(num2 / 360f))) * 360);
			}
		}
		float num6 = num2 - num;
		num6 -= (float)((16384 - (int)(16384.499999999996 - (double)(num6 / 360f))) * 360);
		float num7;
		if (num6 == 0f)
		{
			num7 = timelinesRotation[i];
		}
		else
		{
			float num8;
			float value;
			if (firstFrame)
			{
				num8 = 0f;
				value = num6;
			}
			else
			{
				num8 = timelinesRotation[i];
				value = timelinesRotation[i + 1];
			}
			bool flag = num6 > 0f;
			bool flag2 = num8 >= 0f;
			if (Math.Sign(value) != Math.Sign(num6) && Math.Abs(value) <= 90f)
			{
				if (Math.Abs(num8) > 180f)
				{
					num8 += (float)(360 * Math.Sign(num8));
				}
				flag2 = flag;
			}
			num7 = num6 + num8 - num8 % 360f;
			if (flag2 != flag)
			{
				num7 += (float)(360 * Math.Sign(num8));
			}
			timelinesRotation[i] = num7;
		}
		timelinesRotation[i + 1] = num6;
		num += num7 * alpha;
		bone.rotation = num - (float)((16384 - (int)(16384.499999999996 - (double)(num / 360f))) * 360);
	}

	private void QueueEvents(TrackEntry entry, float animationTime)
	{
		float animationStart = entry.animationStart;
		float animationEnd = entry.animationEnd;
		float num = animationEnd - animationStart;
		float num2 = entry.trackLast % num;
		ExposedList<Event> exposedList = events;
		Event[] items = exposedList.Items;
		int i = 0;
		int count;
		for (count = exposedList.Count; i < count; i++)
		{
			Event obj = items[i];
			if (obj.time < num2)
			{
				break;
			}
			if (!(obj.time > animationEnd))
			{
				queue.Event(entry, obj);
			}
		}
		bool flag = false;
		if ((!entry.loop) ? (animationTime >= animationEnd && entry.animationLast < animationEnd) : (num == 0f || num2 > entry.trackTime % num))
		{
			queue.Complete(entry);
		}
		for (; i < count; i++)
		{
			if (!(items[i].time < animationStart))
			{
				queue.Event(entry, items[i]);
			}
		}
	}

	public void ClearTracks()
	{
		bool drainDisabled = queue.drainDisabled;
		queue.drainDisabled = true;
		int i = 0;
		for (int count = tracks.Count; i < count; i++)
		{
			ClearTrack(i);
		}
		tracks.Clear();
		queue.drainDisabled = drainDisabled;
		queue.Drain();
	}

	public void ClearTrack(int trackIndex)
	{
		if (trackIndex >= tracks.Count)
		{
			return;
		}
		TrackEntry trackEntry = tracks.Items[trackIndex];
		if (trackEntry == null)
		{
			return;
		}
		queue.End(trackEntry);
		DisposeNext(trackEntry);
		TrackEntry trackEntry2 = trackEntry;
		while (true)
		{
			TrackEntry mixingFrom = trackEntry2.mixingFrom;
			if (mixingFrom == null)
			{
				break;
			}
			queue.End(mixingFrom);
			trackEntry2.mixingFrom = null;
			trackEntry2.mixingTo = null;
			trackEntry2 = mixingFrom;
		}
		tracks.Items[trackEntry.trackIndex] = null;
		queue.Drain();
	}

	private void SetCurrent(int index, TrackEntry current, bool interrupt)
	{
		TrackEntry trackEntry = ExpandToIndex(index);
		tracks.Items[index] = current;
		if (trackEntry != null)
		{
			if (interrupt)
			{
				queue.Interrupt(trackEntry);
			}
			current.mixingFrom = trackEntry;
			trackEntry.mixingTo = current;
			current.mixTime = 0f;
			if (trackEntry.mixingFrom != null && trackEntry.mixDuration > 0f)
			{
				current.interruptAlpha *= Math.Min(1f, trackEntry.mixTime / trackEntry.mixDuration);
			}
			trackEntry.timelinesRotation.Clear();
		}
		queue.Start(current);
	}

	public TrackEntry SetAnimation(int trackIndex, string animationName, bool loop)
	{
		Animation animation = data.skeletonData.FindAnimation(animationName);
		if (animation == null)
		{
			throw new ArgumentException("Animation not found: " + animationName, "animationName");
		}
		return SetAnimation(trackIndex, animation, loop);
	}

	public TrackEntry SetAnimation(int trackIndex, Animation animation, bool loop)
	{
		if (animation == null)
		{
			throw new ArgumentNullException("animation", "animation cannot be null.");
		}
		bool interrupt = true;
		TrackEntry trackEntry = ExpandToIndex(trackIndex);
		if (trackEntry != null)
		{
			if (trackEntry.nextTrackLast == -1f)
			{
				tracks.Items[trackIndex] = trackEntry.mixingFrom;
				queue.Interrupt(trackEntry);
				queue.End(trackEntry);
				DisposeNext(trackEntry);
				trackEntry = trackEntry.mixingFrom;
				interrupt = false;
			}
			else
			{
				DisposeNext(trackEntry);
			}
		}
		TrackEntry trackEntry2 = NewTrackEntry(trackIndex, animation, loop, trackEntry);
		SetCurrent(trackIndex, trackEntry2, interrupt);
		queue.Drain();
		return trackEntry2;
	}

	public TrackEntry AddAnimation(int trackIndex, string animationName, bool loop, float delay)
	{
		Animation animation = data.skeletonData.FindAnimation(animationName);
		if (animation == null)
		{
			throw new ArgumentException("Animation not found: " + animationName, "animationName");
		}
		return AddAnimation(trackIndex, animation, loop, delay);
	}

	public TrackEntry AddAnimation(int trackIndex, Animation animation, bool loop, float delay)
	{
		if (animation == null)
		{
			throw new ArgumentNullException("animation", "animation cannot be null.");
		}
		TrackEntry trackEntry = ExpandToIndex(trackIndex);
		if (trackEntry != null)
		{
			while (trackEntry.next != null)
			{
				trackEntry = trackEntry.next;
			}
		}
		TrackEntry trackEntry2 = NewTrackEntry(trackIndex, animation, loop, trackEntry);
		if (trackEntry == null)
		{
			SetCurrent(trackIndex, trackEntry2, interrupt: true);
			queue.Drain();
		}
		else
		{
			trackEntry.next = trackEntry2;
			if (delay <= 0f)
			{
				float num = trackEntry.animationEnd - trackEntry.animationStart;
				if (num != 0f)
				{
					delay = ((!trackEntry.loop) ? (delay + Math.Max(num, trackEntry.trackTime)) : (delay + num * (float)(1 + (int)(trackEntry.trackTime / num))));
					delay -= data.GetMix(trackEntry.animation, animation);
				}
				else
				{
					delay = trackEntry.trackTime;
				}
			}
		}
		trackEntry2.delay = delay;
		return trackEntry2;
	}

	public TrackEntry SetEmptyAnimation(int trackIndex, float mixDuration)
	{
		TrackEntry trackEntry = SetAnimation(trackIndex, EmptyAnimation, loop: false);
		trackEntry.mixDuration = mixDuration;
		trackEntry.trackEnd = mixDuration;
		return trackEntry;
	}

	public TrackEntry AddEmptyAnimation(int trackIndex, float mixDuration, float delay)
	{
		if (delay <= 0f)
		{
			delay -= mixDuration;
		}
		TrackEntry trackEntry = AddAnimation(trackIndex, EmptyAnimation, loop: false, delay);
		trackEntry.mixDuration = mixDuration;
		trackEntry.trackEnd = mixDuration;
		return trackEntry;
	}

	public void SetEmptyAnimations(float mixDuration)
	{
		bool drainDisabled = queue.drainDisabled;
		queue.drainDisabled = true;
		int i = 0;
		for (int count = tracks.Count; i < count; i++)
		{
			TrackEntry trackEntry = tracks.Items[i];
			if (trackEntry != null)
			{
				SetEmptyAnimation(trackEntry.trackIndex, mixDuration);
			}
		}
		queue.drainDisabled = drainDisabled;
		queue.Drain();
	}

	private TrackEntry ExpandToIndex(int index)
	{
		if (index < tracks.Count)
		{
			return tracks.Items[index];
		}
		tracks.Resize(index + 1);
		return null;
	}

	private TrackEntry NewTrackEntry(int trackIndex, Animation animation, bool loop, TrackEntry last)
	{
		TrackEntry trackEntry = trackEntryPool.Obtain();
		trackEntry.trackIndex = trackIndex;
		trackEntry.animation = animation;
		trackEntry.loop = loop;
		trackEntry.holdPrevious = false;
		trackEntry.eventThreshold = 0f;
		trackEntry.attachmentThreshold = 0f;
		trackEntry.drawOrderThreshold = 0f;
		trackEntry.animationStart = 0f;
		trackEntry.animationEnd = animation.Duration;
		trackEntry.animationLast = -1f;
		trackEntry.nextAnimationLast = -1f;
		trackEntry.delay = 0f;
		trackEntry.trackTime = 0f;
		trackEntry.trackLast = -1f;
		trackEntry.nextTrackLast = -1f;
		trackEntry.trackEnd = float.MaxValue;
		trackEntry.timeScale = 1f;
		trackEntry.alpha = 1f;
		trackEntry.interruptAlpha = 1f;
		trackEntry.mixTime = 0f;
		trackEntry.mixDuration = ((last == null) ? 0f : data.GetMix(last.animation, animation));
		return trackEntry;
	}

	private void DisposeNext(TrackEntry entry)
	{
		for (TrackEntry next = entry.next; next != null; next = next.next)
		{
			queue.Dispose(next);
		}
		entry.next = null;
	}

	private void AnimationsChanged()
	{
		animationsChanged = false;
		propertyIDs.Clear();
		TrackEntry[] items = tracks.Items;
		int i = 0;
		for (int count = tracks.Count; i < count; i++)
		{
			TrackEntry trackEntry = items[i];
			if (trackEntry == null)
			{
				continue;
			}
			while (trackEntry.mixingFrom != null)
			{
				trackEntry = trackEntry.mixingFrom;
			}
			do
			{
				if (trackEntry.mixingTo == null || trackEntry.mixBlend != MixBlend.Add)
				{
					ComputeHold(trackEntry);
				}
				trackEntry = trackEntry.mixingTo;
			}
			while (trackEntry != null);
		}
	}

	private void ComputeHold(TrackEntry entry)
	{
		TrackEntry mixingTo = entry.mixingTo;
		Timeline[] items = entry.animation.timelines.Items;
		int count = entry.animation.timelines.Count;
		int[] items2 = entry.timelineMode.Resize(count).Items;
		entry.timelineHoldMix.Clear();
		TrackEntry[] items3 = entry.timelineHoldMix.Resize(count).Items;
		HashSet<int> hashSet = propertyIDs;
		if (mixingTo != null && mixingTo.holdPrevious)
		{
			for (int i = 0; i < count; i++)
			{
				hashSet.Add(items[i].PropertyId);
				items2[i] = 2;
			}
			return;
		}
		for (int j = 0; j < count; j++)
		{
			Timeline timeline = items[j];
			int propertyId = timeline.PropertyId;
			if (!hashSet.Add(propertyId))
			{
				items2[j] = 0;
				continue;
			}
			if (mixingTo == null || timeline is AttachmentTimeline || timeline is DrawOrderTimeline || timeline is EventTimeline || !mixingTo.animation.HasTimeline(propertyId))
			{
				items2[j] = 1;
				continue;
			}
			TrackEntry mixingTo2 = mixingTo.mixingTo;
			while (true)
			{
				if (mixingTo2 != null)
				{
					if (mixingTo2.animation.HasTimeline(propertyId))
					{
						mixingTo2 = mixingTo2.mixingTo;
						continue;
					}
					if (mixingTo2.mixDuration > 0f)
					{
						items2[j] = 3;
						items3[j] = mixingTo2;
						break;
					}
				}
				items2[j] = 2;
				break;
			}
		}
	}

	public TrackEntry GetCurrent(int trackIndex)
	{
		if (trackIndex >= tracks.Count)
		{
			return null;
		}
		return tracks.Items[trackIndex];
	}

	public void ClearListenerNotifications()
	{
		queue.Clear();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int i = 0;
		for (int count = tracks.Count; i < count; i++)
		{
			TrackEntry trackEntry = tracks.Items[i];
			if (trackEntry != null)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(trackEntry.ToString());
			}
		}
		if (stringBuilder.Length == 0)
		{
			return "<none>";
		}
		return stringBuilder.ToString();
	}
}
