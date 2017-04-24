using System.ComponentModel;

namespace Texxtoor.BaseLibrary.Pdf2Epub
{
  public class Pattern : INotifyPropertyChanged
  {
    private bool _Enable;
    public bool Enable
    {
      get { return _Enable; }
      set
      {
        _Enable = value;
        Notify( "Enable" );
      }
    }

    private int _LineCount;
    public int LineCount
    {
      get { return _LineCount; }
      set
      {
        _LineCount = value;
        Notify( "LineCount" );
      }
    }

    private string _RegEx;
    public string RegEx
    {
      get { return _RegEx; }
      set
      {
        _RegEx = value;
        Notify( "RegEx" );
      }
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;
    void Notify( string propName )
    {
      if ( PropertyChanged != null )
        PropertyChanged( this, new PropertyChangedEventArgs( propName ) );
    }
    #endregion
  }

  public class ConversionOptions : INotifyPropertyChanged
  {
    private string _SourceFilename;
    public string SourceFilename
    { 
      get { return _SourceFilename; }
      set
      {
        _SourceFilename = value;
        Notify( "SourceFilename" );
      }
    }

    private string _DestinationFilename;
    public string DestinationFilename
    {
      get { return _DestinationFilename; }
      set
      {
        _DestinationFilename = value;
        Notify( "DestinationFilename" );
      }
    }

    private string _HtmlFilename;
    public string HtmlFilename
    {
      get { return _HtmlFilename; }
      set
      {
        _HtmlFilename = value;
        Notify( "HtmlFilename" );
      }
    }

    private int _MaxSplitSizeInBytes;
    public int MaxSplitSizeInBytes
    {
      get { return _MaxSplitSizeInBytes; }
      set
      {
        _MaxSplitSizeInBytes = value;
        Notify( "MaxSplitSizeInBytes" );
      }
    }


    private Pattern _StripHeader;
    public Pattern StripHeader
    {
      get { return _StripHeader; }
      set
      {
        _StripHeader = value;
        Notify( "StripHeader" );
      }
    }

    private Pattern _StripFooter;
    public Pattern StripFooter
    {
      get { return _StripFooter; }
      set
      {
        _StripFooter = value;
        Notify( "StripFooter" );
      }
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;
    void Notify( string propName )
    {
      if ( PropertyChanged != null )
        PropertyChanged( this, new PropertyChangedEventArgs( propName ) );
    }
    #endregion

  }

}