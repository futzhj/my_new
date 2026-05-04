using System;

namespace my_new.utils;

internal class GlobalVariable
{
	public struct ErrorInfo(int i, string c, string a)
	{
		private int id = i;

		private string cause = c;

		private string advice = a;
	}

	public static ErrorInfo[] erroinfos = new ErrorInfo[5]
	{
		new ErrorInfo(11, "网络错误", "检查网络连接是否正常"),
		new ErrorInfo(12, "网络错误", "检查网络连接是否正常"),
		new ErrorInfo(21, "梦幻西游正在运行", "退出已运行的梦幻西游客户端后再尝试更新"),
		new ErrorInfo(22, "梦幻西游正在运行", "退出已运行的梦幻西游客户端后再尝试更新"),
		new ErrorInfo(3, "梦幻西游正在运行", "退出已运行的梦幻西游客户端后再尝试更新")
	};

	public static int WM_START_NEW_MY_END = 1029;

	private static string g_RootPath;

	public static bool g_isRunningOnWine = false;

	public static int g_ngp_port;

	public static int g_subver;

	public static int g_switch_game_style;

	public static int g_autorun_arg = 0;

	public static bool g_restart = false;

	public static string XY_PATH;

	public static string g_dst_path;

	public static UpdateDetail g_UpdateDetail = new UpdateDetail("");

	public static IntPtr gPartentWnd = IntPtr.Zero;

	public static string g_other_client_name = "client0";

	public static string g_other_client_ini = "xyqs.ini";

	public static string SJF_P1_START_MY_EXE = "start_my_exe";

	public static string SJF_P1_START_RESTART_MY_EXE = "restart_my_exe";

	public static string SJF_P1_START_MY_NEW_EXE = "start_my_new_exe";

	public static string SJF_P1_INIT_MY_EXE = "init_my_exe";

	public static string SJF_P1_INIT_MY_EXE_DONE = "init_my_exe_done";

	public static string SJF_P1_START_UPDATE_CLIENT = "start_update_client";

	public static string SJF_P1_START_UPDATE_COMBO_PATCH = "start_update_combo_patch";

	public static string SJF_P1_UPDATE_COMBO_PATCH_DONE = "update_combo_patch_done";

	public static string SJF_P1_UPDATE_CLOUD_PATCH = "update_cloud_patch";

	public static string SJF_P1_UPDATE_CLOUD_PATCH_DONE = "update_cloud_patch_done";

	public static string SJF_P1_MAKE_OTHER_CLIENT = "start_make_other_client";

	public static string SJF_P1_MAKE_OTHER_CLIENT_DONE = "make_other_client_done";

	public static string SJF_P1_UPDATE_CLIENT_CLIENT_DONE = "update_client_done";

	public static string SJF_P1_LOGIN = "login";

	public static string GAME_PLATFORM_CHECK_UPDATE_ARG => "--ngp_check_update";

	public static string GAME_PLATFORM_UPDATE_ARG => "--ngp_update";

	public static string GAME_PLATFORM_LAUNCH_ARG => "--ngp_launch";

	public static string LAUNCH_ARG => "__only_for_my_exe__";

	public static string SWITCH_ARG => "__mh_switch__";

	public static string AUTORUN_ARG => "___only__for_xyq__autorun__game___";

	public static string AUTORESTART_ARG => "___only__for_xyq__restart__game___";

	public static string SATAT_NEW_MY_ARG => "__only_for_my_new_exe__";

	public static string UPDATE_FILE_NAME => "update.ini";

	public static int MAX_INSTANCE_NUM => 99;

	public static string S_KEY => "Psv5cprrGsoiAdPP4i5RnoFx99Q=";

	public static string RootPath => g_RootPath;

	public static string g_download_path => $"{XY_PATH}\\download\\";

	public static void SetLogRootPath(string rootpath)
	{
		g_RootPath = rootpath;
	}
}
