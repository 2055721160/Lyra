// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;
using System.Collections.Generic;

public class LyraEditorTarget : TargetRules
{
	public LyraEditorTarget( TargetInfo Target) : base(Target)
	{
		Type = TargetType.Editor;
		
		ExtraModuleNames.AddRange(new string[] { "LyraGame", "LyraEditor" });

		// 模块配置
		if (!bBuildAllModules)
		{
			// 不允许 UCLASS 和 USTRUCT 内使用非 TObjectPtr 类型的指针成员的设置
			NativePointerMemberBehaviorOverride = PointerMemberBehavior.Disallow;
		}
		
		LyraGameTarget.ApplySharedLyraTargetSettings(this);
		
		// RemoteSession 此插件用于触屏项目开发
		EnablePlugins.AddRange(new string[] {"RemoteSession"});
	}
}
