# my_new

> 网易《梦幻西游 ONLINE》新版 WPF 启动器源码（基于 ILSpy 反编译还原 + 适配性改造）。

[![Build](https://github.com/futzhj/my_new/actions/workflows/build.yml/badge.svg)](https://github.com/futzhj/my_new/actions/workflows/build.yml)

## 项目结构

```
my_new/                  # 主程序（WPF 启动器, .exe）
├── App.cs               # WPF 应用入口
├── launch.cs            # 启动器主逻辑（更新流程、命令行解析等）
├── MainWindow.cs        # 主窗口
├── SettingDialog.cs     # 设置对话框
├── net/NetServer.cs     # 网络通信
├── utils/Updater.cs     # updater.dll P/Invoke 封装（42 个）
├── utils/PatchUpdater.cs
├── log/                 # 日志系统
├── Spine3_8_95/         # 内嵌 Spine 动画引擎
├── SharpJson/           # 内嵌 JSON 解析库
├── refs/                # 运行时原生 DLL（updater.dll, httpdns.dll）
└── my_new.csproj

myRes/                   # 资源卫星 DLL（.dll）
├── myRes/StaticResource.cs
├── images/, roles/, spine/, themes/  # 嵌入资源
└── myRes.csproj

my_new.sln               # Visual Studio 解决方案
.github/workflows/       # GitHub Actions CI
```

## 来源说明

- **托管层**（`my_new/`、`myRes/`）：原始 `my_new.exe` (5.42 MB) 和 `myRes.dll` (7.22 MB) 通过 [ILSpy](https://github.com/icsharpcode/ILSpy) `ilspycmd` 反编译为 C# 源码。
- **原生层**（`my_new/refs/updater.dll`、`httpdns.dll`）：闭源原生 C++ DLL，**未还原源码**，仅作为运行时二进制依赖随项目发布。所有 P/Invoke 接口（42 个 `Updater::*` + 3 个 `NtHttpDns*`）封装在 `my_new/utils/Updater.cs` 中。

## 构建要求

- Windows 系统（依赖 WPF + WinForms + .NET Framework）
- **.NET SDK 8.0+**（`dotnet build` 命令）
- .NET Framework 4.8 dev pack（一般 Windows 10/11 自带；GitHub Actions 的 `windows-latest` 已预装）

## 本地构建

```powershell
dotnet restore my_new.sln
dotnet build my_new.sln -c Release
```

输出位置：`my_new/bin/x64/Release/net48/my_new.exe`（含 myRes.dll、updater.dll、httpdns.dll）。

## 已做的反编译适配

ILSpy 直出代码无法直接编译，做了下列最小改动以让其可编译：

1. **`my_new.csproj`**：
   - `TargetFramework` 从 `net40` 升级到 `net48`（ILSpy 默认产物用了 C# 6+ 语法，net40 工具链不支持）
   - 添加 `<UseWindowsForms>True</UseWindowsForms>`（代码中使用了 `System.Windows.Forms.Message`、`WebBrowser`）
   - `myRes` 引用从二进制 `<Reference>` 改为 `<ProjectReference>`
2. **`myRes.csproj`**：同上 net40 → net48
3. **`Spine3_8_95/Skin.cs`**：修复 ILSpy 误用 C# 12 primary constructor 的 `this` 语法错误（字段初始化器中改为直接引用参数）

## License

仅作研究分析用途。原始二进制版权归网易公司所有。
