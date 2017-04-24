using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Texxtoor.BaseLibrary.Core.Utilities.Storage {
  public class MimeTypeHelper {

    [DllImport(@"urlmon.dll", CharSet = CharSet.Auto)]
    private extern static System.UInt32 FindMimeFromData(System.UInt32 pBC, [MarshalAs(UnmanagedType.LPStr)] System.String pwzUrl, [MarshalAs(UnmanagedType.LPArray)] byte[] pBuffer, System.UInt32 cbSize, [MarshalAs(UnmanagedType.LPStr)] System.String pwzMimeProposed, System.UInt32 dwMimeFlags, out System.UInt32 ppwzMimeOut, System.UInt32 dwReserverd);

    public static string DefaultMimeType = "application/octet-stream";
    public static int MimeSampleSize = 256;

    public string GetMimeFromFile(string filename) {
      if (!File.Exists(filename)) throw new FileNotFoundException(filename + " not found");
      var buffer = new byte[256];
      using (var fs = new FileStream(filename, FileMode.Open)) {
        if (fs.Length >= 256) fs.Read(buffer, 0, 256);
        else fs.Read(buffer, 0, (int)fs.Length);
      }
      try {
        UInt32 mimetype;
        FindMimeFromData(0, null, buffer, 256, null, 0, out mimetype, 0);
        var mimeTypePtr = new IntPtr(mimetype);
        var mime = Marshal.PtrToStringUni(mimeTypePtr);
        Marshal.FreeCoTaskMem(mimeTypePtr);
        return mime;
      } catch (Exception) {
        return "unknown/unknown";
      }
    }

    public static string GetMimeFromBytes(byte[] data) {
      try {
        uint mimeType;
        FindMimeFromData(0, null, data, (uint)MimeSampleSize, null, 0, out mimeType, 0);
        var mimePointer = new IntPtr(mimeType);
        var mime = Marshal.PtrToStringUni(mimePointer);
        Marshal.FreeCoTaskMem(mimePointer);

        return mime ?? DefaultMimeType;
      } catch {
        return DefaultMimeType;
      }
    }

    public static string GetFromExtension(string ext) {
      string mimeType = "application/unknown";
      RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(ext.ToLower());
      if (regKey != null) {
        object contentType = regKey.GetValue("Content Type");
        if (contentType != null) mimeType = contentType.ToString();
      }
      return mimeType;
    }
  }
}