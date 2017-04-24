using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Texxtoor.Portal.Core.UI {
  public class PropertyUiHintModelMetadata : DataAnnotationsModelMetadata {
    private readonly ModelMetadata _innerMetadata;

    public PropertyUiHintModelMetadata(PropertyUiHintModelMetadataProvider provider, Func<object> modelAccessor, ModelMetadata innerMetadata)
      : base(provider, innerMetadata.ContainerType, modelAccessor, innerMetadata.ModelType, innerMetadata.PropertyName, null) {
      _innerMetadata = innerMetadata;
    }

    public PropertyUiHintModelMetadata(PropertyUiHintModelMetadataProvider provider, Type containerType,
                                        Func<object> modelAccessor, Type modelType, string propertyName,
                                        DisplayColumnAttribute displayColumnAttribute)
        : base( provider, containerType, modelAccessor, modelType, propertyName, displayColumnAttribute) {
    }
 
    public IDictionary<string, object> TemplateControlParameters { get; internal set; }
 
    public T GetTemplateControlParameter<T>(string name) {
        T val;
        if (!TryGetTemplateControlParameter<T>(name, out val))
            throw new ApplicationException("Missing UIHint ControlParameter");
        return val;
    }
    public bool TryGetTemplateControlParameter<T>(string name, out T val) {
        val = default(T);
      try{
          object o = TemplateControlParameters[name];
          val = (T)o;
        }
        catch (Exception) {
            return false;
        }
        return true;
    }

    public override Dictionary<string, object> AdditionalValues { get { return _innerMetadata.AdditionalValues; } }
    public override bool ConvertEmptyStringToNull { get { return _innerMetadata.ConvertEmptyStringToNull; } set { _innerMetadata.ConvertEmptyStringToNull = value; } }
    public override string DataTypeName { get { return _innerMetadata.DataTypeName; } set { _innerMetadata.DataTypeName = value; } }
    public override string Description { get { return _innerMetadata.Description; } set { _innerMetadata.Description = value; } }
    public override string DisplayFormatString { get { return _innerMetadata.DisplayFormatString; } set { _innerMetadata.DisplayFormatString = value; } }
    public override string DisplayName { get { return _innerMetadata.DisplayName; } set { _innerMetadata.DisplayName = value; } }
    public override string EditFormatString { get { return _innerMetadata.EditFormatString; } set { _innerMetadata.EditFormatString = value; } }
    public override IEnumerable<ModelValidator> GetValidators(ControllerContext context) { return _innerMetadata.GetValidators(context); }
    public override bool HideSurroundingHtml { get { return _innerMetadata.HideSurroundingHtml; } set { _innerMetadata.HideSurroundingHtml = value; } }
    public override bool IsReadOnly { get { return _innerMetadata.IsReadOnly; } set { _innerMetadata.IsReadOnly = value; } }
    public override bool IsRequired { get { return _innerMetadata.IsRequired; } set { _innerMetadata.IsRequired = value; } }
    public override string NullDisplayText { get { return _innerMetadata.NullDisplayText; } set { _innerMetadata.NullDisplayText = value; } }
    public override string ShortDisplayName { get { return _innerMetadata.ShortDisplayName; } set { _innerMetadata.ShortDisplayName = value; } }
    public override bool ShowForDisplay { get { return _innerMetadata.ShowForDisplay; } set { _innerMetadata.ShowForDisplay = value; } }
    public override bool ShowForEdit { get { return _innerMetadata.ShowForEdit; } set { _innerMetadata.ShowForEdit = value; } }
    public override string SimpleDisplayText { get { return _innerMetadata.SimpleDisplayText; } set { _innerMetadata.SimpleDisplayText = value; } }
    public override string TemplateHint { get { return _innerMetadata.TemplateHint; } set { _innerMetadata.TemplateHint = value; } }
    public override string Watermark { get { return _innerMetadata.Watermark; } set { _innerMetadata.Watermark = value; } }
     
    public override bool IsComplexType {
      get{
        return string.IsNullOrEmpty(TemplateHint) && _innerMetadata.IsComplexType;
      }
    }
  }
}