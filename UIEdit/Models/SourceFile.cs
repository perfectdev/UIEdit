using System.IO;

namespace UIEdit.Models {
    public class SourceFile {
        public string ProjectPath { get; set; }
        public string FileName { get; set; }
        public string ShortFileName { get { return Path.GetFileName(FileName); } }
        public string FilePath {
            get {
                var tmp = Path.GetFullPath(FileName).Substring(0, FileName.Length - ShortFileName.Length - 1).Substring(ProjectPath.Length);
                return tmp.Length == 0 ? @"\" : tmp; 
            }
        }
        public bool PrefixExists { get; set; }
    }
}
