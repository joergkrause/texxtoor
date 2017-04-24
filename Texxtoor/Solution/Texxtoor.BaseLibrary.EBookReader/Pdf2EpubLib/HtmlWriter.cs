using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Texxtoor.BaseLibrary.Pdf2Epub
{
  public class HtmlWriter
  {
    private string _WorkingDirectory;
    private string _HtmlHeader;
    private string _HtmlFooter;
    private StreamWriter _OutputStream;
    private int _OutputSize;
    private int _MaxOutputSize;
    private int _FileCount;
    public string CurrentFilename;

    public HtmlWriter( string workingDirectory, string htmlHeader, string htmlFooter, int maxOutputSize )
    {
      _WorkingDirectory = workingDirectory;
      _HtmlHeader = htmlHeader;
      _HtmlFooter = htmlFooter;
      _FileCount = 0;
      _OutputSize = 0;
      _MaxOutputSize = maxOutputSize;
      _OutputStream = StartNewOutputStream();
    }

    public StreamWriter StartNewOutputStream()
    {
      if ( _OutputStream != null )
      {
        CloseOutputStream();
      }
      string filePath = BuildFilePath();
      StreamWriter writer = new StreamWriter( filePath );
      writer.WriteLine( _HtmlHeader );
      _OutputSize = _HtmlHeader.Length;
      return writer;
    }

    private string BuildFilePath()
    {
      CurrentFilename = string.Format( "Part{0}.html", _FileCount );
      _FileCount += 1;
      return Path.Combine( _WorkingDirectory, CurrentFilename );

    }

    public void CloseOutputStream()
    {
      if ( _OutputStream != null )
      {
        _OutputStream.WriteLine( _HtmlFooter );
        _OutputStream.Close();
      }
      _OutputStream = null;
    }

    public void AddPage( string newPage )
    {
      if ( NeedNewStream( newPage.Length ) == true )
      {
        _OutputStream = StartNewOutputStream();  
      }
      WritePageToStream( newPage );
    }


    private bool NeedNewStream( int length )
    {
      return ( ( _OutputStream == null ) || ( ( _OutputSize + length ) >= _MaxOutputSize ) );
    }

    private void WritePageToStream( string newPage )
    {
      _OutputStream.WriteLine( newPage );
      _OutputSize += newPage.Length;
    }
  }
}
