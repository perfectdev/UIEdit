using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Ookii.Dialogs.Wpf;
using UIEdit.Models;

namespace UIEdit.Controllers {
    public class ProjectController {
        public string InterfacesPath { get; set; }
        public string SurfacesPath { get; set; }
        public List<SourceFile> Files { get; set; }

        public void SetLocationInterfaces() {
            var dlg = new VistaFolderBrowserDialog();
            var showDialog = dlg.ShowDialog();
            if (showDialog == null || !((bool) showDialog)) return;
            InterfacesPath = dlg.SelectedPath;
            //InterfacesPath = @"B:\DEV\UIEdit\UIEdit\bin\Debug\interfaces.pck.files";
            Files = new List<SourceFile>();
            foreach (var file in Directory.GetFiles(InterfacesPath, "*.xml", SearchOption.AllDirectories)) {
                Files.Add(new SourceFile { FileName = file, ProjectPath = InterfacesPath});
            }
        }

        public void SetLocationSurfaces() {
            var dlg = new VistaFolderBrowserDialog();
            var showDialog = dlg.ShowDialog();
            if (showDialog == null || !((bool) showDialog)) return;
            SurfacesPath = dlg.SelectedPath;
            //SurfacesPath = @"B:\DEV\UIEdit\UIEdit\bin\Debug\surfaces.pck.files";
        }

        public string GetSourceFileContent(SourceFile currentSourceFile) {
            var buffer = Encoding.GetEncoding("utf-16LE").GetBytes(File.ReadAllText(currentSourceFile.FileName));
            using (var ms = new MemoryStream(buffer)) {
                using (var br = new BinaryReader(ms)) {
                    var i = 0;
                    while ((br.ReadInt16() == -257)) {
                        i += 2;
                        currentSourceFile.PrefixExists = true;
                    }
                    ms.Position -= 2;
                    if (i > 0)
                        return SafeXml(Encoding.GetEncoding("utf-16LE").GetString(br.ReadBytes(buffer.Length - i)));
                }
            }
            return SafeXml(Encoding.GetEncoding("utf-16LE").GetString(buffer));
        }

        public void SaveSourceFileContent(SourceFile currentSourceFile, string text) {
            if (currentSourceFile.PrefixExists) {
                var prefixBytes = new byte[] { 255, 254 };
                var prefix = Encoding.GetEncoding("utf-16LE").GetString(prefixBytes);
                text = string.Format("{0}{1}", prefix, text);
            }
            File.WriteAllText(currentSourceFile.FileName, UnsafeXml(text), Encoding.GetEncoding("utf-16LE"));
        }

        private string SafeXml(string xml) {
            foreach (var match in Regex.Matches(xml, '"'+@"\S+<\S+"+'"', RegexOptions.IgnoreCase)) {
                var safeString = match.ToString().Replace("<", "###LeftArrow###").Replace(">", "###RightArrow###");
                xml = xml.Replace(match.ToString(), safeString);
            }
            return xml;
        }

        private string UnsafeXml(string xml) {
            return xml.Replace("###LeftArrow###", "<").Replace("###RightArrow###", ">");
        }
    }
}
