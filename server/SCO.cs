using System.Collections.Generic;
using Newtonsoft.Json;

namespace adobe_manager
{
    public class SCO
    {
        [JsonProperty("@sco-id")] public string id { get; set; }
        [JsonProperty("@depth")] public int depth { get; set; }
        [JsonProperty("name")] public string title { get; set; }
        [JsonProperty("@icon")] public string type { get; set; }
        [JsonProperty("@byte-count")] public string size { get; set; }
        [JsonProperty("@folder-id")] public string folder_id { get; set; }
        [JsonProperty("date-created")] public string date { get; set; }
    }

    public class SCOINFO
    {
        [JsonProperty("name")] public string name { get; set; }
        [JsonProperty("folder_id")] public string folder_id { get; set; }
    }

    public class DeleteSCOViewModel
    {
        public string url { get; set; }
        public string session { get; set; }
        public List<string> ids { get; set; }
    }

    public class FilePathViewModel
    {
        public string url { get; set; }
        public string session { get; set; }
        public string id { get; set; }
        public int depth { get; set; }
    }

    public class Folder
    {
        [JsonProperty("@sco-id")] public string id { get; set; }
        [JsonProperty("name")] public string name { get; set; }
    }

    public class Jtoken
    {
        [JsonProperty("expanded-scos")] public string expandedScos { get; set; }
    }
}