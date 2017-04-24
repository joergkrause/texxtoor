using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Texxtoor.BusinessLayer.Finder {
  internal static class Helper {
    public static string GetMimeType(FileInfo file){
      return file.Extension.Length > 1 ? Mime.GetMimeType(file.Extension.ToLower().Substring(1)) : "unknown";
    }

    public static string GetMimeType(string fileName){
      if (String.IsNullOrEmpty(fileName)) return "nofile";
      if (fileName.Contains(".")) {
        var ext = Path.GetExtension(fileName);
        if (ext.Length >= 3 && ext.StartsWith(".")) {
          ext = ext.Substring(1);
        }
        return Mime.GetMimeType(ext);
      } else {
        return null;
      }
    }
    public static string EncodePath(Guid path) {
      return HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(path.ToString()));
    }
    public static string DecodePath(string path) {
      return Encoding.UTF8.GetString(HttpServerUtility.UrlTokenDecode(path));
    }

    public static string GetFileMd5(FileInfo info) {
      return GetFileMd5(info.Name, info.LastWriteTimeUtc);
    }

    public static string GetFileMd5(string fileName, DateTime modified) {
      fileName += modified.ToFileTimeUtc();
      var fileNameChars = fileName.ToCharArray();
      var buffer = new byte[_stringEncoder.GetByteCount(fileNameChars, 0, fileName.Length, true)];
      _stringEncoder.GetBytes(fileNameChars, 0, fileName.Length, buffer, 0, true);
      return BitConverter.ToString(_md5CryptoProvider.ComputeHash(buffer)).Replace("-", string.Empty);
    }

    public static string GetDuplicatedName(FileInfo file) {
      var parentPath = file.DirectoryName;
      var name = Path.GetFileNameWithoutExtension(file.Name);
      var ext = file.Extension;

      var newName = string.Format(@"{0}\{1} copy{2}", parentPath, name, ext);
      if (!File.Exists(newName)) {
        return newName;
      } else {
        bool finded = false;
        for (int i = 1; i < 10 && !finded; i++) {
          newName = string.Format(@"{0}\{1} copy {2}{3}", parentPath, name, i, ext);
          if (!File.Exists(newName))
            finded = true;
        }
        if (!finded)
          newName = string.Format(@"{0}\{1} copy {2}{3}", parentPath, name, Guid.NewGuid(), ext);
      }

      return newName;
    }

    private static readonly Encoder _stringEncoder = Encoding.UTF8.GetEncoder();
    private static readonly MD5CryptoServiceProvider _md5CryptoProvider = new MD5CryptoServiceProvider();
  }
}