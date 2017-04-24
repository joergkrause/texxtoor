using System;
using System.Text.RegularExpressions;

namespace Texxtoor.BaseLibrary.Pdf2Epub
{
  public class MetaData
  {
    public string Title;
    public string Author;
    public string Date;

    public void Parse( string filename)
    {
      string data = FileIO.ReadToString( filename );
      FindMetaInfo( data );
    }

    private void FindMetaInfo( string data )
    {
      Title = GrabTagInfo( data, "Title" );
      Author = GrabMetaInfo( data, "author" );
      Date = GrabMetaInfo( data, "date" );
    }

    private string GrabTagInfo( string data, string title )
    {
      string searchStr = string.Format( "<{0}.*?>(?<info>.*?)</{0}>", title );
      Match match = Regex.Match( data, searchStr, RegexOptions.IgnoreCase | RegexOptions.Multiline );
      if ( match.Success == true )
      {
        return match.Groups[ "info" ].Value;
      }
      return string.Empty;
    }

    private string GrabMetaInfo( string data, string meta )
    {
      string searchStr = string.Format( "<META name=\"{0}\" content=\"(?<content>.*?)\">", meta );
      Match match = Regex.Match( data, searchStr, RegexOptions.IgnoreCase | RegexOptions.Multiline );
      if ( match.Success == true )
      {
        return match.Groups[ "content" ].Value;
      }
      return string.Empty;
    }

  
  }
}