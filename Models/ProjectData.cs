using HereticalEngineEditor.Models.PluginTools;
using Newtonsoft.Json;

namespace HereticalEngineEditor.Models;

public class ProjectData
{
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }

    [JsonIgnore]
    public string projectPath = "";

    [JsonIgnore]
    public PluginManager PluginManager { get; set; }
}
