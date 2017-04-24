using System;

namespace Texxtoor.BaseLibrary.Core.Extensions {
  public static class StreamHelper {

    private static readonly int MB = Convert.ToInt32(Math.Pow(2, 20));
    private static readonly int KB = Convert.ToInt32(Math.Pow(2, 10));

    public static string SizeFromStream(this System.IO.Stream stream){
      long length = 0;
      try {
        length = stream.Length;
      } catch {
      } 
      if (length > MB) {
        return String.Format("{0:#.##} MB", length / MB);
      }
      if (length > KB) {
        return String.Format("{0:#.##} KB", length / KB);
      }
      return String.Format("{0:#.##} Byte", length);
    }

    public static byte[] ReadToEnd(this System.IO.Stream stream) {
      var originalPosition = stream.Position;
      stream.Position = 0;

      try {
        var readBuffer = new byte[4096];

        var totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0) {
          totalBytesRead += bytesRead;

          if (totalBytesRead != readBuffer.Length) continue;
          var nextByte = stream.ReadByte();
          if (nextByte == -1) continue;
          var temp = new byte[readBuffer.Length * 2];
          Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
          Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
          readBuffer = temp;
          totalBytesRead++;
        }

        var buffer = readBuffer;
        if (readBuffer.Length != totalBytesRead) {
          buffer = new byte[totalBytesRead];
          Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
        }
        return buffer;
      } finally {
        stream.Position = originalPosition;
      }
    }
  }
}
