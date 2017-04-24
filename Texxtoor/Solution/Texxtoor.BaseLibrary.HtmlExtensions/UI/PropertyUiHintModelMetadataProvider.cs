using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core;
using Texxtoor.DataModels.DataAnnotations;

namespace Texxtoor.Portal.Core.UI {
  public class PropertyUiHintModelMetadataProvider : DataAnnotationsModelMetadataProvider {
    //private readonly ModelMetadataProvider _innerProvider;

    //public PropertyUiHintModelMetadataProvider(ModelMetadataProvider innerProvider) {
    //  _innerProvider = innerProvider;
    //}

    //public override IEnumerable<ModelMetadata> GetMetadataForProperties(object container, Type containerType) {
    //  foreach (var modelMetadata in _innerProvider.GetMetadataForProperties(container, containerType)) {
    //    var metadata = modelMetadata;
    //    Func<object> modelAccessor = () => metadata.Model;
    //    yield return new PropertyUiHintModelMetadata(this, modelAccessor, modelMetadata);
    //  }
    //}

    //public override ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, string propertyName) {
    //  var modelMetadata = _innerProvider.GetMetadataForProperty(modelAccessor, containerType, propertyName);
    //  return new PropertyUiHintModelMetadata(this, modelAccessor, modelMetadata);
    //}

    //public override ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType) {
    //  var modelMetadata = _innerProvider.GetMetadataForType(modelAccessor, modelType);
    //  return new PropertyUiHintModelMetadata(this, modelAccessor, modelMetadata);
    //}

    protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName) {
      var attributeList = new List<Attribute>(attributes);
      var modelMetadata = base.CreateMetadata(attributes, containerType, modelAccessor, modelType, propertyName);
      var result = new PropertyUiHintModelMetadata(this, modelAccessor, modelMetadata);
      //var result = new PropertyUiHintModelMetadata(this, containerType, modelAccessor, modelType, propertyName, displayColumnAttribute);

      // Do [HiddenInput] before [UIHint], so you can override the template hint
      var hiddenInputAttribute = attributeList.OfType<HiddenInputAttribute>().FirstOrDefault();
      if (hiddenInputAttribute != null) {
        result.TemplateHint = "HiddenInput";
        result.HideSurroundingHtml = !hiddenInputAttribute.DisplayValue;
      }

      // We prefer [UIHint("...", PresentationLayer = "MVC")] but will fall back to [UIHint("...")]
      var uiHintAttributes = attributeList.OfType<UIHintAttribute>();
      var uiHintAttribute = uiHintAttributes.FirstOrDefault(a => String.Equals(a.PresentationLayer, "MVC", StringComparison.OrdinalIgnoreCase))
                                      ?? uiHintAttributes.FirstOrDefault(a => String.IsNullOrEmpty(a.PresentationLayer));
      if (uiHintAttribute != null) {
        result.TemplateHint = uiHintAttribute.UIHint;
      }

      var dataTypeAttribute = attributeList.OfType<DataTypeAttribute>().FirstOrDefault();
      if (dataTypeAttribute != null) {
        result.DataTypeName = dataTypeAttribute.GetDataTypeName();
      }

      var readOnlyAttribute = attributeList.OfType<ReadOnlyAttribute>().FirstOrDefault();
      if (readOnlyAttribute != null) {
        result.IsReadOnly = readOnlyAttribute.IsReadOnly;
      }

      var displayFormatAttribute = attributeList.OfType<DisplayFormatAttribute>().FirstOrDefault();
      if (displayFormatAttribute == null && dataTypeAttribute != null) {
        displayFormatAttribute = dataTypeAttribute.DisplayFormat;
      }
      if (displayFormatAttribute != null) {
        result.NullDisplayText = displayFormatAttribute.NullDisplayText;
        result.DisplayFormatString = displayFormatAttribute.DataFormatString;
        result.ConvertEmptyStringToNull = displayFormatAttribute.ConvertEmptyStringToNull;

        if (displayFormatAttribute.ApplyFormatInEditMode) {
          result.EditFormatString = displayFormatAttribute.DataFormatString;
        }
      }

      var scaffoldColumnAttribute = attributeList.OfType<ScaffoldColumnAttribute>().FirstOrDefault();
      if (scaffoldColumnAttribute != null) {
        result.ShowForDisplay = result.ShowForEdit = scaffoldColumnAttribute.Scaffold;
      }

      var dynamicScaffoldAttribute = attributeList.OfType<DynamicScaffoldAttribute>().FirstOrDefault();
      if (dynamicScaffoldAttribute != null && HttpContext.Current != null && HttpContext.Current.Session["RunControl"] != null) {
        // TODO: Provide simplified value in views (instead of RunControl class)
        //var targetComplexity = (bool)HttpContext.Current.Session["RunControl"];
        //result.ShowForDisplay = result.ShowForEdit = dynamicScaffoldAttribute.ScaffoldFor(targetComplexity ? Complexity.Full : Complexity.Simple);
        result.ShowForDisplay = result.ShowForEdit = true;
      }

      var displayNameAttribute = attributeList.OfType<DisplayNameAttribute>().FirstOrDefault();
      if (displayNameAttribute != null) {
        result.DisplayName = displayNameAttribute.DisplayName;
      }

      var displayAttribute = attributeList.OfType<DisplayAttribute>().FirstOrDefault();
      if (displayAttribute != null) {
        result.DisplayName = displayAttribute.GetName();
        result.Description = displayAttribute.GetDescription();
        result.ShortDisplayName = displayAttribute.GetShortName();
        result.Order = displayAttribute.GetOrder().GetValueOrDefault();
      }

      var requiredAttribute = attributeList.OfType<RequiredAttribute>().FirstOrDefault();
      if (requiredAttribute != null) {
        result.IsRequired = true;
      }
      
      // Support for Watermark Attribute
      attributeList.OfType<MetadataAttribute>().ToList().ForEach(x => x.Process(result));

      return result;
    }
 
  }
}
