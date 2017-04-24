using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Texxtoor.DataModels.Models.Common;

namespace Texxtoor.Portal.Core.Configuration {
  public class TexxtoorConfiguration : ConfigurationSection {

    [ConfigurationProperty("runModes", IsRequired = true)]
    [ConfigurationCollection(typeof(TexxtoorRunModeElement), AddItemName="add", ClearItemsName="clear", RemoveItemName="remove")]
    public TexxtoorRunModeCollection RunModes {
      get { return (TexxtoorRunModeCollection)this["runModes"]; }
    }

  }

  public class TexxtoorRunModeCollection : ConfigurationElementCollection {
    public TexxtoorRunModeElement this[int index] {
      get { return (TexxtoorRunModeElement)BaseGet(index); }
      set {
        if (BaseGet(index) != null) {
          BaseRemoveAt(index);
        }
        BaseAdd(index, value);
      }
    }

    public void Add(TexxtoorRunModeElement serviceConfig) {
      BaseAdd(serviceConfig);
    }

    public void Clear() {
      BaseClear();
    }

    protected override ConfigurationElement CreateNewElement() {
      return new TexxtoorRunModeElement();
    }

    protected override object GetElementKey(ConfigurationElement element) {
      return ((TexxtoorRunModeElement)element).Url;
    }

    public void Remove(TexxtoorRunModeElement serviceConfig) {
      BaseRemove(serviceConfig.Url);
    }

    public void RemoveAt(int index) {
      BaseRemoveAt(index);
    }

    public void Remove(string name) {
      BaseRemove(name);
    }
  }

  public class TexxtoorRunModeElement : ConfigurationElement {

    [ConfigurationProperty("url", IsRequired = true)]
    //[StringValidator(MinLength = 6, MaxLength = 1000, InvalidCharacters = " ")]
    public string Url {
      get { return (string) this["url"]; }
      set { this["url"] = value; }
    }

    [ConfigurationProperty("targetRunMode", IsRequired = true)]
    public RunMode TargetRunMode {
      get { return (RunMode) Enum.Parse(typeof(RunMode), this["targetRunMode"].ToString(), false); }
      set { this["targetRunMode"] = value.ToString(); }
    }

    [ConfigurationProperty("connectionStringName", IsRequired = true)]
    //[StringValidator(MinLength = 20, MaxLength = 1000)]
    public string ConnectionStringName {
      get { return (string)this["connectionStringName"]; }
      set { this["connectionStringName"] = value; }
    }


  }

}