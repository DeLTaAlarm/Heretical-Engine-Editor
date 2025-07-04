using System;
using System.Diagnostics;
using System.IO;

namespace HereticalEngineEditor.Models;

public class ProjectBuilder
{
    private readonly string _projectRootPath;
    private readonly string _projectName;
    private readonly string _targetFramework;
    private readonly string _runtimeIdentifier;
    private readonly string _configuration;

    public ProjectBuilder(string projectRootPath, string projectName,
                          string targetFramework = "net6.0-windows",
                          string runtimeIdentifier = "win-x64",
                          string configuration = "Release")
    {
        _projectRootPath = projectRootPath;
        _projectName = projectName;
        _targetFramework = targetFramework;
        _runtimeIdentifier = runtimeIdentifier;
        _configuration = configuration;
    }

    /// <summary>
    /// Сборка проекта
    /// </summary>
    public bool BuildProject(out string outputExePath, out string log, string? customOutputDirectory = null)
    {
        outputExePath = string.Empty;
        log = string.Empty;

        string csprojPath = Path.Combine(_projectRootPath, _projectName + ".csproj");
        if (!File.Exists(csprojPath))
        {
            log = "Файл проекта не найден: " + csprojPath;
            return false;
        }

        // Куда класть exe
        string publishDir = customOutputDirectory ?? Path.Combine(_projectRootPath, "Build");

        // Убедимся, что директория есть
        Directory.CreateDirectory(publishDir);

        // Минимальная, framework-dependent сборка (без рантайма, но один .exe)
        string publishArgs = $"publish \"{csprojPath}\" -c {_configuration} -r {_runtimeIdentifier} " +
                             $"/p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true " +
                             $"--output \"{publishDir}\"";

        var psi = new ProcessStartInfo("dotnet", publishArgs)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _projectRootPath
        };

        try
        {
            using Process proc = Process.Start(psi)!;
            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            log = output + Environment.NewLine + error;

            if (proc.ExitCode != 0)
                return false;

            outputExePath = Path.Combine(publishDir, _projectName + ".exe");
            return File.Exists(outputExePath);
        }
        catch (Exception ex)
        {
            log = "Ошибка сборки: " + ex.Message;
            return false;
        }
    }



    /// <summary>
    /// Запуск скомпилированного .exe
    /// </summary>
    public bool RunBuiltProject(out string log)
    {
        log = string.Empty;

        if (!BuildProject(out string exePath, out string buildLog))
        {
            log = "Сборка не удалась:\n" + buildLog;
            return false;
        }

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(exePath)
            {
                WorkingDirectory = Path.GetDirectoryName(exePath)
            };

            Process.Start(startInfo);
            return true;
        }
        catch (Exception ex)
        {
            log = "Ошибка запуска: " + ex.Message;
            return false;
        }
    }
}
