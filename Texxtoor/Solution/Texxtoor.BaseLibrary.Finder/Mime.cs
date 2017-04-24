using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Texxtoor.BusinessLayer.Finder {
  internal static class Mime {
    private static Dictionary<string, string> _mimeTypes;
    static Mime() {
      _mimeTypes = new Dictionary<string, string>();
      var assembly = Assembly.GetExecutingAssembly();
      using (var stream = assembly.GetManifestResourceStream("Texxtoor.BaseLibrary.Finder.mimeTypes.txt")) {
        using (var reader = new StreamReader(stream)) {
          while (!reader.EndOfStream) {
            var line = reader.ReadLine();
            line = line.Trim();
            if (string.IsNullOrWhiteSpace(line) || line[0] == '#') continue;
            var parts = line.Split(' ');
            if (parts.Length <= 1) continue;
            var mime = parts[0];
            for (var i = 1; i < parts.Length; i++) {
              var ext = parts[i].ToLower();
              if (!_mimeTypes.ContainsKey(ext)) {
                _mimeTypes.Add(ext, mime);
              }
            }
          }
        }
      }
    }
    public static string GetMimeType(string extension) {
      return _mimeTypes.ContainsKey(extension) ? _mimeTypes[extension] : "unknown";
    }
  }
}
