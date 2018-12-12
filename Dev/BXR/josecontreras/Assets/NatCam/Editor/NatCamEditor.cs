/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatCamU.Extensions {

	using UnityEditor;
	using System;

	#if UNITY_IOS
	using UnityEditor.Callbacks;
	using UnityEditor.iOS.Xcode;
	using System.IO;
	#endif

	public static class NatCamEditor {

		#if UNITY_IOS

		[PostProcessBuild]
		static void LinkFrameworks (BuildTarget buildTarget, string path) {
			if (buildTarget != BuildTarget.iOS) return;
			string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
			PBXProject proj = new PBXProject();
			proj.ReadFromString(File.ReadAllText(projPath));
			string target = proj.TargetGuidByName("Unity-iPhone");
			foreach (var framework in new [] { "Accelerate.framework", "CoreImage.framework" })
				proj.AddFrameworkToProject(target, framework, true);
			File.WriteAllText(projPath, proj.WriteToString());
		}
		#endif
	}
}