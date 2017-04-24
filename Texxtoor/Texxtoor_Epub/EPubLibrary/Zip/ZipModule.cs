using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;

namespace Texxtoor.BaseLibrary.Pdf2Epub
{
  public class ZipModule
  {
    public static List<string> ScanFiles( string directory )
    {
      List<string>  files = new List< string >();
      Scan( directory, files );
      return files;
    }

    private static void Scan( string directory, List<string> files )
    {
      string[] filenames = Directory.GetFiles( directory );
      files.AddRange( filenames );

      string[] subs = Directory.GetDirectories( directory );
      foreach ( string sub in subs )
      {
        string subDir = Path.Combine( directory, sub );
        Scan( subDir, files );
      }
    }


    public static void BuildZipFile( string outputFile, List<string> filenames, string directory )
    {
      CompressionOption compression = CompressionOption.NotCompressed; // Don't compress first file.
      File.Delete( outputFile );
      foreach ( string filename in filenames )
      {
        Console.Write( "Adding file:" + filename + "..." );
        try
        {
          AddFileToZip( outputFile, filename, directory, compression );
          compression = CompressionOption.Maximum;
          Console.WriteLine( " Done!" );
        }
        catch ( Exception exception )
        {
          Console.WriteLine( "Error: " + exception.Message );
        }
      }
    }

    private const long BUFFER_SIZE = 4096;

    private static void AddFileToZip( string zipFilename, string fileToAdd, string directory, CompressionOption compression )
    {
      using ( Package zip = System.IO.Packaging.Package.Open( zipFilename, FileMode.OpenOrCreate ) )
      {
        string destFilename = "." + fileToAdd.Substring( directory.Length  );
        Uri uri = PackUriHelper.CreatePartUri( new Uri( destFilename, UriKind.Relative ) );
        if ( zip.PartExists( uri ) )
        {
          zip.DeletePart( uri );
        }
        PackagePart part = zip.CreatePart( uri, "", compression );
        using ( FileStream fileStream = new FileStream( fileToAdd, FileMode.Open, FileAccess.Read ) )
        {
          using ( Stream dest = part.GetStream() )
          {
            CopyStream( fileStream, dest );
          }
        }
      }
    }

    private static void CopyStream( System.IO.FileStream inputStream, System.IO.Stream outputStream )
    {
      long bufferSize = inputStream.Length < BUFFER_SIZE ? inputStream.Length : BUFFER_SIZE;
      byte[] buffer = new byte[ bufferSize ];
      int bytesRead = 0;
      long bytesWritten = 0;
      while ( ( bytesRead = inputStream.Read( buffer, 0, buffer.Length ) ) != 0 )
      {
        outputStream.Write( buffer, 0, bytesRead );
        bytesWritten += bufferSize;
      }
    }


  }
}