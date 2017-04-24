using System;
using System.Resources;

namespace Texxtoor.BaseLibrary.Core {

  /// <summary>
  /// Editor templates can use this to add data for jquery based watermark in input fields
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public class WatermarkAttribute : MetadataAttribute {

    private readonly string _privateText;

    public WatermarkAttribute(string text) {
      _privateText = text;
    }

    public WatermarkAttribute(Type resourceType, string resourceKey) {
      ResourceType = resourceType;
      ResourceKey = resourceKey;
    }

    public Type ResourceType { get; set; }

    public string ResourceKey { get; set; }

    public string GetValue() {
      if (String.IsNullOrEmpty(ResourceKey) || ResourceType == null) {
        return _privateText;
      }
      var property = ResourceType.GetProperty(ResourceKey);
      if (property != null) {
        return property.GetValue(null, null) as string;
      }
      return ResourceKey;
    }

    public override void Process(System.Web.Mvc.ModelMetadata modelMetaData) {
      modelMetaData.Watermark = GetValue();
    }
  }
}
