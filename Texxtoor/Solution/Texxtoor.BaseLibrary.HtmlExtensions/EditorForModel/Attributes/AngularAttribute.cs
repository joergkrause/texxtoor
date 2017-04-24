using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Texxtoor.Portal.Core.Extensions.Attributes {

  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
  public class AngularAttribute : Attribute {

    /// <summary>
    /// Manage generation if AngularJS attributes.
    /// </summary>
    /// <remarks>
    /// In the forms we have a viewmodel defined like ng-repeat="item in items" or ng-controller="projects as vm".
    /// Depending on form usage this property defines the string used to decorate the ng-model attribute, either
    /// "item" or "vm" in the examples.
    /// </remarks>
    public string ViewModelPrefix { get; set; }

    /// <summary>
    /// The name of the container class.
    /// </summary>
    /// <remarks>
    /// Can be left empty if the name is exactly (case-sensitive) to the model type. If the controller provides
    /// a view model in vm and in it an object of name "project" than set this name to "project". It overwrites
    /// the name of the model type.
    /// </remarks>
    public string ContainerName { get; set; }


    public AngularAttribute() {

    }

    public AngularAttribute(string viewModelPrefix, string containerName = null) {
      ViewModelPrefix = viewModelPrefix;
      ContainerName = containerName;
    }

  }
}