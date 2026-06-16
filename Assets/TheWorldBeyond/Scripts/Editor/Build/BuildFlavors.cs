// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using System.IO;
using UnityEditor;
using UnityEditor.Build;

[MetaCodeSample("TheWorldBeyond")]
public class BuildFlavors
{
    private const string ApkAppName = "TheWorldBeyond";
    private const string SceneRoot = "Assets/";
    private const string buildFolderName = "build";
    private static readonly string[] projectScenes = {
    SceneRoot + "TheWorldBeyond.unity"
  };

    public static void BuildWin()
    {
        var buildPath = Path.Combine(Path.GetFullPath("."), buildFolderName);
        BuildGeneric("MyScenes.exe",
          projectScenes,
          BuildOptions.ShowBuiltPlayer,
          buildPath,
          BuildTarget.StandaloneWindows64);
    }

    public static void BuildAndroid64()
    {
        Android(AndroidArchitecture.ARM64);
    }

    public static void BuildAndroid32()
    {
        Android(AndroidArchitecture.ARMv7);
    }

    public static void Android(AndroidArchitecture architecture)
    {
        var previousAppIdentifier = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
        PlayerSettings.Android.targetArchitectures = architecture;
        var friendlyPrint = architecture == AndroidArchitecture.ARMv7 ? "32" : "64";
        var implementation = architecture == AndroidArchitecture.ARM64 ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, implementation);
        
        var activeProfile = UnityEditor.Build.Profile.BuildProfile.GetActiveBuildProfile();
        UnityEditor.Build.Reporting.BuildReport error;
        if (activeProfile != null)
        {
            UnityEngine.Debug.Log("Building using Active Build Profile: " + activeProfile.name);
            var profileOptions = new UnityEditor.BuildPlayerWithProfileOptions
            {
                buildProfile = activeProfile,
                locationPathName = string.Format("builds/{0}_{1}.apk", ApkAppName, friendlyPrint),
                options = BuildOptions.None
            };
            try
            {
                error = BuildPipeline.BuildPlayer(profileOptions);
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, previousAppIdentifier);
                HandleBuildError.Check(error);
            }
            catch
            {
                UnityEngine.Debug.Log("Exception while building with profile: exiting with exit code 2");
                EditorApplication.Exit(2);
            }
        }
        else
        {
            UnityEngine.Debug.Log("Building using standard Android settings (no active Build Profile found)");
            var buildOptions = new BuildPlayerOptions
            {
                locationPathName = string.Format("builds/{0}_{1}.apk", ApkAppName, friendlyPrint),
                scenes = projectScenes,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = new BuildOptions()
            };
            try
            {
                error = BuildPipeline.BuildPlayer(buildOptions);
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, previousAppIdentifier);
                HandleBuildError.Check(error);
            }
            catch
            {
                UnityEngine.Debug.Log("Exception while building: exiting with exit code 2");
                EditorApplication.Exit(2);
            }
        }
    }

    private static void BuildGeneric(string buildName,
      string[] scenes,
      BuildOptions buildOptions,
      string path,
      BuildTarget target)
    {

        if (!string.IsNullOrEmpty(buildName) && null != scenes && scenes.Length > 0)
        {
            var fullPath = Path.Combine(path, buildName);
            if (!string.IsNullOrEmpty(path))
            {
                BuildPipeline.BuildPlayer(scenes, fullPath, target, buildOptions);
            }
            else
            {
                UnityEngine.Debug.Log("Invalid build path!");
            }
        }
        else
        {
            UnityEngine.Debug.Log("Invalid build configuration!");
        }
    }
}

[MetaCodeSample("TheWorldBeyond")]
public class HandleBuildError
{
    public static void Check(UnityEditor.Build.Reporting.BuildReport buildReport)
    {
        bool buildSucceeded =
            buildReport.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
        if (buildReport.summary.platform == BuildTarget.Android)
        {
            // Android can fail to produce the output even if the build is marked as succeeded in some rare
            // scenarios, notably if the Unity directory is read-only... Annoying, but needs to be handled!
            buildSucceeded = buildSucceeded && File.Exists(buildReport.summary.outputPath);
        }
        if (buildSucceeded)
        {
            UnityEngine.Debug.Log("Exiting with exit code 0");
            EditorApplication.Exit(0);
        }
        else
        {
            UnityEngine.Debug.Log("Exiting with exit code 1");
            EditorApplication.Exit(1);
        }
    }
}
