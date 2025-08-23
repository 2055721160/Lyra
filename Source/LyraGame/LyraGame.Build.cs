// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;

public class LyraGame : ModuleRules
{
	public LyraGame(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PublicIncludePaths.AddRange(
				new string[] {
					"LyraGame"
				}
			);
		
		PrivateIncludePaths.AddRange(
			new string[] {
			}
		);
		
		PublicDependencyModuleNames.AddRange(new string[] { "Core", "CoreUObject", "Engine", "InputCore", "EnhancedInput" });

		PrivateDependencyModuleNames.AddRange(new string[] {  });

		// 若项目中使用Slate UI界面，请取消以下注释
		// PrivateDependencyModuleNames.AddRange(new string[] { "Slate", "SlateCore" });
		
		// 若项目需要使用在线功能，请取消以下注释
		// PrivateDependencyModuleNames.Add("OnlineSubsystem");
		
		// 如需启用OnlineSubsystemSteam在线子系统，请于uproject文件的插件列表中添加该插件并将Enabled属性设为true
	}
}
