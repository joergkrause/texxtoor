using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Web.UI;
using Texxtoor.BaseLibrary.Core.Extensions;
using Texxtoor.BaseLibrary.Globalization.Support;
using Texxtoor.DataModels.Context;
using Texxtoor.DataModels.Model.Cms.Localization;

namespace Texxtoor.BaseLibrary.Globalization.Provider {

  /// <summary>
  /// This class provides the Data Access to the database
  /// for the DbResourceManager, Provider and design time
  /// services. This class acts as a Business layer
  /// and uses the SqlConnection DAL for its data access.
  /// 
  /// Dependencies:
  /// DbResourceConfiguration   (holds and reads all config data from .Current)
  /// SqlConnection             (provides a data access (DAL))
  /// </summary>
  public static class DbResourceDataManager {
    static DbSimpleResourceProvider _provider;
    private static readonly PortalContext ctx = UnitOfWorkFactory.GetIUnitOfWorkContext<PortalContext>();

    /// <summary>
    /// Error message that can be checked after a method complets
    /// and returns a failure result.
    /// </summary>
    public static string ErrorMessage {
      get;
      set;
    }

    /// <summary>
    /// Default constructor. Instantiates with the default connection string
    /// which is loaded from the configuration section.
    /// </summary>
    public static void SetDbResourceDataManager(DbSimpleResourceProvider provider) {
      _provider = provider;
    }

    private static string LocaleFromCulture(CultureInfo culture) {
      var localeId = (Equals(culture.Parent, CultureInfo.InvariantCulture)) ? culture.TwoLetterISOLanguageName : culture.Parent.TwoLetterISOLanguageName;
      localeId = localeId == "iv" ? String.Empty : localeId;
      return localeId;
    }

    /// <summary>
    /// Returns a specific set of resources for a given culture and 'resource set' which
    /// in this case is just the virtual directory and culture.
    /// </summary>
    /// <param name="culture"></param>
    /// <param name="resourceSet"></param>
    /// <returns></returns>
    public static Dictionary<string, object> GetResourceSet(CultureInfo culture, string resourceSet) {
      var hashTable = new Dictionary<string, object>();
      var localeId = LocaleFromCulture(culture);
      var result = (from r in ctx.Localization
                    where r.ResourceSet == resourceSet && r.LocaleId == localeId
                    select r);
      if (!result.Any()) {
        return hashTable;
      }
      var r1 = result.OfType<ImageResource>().Iterate(r => r.BinData = LoadFileResource(r));
      var r2 = result.OfType<StringResource>().Iterate(r => r.Value = r.Value ?? String.Empty);
      var r3 = r1.Union<ResourceBase>(r2);
      // convert to non-generic because the resource reader requests IDictionaryEnumerator
      try {
        return r3.ToDictionary(k => k.ResourceId, k => (object)k.Value);
      } catch (Exception ex) {
        Trace.TraceError(ex.Message);
        return hashTable;
      }
    }

    /// <summary>
    /// Returns a fully normalized list of resources that contains the most specific
    /// locale version for the culture provided.
    ///                 
    /// This means that this routine walks the resource hierarchy and returns
    /// the most specific value in this order: de-ch, de, invariant.
    /// </summary>
    /// <param name="culture"></param>
    /// <param name="resourceSet"></param>
    /// <returns></returns>
    public static Dictionary<string, object> GetResourceSetNormalizedForLocaleId(string culture, string resourceSet) {
      if (culture == null)
        culture = "";
      var formatter = new LosFormatter();
      var result = from r in ctx.Localization
                   where r.ResourceSet == resourceSet && r.LocaleId == ""
                         || r.LocaleId == culture
                         || (culture.Contains("-") && r.LocaleId == culture.Split('-')[0])
                   orderby r.ResourceId, r.LocaleId descending
                   let isBinary = r.GetType().Name.Equals("ImageResource")
                   let deserialized =
                     isBinary
                       ? LoadFileResource(r as ImageResource)
                       : formatter.Deserialize((r as StringResource).Value)
                   select new {
                     ResourceId = r.ResourceId,
                     Value = isBinary ? deserialized : (r as StringResource).Value
                   };

      if (!result.Any()) {
        return new Dictionary<string, object>();
      }
      // TODO: We need to get the most specific culture at this point only
      return result.ToDictionary(k => k.ResourceId, k => k.Value);
    }


    /// <summary>
    /// Internal method used to parse the data in the database into a 'real' value.
    /// 
    /// Value field hold filename and type string
    /// TextFile,BinFile hold the actual file content
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static byte[] LoadFileResource(ImageResource value) {
      byte[] buffer = null;

      try {
        buffer = value.BinData;
      } catch (Exception ex) {
        ErrorMessage = value.ResourceId + ": " + ex.Message;
      }

      return buffer;
    }

    /// <summary>
    /// Returns a data table of all the resources for all locales. The result is in a 
    /// table called TResources that contains all fields of the table. The table is
    /// ordered by LocaleId.
    /// 
    /// Fields:
    /// ResourceId,Value,LocaleId,ResourceSet,Type
    /// </summary>
    /// <param name="localResources"></param>
    /// <returns></returns>
    public static IEnumerable<ResourceBase> GetAllResourcesForResourceSet(bool localResources) {
      var result = from r in ctx.Localization
                   where localResources ? r.ResourceSet.Contains(".") : !r.ResourceSet.Contains(".")
                   orderby r.ResourceSet, r.LocaleId
                   select r;

      if (!result.Any()) {
        ErrorMessage = "Error";
        return null;
      }

      return result;
    }


    /// <summary>
    /// Returns all available resource ids for a given resource set in all languages.
    /// 
    /// Returns a DataTable called TResoureIds. (ResourecId field)
    /// </summary>
    /// <param name="resourceSet"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetAllResourceIds(string resourceSet) {
      var result = from r in ctx.Localization
                   where r.ResourceSet == resourceSet
                   group r by r.ResourceId
                     into resourceId
                     select new {
                       ResourceId = resourceId
                     };

      return result.Cast<string>();
    }

    /// <summary>
    /// Returns all available resource sets
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<string> GetAllResourceSets(ResourceListingTypes Type) {

      IEnumerable<string> result = null;
      switch (Type) {
        case ResourceListingTypes.AllResources:
          result = from r in ctx.Localization
                   orderby r.ResourceSet
                   select r.ResourceSet.ToLower();
          break;
        case ResourceListingTypes.LocalResourcesOnly:
          result = from r in ctx.Localization
                   where r.ResourceSet.Contains(".")
                   group r by r.ResourceSet
                     into resourceSet
                     select resourceSet.Key.ToLower();
          break;
        case ResourceListingTypes.GlobalResourcesOnly:
          result = from r in ctx.Localization
                   where !r.ResourceSet.Contains(".")
                   group r by r.ResourceSet
                     into resourceSet
                     select resourceSet.Key.ToLower();
          break;
      }
      if (!result.Any())
        ErrorMessage = "Error";

      return result;
    }

    /// <summary>
    /// Gets all the locales for a specific resource set.
    /// 
    /// Returns a table named TLocaleIds (LocaleId field)
    /// </summary>
    /// <param name="resourceSet"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetAllLocaleIds(string resourceSet) {
      var result = from r in ctx.Localization
                   where r.ResourceSet == resourceSet
                   group r by r.LocaleId
                     into localeId
                     select new {
                       Language = localeId
                     };

      return result.Cast<string>();
    }

    /// <summary>
    /// Gets all the Resourecs and ResourceIds for a given resource set and Locale
    /// 
    /// returns a table "TResource" ResourceId, Value fields
    /// </summary>
    /// <param name="resourceSet"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public static IEnumerable GetAllResourcesForCulture(string resourceSet, CultureInfo culture) {
      var result = from r in ctx.Localization.OfType<StringResource>()
                   where r.ResourceSet == resourceSet && r.Culture == culture
                   select new {
                     ResourceId = r.ResourceId,
                     Value = r.Value
                   };

      return result.ToList();
    }


    /// <summary>
    /// Returns an individual Resource String from the database
    /// </summary>
    /// <param name="resourceId"></param>
    /// <param name="resourceSet"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public static string GetResourceString(string resourceId, string resourceSet, CultureInfo culture) {
      ErrorMessage = "";
      var localeId = LocaleFromCulture(culture);
      var result =
        ctx.Localization.OfType<StringResource>()
            .FirstOrDefault(r => r.ResourceId == resourceId && r.ResourceSet == resourceSet && r.LocaleId == localeId);
      return result.Value;
    }

    /// <summary>
    /// Returns all the resource strings for all cultures.
    /// </summary>
    /// <param name="resourceId"></param>
    /// <param name="resourceSet"></param>
    /// <returns></returns>
    public static Dictionary<string, string> GetResourceStrings(string resourceId, string resourceSet) {
      var result = from r in ctx.Localization.OfType<StringResource>()
                   where r.ResourceId == resourceId && r.ResourceSet == resourceSet
                   select r;
      if (!result.Any()) {
        return null;
      }
      return result.ToDictionary(k => k.LocaleId, k => k.Value);
    }

    /// <summary>
    /// Returns an object from the Resources. Use this for any non-string
    /// types. While this method can be used with strings GetREsourceString
    /// is much more efficient.
    /// </summary>
    /// <param name="resourceId"></param>
    /// <param name="resourceSet"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public static object GetResourceObject(string resourceId, string resourceSet, CultureInfo culture) {
      ErrorMessage = "";
      var localeId = LocaleFromCulture(culture);
      var result =
        ctx.Localization.FirstOrDefault(
          r => r.ResourceId == resourceId && r.ResourceSet == resourceSet && r.LocaleId == localeId);

      if (result == null) {
        return null;
      }

      if (result.GetType().Name.Equals("StringResources")) {
        return ((StringResource)result).Value;
      }
      return null;
    }

    /// <summary>
    /// Updates a resource if it exists, if it doesn't one is created
    /// </summary>
    /// <param name="resourceId"></param>
    /// <param name="value"></param>
    /// <param name="culture"></param>
    /// <param name="resourceSet"></param>
    public static int UpdateOrAdd(string resourceId, object value, CultureInfo culture, string resourceSet) {
      return UpdateOrAdd(resourceId, value, culture, resourceSet, false);
    }


    /// <summary>
    /// Updates a resource if it exists, if it doesn't one is created
    /// </summary>
    /// <param name="resourceId"></param>
    /// <param name="value"></param>
    /// <param name="culture"></param>
    /// <param name="resourceSet"></param>
    /// <param name="Type"></param>
    public static int UpdateOrAdd(string resourceId, object value, CultureInfo culture, string resourceSet, bool valueIsFileName) {

      int result;
      result = UpdateResource(resourceId, value, culture, resourceSet, valueIsFileName);

      // We either failed or we updated
      if (result != -1)
        return result;

      // We have no records matched in the Update - Add instead
      result = AddResource(resourceId, value, culture, resourceSet, valueIsFileName);

      if (result == -1)
        return -1;

      ResetCache();

      return 1;
    }

    /// <summary>
    /// Adds a resource to the Localization Table
    /// </summary>
    /// <param name="resourceId"></param>
    /// <param name="value"></param>
    /// <param name="culture"></param>
    /// <param name="resourceSet"></param>
    public static int AddResource(string resourceId, object value, CultureInfo culture, string resourceSet) {
      return AddResource(resourceId, value, culture, resourceSet, false);
    }

    /// <summary>
    /// Adds a resource to the Localization Table
    /// </summary>
    /// <param name="resourceId"></param>
    /// <param name="value"></param>
    /// <param name="resourceSet"></param>
    /// <param name="valueIsFileName">if true the Value property is a filename to import</param>
    public static int AddResource(string resourceId, object value, CultureInfo culture, string resourceSet, bool valueIsFileName) {
      string type = GetTypeForObject(value);

      byte[] binFile = null;
      var fileName = "";
      value = value ?? String.Empty;
      ResourceBase newResource = null;

      if (valueIsFileName) {
        newResource = new ImageResource {
          FileName = fileName,
          BinData = binFile
        };
      } else {
        newResource = new StringResource {
          ResourceId = resourceId,
          Value = value as string,
          Culture = culture,
          ResourceSet = resourceSet,
        };
      }
      ctx.Localization.Add(newResource);
      ctx.SaveChanges();
      ResetCache();
      return 1;
    }



    /// <summary>
    /// Updates an existing resource in the Localization table
    /// </summary>
    /// <param name="resourceId">The Resource id to update</param>
    /// <param name="value">The value to set it to</param>
    /// <param name="culture">The 2 (en) or 5 character (en-us)culture. Or "" for Invariant </param>
    /// <param name="resourceSet">The name of the resourceset.</param>        
    public static int UpdateResource(string resourceId, object value, CultureInfo culture, string resourceSet) {
      return UpdateResource(resourceId, value, culture, resourceSet, false);
    }

    /// <summary>
    /// Updates an existing resource in the Localization table
    /// </summary>
    /// <param name="resourceId"></param>
    /// <param name="value"></param>
    /// <param name="culture"></param>
    /// <param name="resourceSet"></param>
    public static int UpdateResource(string resourceId, object value, CultureInfo culture, string resourceSet, bool valueIsFileName) {
      value = value ?? String.Empty;
      var localeId = LocaleFromCulture(culture);
      var result =
        ctx.Localization.FirstOrDefault(
          r => (r.ResourceSet == resourceSet && r.ResourceId == resourceId && r.LocaleId == localeId));

      if (result != null) {
        if (result.GetType().Name.Equals("BinaryLocalization")) {
          string textFile = null;
          byte[] binFile = null;
          FileInfoFormat fi = FileObjectToBinary(value, out binFile, out textFile);
          ((ImageResource)result).FileName = fi.FileName;
          ((ImageResource)result).BinData = fi.BinContent;
        } else {
          ((StringResource)result).Value = value as string;
        }
      } else {
        ErrorMessage = "Error";
        return -1;
      }
      ResetCache();
      ctx.SaveChanges();
      return 1;
    }

    private static string GetTypeForObject(object value) {
      string typeName;
      if (value != null && !(value is string)) {
        typeName = value.GetType().AssemblyQualifiedName;
        try {
          var output = new LosFormatter();
          var writer = new StringWriter();
          output.Serialize(writer, value);
        } catch (Exception ex) {
          ErrorMessage = ex.Message;
          return null;
        }
      } else {
        typeName = String.Empty;
      }
      return typeName;
    }

    private static FileInfoFormat FileObjectToBinary(object value, out byte[] binFile, out string textFile) {
      FileInfoFormat fileData = null;
      binFile = null;
      textFile = null;
      try {
        fileData = GetFileInfo(value as string);
      } catch (Exception ex) {
        ErrorMessage = ex.Message;
        return null;
      }

      if (fileData.FileFormatType == FileFormatTypes.Text)
        textFile = fileData.TextContent;
      else
        binFile = fileData.BinContent;
      return fileData;
    }

    /// <summary>
    /// Internal routine that looks at a file and based on its
    /// extension determines how that file needs to be stored in the
    /// database. Returns FileInfoFormat structure
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private static FileInfoFormat GetFileInfo(string fileName) {
      var details = new FileInfoFormat();

      var fi = new FileInfo(fileName);
      if (!fi.Exists)
        throw new InvalidOperationException("Invalid Filename");

      var extension = fi.Extension.ToLower().TrimStart('.');
      details.FileName = fi.Name;

      switch (extension) {
        case "js":
        case "css":
        case "txt":
          details.FileFormatType = FileFormatTypes.Text;
          using (var sr = new StreamReader(fileName, Encoding.Default, true)) {
            details.TextContent = sr.ReadToEnd();
          }
          details.ValueString = details.FileName + ";" + typeof(string).AssemblyQualifiedName + ";" + Encoding.Default.HeaderName;
          break;
        case "png":
        case "bmp":
        case "jpg":
        case "gif":
          details.FileFormatType = FileFormatTypes.Image;
          details.BinContent = File.ReadAllBytes(fileName);
          details.ValueString = details.FileName + ";" + typeof(Bitmap).AssemblyQualifiedName;
          break;
        default:
          details.BinContent = File.ReadAllBytes(fileName);
          details.ValueString = details.FileName + ";" + typeof(System.Byte[]).AssemblyQualifiedName;
          break;
      }

      return details;
    }

    /// <summary>
    /// Deletes a specific resource ID based on ResourceId, ResourceSet and Culture.
    /// If an empty culture is passed the entire group is removed (ie. all locales).
    /// </summary>
    /// <param name="resourceId"></param>
    /// <param name="culture"></param>
    /// <param name="resourceSet"></param>
    /// <returns></returns>
    public static bool DeleteResource(string resourceId, string culture, string resourceSet) {
      var invariant = String.IsNullOrEmpty(culture);
      var result =
        ctx.Localization.Where(
          r => r.ResourceSet == resourceSet && r.ResourceId == resourceId && (invariant || r.LocaleId == culture));

      if (!result.Any()) {
        ErrorMessage = "Error";
        return false;
      }

      if (result.Any()) {
        result.ForEach(r => ctx.Localization.Remove(r));
        ctx.SaveChanges();
        return true;
      }

      return false;
    }

    /// <summary>
    /// Renames a given resource in a resource set. Note all languages will be renamed
    /// </summary>
    /// <param name="resourceId"></param>
    /// <param name="newResourceId"></param>
    /// <param name="resourceSet"></param>
    /// <returns></returns>
    public static bool RenameResource(string resourceId, string newResourceId, string resourceSet) {
      var result =
        ctx.Localization.Where(r => (r.ResourceId == resourceId && r.ResourceSet == resourceSet))
            .Iterate(r => r.ResourceId = newResourceId)
            .Select(r => r);
      if (!result.Any()) {
        ErrorMessage = "Invalid ResourceId";
        return false;
      }
      ctx.SaveChanges();
      return true;
    }

    /// <summary>
    /// Renames all property keys for a given property prefix. So this routine updates
    /// lblName.Text, lblName.ToolTip to lblName2.Text, lblName2.ToolTip if the property
    /// is changed from lblName to lblName2.
    /// </summary>
    /// <param name="property"></param>
    /// <param name="newProperty"></param>
    /// <param name="resourceSet"></param>
    /// <returns></returns>
    public static bool RenameResourceProperty(string property, string newProperty, string resourceSet) {
      var result =
        ctx.Localization.Where(r => r.ResourceSet == resourceSet && r.ResourceId.Contains(property))
            .Iterate(r => r.ResourceId = r.ResourceId.Replace(property, newProperty))
            .Select(r => r);
      ctx.SaveChanges();
      return true;
    }

    /// <summary>
    /// Deletes an entire resource set from the database. Careful with this function!
    /// </summary>
    /// <param name="resourceSet"></param>
    /// <returns></returns>
    public static bool DeleteResourceSet(string resourceSet) {
      return DeleteResourceSet(resourceSet, null);
    }

    /// <summary>
    /// Deletes an entire resource set from the database. Careful with this function!
    /// </summary>
    /// <param name="resourceSet"></param>
    /// <param name="culture">Culture to limit delition. Deletes all if null.</param>
    /// <returns></returns>
    public static bool DeleteResourceSet(string resourceSet, CultureInfo culture) {
      if (string.IsNullOrEmpty(resourceSet))
        throw new ArgumentNullException("resourceSet");
      var localeId = LocaleFromCulture(culture);
      var result =
        ctx.Localization.Where(r => r.ResourceSet == resourceSet)
            .Where(r => culture.IsNeutralCulture || r.LocaleId == localeId);

      if (result.Any()) {
        result.ForEach(r => ctx.Localization.Remove(r));
        ctx.SaveChanges();
        return true;
      }

      return false;
    }

    /// <summary>
    /// Renames a resource set. Useful if you need to move a local page resource set
    /// to a new page. ResourceSet naming for local resources is application relative page path:
    /// 
    /// test.aspx
    /// subdir/test.aspx
    /// 
    /// Global resources have a simple name
    /// </summary>
    /// <param name="oldResourceSet">Name of the existing resource set</param>
    /// <param name="newResourceSet">Name to set the resourceset name to</param>
    /// <returns></returns>
    public static bool RenameResourceSet(string oldResourceSet, string newResourceSet) {
      var result =
        ctx.Localization.Where(r => r.ResourceSet == oldResourceSet)
            .Iterate(r => r.ResourceSet = newResourceSet)
            .Select(r => r);

      ctx.SaveChanges();

      if (result.Any()) {
        ErrorMessage = "No matching Recordset found.";
        return false;
      }

      return true;
    }

    /// <summary>
    /// Checks to see if a resource exists in the resource store
    /// </summary>
    /// <param name="resourceId"></param>
    /// <param name="culture"></param>
    /// <param name="resourceSet"></param>
    /// <returns></returns>
    public static bool ResourceExists(string resourceId, CultureInfo culture, string resourceSet) {
      var result = from r in ctx.Localization
                   where r.ResourceId == resourceId
                         && r.Culture == culture
                         && r.ResourceSet == resourceSet
                   group r by r.ResourceId
                     into rr
                     select rr;

      return result.Any();
    }

    /// <summary>
    /// Persists resources to the database - first wipes out all resources, then writes them back in
    /// from the ResourceSet
    /// </summary>
    /// <param name="resourceList"></param>
    /// <param name="culture"></param>
    /// <param name="baseName"></param>
    public static bool GenerateResources(IDictionary resourceList, CultureInfo culture, string baseName, bool deleteAllResourceFirst) {
      if (resourceList == null)
        throw new InvalidOperationException("No Resources");

      using (var transaction = ctx.BeginTransaction()) {
        try {
          // First delete all resources for this resource set
          if (deleteAllResourceFirst) {
            if (!DeleteResourceSet(baseName, culture)) {
              return false;
            }
          }

          // Now add them all back in one by one
          foreach (DictionaryEntry entry in resourceList) {
            if (entry.Value == null) continue;
            var result = 0;
            result = deleteAllResourceFirst ? AddResource(entry.Key.ToString(), entry.Value, culture, baseName) : UpdateOrAdd(entry.Key.ToString(), entry.Value, culture, baseName);

            if (result == -1) {
              return false;
            }
          }
          transaction.Commit();
        } catch {
          return false;
        }
      }

      return true;
    }


    /// <summary>
    /// Creates an global JavaScript object object that holds all non-control 
    /// local string resources as property values.
    /// 
    /// All resources are returned in normalized fashion from most specifc
    /// to more generic (de-ch,de,invariant depending on availability)
    /// </summary>
    /// <param name="javaScriptVarName">Name of the JS object variable to createBackupTable</param>
    /// <param name="resourceSet">ResourceSet name. Pass NULL for locale Resources</param>
    /// <param name="localeId"></param>
    public static string GetResourcesAsJavascriptObject(string javaScriptVarName, string resourceSet, string localeId) {
      if (localeId == null)
        localeId = CultureInfo.CurrentUICulture.IetfLanguageTag;
      if (resourceSet == null)
        resourceSet = System.Web.VirtualPathUtility.ToAppRelative(".");

      IDictionary resources = GetResourceSetNormalizedForLocaleId(
          localeId, resourceSet);

      // Filter the list to non-control resources 
      var localRes = resources.Keys.Cast<string>().Where(key => !key.Contains(".") && resources[key] is string).ToDictionary(key => key, key => resources[key] as string);

      //JSONSerializer ser = new JSONSerializer();
      //ser.FormatJsonOutput = HttpContext.Current.IsDebuggingEnabled;
      string json = ""; //ser.Serialize(localRes);

      return "var " + javaScriptVarName + " = " + json + ";\r\n";
    }

    /// <summary>
    /// Creates an global JavaScript object object that holds all non-control 
    /// local string resources as property values and embeds this object
    /// directly into an ASP.NET page.
    /// </summary>
    public static void EmbedResourcesAsJavascriptObject(string javaScriptVarName, string resourceSet, Page page) {
      var script = GetResourcesAsJavascriptObject(javaScriptVarName, resourceSet, null);
      //ClientScriptProxy.Current.RegisterClientScriptBlock(page,typeof(Page),javaScriptVarName + "_res",script,true);
    }

    private static void ResetCache() {
      _provider.Resources.Clear();
    }
  }
}