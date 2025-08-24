// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;

/*
 * 扩展编辑器功能：为 Lyra 游戏提供专属的编辑器工具（如技能编辑、标签管理）。
 * 集成游戏系统：依赖 LyraGame 和 GameplayAbilities，实现游戏逻辑与编辑器的联动。
 * 安全与调试控制：通过条件编译限制敏感功能（RPC、Debug）在发布版本中的使用。
 **/
public class LyraEditor : ModuleRules
{
	public LyraEditor(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;
		
		// 这样包含会简化我们头文件的包含路径
		PublicIncludePaths.AddRange(
			new string[] {
				"LyraEditor"
			}
		);

		PrivateIncludePaths.AddRange(
			new string[] {
			}
		);
		
		PublicDependencyModuleNames.AddRange(
			new string[] {
				
				// UE 基础类型（字符串、容器等）
				"Core",
				
				// UObject 系统（反射、序列化）
				"CoreUObject",
				
				// 引擎核心功能（材质、粒子等）
				"Engine",
				
				// 编辑器扩展框架（自定义资产编辑器等）
				"EditorFramework",
				
				// 虚幻编辑器核心功能（关卡编辑、细节面板等）, 记住这个模块是运行在编辑器里的, 如果包含在 runtime.build.cs 会导致打包失败
				"UnrealEd",
				
				// 物理系统基础支持
				"PhysicsCore",
				
				// 游戏标签（GameplayTags）的编辑器支持（如标签管理界面）
				"GameplayTagsEditor",
				
				// 游戏任务系统的编辑器工具
				"GameplayTasksEditor",
				
				// 游戏技能系统（GA）的运行时逻辑
				"GameplayAbilities",
				
				// GA 系统的编辑器工具（如技能蓝图编辑）
				"GameplayAbilitiesEditor",
				
				// 开发工作室的遥测数据收集（用于分析开发行为）
				"StudioTelemetry",
				
				// Lyra 游戏本身的运行时模块（依赖游戏逻辑）
				"LyraGame",
			}
		);

		PrivateDependencyModuleNames.AddRange(
			new string[] {
				
				// 输入设备（键盘、鼠标）支持
				"InputCore",
				
				// UE 的 UI 框架（编辑器界面元素）
				"Slate",
				"SlateCore",
				
				// 编辑器工具栏和菜单扩展
				 "ToolMenus",
				 
				 // 编辑器UI样式（图标、字体等）
				 "EditorStyle",
				 
				 // 数据校验工具（防止无效资产）
				 "DataValidation",
				 
				 // 编辑器日志系统
				 "MessageLog",
				 
				 // 项目管理（插件、游戏路径等）
				 "Projects",
				 
				 // 开发者工具配置
				 "DeveloperToolSettings",
				 
				 // 资产集合管理（分类、标签等）
				 "CollectionManager",
				 
				 // 版本控制集成（Git、Perforce等）
				 "SourceControl",
				 
				 // Chaos 物理引擎支持
				 "Chaos"
			}
		);

		DynamicallyLoadedModuleNames.AddRange(
			new string[] {
			}
		);
		
		// 外部 RPC 框架基本设置
		// 在正式发布版本 (Shipping) 中, 框架内的功能将被剔除, 以消除潜在安全漏洞
		PrivateDependencyModuleNames.Add("ExternalRpcRegistry");
		if (Target.Configuration == UnrealTargetConfiguration.Shipping)
		{
			PublicDefinitions.Add("WITH_RPC_REGISTRY=0");
			PublicDefinitions.Add("WITH_HTTPSERVER_LISTENERS=0");
		}
		else
		{
			// HTTP 服务器功能 (仅开发/测试版启用)
			PrivateDependencyModuleNames.Add("HTTPServer");
			PublicDefinitions.Add("WITH_RPC_REGISTRY=1");
			PublicDefinitions.Add("WITH_HTTPSERVER_LISTENERS=1");
		}
		// 若在 Shipping/Test 版本中使用 DrawDebug 函数, 会生成编译报错
		PublicDefinitions.Add("SHIPPING_DRAW_DEBUG_ERROR=1");
	}
}