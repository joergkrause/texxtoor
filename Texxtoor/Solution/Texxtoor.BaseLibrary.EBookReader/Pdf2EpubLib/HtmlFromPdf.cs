using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Texxtoor.BaseLibrary.Pdf2Epub
{
  public static class HtmlFromPdf
  {
    public static string CreateHtml( string pdfFile )
    {
      string workPath;
      BuildEpubOutputDir( pdfFile, out workPath );
      string htmlFilename = "";
      // TODO: Use C Lib Texxtoor.BaseLibrary.Converter's entry points to start conversion
      return htmlFilename;
    }

    private static void BuildEpubOutputDir( string pdfFile, out string workPath )
    {
      string workingDir = GetWorkingDir( pdfFile );
      string baseName = Path.GetFileNameWithoutExtension( pdfFile );
      workPath = BuildSubDirName( workingDir, baseName );
      Directory.CreateDirectory( workPath );
      workPath += "\\OEBPS";
      Directory.CreateDirectory( workPath );
    }

    private static string GetWorkingDir( string fileName )
    {
      string fullPath = Path.GetFullPath( fileName );
      return Path.GetDirectoryName( fullPath );

    }

    private static string BuildSubDirName( string dir, string name )
    {
      string subDir = Path.Combine( dir, name );
      for ( int number = 0; ( Directory.Exists( subDir )) && (number < 20000); number++ )
      {
        subDir = Path.Combine( dir, string.Format("{0}({1})",name, number) );  
      }
      if ( Directory.Exists( subDir ) )
      {
        throw new ApplicationException("Unable to find new subdirectory for output!");
      }
      return subDir;
    }

  
  }
}
