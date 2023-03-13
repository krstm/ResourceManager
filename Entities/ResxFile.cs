
using System.Collections.Generic;

namespace ResourceManager.Entities
{
    public class ResxFile
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public Dictionary<string, string> KeysAndWordings { get; set; }
    }
}
