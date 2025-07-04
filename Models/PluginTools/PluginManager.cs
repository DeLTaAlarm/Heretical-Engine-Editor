using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia;
using PluginBase;

namespace HereticalEngineEditor.Models.PluginTools;

public class PluginManager
{
    private string projectPath;
    private string editorPluginsPath;
    private string editorPluginLibsPath;
    public List<(PluginBaseClass plugin, PluginDataStructure data)> plugins;

    public PluginManager(string projectPath)
    {
        this.projectPath = projectPath;
        editorPluginsPath = Path.Combine(projectPath, "Plugins", "EditorPlugins");
        this.editorPluginLibsPath = Path.Combine(editorPluginsPath, "Libs");
        plugins = new();
        RefreshPlugins();
    }

    public void RefreshPlugins()
    {
        plugins.Clear();

        LoadBuiltinPlugins();

        if (!Directory.Exists(editorPluginsPath))
        {
            return;
        }

        string[] pluginDirs = Directory.GetDirectories(editorPluginsPath)
            .Where(directory => Path.GetFileName(directory) != "Libs")
            .ToArray();

        foreach (string dir in pluginDirs)
        {
            string[] dllFiles = Directory.GetFiles(dir, "*.dll", SearchOption.TopDirectoryOnly);

            foreach (string dll in dllFiles)
            {
                try
                {
                    PluginLoadContext context = new PluginLoadContext(dll, editorPluginLibsPath);
                    Assembly assembly = context.LoadFromAssemblyPath(dll);

                    Type[] types = assembly.GetTypes();

                    foreach (Type type in types)
                    {
                        if (typeof(PluginBaseClass).IsAssignableFrom(type) && !type.IsAbstract)
                        {
                            PluginDataAttribute attribute = type.GetCustomAttribute<PluginDataAttribute>();

                            if (attribute != null)
                            {
                                PluginBaseClass instance = (PluginBaseClass)Activator.CreateInstance(type);
                                plugins.Add((instance, attribute.data));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"[PluginManager] Ошибка при загрузке {dll}: {ex.Message}");
                }
            }
        }
    }

    private void LoadBuiltinPlugins()
    {
        Assembly currentAssembly = Assembly.GetExecutingAssembly();
        Type[] types = currentAssembly.GetTypes();

        foreach (Type type in types)
        {
            if (typeof(PluginBaseClass).IsAssignableFrom(type) && !type.IsAbstract)
            {
                PluginDataAttribute attribute = type.GetCustomAttribute<PluginDataAttribute>();

                if (attribute != null)
                {
                    PluginBaseClass instance = (PluginBaseClass)Activator.CreateInstance(type);
                    plugins.Add((instance, attribute.data));
                }
            }
        }
    }

    public IEnumerable<(PluginBaseClass plugin, PluginDataStructure data)> GetPlugins()
    {
        return plugins;
    }

    public void Compile(string path)
    {

    }

    public void Delete(string path)
    {

    }

    public void Open(string path)
    {

    }

    public void Save(string path)
    {

    }

    public void Rename(string path)
    {

    }

    public void Move(string path)
    {

    }

    public PluginBaseClass? GetPluginByExtension(string extension)
    {
        return plugins.FirstOrDefault(p =>
            string.Equals(p.data.fileExtension, extension, StringComparison.OrdinalIgnoreCase)).plugin;
    }

    public PluginBaseClass? GetPluginByPath(string path)
    {
        string ext = Path.GetExtension(path);
        return GetPluginByExtension(ext);
    }
}

