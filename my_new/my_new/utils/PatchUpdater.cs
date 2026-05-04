using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using my_new.log;
using my_new.net;

namespace my_new.utils;

public class PatchUpdater
{
	private object m_lock_object = new object();

	private Task m_task_update;

	private Task m_task_warmup;

	private launch m_launch;

	private int m_curPartProgressIdx;

	private bool m_error;

	private bool g_restart;

	private bool g_restart_other;

	public event FinishDownLoadFunc FinishEvent;

	public event FinishWarmupFunc WarmupEvent;

	public PatchUpdater(launch launch)
	{
		m_launch = launch;
		_InitCloudDefault();
	}

	public void Start()
	{
		m_task_update = new Task(UpdateClient);
		m_task_update.Start();
	}

	public void StartWarmup()
	{
		m_task_warmup = new Task(WarmupClient);
		m_task_warmup.Start();
	}

	public void WaitForWarmupEnd()
	{
		if (m_task_warmup != null)
		{
			if (!m_task_warmup.Wait(TimeSpan.FromSeconds(2.0)))
			{
				Console.WriteLine("等待预热线程结束超时！");
				LogAPI.LogWrite("Wait warmup timeout!");
			}
			else
			{
				Console.WriteLine("预热任务已经结束");
			}
			m_task_warmup = null;
		}
	}

	public bool isDownLoading()
	{
		return m_task_update != null;
	}

	public void WaitForEnd()
	{
		if (m_task_update != null)
		{
			if (!m_task_update.Wait(TimeSpan.FromSeconds(2.0)))
			{
				Console.WriteLine("等待更新线程结束超时！");
				LogAPI.LogWrite("Wait update timeout!");
			}
			else
			{
				Console.WriteLine("更新任务已经结束");
			}
			m_task_update = null;
		}
	}

	public bool GetError()
	{
		return m_error;
	}

	public bool NeedRestart()
	{
		return g_restart;
	}

	public bool NeedRestartOther()
	{
		return g_restart_other;
	}

	private void UpdateClient()
	{
		int update_res = 0;
		try
		{
			lock (m_lock_object)
			{
				update_res = _UpdateClient();
			}
		}
		catch (Exception ex)
		{
			update_res = 0;
			LogAPI.LogError($"UpdateClient失败，error: {ex.ToString()}");
		}
		Application.Current.Dispatcher.BeginInvoke((Action)delegate
		{
			this.FinishEvent?.Invoke(update_res);
		});
	}

	private void WarmupClient()
	{
		WarmupCallback cb = delegate(int percentage)
		{
			LogAPI.LogWrite($"WarmupClient progress: {percentage}");
			Application.Current.Dispatcher.BeginInvoke((Action)delegate
			{
				this.WarmupEvent?.Invoke(percentage);
			});
		};
		LogAPI.LogWrite($"WarmupClient开始");
		bool flag = false;
		try
		{
			flag = Updater.warm_up(cb);
		}
		catch (Exception ex)
		{
			LogAPI.LogError($"WarmupClient失败，error: {ex.ToString()}");
			return;
		}
		LogAPI.LogWrite($"WarmupClient结束:{flag}");
	}

	private void _SetDlgPatchStatus(string s)
	{
		if (m_launch != null)
		{
			m_launch.SetDlgPatchStatus(s, thread: true);
		}
	}

	private void _SetMainProgress(int i)
	{
		if (m_launch != null)
		{
			m_launch.SetMainProgress(i, thread: true);
		}
	}

	private void _SetCurrentPartProgress(int idx)
	{
		m_curPartProgressIdx = idx;
		if (m_launch != null)
		{
			m_launch.SetCurrentPartProgress(idx);
		}
	}

	private void _SetDlgPatchName(string s)
	{
		if (m_launch != null)
		{
			m_launch.SetDlgPatchName(s, thread: true);
		}
		GlobalVariable.g_UpdateDetail.filename = s;
	}

	private void _SetDlgPatchSize(string s)
	{
		if (m_launch != null)
		{
			m_launch.SetDlgPatchSize(s, thread: true);
		}
	}

	private void _SetPartProgress(int progress)
	{
		if (m_launch != null)
		{
			m_launch.SetPartProgress(progress, thread: true);
		}
	}

	private void _SetPartProgressCount(int count)
	{
		if (m_launch != null)
		{
			m_launch.SetPartProgressCount(count);
		}
	}

	private void _IncrCurrentPartProgress()
	{
		m_curPartProgressIdx++;
		_SetCurrentPartProgress(m_curPartProgressIdx);
		_SetPartProgress(0);
	}

	private bool _InitCloudDefault()
	{
		bool num = Updater.cloud_init(GlobalVariable.g_dst_path + "\\");
		if (!num)
		{
			Console.WriteLine("[Error] init cloud file systeam fail");
		}
		return num;
	}

	private int _FilterInvalidMode(int subclient_mode)
	{
		return subclient_mode;
	}

	private void update_other_client_config()
	{
		string strValue = "0";
		UtilsMethod.WriteSettingEx(Path.Combine(GlobalVariable.g_other_client_name, "client.ini"), "Setting", "SpVer", strValue);
	}

	private bool need_switch_to_other_client()
	{
		if (!File.Exists(Path.Combine(GlobalVariable.g_dst_path, GlobalVariable.g_other_client_ini)))
		{
			UtilsMethod.WriteSettingEx(GlobalVariable.g_other_client_ini, "Setting", "SwitchTo", "0");
			return false;
		}
		if (UtilsMethod.GetSetting(GlobalVariable.g_other_client_ini, "SwitchTo", "0") != "1")
		{
			return false;
		}
		bool flag = true;
		string[] array = new string[2] { "cloud.json", "update.ini" };
		foreach (string path in array)
		{
			if (!File.Exists(Path.Combine(GlobalVariable.g_dst_path, GlobalVariable.g_other_client_name, path)))
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			string text = UtilsMethod.GetSetting("update.ini", "OtherClientPlist", "0");
			if (text == "0")
			{
				string setting = UtilsMethod.GetSetting(GlobalVariable.g_other_client_ini, "OtherClientPlist", "0");
				text = ((!(setting != "0")) ? "https://mhxyxxsj.gsf.netease.com/client0/createClient0.plist" : setting);
			}
			if (text.IndexOf("http") != 0)
			{
				return false;
			}
			int num = text.LastIndexOf('/');
			if (num == -1)
			{
				return false;
			}
			string text2 = text.Substring(0, num + 1);
			string text3 = HttpPoster.SendGet(text, "", "utf-8");
			if (text3 == null)
			{
				return false;
			}
			string text4 = Path.Combine(GlobalVariable.g_dst_path, GlobalVariable.g_other_client_name);
			if (!File.Exists(text4))
			{
				Directory.CreateDirectory(text4);
			}
			array = text3.Split(new char[2] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string text5 in array)
			{
				byte[] array2 = HttpPoster.SendDownload(text2 + text5.Trim(), "");
				if (array2 == null)
				{
					return false;
				}
				File.WriteAllBytes(Path.Combine(text4, text5), array2);
			}
		}
		return true;
	}

	private int _UpdateClient()
	{
		bool check_always = false;
		try
		{
			if (Updater.is_32app_in_64sys() && UtilsMethod.GetSettingEx(Path.Combine(GlobalVariable.g_dst_path, "client.ini"), "Setting", "spver", "") == "")
			{
				MessageBoxResult ret = MessageBoxResult.None;
				ManualResetEvent waitHandle = new ManualResetEvent(initialState: false);
				Application.Current.Dispatcher.Invoke((Action)delegate
				{
					try
					{
						ret = launch.ShowMyMessageBox("本程序仅支持32位操作系统，将自动为您切换为64位客户端以确保最佳兼容性。", "温馨提醒", MessageBoxButton.OK);
					}
					finally
					{
						waitHandle.Set();
					}
				}, new object[0]);
				waitHandle.WaitOne();
				check_always = true;
			}
		}
		catch (Exception ex)
		{
			LogAPI.LogWrite($"处理32位提示失败:");
			LogAPI.LogWrite(ex.Message);
		}
		LogAPI.LogWrite("UpdateClient begin");
		m_error = false;
		Updater.set_p1_xxsj_async(GlobalVariable.SJF_P1_START_UPDATE_CLIENT);
		Updater.httpdns_init();
		LogAPI.LogWrite("httpdns_init done");
		_SetDlgPatchStatus("获取服务器列表...");
		if (Updater.fetch_server_list() == 0L)
		{
			Application.Current.Dispatcher.Invoke((Action)delegate
			{
				launch.ShowMyMessageBox("服务器列表获取失败", "警告", MessageBoxButton.OK);
			}, new object[0]);
		}
		_SetDlgPatchStatus("获取游戏公告...");
		if (Updater.fetch_remote_note() == 0L)
		{
			Application.Current.Dispatcher.Invoke((Action)delegate
			{
				launch.ShowMyMessageBox("游戏公告获取失败", "警告", MessageBoxButton.OK);
			}, new object[0]);
		}
		_SetDlgPatchStatus("正在获取版本号信息...");
		Updater.enable_time_stat(enable: true);
		uint avg_speed_overall = 0u;
		DateTime last_time = new DateTime(0L);
		ulong last_bytes = 0uL;
		ulong total_bytes = 0uL;
		_SetMainProgress(0);
		_SetCurrentPartProgress(0);
		_SetDlgPatchName("获取资源包");
		ComboCallback cb = delegate(ulong current, ulong total)
		{
			try
			{
				DateTime now2 = DateTime.Now;
				string text5 = "";
				if (last_bytes == 0L)
				{
					last_bytes = current;
					last_time = now2;
					if (total != 0L)
					{
						total_bytes = total;
						text5 = $"大小: {(float)total / 1024f / 1024f:F1} MB";
						_SetDlgPatchSize(text5);
					}
				}
				else if ((now2 - last_time).TotalMilliseconds >= 1000.0)
				{
					double totalMilliseconds = (now2 - last_time).TotalMilliseconds;
					uint num8 = (uint)((double)(current - last_bytes) / totalMilliseconds);
					last_time = now2;
					last_bytes = current;
					if (avg_speed_overall == 0)
					{
						avg_speed_overall = num8;
					}
					else
					{
						avg_speed_overall = (uint)((float)num8 * 0.1f + (float)avg_speed_overall * 0.9f);
					}
				}
				float num9 = 0f;
				num9 = ((total != 0L) ? ((float)((double)current * 100.0 / (double)total)) : 0f);
				_SetPartProgress((int)num9);
				uint num10 = 0u;
				if (avg_speed_overall != 0)
				{
					num10 = (uint)((total - current) / (ulong)((long)avg_speed_overall * 1000L));
					if (num10 / 3600 > 99)
					{
						num10 = 0u;
					}
				}
				text5 = "";
				text5 = ((num10 != 0) ? $"下载:{avg_speed_overall}kB/s 剩余时间:{num10 / 3600:D2}:{num10 / 60 % 60:D2}:{num10 % 60:D2}({num9:F2}%)" : $"下载:{avg_speed_overall}kB/s 剩余时间:未知");
				_SetDlgPatchStatus(text5);
				return true;
			}
			catch (Exception ex3)
			{
				LogAPI.LogWrite($"mainPatch progress_cb回调失败，current {current}, total {total}");
				LogAPI.LogWrite(ex3.Message);
				return false;
			}
		};
		DateTime now = DateTime.Now;
		LogAPI.LogWrite("need_update_main_version begin");
		bool flag = Updater.need_update_main_version();
		LogAPI.LogWrite("need_update_main_version : {0}", flag.ToString());
		Updater.set_p1_xxsj_async(GlobalVariable.SJF_P1_START_UPDATE_COMBO_PATCH);
		bool flag2 = Updater.try_fetch_combo(cb);
		LogAPI.LogWrite("try_fetch_combo : {0}", flag2.ToString());
		if (flag2)
		{
			avg_speed_overall = (uint)((double)((total_bytes >> 10) * 1000) / (DateTime.Now - now).TotalMilliseconds);
			Updater.add_ext_time_stat("S0", avg_speed_overall);
			Updater.add_ext_time_stat("KB0", total_bytes >> 10);
		}
		Updater.set_p1_xxsj_async(GlobalVariable.SJF_P1_UPDATE_COMBO_PATCH_DONE);
		int num;
		if (flag)
		{
			num = Updater.get_subclient_mode(GlobalVariable.g_dst_path);
			if (num > 0)
			{
				int num2 = 2;
				num >>= 1;
				while (num != 0)
				{
					int num3 = num2 * (num & 1);
					if (num3 != 0 && !Updater.is_xyqs_main_subversion(num3))
					{
						Updater.cleanup_subclient(GlobalVariable.g_dst_path, num3);
					}
					num >>= 1;
					num2++;
				}
				Updater.set_subclient_mode(1, GlobalVariable.g_dst_path);
				GlobalVariable.g_subver = 0;
				UtilsMethod.WriteSettingEx("client.ini", "Setting", "CurrentSubClient", GlobalVariable.g_subver.ToString());
			}
		}
		int num4 = GlobalVariable.g_subver;
		if (Updater.get_subclient_mode(GlobalVariable.g_dst_path) != 0)
		{
			Updater.add_subclient_mode(num4, GlobalVariable.g_dst_path);
		}
		num = Updater.get_subclient_mode(GlobalVariable.g_dst_path);
		if (num > 0 && (num & 1) == 0)
		{
			Updater.add_subclient_mode(1, GlobalVariable.g_dst_path);
			num = Updater.get_subclient_mode(GlobalVariable.g_dst_path);
		}
		num = _FilterInvalidMode(num);
		int num5 = UtilsMethod.countBits(num);
		bool flag3 = Updater.has_subclient(GlobalVariable.g_dst_path);
		if (num4 > 0 && (num & (1 << num4 - 1)) != 0)
		{
			num4 = 0;
		}
		else if (num == 0 && flag3)
		{
			_SetMainProgress(0);
			_SetCurrentPartProgress(0);
			_SetDlgPatchName("版本切换更改为手动确认中");
			Updater.cleanup_all_subclient(GlobalVariable.g_dst_path);
		}
		_SetPartProgressCount(2 + 2 * num5);
		_SetCurrentPartProgress(0);
		_SetPartProgress(0);
		total_bytes = 0uL;
		avg_speed_overall = 0u;
		int pre_state = 0;
		int dl_bytes = 0;
		string subname = "";
		StatusCallback sf = delegate(int stage, int s, int total, int finished, long totalbytes, long dlbytes, int avg_speed)
		{
			try
			{
				if (pre_state != stage)
				{
					pre_state = stage;
					if (stage == 1)
					{
						string s2 = subname + "更新程序中";
						_SetDlgPatchName(s2);
					}
					else
					{
						string s3 = subname + "更新资源中";
						_IncrCurrentPartProgress();
						_SetDlgPatchName(s3);
					}
				}
				switch (s)
				{
				case 1:
				{
					_SetDlgPatchStatus("正在检查更新..");
					string s4 = "未知大小";
					_SetPartProgress(0);
					_SetDlgPatchSize(s4);
					break;
				}
				case 4:
				{
					_SetDlgPatchStatus("正在检查更新...");
					string s4 = "未知大小";
					_SetPartProgress(0);
					_SetDlgPatchSize(s4);
					break;
				}
				case 5:
				{
					_SetDlgPatchStatus("检查更新完毕");
					string s4 = $"大小: {(float)totalbytes / 1024f / 1024f:F1} MB";
					_SetPartProgress(0);
					_SetDlgPatchSize(s4);
					total_bytes = (ulong)totalbytes;
					break;
				}
				case 6:
				{
					dl_bytes = (int)dlbytes;
					float num8 = (float)finished * 100f / (float)total;
					int num9 = (int)num8;
					if (num9 > 0)
					{
						_SetPartProgress(num9);
					}
					uint num10 = 0u;
					if (avg_speed > 1)
					{
						if (avg_speed_overall == 0)
						{
							avg_speed_overall = (uint)avg_speed;
						}
						else
						{
							avg_speed_overall = (uint)((float)avg_speed * 0.1f + (float)avg_speed_overall * 0.9f);
						}
						num10 = (uint)((totalbytes - dlbytes) / ((long)avg_speed_overall * 1000L));
						if (num10 / 3600 > 99)
						{
							num10 = 0u;
						}
					}
					string s4 = ((num10 != 0) ? $"更新:{avg_speed_overall}kB/s  剩余时间:{num10 / 3600:D2}:{num10 / 60 % 60:D2}:{num10 % 60:D2}({num8:F2}%)" : $"更新:{avg_speed_overall}kB/s  剩余时间:未知");
					_SetDlgPatchStatus(s4);
					break;
				}
				case 7:
					_SetPartProgress(100);
					_SetDlgPatchStatus("文件写入");
					break;
				case 8:
					_SetPartProgress(100);
					_SetDlgPatchStatus("更新完成");
					UtilsMethod.FixMicroVerDone();
					break;
				}
			}
			catch (Exception ex3)
			{
				LogAPI.LogWrite($"updatePatch progress2_cb回调失败，stage, s, total, finished, totalbytes, dlbytes, avg_speed");
				LogAPI.LogWrite($"{stage}, {s}, {total}, {finished}, {totalbytes}, {dlbytes}, {avg_speed}");
				LogAPI.LogWrite(ex3.Message);
			}
		};
		CallbackHandler.SetupCallback(m_launch);
		LogAPI.LogWrite("g_restart preupdate : {0}", Updater.is_self_updated().ToString());
		bool flag4 = Updater.update(num4, sf, check_always);
		Updater.set_p1_xxsj_async(GlobalVariable.SJF_P1_UPDATE_CLOUD_PATCH);
		LogAPI.LogWrite("update done: {0}, error: {1}", flag4.ToString(), Updater.get_last_error().ToString());
		bool flag5 = UtilsMethod.my_exchanged();
		LogAPI.LogWrite("g_restart: {0}", Updater.is_self_updated().ToString());
		int num6 = 1;
		while (flag4 && num != 0)
		{
			total_bytes = 0uL;
			avg_speed_overall = 0u;
			pre_state = 0;
			dl_bytes = 0;
			num4 = (1 << num6 - 1) * (num & 1);
			num >>= 1;
			subname = "版本切换";
			if (num4 > 0 && (num6 == GlobalVariable.g_subver || !Updater.has_subclient_version(GlobalVariable.g_dst_path, num6) || flag))
			{
				_IncrCurrentPartProgress();
				char[] array = new char[256];
				if (!Updater.make_or_update_subclient(GlobalVariable.g_dst_path, num6, sf, array))
				{
					flag4 = false;
					break;
				}
				UtilsMethod.OutputToString(array);
				if (Updater.fetch_server_list_subclient(num6) == 0L)
				{
					string sourceFileName = Path.Combine(GlobalVariable.g_dst_path, "server.ini");
					string text = "";
					text = ((!Updater.is_shiwan_subversion(num6)) ? Path.Combine(GlobalVariable.g_dst_path, $"subclient{num6}", "server.ini") : Path.Combine(GlobalVariable.g_dst_path, "subclientshiwan", "server.ini"));
					try
					{
						File.Copy(sourceFileName, text, overwrite: true);
					}
					catch
					{
					}
				}
				if (Updater.fetch_remote_note_subclient(num6) == 0L)
				{
					string sourceFileName2 = Path.Combine(GlobalVariable.g_dst_path, "note.txt");
					string text2 = "";
					text2 = ((!Updater.is_shiwan_subversion(num6)) ? Path.Combine(GlobalVariable.g_dst_path, $"subclient{num6}", "note.txt") : Path.Combine(GlobalVariable.g_dst_path, "subclientshiwan", "note.txt"));
					try
					{
						File.Copy(sourceFileName2, text2, overwrite: true);
					}
					catch
					{
					}
				}
				string[] array2 = new string[1] { "xy1.ini" };
				for (int num7 = 0; num7 < 1; num7++)
				{
					string text3 = Path.Combine(GlobalVariable.g_dst_path, array2[num7]);
					if (File.Exists(text3))
					{
						string text4 = "";
						text4 = ((!Updater.is_shiwan_subversion(num6)) ? Path.Combine(GlobalVariable.g_dst_path, $"subclient{num6}", array2[num7]) : Path.Combine(GlobalVariable.g_dst_path, "subclientshiwan", array2[num7]));
						try
						{
							File.Copy(text3, text4, overwrite: true);
						}
						catch
						{
						}
					}
				}
			}
			num6++;
		}
		if (!flag4)
		{
			m_error = true;
			switch (Updater.get_last_error())
			{
			case 1:
				_SetDlgPatchStatus("版本号获取失败");
				break;
			case 2:
				_SetDlgPatchStatus("资源列表获取失败");
				break;
			case 3:
				_SetDlgPatchStatus("资源校验失败");
				break;
			case 4:
				_SetDlgPatchStatus("下载发生错误");
				break;
			case 5:
				_SetDlgPatchStatus("写入文件发生错误");
				break;
			case 6:
				_SetDlgPatchStatus("请先关闭其他客户端");
				break;
			case 7:
				_SetDlgPatchStatus("更新发生错误");
				break;
			}
			return 1;
		}
		if (!flag5)
		{
			g_restart |= Updater.is_self_updated();
		}
		Updater.set_p1_xxsj_async(GlobalVariable.SJF_P1_UPDATE_CLOUD_PATCH_DONE);
		if (need_switch_to_other_client())
		{
			Updater.set_p1_xxsj_async(GlobalVariable.SJF_P1_MAKE_OTHER_CLIENT);
			update_other_client_config();
			_SetCurrentPartProgress(0);
			_SetPartProgress(0);
			string setting = UtilsMethod.GetSetting(GlobalVariable.g_other_client_ini, "subver", "0");
			try
			{
				num4 = int.Parse(setting);
			}
			catch (Exception)
			{
				num4 = 0;
			}
			if (Updater.has_subclient(Path.Combine(GlobalVariable.g_dst_path, GlobalVariable.g_other_client_name)))
			{
				num4 = 0;
			}
			char[] output = new char[256];
			if (Updater.make_or_update_client(GlobalVariable.g_dst_path, num4, sf, output))
			{
				Updater.set_p1_xxsj_async(GlobalVariable.SJF_P1_MAKE_OTHER_CLIENT_DONE);
				UtilsMethod.WriteSettingEx(GlobalVariable.g_other_client_name + "/" + GlobalVariable.g_other_client_ini, "Setting", "subver", setting);
				UtilsMethod.WriteSettingEx(GlobalVariable.g_other_client_ini, "Setting", "SwitchTo", "2");
				g_restart = true;
				g_restart_other = true;
			}
		}
		Updater.dump_time_stat("patch_perf.json");
		LogAPI.LogWrite("Update Done");
		CallbackHandler.Release();
		Updater.set_p1_xxsj_async(GlobalVariable.SJF_P1_UPDATE_CLIENT_CLIENT_DONE);
		return 0;
	}
}
