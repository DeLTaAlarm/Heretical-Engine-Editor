using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace HereticalEngineEditor.Models;

public static class ProjectManager
{
    public static ProjectData CreateProject(string path, string projectName)
    {
        ProjectData projectData = new ProjectData();
        projectData.ProjectName = projectName;
        projectData.Description = "A new project";
        projectData.Version = "0.0.0.0";

        CreateProjectDirectories(path, projectData);
        CreateCSProject(path, projectData);
        CreateEngineProject(path, projectData);

        projectData.PluginManager = new PluginTools.PluginManager(projectData.projectPath);
        return projectData;
    }

    private static void CreateProjectDirectories(string path, ProjectData projectData)
    {
        Directory.CreateDirectory(path);

        string contentPath = Path.Combine(path, "Content");
        string pluginsPath = Path.Combine(path, "Plugins");
        string corePath = Path.Combine(path, "Core");
        string configsPath = Path.Combine(path, "Configs");
        string sourcePath = Path.Combine(path, "Source");
        string binariesPath = Path.Combine(path, "Bin");
        string logsPath = Path.Combine(path, "Logs");

        Directory.CreateDirectory(contentPath);
        Directory.CreateDirectory(pluginsPath);
        Directory.CreateDirectory(corePath);
        Directory.CreateDirectory(configsPath);
        Directory.CreateDirectory(sourcePath);
        Directory.CreateDirectory(binariesPath);
        Directory.CreateDirectory(logsPath);

        string editorPluginsPath = Path.Combine(pluginsPath, "EditorPlugins");
        string enginePluginsPath = Path.Combine(pluginsPath, "EnginePlugins");
       

        Directory.CreateDirectory(editorPluginsPath);
        Directory.CreateDirectory(enginePluginsPath);
        
        string editorPluginsLibsPath = Path.Combine(editorPluginsPath, "Lib");

        Directory.CreateDirectory(editorPluginsLibsPath);
    }

    private static void CopyDependenciesAndPlugins(string projectPath)
    {
        string exeDir = AppContext.BaseDirectory;

        string libsTargetPath = Path.Combine(projectPath, "Plugins", "EditorPlugins", "Lib");

        string[] dllFiles = new string[]
        {
            "Avalonia.dll",
            "Avalonia.Base.dll",
            "Avalonia.Controls.dll",
            "Avalonia.Markup.dll",
            "Avalonia.Markup.Xaml.dll"
        };

        foreach (var dll in dllFiles)
        {
            string sourceFile = Path.Combine(exeDir, dll);
            string destFile = Path.Combine(libsTargetPath, dll);

            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, destFile, overwrite: true);
            }
        }

        string sourceEditorPluginsPath = Path.Combine(exeDir, "EditorPlugins");
        string targetEditorPluginsPath = Path.Combine(projectPath, "Plugins", "EditorPlugins");

        if (Directory.Exists(sourceEditorPluginsPath))
        {
            foreach (var dir in Directory.GetDirectories(sourceEditorPluginsPath))
            {
                string dirName = Path.GetFileName(dir);
                string targetDir = Path.Combine(targetEditorPluginsPath, dirName);
                CopyDirectory(dir, targetDir);
            }
        }
    }

    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string targetFilePath = Path.Combine(targetDir, fileName);
            File.Copy(file, targetFilePath, overwrite: true);
        }

        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(directory);
            string targetSubDir = Path.Combine(targetDir, dirName);
            CopyDirectory(directory, targetSubDir);
        }
    }

    private static void CreateCSProject(string path,
                                    ProjectData projectData,
                                    string outputType = "WinExe",
                                    string targetFramework = "net6.0-windows")
    {
        string engineDir = AppDomain.CurrentDomain.BaseDirectory;
        string engineCorePath = Path.Combine(engineDir, "EngineCore");
        string sourcePath = Path.Combine(path, "Source");
        string projectCorePath = Path.Combine(path, "Core");

        foreach (string file in Directory.GetFiles(engineCorePath, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(engineCorePath, file);
            string destFile = Path.Combine(projectCorePath, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(destFile));
            File.Copy(file, destFile, overwrite: true);
        }

        string[] dllFiles = Directory.Exists(projectCorePath)
            ? Directory.GetFiles(projectCorePath, "*.dll", SearchOption.AllDirectories)
            : Array.Empty<string>();

        Directory.CreateDirectory(sourcePath);
        string programCode = """
                            using HCore;
                        
                            AssemblyLoader assemblyLoader = new AssemblyLoader();
                            using GameLoopCore game = new GameLoopCore(assemblyLoader);
                            assemblyLoader.LoadAssembly();
                            game.Run();
                            """;

        string programPath = Path.Combine(sourcePath, "Program.cs");
        File.WriteAllText(programPath, programCode);

        StringBuilder csprojContent = new StringBuilder();
        csprojContent.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
        csprojContent.AppendLine();
        csprojContent.AppendLine("  <PropertyGroup>");
        csprojContent.AppendLine($"    <OutputType>{outputType}</OutputType>");
        csprojContent.AppendLine($"    <TargetFramework>{targetFramework}</TargetFramework>");
        csprojContent.AppendLine("    <Nullable>enable</Nullable>");
        csprojContent.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");
        csprojContent.AppendLine("    <UseWindowsForms>true</UseWindowsForms>");
        csprojContent.AppendLine("  </PropertyGroup>");
        csprojContent.AppendLine();

        if (dllFiles.Length > 0)
        {
            csprojContent.AppendLine("  <ItemGroup>");
            foreach (string dll in dllFiles)
            {
                string relativeDllPath = Path.GetRelativePath(sourcePath, dll).Replace("\\", "/");
                csprojContent.AppendLine($"    <Reference Include=\"{Path.GetFileNameWithoutExtension(dll)}\">");
                csprojContent.AppendLine($"      <HintPath>{relativeDllPath}</HintPath>");
                csprojContent.AppendLine("    </Reference>");
            }
            csprojContent.AppendLine("  </ItemGroup>");
            csprojContent.AppendLine();
        }

        csprojContent.AppendLine("</Project>");

        string csprojPath = Path.Combine(sourcePath, $"{projectData.ProjectName}.csproj");
        File.WriteAllText(csprojPath, csprojContent.ToString());
    }

    private static void CreateEngineProject(string path, ProjectData projectData)
    {
        string projectFilePath = Path.Combine(path, $"{projectData.ProjectName}.heproj");

        var json = JsonConvert.SerializeObject(projectData, Formatting.Indented);

        File.WriteAllText(projectFilePath, json);

    }

    public static ProjectData OpenEngineProject(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        string json = File.ReadAllText(path);

        ProjectData projectData = JsonConvert.DeserializeObject<ProjectData>(json);
        projectData.projectPath = Directory.GetParent(path).ToString();
        projectData.PluginManager = new PluginTools.PluginManager(projectData.projectPath);
        return projectData;
    }
}
