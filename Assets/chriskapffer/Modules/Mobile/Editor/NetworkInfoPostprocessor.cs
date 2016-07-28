using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace ChrisKapffer.Mobile
{
    /// <summary>
    /// This makes sure that the CoreTelephony framework is added to Xcode's list of linked libraries.
    /// </summary>
	public static class NetworkInfoPostprocessor
	{
		[PostProcessBuild]
		public static void OnPostProcessBuild(BuildTarget target, string path) {
			if (target == BuildTarget.iOS) {
				PostProcessBuild_iOS(path);
			}
		}

		private static void PostProcessBuild_iOS(string path) {
            string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

            PBXProject proj = new PBXProject();

            // get all previous project settings
            proj.ReadFromString(File.ReadAllText(projPath));

            string target = proj.TargetGuidByName("Unity-iPhone");
            proj.AddFrameworkToProject(target, "CoreTelephony.framework", true);

            // update Xcode project file
            File.WriteAllText(projPath, proj.WriteToString());
		}
	}
}
