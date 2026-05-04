using System.Runtime.InteropServices;

namespace my_new.utils;

public static class Updater
{
	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool cloud_init(string workpath);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool httpdns_init();

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern void set_check_func(CheckCallback sf);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool update(int switch_sub, StatusCallback sf, bool check_always);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern void tick_update();

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool make_or_update_subclient(string basepath, int subver, StatusCallback sf, [Out] char[] output);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool make_or_update_client(string basepath, int subver, StatusCallback sf, [Out] char[] output);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool stop_update();

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern int get_last_error();

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool is_self_updated();

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern int get_subclient_mode(string basepath);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern void set_subclient_mode(int mode, string basepath);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern void add_subclient_mode(int subversion, string basepath);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool cleanup_subclient(string basepath, int subver);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool cleanup_all_subclient(string basepath);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool has_subclient(string basepath);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool has_subclient_version(string basepath, int version);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern int get_max_subclient_mode();

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern void get_subclient_path(string basepath, int subver, [Out] char[] output);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool copy_diectory_recursively(string source, string dest);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool engine_exchange(string main_path, string target_path, int version);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool is_shiwan_subversion(int subver);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool is_xyqs_subversion(int subver);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool is_xyqs_main_subversion(int subver);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool need_update_main_version();

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern void enable_time_stat(bool enable);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern void add_ext_time_stat(string key, ulong value);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool has_subclient_serverlist(int subver);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool has_subclient_gamenote(int subver);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool dump_time_stat(string filename);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool get_flash_cookie([Out] char[] output);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool try_fetch_combo(ComboCallback cb);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern ulong fetch_server_list();

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern ulong fetch_remote_note();

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern ulong fetch_server_list_subclient(int subver);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	public static extern ulong fetch_remote_note_subclient(int subver);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool is_inside_wine();

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool is_32app_in_64sys();

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool set_p1_xxsj(string filename);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool set_p1_xxsj_async(string filename);

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool need_warm_up();

	[DllImport("updater.dll", CallingConvention = CallingConvention.StdCall)]
	[return: MarshalAs(UnmanagedType.I1)]
	public static extern bool warm_up(WarmupCallback cb);
}
