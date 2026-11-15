using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Game.Editor
{
    // Steamworks.NET needs steam_appid.txt next to the executable when the game is not launched by Steam.
    public sealed class SteamAppIdPostprocessor : IPostprocessBuildWithReport
    { 
        private const string FileName = "steam_appid.txt";

        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platformGroup != BuildTargetGroup.Standalone || !File.Exists(FileName)) return;
             
            string buildDirectory = Path.GetDirectoryName(report.summary.outputPath);
            File.Copy(FileName, Path.Combine(buildDirectory, FileName), overwrite: true);
        }
    }
}