namespace Texxtoor.BaseLibrary.Mashup.Export{
  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  public partial class ProductContributor {

    private byte sequenceNumberField;

    private string contributorRoleField;

    private string personNameInvertedField;

    private string biographicalNoteField;

    /// <remarks/>
    public byte SequenceNumber {
      get {
        return this.sequenceNumberField;
      }
      set {
        this.sequenceNumberField = value;
      }
    }

    /// <remarks/>
    public string ContributorRole {
      get {
        return this.contributorRoleField;
      }
      set {
        this.contributorRoleField = value;
      }
    }

    /// <remarks/>
    public string PersonNameInverted {
      get {
        return this.personNameInvertedField;
      }
      set {
        this.personNameInvertedField = value;
      }
    }

    /// <remarks/>
    public string BiographicalNote {
      get {
        return this.biographicalNoteField;
      }
      set {
        this.biographicalNoteField = value;
      }
    }
  }
}