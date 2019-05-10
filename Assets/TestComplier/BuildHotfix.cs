using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

public static class BuildHotfix 
{

    public const string HotFixCodePath = "../Hotfix/";
    public const string HotFixDllPath = "Data/HotfixCode/";

    public const string HotFixCsproj = "../Hotfix/Hotfix.csproj";


    /// <summary>
    /// 使用Unity 内置接口进行Build,目前没有接口可以让打出来的DLL 为DebugMode 无法调试~
    /// </summary>
    [MenuItem("Test/BuildHotfix")]
    public static void Build()
    {
        var getFilePath = Application.dataPath+"/" + HotFixCodePath;
        Debug.Log("getFilePath" + getFilePath);
        string[] scritpsFilePaths = Directory.GetFiles(getFilePath,"*.cs",SearchOption.AllDirectories).Where(
            x=> !x.Contains("AssemblyInfo")&& !x.Contains("TemporaryGeneratedFile")).ToArray();

        #region 根据csproj过滤工程

        //var csProjTxt= File.ReadAllText(Application.dataPath + "/" +HotFixCsproj);
        //XMLParser p =new XMLParser();
        //p.Parse(csProjTxt);
        //var projXml = p.ToXml();
        //projXml.GetChildNodeList("ItemGroup");
        #endregion



        AssemblyBuilder assemblyBuilder = new AssemblyBuilder("Assets/Resources/Data/HotfixCode/Hotfix_dll.bytes", scritpsFilePaths);
        assemblyBuilder.buildTarget = EditorUserBuildSettings.activeBuildTarget;
        List<string> assemblyNamesList = new List<string>();
        foreach (UnityEditor.Compilation.Assembly assembly in CompilationPipeline.GetAssemblies())
        {
            if ((assembly.flags & AssemblyFlags.EditorAssembly) == AssemblyFlags.None)
            {
                if(assembly.name.Contains("spine"))
                    continue;
                assemblyNamesList.Add("Temp/UnityVS_bin/Debug/" + assembly.name + ".dll");
            }
        }
        //EditorBuildRules
        assemblyBuilder.additionalReferences = assemblyNamesList.ToArray();

        assemblyBuilder.flags = AssemblyBuilderFlags.DevelopmentBuild;
        //assemblyBuilder.c
        assemblyBuilder.buildFinished += delegate (string assemblyPath, CompilerMessage[] compilerMessages)
        {
            var errorCount = compilerMessages.Count(m => m.type == CompilerMessageType.Error);
            var warningCount = compilerMessages.Count(m => m.type == CompilerMessageType.Warning);

            Debug.LogFormat("Assembly build finished for {0}", assemblyPath);
            Debug.LogFormat("Warnings: {0} - Errors: {0}", errorCount, warningCount);
            if (errorCount > 0)
            {
                foreach (var _compilerMessage in compilerMessages)
                {
                    if (_compilerMessage.type == CompilerMessageType.Error)
                    {
                        Debug.LogErrorFormat("File {0} Msg {1} ", _compilerMessage.file, _compilerMessage.message);
                    }
                    else
                    {
                        Debug.LogFormat("File {0} Msg {1} ", _compilerMessage.file, _compilerMessage.message);
                    }
                    
                }

                
            }
        };
        if (assemblyBuilder.Build())
        {
            Debug.Log("BuildSuccess");
        }
        else
        {
            Debug.LogErrorFormat("Failed to start build of assembly {0}!", assemblyBuilder.assemblyPath);
        }
        
    }
}
