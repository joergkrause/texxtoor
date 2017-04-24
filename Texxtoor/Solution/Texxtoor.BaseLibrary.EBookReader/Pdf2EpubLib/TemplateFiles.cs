using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Xml.Linq;

namespace Texxtoor.BaseLibrary.Pdf2Epub
{
  public class TemplateFiles
  {
    static Dictionary<string, string> _MediaTypes = new Dictionary< string, string >(); 


    static TemplateFiles()
    {
      _MediaTypes.Add( ".html", "application/xhtml+xml" );
      _MediaTypes.Add( ".jpeg", "image/jpeg" );
      _MediaTypes.Add( ".jpg", "image/jpeg" );
      _MediaTypes.Add( ".png", "image/png" );
      _MediaTypes.Add( ".css", "text/css" );
      _MediaTypes.Add( ".ncx", "application/x-dtbncx+xml" );
    }


    public static string  Read( string filename )
    {
      string path = GetFullTemplateName( filename );
      return File.ReadAllText( path );
    }

    private static string GetFullTemplateName( string filename )
    {
      string templateDir = GetTemplateDirectory();
      return Path.Combine( templateDir, filename );
    }

    private static string GetTemplateDirectory()
    {
      return Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "Template" );  
   }

    public static void Write( string filename, string data )
    {
      File.WriteAllText( filename, data );
    }

    public static void AddAllTemplateFiles( string workingDir, MetaData data, string name, IDictionary<string, string> parameters )
    {
      Directory.CreateDirectory( Path.Combine(workingDir, "..\\META-INF") );
      CopyFile( "container.xml", Path.Combine( workingDir, "..\\META-INF\\container.xml" ) );
      CopyFile( "mimetype", Path.Combine( workingDir, "..\\mimetype" ) );
      CopyFile( "stylesheet.css", Path.Combine( workingDir, "stylesheet.css" ) );
      CopyFile( "titlepage.css", Path.Combine( workingDir, "titlepage.css" ) );
      CopyFile( "titlepage.html", Path.Combine( workingDir, "titlepage.html" ) );

      List<string> files = FindFiles( workingDir, name );
      string filesAsXml = FileParameters( files );
      parameters.Add( "##files##", filesAsXml );
      CopyAndUpdateFile( "oebps.opf", Path.Combine( workingDir, Path.Combine( workingDir, "oebps.opf" )), parameters );
      CopyAndUpdateFile( "titlepage.html", Path.Combine( workingDir, "titlepage.html" ), parameters );
    }



    private static void CopyFile( string source, string destination )
    {
      string data = Read( source );
      Write( destination, data );
    }

    private static void CopyAndUpdateFile( string source, string destination, IDictionary<string, string> parameters )
    {
      string data = Read( source );
      foreach ( KeyValuePair< string, string > pair in parameters )
      {
        data = data.Replace( pair.Key, pair.Value );
      }
      Write( destination, data );
    }

    private static List<string> FindFiles( string workingDirectory, string projectname )
    {
      List<string > files = new List< string >();
      string[] filenames = Directory.GetFiles( workingDirectory );
      foreach ( string filename in filenames )
      {
        string name = filename.Substring( workingDirectory.Length + 1 );
        if ( IsSourceFile( name, projectname ) == false )
        {
          files.Add( name );
        }
        else
        {
          File.Delete( filename );
        }
      }
      return files;
    }

    private static bool IsSourceFile( string name, string projectname )
    {
      if ( ( Path.GetExtension( name ) == ".html" ) && ( name.StartsWith( projectname ) ) )
      {
        return true;  
      }
      return false;
    }

    private static string FileParameters( List<string> files )
    {
      List<string>  htmlFiles = GetHtmlFiles( files );
      string manifestTag =  BuildManifestTag( files, htmlFiles );
      string spineTag = BuildSpineTag( htmlFiles );
      return manifestTag + "\n" + spineTag;
    }

    private static string BuildManifestTag( IEnumerable< string > files, ICollection< string > htmlFiles )
    {
      XElement root = new XElement( "manifest" );
      foreach ( string filename in files )
      {
        XElement child = BuildItem( filename );
        root.Add( child );
      }
      return root.ToString();
    }


    private static List< string > GetHtmlFiles( List< string > files )
    {
      List<string> htmlFiles = new List< string >();
      foreach ( string filename in files )
      {
        if ( filename.ToLower().EndsWith( ".html" ) )
        {
          htmlFiles.Add( filename );
        }
      }
      OrderHtmlFiles( htmlFiles );
      return htmlFiles;
    }

    private static void OrderHtmlFiles( List< string > htmlFiles )
    {
      htmlFiles.Sort();
      htmlFiles.Remove( "titlepage.html" );
      htmlFiles.Insert( 0, "titlepage.html" );
    }

    private static XElement BuildItem( string filename )
    {
      string extention = Path.GetExtension( filename );
      XElement item = new XElement( "item" );
        item.Add( new XAttribute( "href", filename ) );
        item.Add( new XAttribute( "id", GetId( filename, extention ) ) );
        item.Add( new XAttribute( "media-type", GetMediaType( extention ) ) );
      return item;
    }

    private static string GetId( string filename, string extention )
    {
      if ( extention.ToLower() == ".html" )
      {
        return Path.GetFileNameWithoutExtension( filename );
      }
      else if ( extention.ToLower() == ".ncx" )
      {
        return "ncx";
      }
      else
      {
        return filename.Replace( '.', '-' );
      }
    }

    private static string GetMediaType( string extention )
    {
      if ( _MediaTypes.ContainsKey( extention ) )
      {
        return _MediaTypes[ extention ];
      }
      else
      {
        return MediaTypeNames.Text.Plain;
      }
    }

    private static string BuildSpineTag( IEnumerable< string > files )
    {
      XElement root = new XElement( "spine" );
      root.Add( new XAttribute( "toc", "ncx" ) );
      foreach ( string filename in files )
      {
        XElement child = BuildItemRef( filename );
        root.Add( child );
      }
      return root.ToString();
    }

    private static XElement BuildItemRef( string filename )
    {
      string extention = Path.GetExtension( filename );
      XElement itemRef = new XElement( "itemref" );
      //itemRef.Add( new XAttribute( "href", filename ) );
      itemRef.Add( new XAttribute( "idref", GetId( filename, extention ) ) );
      //itemRef.Add( new XAttribute( "linear", "yes" ) );
      return itemRef;
    }
  }
}