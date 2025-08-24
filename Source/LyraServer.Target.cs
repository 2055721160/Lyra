// Copyright Epic Games, Inc. All Rights Reserved.

using UnrealBuildTool;
using System.Collections.Generic;

public class LyraServerTarget : TargetRules
{
	public LyraServerTarget(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Server;
		
		ExtraModuleNames.AddRange(new string[] { "LyraGame" });
		
		LyraGameTarget.ApplySharedLyraTargetSettings(this);

		// 在游戏的测试版/最终发布版本（Shipping Build）中启用 检查（Checks）,方便定位问题
		bUseChecksInShipping = true;
	}
}
