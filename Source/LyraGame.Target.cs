// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;
using System;
using System.IO;
using EpicGames.Core;
using System.Collections.Generic;
using UnrealBuildBase;
using Microsoft.Extensions.Logging;

public class LyraGameTarget : TargetRules
{
	public LyraGameTarget(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Game;

		ExtraModuleNames.Add("LyraGame");
		
		LyraGameTarget.ApplySharedLyraTargetSettings(this);
	}
	
	// 用于判断是否已经打印该提示日志, 如果为 true 就跳过该日志的打印
	private static bool bHasWarnedAboutShared = false;
	internal static void ApplySharedLyraTargetSettings(TargetRules Target)
	{
		// 用于记录该 Target 的日志内容, 需在 RulesCompiler 构造之前进行设置
		ILogger Logger = Target.Logger;
		
		// 指定兼容的引擎版本
		Target.DefaultBuildSettings = BuildSettingsVersion.V5;
		
		// 设置此 Target 启用何种包含顺序, 可使用命令行中的 -ForceIncludeOrder 参数进行覆盖.
		Target.IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_6;
		
		// 解决方案的配置
		// Test 测试配置
		bool bIsTest = Target.Configuration == UnrealTargetConfiguration.Test;
		
		// Shipping 发行配置
		bool bIsShipping = Target.Configuration == UnrealTargetConfiguration.Shipping;
		
		// DedicatedServer 专属服务器
		bool bIsDedicatedServer = Target.Type == TargetType.Server;
		
		// 引擎的二进制文件和中间文件是针对此目标特定的, 判断 “自定义的” 或 “非原生的” 构建环境
		// 源码版引擎/Linux交叉编译/嵌入式或专用平台(为 PlayStation, Xbox, Switch 等主机平台进行编译)等, 为 Unique 自定义的情况
		// 发布版引擎/在 Windows 上使用 Visual Studio 的 MSVC 编译器, 为 Shared 共享情况
		if (Target.BuildEnvironment == TargetBuildEnvironment.Unique)
		{
			// 输出的警告级别为错误
			Target.CppCompileWarningSettings.ShadowVariableWarningLevel = WarningLevel.Error;

			// 开启测试/发布版本的日志记录功能
			Target.bUseLoggingInShipping = true;

			// 是否在测试配置中跟踪 RHI 资源的所有者（资产名称）. 此功能对 “列表着色器映射” 和 “列表着色器库” 命令非常有用.
			Target.bTrackRHIResourceInfoForTest = true;

			// 发行模式且非专属服务器
			if (bIsShipping && !bIsDedicatedServer)
			{
				// 请确保对用于 HTTPS 流量的证书进行验证

				// 允许通过引擎配置来决定我们是否能够加载未经验证的证书。
				Target.bDisableUnverifiedCertificates = true;

				// 取消这些行的注释即可锁定命令行处理过程
				// 这将仅允许对指定的命令行参数进行解析。
				//Target.GlobalDefinitions.Add("UE_COMMAND_LINE_USES_ALLOW_LIST=1");
				//Target.GlobalDefinitions.Add("UE_OVERRIDE_COMMAND_LINE_ALLOW_LIST=\"-space -separated -list -of -commands\"");

				// 取消注释这一行，以便过滤掉那些敏感的命令行参数。您需要这样做。
				// 不想查看日志文件（例如，如果您正在上传日志的话）
				//Target.GlobalDefinitions.Add("FILTER_COMMANDLINE_LOGGING=\"-some_connection_id -some_other_arg\"");
			}

			// 发行模式或者测试模式下
			if (bIsShipping || bIsTest)
			{
				// 禁用读取生成的/非 UFS 格式的 ini 文件

				// 是否在烘焙版本中加载生成的 ini 文件？（无论采用哪种方式加载，GameUserSettings.ini 都会被加载）
				Target.bAllowGeneratedIniWhenCooked = false;

				// 在烘焙版本中是否要加载非 ufs 格式的 ini 文件？（无论采用哪种方式加载，GameUserSettings.ini 都会被加载）
				Target.bAllowNonUFSIniWhenCooked = false;
			}

			// 非编辑器模式下 (打包)
			if (Target.Type != TargetType.Editor)
			{
				// 禁掉一些用不上的东西
				
				// 我们在运行时并不使用路径追踪功能，仅用于制作精美的效果图，而且这个动态链接库的体积相当大。
				Target.DisablePlugins.AddRange(new string[] { "OpenImageDenoise" });

				// 减少资产注册表中始终加载数据所占用的内存，但增加一些计算资源消耗较大的查询操作
				Target.GlobalDefinitions.Add("UE_ASSETREGISTRY_INDIRECT_ASSETDATA_POINTERS=1");
			}

			LyraGameTarget.ConfigureGameFeaturePlugins(Target);
		}
		else
		{
			// !!!!!!!!!!!! WARNING !!!!!!!!!!!!!
			// 此处的任何修改都不得影响预编译头文件的生成, 否则目标环境必须设置为TargetBuildEnvironment.Unique
			
			//如果你用的是 源码引擎，你可以随意玩这些功能；
			//但如果用的是 Launcher 装的官方二进制引擎(发布版引擎)，那一些动态 target 配置就不能用了（因为它们依赖 Unique 环境和源码可改动的部分）
			
			// 如果是在 编辑器里运行游戏，就会调用 ConfigureGameFeaturePlugins，让 Lyra 的 Game Feature Plugins 系统（类似插件模块）正确挂载
			if (Target.Type == TargetType.Editor)
			{
				LyraGameTarget.ConfigureGameFeaturePlugins(Target);
			}
			else
			{
				if (!bHasWarnedAboutShared)
				{
					bHasWarnedAboutShared = true;
					// "LyraGameEOS（Lyra 的在线子系统插件）和动态 Target 选项，在打包 已安装版引擎 时会被禁用"
					Logger.LogWarning("LyraGameEOS and dynamic target options are disabled when packaging from an installed version of the engine");
				}
			}
		}
	}

	static public bool ShouldEnableAllGameFeaturePlugins(TargetRules Target)
	{
		if (Target.Type == TargetType.Editor)
		{
			// 若设置为 “true”，编辑器将构建所有游戏功能插件，但这些插件是否会被全部加载则取决于具体情况。
			// 这样您就可以在编辑器中启用插件，而无需编译代码。
			
			// return true;
		}

		bool bIsBuildMachine = (Environment.GetEnvironmentVariable("IsBuildMachine") == "1");
		
		if (bIsBuildMachine)
		{
			// 这可以用于为构建机器启用所有插件
			// return true;
		}

		// 默认情况下，将使用插件浏览器在编辑器中设置的默认插件规则
		// 这非常重要，因为对于安装在启动器中的引擎版本，这段代码可能根本不会被执行
		return false;
	}
	
	// 一个插件名和它的 JsonObject 的对象, 用于获取插件描述信息(.uplugin文件)
	private static Dictionary<string, JsonObject> AllPluginRootJsonObjectsByName = new Dictionary<string, JsonObject>();
	
	// 用于配置我们希望启用哪些游戏功能插件
	// 这是一种相对简单的实现方式，但您也可以根据当前分支的目标发布版本来构建不同的插件，例如，在主分支中启用正在进行中的功能，而在当前发布分支中则禁用这些功能。
	static public void ConfigureGameFeaturePlugins(TargetRules Target)
	{
		// 获取日志器
		ILogger Logger = Target.Logger;
		
		// 打印当前分支
		Log.TraceInformationOnce("Compiling GameFeaturePlugins in branch {0}", Target.Version.BranchName);
		
		// 获取是否构建所有GameFeature插件
		bool bBuildAllGameFeaturePlugins = ShouldEnableAllGameFeaturePlugins(Target);
		
		// 加载所有的 GameFeature 插件描述器.
		
		//创建一个FileReference的容器
		List<FileReference> CombinedPluginList = new List<FileReference>();
		
		// 获取所有GameFeature的插件引用
		List<DirectoryReference> GameFeaturePluginRoots = Unreal.GetExtensionDirs(Target.ProjectFile.Directory, Path.Combine("Plugins", "GameFeatures"));
		
		foreach (DirectoryReference SearchDir in GameFeaturePluginRoots)
		{
			// 填充容器
			CombinedPluginList.AddRange(PluginsBase.EnumeratePlugins(SearchDir));
		}

		if (CombinedPluginList.Count > 0)
		{
			// 记录所有引用到的插件, 因为插件可能存在有外部依赖关系
			Dictionary<string, List<string>> AllPluginReferencesByName = new Dictionary<string, List<string>>();

			foreach (FileReference PluginFile in CombinedPluginList)
			{
				// 判断找个插件是否真实存在
				if (PluginFile != null && FileReference.Exists(PluginFile))
				{
					bool bEnabled = false;
					bool bForceDisabled = false;

					try
					{
						// 获取并添加到插件的JsonObject的字典中
						JsonObject RawObject;
						if (!AllPluginRootJsonObjectsByName.TryGetValue(PluginFile.GetFileNameWithoutExtension(), out RawObject))
						{
							RawObject = JsonObject.Read(PluginFile);
							AllPluginRootJsonObjectsByName.Add(PluginFile.GetFileNameWithoutExtension(), RawObject);
						}
						
						// 确认所有游戏功能插件默认均已禁用
						// 如果 EnabledByDefault 为真且某个插件处于禁用状态，则该插件的名称将被嵌入到可执行文件中
						// 如果出现此问题，请启用此警告，并将游戏功能编辑插件模板修改为在新插件中禁用 EnabledByDefault 参数
						bool bEnabledByDefault = false;
						if (!RawObject.TryGetBoolField("EnabledByDefault", out bEnabledByDefault) || bEnabledByDefault == true)
						{
							//Log.TraceWarning("GameFeaturePlugin {0}, does not set EnabledByDefault to false. This is required for built-in GameFeaturePlugins.", PluginFile.GetFileNameWithoutExtension());
						}
						
						// 确认所有游戏功能插件均已设置为 “明确加载” 状态
						// 这点非常重要，因为游戏功能插件需要在项目启动后才进行加载
						bool bExplicitlyLoaded = false;
						if (!RawObject.TryGetBoolField("ExplicitlyLoaded", out bExplicitlyLoaded) || bExplicitlyLoaded == false)
						{
							Logger.LogWarning("GameFeaturePlugin {0}, does not set ExplicitlyLoaded to true. This is required for GameFeaturePlugins.", PluginFile.GetFileNameWithoutExtension());
						}
						
						// 您在此处还可以添加一个项目特定的额外字段，例如，
						//string PluginReleaseVersion;
						//if (RawObject.TryGetStringField("MyProjectReleaseVersion", out PluginReleaseVersion))
						//{
						//		bEnabled = SomeFunctionOf(PluginReleaseVersion, CurrentReleaseVersion) || bBuildAllGameFeaturePlugins;
						//}
						
						if (bBuildAllGameFeaturePlugins)
						{
							// 我们目前处于这样一种状态：我们需要所有的游戏功能插件，但不包括那些我们无法加载或编译的插件。
							bEnabled = true;
						}
						
						// 防止在非编辑器版本中(打包)使用仅适用于编辑器的功能插件
						bool bEditorOnly = false;
						if (RawObject.TryGetBoolField("EditorOnly", out bEditorOnly))
						{
							if (bEditorOnly && (Target.Type != TargetType.Editor) && !bBuildAllGameFeaturePlugins)
							{
								// 该插件仅适用于编辑器使用，而我们正在构建一个非编辑器版本，因此该插件已被禁用。
								bForceDisabled = true;
							}
						}
						else
						{
							// 编辑器下专用的插件（可选）
						}
						
						//  有些插件仅应在特定分支中可用
						string RestrictToBranch;
						if (RawObject.TryGetStringField("RestrictToBranch", out RestrictToBranch))
						{
							if (!Target.Version.BranchName.Equals(RestrictToBranch, StringComparison.OrdinalIgnoreCase))
							{
								// The plugin is for a specific branch, and this isn't it
								// 该插件是针对特定分支设计的，而这里并非该分支。
								bForceDisabled = true;
								Logger.LogDebug("GameFeaturePlugin {Name} was marked as restricted to other branches. Disabling.", PluginFile.GetFileNameWithoutExtension());
							}
							else
							{
								Logger.LogDebug("GameFeaturePlugin {Name} was marked as restricted to this branch. Leaving enabled.", PluginFile.GetFileNameWithoutExtension());
							}
						}
						
						// 可以将插件标记为“从不编译”，这将覆盖上述设置。
						bool bNeverBuild = false;
						if (RawObject.TryGetBoolField("NeverBuild", out bNeverBuild) && bNeverBuild)
						{
							// 此插件已被标记为永远不进行编译，所以请勿进行编译操作。
							bForceDisabled = true;
							Logger.LogDebug("GameFeaturePlugin {Name} was marked as NeverBuild, disabling.", PluginFile.GetFileNameWithoutExtension());
						}
						
						// 记录插件的引用信息，以便后续进行验证操作
						JsonObject[] PluginReferencesArray;
						if (RawObject.TryGetObjectArrayField("Plugins", out PluginReferencesArray))
						{
							foreach (JsonObject ReferenceObject in PluginReferencesArray)
							{
								bool bRefEnabled = false;
								if (ReferenceObject.TryGetBoolField("Enabled", out bRefEnabled) && bRefEnabled == true)
								{
									string PluginReferenceName;
									if (ReferenceObject.TryGetStringField("Name", out PluginReferenceName))
									{
										string ReferencerName = PluginFile.GetFileNameWithoutExtension();
										
										if (!AllPluginReferencesByName.ContainsKey(ReferencerName))
										{
											AllPluginReferencesByName[ReferencerName] = new List<string>();
										}
										AllPluginReferencesByName[ReferencerName].Add(PluginReferenceName);
									}
								}
							}
						}
					}
					catch (Exception ParseException)
					{
						// 这个插件有问题 强制停用 保证鲁棒性
						Logger.LogWarning("Failed to parse GameFeaturePlugin file {Name}, disabling. Exception: {1}", PluginFile.GetFileNameWithoutExtension(), ParseException.Message);
						bForceDisabled = true;
					}
					
					// 禁用状态优先于启用状态
					if (bForceDisabled)
					{
						bEnabled = false;
					}
					
					// 输出此插件的最终决策结果
					Logger.LogDebug("ConfigureGameFeaturePlugins() has decided to {Action} feature {Name}", bEnabled ? "enable" : (bForceDisabled ? "disable" : "ignore"), PluginFile.GetFileNameWithoutExtension());
					
					// 启用或禁用它
					if (bEnabled)
					{
						Target.EnablePlugins.Add(PluginFile.GetFileNameWithoutExtension());
					}
					else if (bForceDisabled)
					{
						Target.DisablePlugins.Add(PluginFile.GetFileNameWithoutExtension());
					}
				}
			}
			
			// 如果您使用的是某个发布版本，请考虑进行参考性验证，以确保那些发布版本较早的插件不会依赖于发布版本较晚的内容。
		}
	}
	
}
