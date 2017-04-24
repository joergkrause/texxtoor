using System.IO;

namespace Texxtoor.BaseLibrary.Pdf2Epub
{
  public class FileIO
  {
    public static string ReadToString( string filename)
    {
      using ( TextReader reader = new StreamReader( filename) )
      {
        return reader.ReadToEnd();  
      }  
    }

    public static void WriteToFile( string filename, string data)
    {
      using ( TextWriter writer = new StreamWriter( filename ) )
      {
        writer.Write( data );
      }
    }
  }
}