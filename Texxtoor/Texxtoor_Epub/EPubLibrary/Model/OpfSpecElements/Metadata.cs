using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Xml.Linq;
using Texxtoor.Models.BaseEntities.Epub;

namespace Texxtoor.BaseLibrary.EPub.Model {

  /// <summary>
  /// The metadata element encapsulates Publication meta information.
  /// </summary>
  [Table("Metadata", Schema = "Epub")]
  public class MetaData : EntityBase {

    public static readonly XNamespace DcNamespace = XNamespace.Get("http://purl.org/dc/elements/1.1/");
    public static readonly XNamespace OpfNamespace = XNamespace.Get("http://www.idpf.org/2007/opf");

    internal static MetaData CreateMetaData(XElement metaDataElement) {
      var mf = new MetaData();
      // mandatory
      mf.Title = MetaData.CreateElement<TitleElement>(metaDataElement);
      mf.Identifier = MetaData.CreateElement<IdentifierElement>(metaDataElement);
      mf.Language = MetaData.CreateElement<LanguageElement>(metaDataElement);
      // optional
      mf.Creator = MetaData.CreateElement<CreatorElement>(metaDataElement);
      mf.Rights = MetaData.CreateElement<RightsElement>(metaDataElement);
      mf.Publisher = MetaData.CreateElement<PublisherElement>(metaDataElement);
      mf.Contributor = MetaData.CreateElement<ContributorElement>(metaDataElement);
      mf.Coverage = MetaData.CreateElement<CoverageElement>(metaDataElement);
      mf.Date = MetaData.CreateElement<DateElement>(metaDataElement);
      mf.Description = MetaData.CreateElement<DescriptionElement>(metaDataElement);
      mf.Format = MetaData.CreateElement<FormatElement>(metaDataElement);
      // Link = MetaData.CreateElement<MetaData.link>(e),
      // Meta = MetaData.CreateElement<MetaData.IdentifierElement>(e),
      mf.Relation = MetaData.CreateElement<RelationElement>(metaDataElement);
      mf.Source = MetaData.CreateElement<SourceElement>(metaDataElement);
      mf.Subject = MetaData.CreateElement<SubjectElement>(metaDataElement);
      mf.Type = MetaData.CreateElement<TypeElement>(metaDataElement);
      return mf;
    }

    internal static T CreateElement<T>(XElement metadataElement) where T : DcmesElement {
      T instance = Activator.CreateInstance<T>();
      var elementName = typeof(T).GetCustomAttributes(typeof(EPubElement), true).FirstOrDefault() as EPubElement;
      XElement element = metadataElement.Element(DcNamespace + elementName.Name);
      if (element == null)
        return instance; // element might not be there but we write an empty element to satisfy the EF
      instance.Text = element.Value;
      instance.Dir = element.Attribute("dir") == null
          ? ProgressionDirection.Default
          : (ProgressionDirection)Enum.Parse(typeof(ProgressionDirection), element.Attribute("dir").Value, true);
      instance.Lang = element.Attribute(element.GetNamespaceOfPrefix("xml") + "lang") == null
          ? String.Empty
          : element.Attribute(element.GetNamespaceOfPrefix("xml") + "lang").Value;
      return instance;
    }

    internal static XElement CreateXElement(MetaData metaData) {
      var xe = new XElement(OpfNamespace + "metadata");
      MetaData.CreateXElement<CreatorElement>(xe, metaData.Creator);
      MetaData.CreateXElement<LanguageElement>(xe, metaData.Language);
      MetaData.CreateXElement<RightsElement>(xe, metaData.Rights);
      MetaData.CreateXElement<PublisherElement>(xe, metaData.Publisher);
      MetaData.CreateXElement<IdentifierElement>(xe, metaData.Identifier);
      MetaData.CreateXElement<ContributorElement>(xe, metaData.Contributor);
      MetaData.CreateXElement<CoverageElement>(xe, metaData.Coverage);
      MetaData.CreateXElement<DateElement>(xe, metaData.Date);
      MetaData.CreateXElement<DescriptionElement>(xe, metaData.Description);
      MetaData.CreateXElement<FormatElement>(xe, metaData.Format);
      //MetaData.CreateXElement<MetaData.(metaData.Link),
      //MetaData.CreateXElement<MetaData(metaData.Meta),
      MetaData.CreateXElement<RelationElement>(xe, metaData.Relation);
      MetaData.CreateXElement<SourceElement>(xe, metaData.Source);
      MetaData.CreateXElement<SubjectElement>(xe, metaData.Subject);
      MetaData.CreateXElement<TypeElement>(xe, metaData.Type);
      return xe;
    }

    /// <summary>
    /// Supports the save infrastructure and creates the XElement based on current object's data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    private static void CreateXElement<T>(XElement xe, DcmesElement value) {
      if (value == null) return;
      var elementName = typeof(T).GetCustomAttributes(typeof(EPubElement), true).FirstOrDefault() as EPubElement;
      XElement se = new XElement(DcNamespace + elementName.Name,
          value.Text,
          new XAttribute("id", value.Identifier ?? String.Empty),
          new XAttribute("lang", value.Lang ?? String.Empty),
          new XAttribute("dir", value.Dir.ToString())
          );
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

    public IList<Meta> Meta { get; set; }
    /// <summary>
    /// The link element is used to associate resources with a Publication, such as metadata records.
    /// </summary>
    public IList<Link> Link { get; set; }

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

  //[Table("Epub.Dcmes.ContributorElement")]
  [EPubElement("contributor")]
  public class ContributorElement : DcmesElement {
  }
  //[Table("Epub.Dcmes.CoverageElement")]
  [EPubElement("coverage")]
  public class CoverageElement : DcmesElement {
  }
  //[Table("Epub.Dcmes.CreatorElement")]
  [EPubElement("creator")]
  public class CreatorElement : DcmesElement {
  }
  //[Table("Epub.Dcmes.DateElement")]
  [EPubElement("date")]
  public class DateElement : DcmesElement {
    [NotMapped]
    public DateTime Value {
      get { return DateTime.Parse(Text); }
      set { Text = value.ToLongDateString(); }
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
