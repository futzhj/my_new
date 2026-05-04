using System;
using System.Runtime.InteropServices;

namespace my_new.utils;

public static class NativeMethods
{
	public enum JobObjectInfoType
	{
		AssociateCompletionPortInformation = 7,
		BasicLimitInformation = 2,
		BasicUIRestrictions = 4,
		EndOfJobTimeInformation = 6,
		ExtendedLimitInformation = 9,
		SecurityLimitInformation = 5,
		GroupInformation = 11
	}

	public struct JOBOBJECT_BASIC_LIMIT_INFORMATION
	{
		public long PerProcessUserTimeLimit;

		public long PerJobUserTimeLimit;

		public ushort LimitFlags;

		public uint MinimumWorkingSetSize;

		public uint MaximumWorkingSetSize;

		public ushort ActiveProcessLimit;

		public uint Affinity;

		public ushort PriorityClass;

		public ushort SchedulingClass;
	}

	public struct IO_COUNTERS
	{
		public ulong ReadOperationCount;

		public ulong WriteOperationCount;

		public ulong OtherOperationCount;

		public ulong ReadTransferCount;

		public ulong WriteTransferCount;

		public ulong OtherTransferCount;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	public struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
	{
		public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;

		public IO_COUNTERS IoInfo;

		public uint ProcessMemoryLimit;

		public uint JobMemoryLimit;

		public uint PeakProcessMemoryUsed;

		public uint PeakJobMemoryUsed;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string lpName);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);
}
