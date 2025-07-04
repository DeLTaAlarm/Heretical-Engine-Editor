using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace HereticalEngineEditor.Models.PluginTools;

public class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver resolver;

    public PluginLoadContext(string mainAssemblyPath, string libsPath) : base(isCollectible: false)
    {
        resolver = new AssemblyDependencyResolver(mainAssemblyPath);
        this.libsPath = libsPath;
    }

    private readonly string libsPath;

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? path = resolver.ResolveAssemblyToPath(assemblyName);

        if (path == null && !string.IsNullOrEmpty(libsPath))
        {
            string potentialPath = Path.Combine(libsPath, assemblyName.Name + ".dll");
            if (File.Exists(potentialPath))
                return LoadFromAssemblyPath(potentialPath);
        }

        return path != null ? LoadFromAssemblyPath(path) : null;
    }
}
