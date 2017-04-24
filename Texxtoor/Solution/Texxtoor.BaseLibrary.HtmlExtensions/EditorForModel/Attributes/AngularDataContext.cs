using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Texxtoor.Portal.Core.Extensions.Attributes {

  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
  public class NgDataContext : NgAttribute {

    private static string JSCORE = @"(function () {
      'use strict';

      var serviceId = 'datacontext';
      angular.module('texxtoor').factory(serviceId, ['common', '$http', datacontext]);

      function datacontext(common, $http) {
        var $q = common.$q;

        var service = {
          [[ReturnFuncs]]
        };

        return service;

        [[FuncContainer]]   

      }
    })();";

    private static string JSCALL = @"function [[FuncName]]([[FuncParam]]) {
         var promise = $http.[[VERB]]('/[[ApiPath]]/[[ServerFuncName]]'[[CallParam]]);
      return promise.then(function(data) { return data.data; });
    }";

    public NgDataContext(){
      
    }

    public NgDataContext(Type apiType, string apiPath){
      ApiPath = apiPath;
      ApiType = apiType;
    }

    public string ApiPath { get; set; }

    public Type ApiType { get; set; }

    public string GetDataContext(){
      // collect functions
      var exportFunctions = ApiType.GetMethods()
                                   .Where(m => m.GetCustomAttributes(true).OfType<NgDataContextFunction>().Any());
      var core = JSCORE;
      var returnFunctions = new List<string>();
      foreach (var exportFunction in exportFunctions){
        var exportAttribute = exportFunction.GetCustomAttributes(typeof (NgDataContextFunction), true).OfType<NgDataContextFunction>().Single();
        var postAttribute = exportFunction.GetCustomAttributes(typeof(HttpPostAttribute), true).OfType<HttpPostAttribute>().SingleOrDefault();
        var export = JSCALL;
        // use get or post
        export = export.Replace("[[VERB]]", postAttribute != null ? "post" : "get");
        var exportName = exportAttribute.ExportName ?? exportFunction.Name;
        export = export.Replace("[[FuncName]]", ApplyCaseNotion(exportName, CaseNotion.CamelCase));
        var parameters = String.Join(", ", exportFunction.GetParameters().Select(p => p.Name).ToArray());
        export = export.Replace("[[FuncParam]]", parameters);
        if (!String.IsNullOrEmpty(parameters)) {
          var forwardParameters = String.Join(", ", exportFunction.GetParameters().Select(p => String.Format("{0}:{0}", p.Name)).ToArray());
          export = export.Replace("[[CallParam]]", String.Format(", {{ {0} }}", forwardParameters));
        }
        else {
          export = export.Replace("[[CallParam]]", "");
        }
        export = export.Replace("[[ApiPath]]", ApiPath);
        export = export.Replace("[[ServerFuncName]]", exportName);
        core = core.Replace("[[FuncContainer]]",
                            Environment.NewLine + export + "[[FuncContainer]]"
          );
        returnFunctions.Add(
          String.Format("{0}:{1}",
          ApplyCaseNotion(exportName, CaseNotion.CamelCase), ApplyCaseNotion(exportName, CaseNotion.CamelCase)
          )); 
      }
      // create return object
      core = core.Replace("[[ReturnFuncs]]", 
        String.Join(", \n", returnFunctions.ToArray()));

      return core;
    } 

  }
}