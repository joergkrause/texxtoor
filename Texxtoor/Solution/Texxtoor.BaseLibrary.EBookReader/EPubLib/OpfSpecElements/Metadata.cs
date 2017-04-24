using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Xml.Linq;
using Texxtoor.BaseLibrary.Core.BaseEntities;

namespace Texxtoor.BaseLibrary.EPub.Model {

  /// <summary>
  /// The metadata element encapsulates Publication meta information.
  /// </summary>
  [Table("Metadata", Schema = "Epub")]
  public class MetaData : EntityBase {

    public static readonly XNamespace DcNamespace = XNamespace.Get("http://purl.org/dc/elements/1.1/");
    public static readonly XNamespace OpfNamespace = XNamespace.Get("http://www.idpf.org/2007/opf");

    internal static MetaData CreateMetaData(XElement metaDataElement) {
      var mf = new MetaData {
        Title = CreateElement<TitleElement>(metaDataElement),
        Identifier = CreateElement<IdentifierElement>(metaDataElement),
        Language = CreateElement<LanguageElement>(metaDataElement),
        Creator = CreateElement<CreatorElement>(metaDataElement),
        Rights = CreateElement<RightsElement>(metaDataElement),
        Publisher = CreateElement<PublisherElement>(metaDataElement),
        Contributor = CreateElement<ContributorElement>(metaDataElement),
        Coverage = CreateElement<CoverageElement>(metaDataElement),
        Date = CreateElement<DateElement>(metaDataElement),
        Description = CreateElement<DescriptionElement>(metaDataElement),
        Format = CreateElement<FormatElement>(metaDataElement),
        Relation = CreateElement<RelationElement>(metaDataElement),
        Source = CreateElement<SourceElement>(metaDataElement),
        Subject = CreateElement<SubjectElement>(metaDataElement),
        Type = CreateElement<TypeElement>(metaDataElement)
      };
      // mandatory
      // optional
      // Link = MetaData.CreateElement<MetaData.link>(e),
      // Meta = MetaData.CreateElement<MetaData.IdentifierElement>(e),
      return mf;
    }

    internal static T CreateElement<T>(XElement metadataElement) where T : DcmesElement {
      var instance = Activator.CreateInstance<T>();
      var elementName = typeof(T).GetCustomAttributes(typeof(EPubElement), true).FirstOrDefault() as EPubElement;
      if (elementName != null) {
        var element = metadataElement.Element(DcNamespace + elementName.Name);
        if (element == null)
          return instance; // element might not be there but we write an empty element to satisfy the EF
        instance.Text = element.Value;
        instance.Dir = element.Attribute("dir") == null
                         ? ProgressionDirection.Default
                         : (ProgressionDirection)Enum.Parse(typeof(ProgressionDirection), element.Attribute("dir").Value, true);
        instance.Lang = element.Attribute(element.GetNamespaceOfPrefix("xml") + "lang") == null
                          ? String.Empty
                          : element.Attribute(element.GetNamespaceOfPrefix("xml") + "lang").Value;
      }
      return instance;
    }

    internal static XElement CreateXElement(MetaData metaData) {
      var xe = new XElement(OpfNamespace + "metadata",
        new XAttribute(XNamespace.Xmlns + "dc", DcNamespace),
        new XAttribute(XNamespace.Xmlns + "opf", OpfNamespace)
        );
      CreateXElement<CreatorElement>(xe, metaData.Creator);
      CreateXElement<LanguageElement>(xe, metaData.Language);
      CreateXElement<RightsElement>(xe, metaData.Rights);
      CreateXElement<PublisherElement>(xe, metaData.Publisher);
      CreateXElement<IdentifierElement>(xe, metaData.Identifier);
      CreateXElement<TitleElement>(xe, metaData.Title);
      CreateXElement<ContributorElement>(xe, metaData.Contributor);
      CreateXElement<CoverageElement>(xe, metaData.Coverage);
      CreateXElement<DateElement>(xe, metaData.Date);
      CreateXElement<DescriptionElement>(xe, metaData.Description);
      CreateXElement<FormatElement>(xe, metaData.Format);
      //MetaData.CreateXElement<MetaData.(metaData.Link),
      //MetaData.CreateXElement<MetaData(metaData.Meta),
      CreateXElement<RelationElement>(xe, metaData.Relation);
      CreateXElement<SourceElement>(xe, metaData.Source);
      CreateXElement<SubjectElement>(xe, metaData.Subject);
      CreateXElement<TypeElement>(xe, metaData.Type);
      return xe;
    }

    /// <summary>
    /// Supports the save infrastructure and creates the XElement based on current object's data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="xe"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private static void CreateXElement<T>(XElement xe, DcmesElement value) {
      if (value == null) return;
      var elementName = typeof(T).GetCustomAttributes(typeof(EPubElement), true).OfType<EPubElement>().Single();
      if (elementName == null) return;
      var se = new XElement(DcNamespace + elementName.Name,
                            value.Text);
      if (value.Identifier != null) {
        se.Add(new XAttribute("id", value.Identifier));
      }
      if (value.Lang != null) {
        se.Add(new XAttribute("lang", value.Lang));
      }
      if (value.Dir != ProgressionDirection.Default) {
        //se.Add(new XAttribute("dir", value.Dir));
      }                           
      xe.Add(se);
    }

    # region Mandatory
    public virtual IdentifierElement Identifier { get; set; }
    public virtual TitleElement Title { get; set; }
    public virtual LanguageElement Language { get; set; }
    # endregion

    # region Optional
    public PublisherElement Publisher { get; set; }
    public CoverageElement Coverage { get; set; }
    public ContributorElement Contributor { get; set; }
    public CreatorElement Creator { get; set; }
    public DateElement Date { get; set; }
    public DescriptionElement Description { get; set; }
    public FormatElement Format { get; set; }
    public RelationElement Relation { get; set; }
    public RightsElement Rights { get; set; }
    public SourceElement Source { get; set; }
    public SubjectElement Subject { get; set; }
    public TypeElement Type { get; set; }
    # endregion

    public virtual IList<Meta> Meta { get; set; }
    /// <summary>
    /// The link element is used to associate resources with a Publication, such as metadata records.
    /// </summary>
    public virtual IList<Link> Link { get; set; }

  }

  [ComplexType]
  public abstract class DcmesElement {
    [StringLength(32)]
    public string Identifier { get; set; }
    [StringLength(10)]
    public string Lang { get; set; }
    public ProgressionDirection Dir { get; set; }
    [StringLength(512)]
    public string Text { get; set; }

    public override string ToString() {
      return Text ?? String.Empty;
    }
  }

  [EPubElement("title")]
  public class TitleElement : DcmesElement {
  }

  [EPubElement("identifier")]
  public class IdentifierElement : DcmesElement {
  }

  [EPubElement("language")]
  public class LanguageElement : DcmesElement {
  }

  [EPubElement("contributor")]
  public class ContributorElement : DcmesElement {
  }
  [EPubElement("coverage")]
  public class CoverageElement : DcmesElement {
  }
  [EPubElement("creator")]
  public class CreatorElement : DcmesElement {
  }
  [EPubElement("date")]
  public class DateElement : DcmesElement {
    [NotMapped]
    public DateTime Value {
      get { return DateTime.Parse(Text); }
      set {        
        Text = String.Format("{0}-{1}-{2}", value.Year, value.Month, value.Day);
      }
    }
  }
  //[Table("Epub.Dcmes.DescriptionElement")]
  [EPubElement("description")]
  public class DescriptionElement : DcmesElement {
  }
  //[Table("Epub.Dcmes.FormatElement")]
  [EPubElement("format")]
  public class FormatElement : DcmesElement {
  }
  //[Table("Epub.Dcmes.RelationElement")]
  [EPubElement("relation")]
  public class RelationElement : DcmesElement {
  }
  //[Table("Epub.Dcmes.PublisherElement")]
  [EPubElement("publisher")]
  public class PublisherElement : DcmesElement {
  }
  //[Table("Epub.Dcmes.RightsElement")]
  [EPubElement("rights")]
  public class RightsElement : DcmesElement {
  }
  //[Table("Epub.Dcmes.SourceElement")]
  [EPubElement("source")]
  public class SourceElement : DcmesElement {
  }
  //[Table("Epub.Dcmes.SubjectElement")]
  [EPubElement("subject")]
  public class SubjectElement : DcmesElement {
  }
  //[Table("Epub.Dcmes.TypeElement")]
  [EPubElement("type")]
  public class TypeElement : DcmesElement {
  }

}
