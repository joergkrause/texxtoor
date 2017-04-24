using System;
using System.Xml.Linq;

namespace Texxtoor.BaseLibrary.Pdf2Epub
{
  public class NcxCreator
  {
    private readonly XNamespace _Namespace = "http://www.daisy.org/z3986/2005/ncx/";
    private XDocument _Document;
    private XElement _NavRoot;
    private int _NavCounter = 1;

    public NcxCreator( string title, string author )
    {
      XElement root = new XElement( _Namespace + "ncx" );
      root.Add(new XAttribute( "version", "2005-1" ));

      _Document = new XDocument( root );
      AddHead( _Document );
      AddTitle( _Document, title );
      AddAuthor( _Document, author );
      _NavRoot = CreateNavRoot();
      _Document.Root.Add( _NavRoot );
    }

    private void AddHead( XDocument document )
    {
      XElement head = new XElement( _Namespace + "head" );
      head.Add( CreateMetaTag( "dtb:uid", Guid.NewGuid().ToString("D") ));
      head.Add( CreateMetaTag( "epub-creator", "EpubToPdf (Version 0.0.1)" ) );
      head.Add( CreateMetaTag( "dtb:depth", "1" ) );
      head.Add( CreateMetaTag( "dtb:totalPageCount", "0" ) );
      head.Add( CreateMetaTag( "dtb:maxPageNumber", "0" ) );
      document.Root.Add( head );
    }

    private XElement CreateMetaTag( string name, string content )
    {
      return new XElement( _Namespace + "meta", 
                              new XAttribute( "name", name ) ,
                              new XAttribute( "content", content ));
    }

    private void AddTitle( XDocument document, string title )
    {
      XElement element = new XElement( _Namespace + "docTitle", 
                                    new XElement( _Namespace+"text", title ));
      document.Root.Add( element );
    }

    private void AddAuthor( XDocument document, string author )
    {
      XElement element = new XElement( _Namespace + "docAuthor",
                                    new XElement( _Namespace + "text", author ) );
      document.Root.Add( element );
    }

    private XElement CreateNavRoot()
    {
      return new XElement( _Namespace + "navMap" );

    }

    public void AddNavPoint( string title, string href )
    {
      XElement element = new XElement( _Namespace + "navPoint",
                                    new XAttribute( "id", string.Format( "NavPoint-{0}", _NavCounter ) ),
                                    new XAttribute( "playOrder", _NavCounter.ToString()),
                                    new XElement( _Namespace + "navLabel",
                                        new XElement( _Namespace + "text", title ) ),
                                    new XElement( _Namespace + "content",
                                        new XAttribute( "src", href )));
      _NavRoot.Add( element );
      _NavCounter += 1;
    }

    public void SaveDocument( string filename )
    {
      _Document.Save( filename );
    }
  }
}