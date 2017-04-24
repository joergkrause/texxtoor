using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Texxtoor.Portal.Core.Extensions.Attributes {
  
  /// <summary>
  /// Creates a JavaScript model class out of a C# model class. We also add the model to a factory
  /// to provide the class as a service to angular. The class can provide optionally direct function
  /// calls to make elementary operations available apart from the datacontext.
  /// </summary>
  /// <remarks>
  /// <code>
  /// app.factory('Book', ['$http', function($http) {
  ///  function Book(bookData) {
  ///      if (bookData) {
  ///          this.setData(bookData):
  ///      }
  ///      // Some other initializations related to book
  ///  };
  ///  Book.prototype = {
  ///      setData: function(bookData) {
  ///          angular.extend(this, bookData);
  ///      },
  ///      load: function(id) {
  ///          var scope = this;
  ///          $http.get('ourserver/books/' + bookId).success(function(bookData) {
  ///              scope.setData(bookData);
  ///          });
  ///      },
  ///      delete: function() {
  ///          $http.delete('ourserver/books/' + bookId);
  ///      },
  ///      update: function() {
  ///          $http.put('ourserver/books/' + bookId, this);
  ///      },
  ///      getImageUrl: function(width, height) {
  ///          return 'our/image/service/' + this.book.id + '/width/height';
  ///      },
  ///      isAvailable: function() {
  ///          if (!this.book.stores || this.book.stores.length === 0) {
  ///              return false;
  ///          }
  ///          return this.book.stores.some(function(store) {
  ///              return store.quantity > 0;
  ///          });
  ///      }
  ///  };
  ///  return Book;
  //}]);
  /// </code>
  /// </remarks>
  public class NgModelAttribute : Attribute{

    private const string modelTemplate = @"
      app.factory('[[ClassName]]', ['$http', function($http) {
        function [[ClassName]](data) {
            if (data) {
                this.setData(data):
            }
            // Some other initializations related to object
            // [[Init]]
        };
        [[ClassName]].prototype = {
            setData: function(data) {
                angular.extend(this, data);
            },
            load: function(id) {
                var scope = this;
                $http.get('[[Url]]/[[Action]]/' + bookId).success(function(data) {
                    scope.setData(data);
                });
            },
            delete: function() {
                $http.delete('[[Url]]/[[Action]]/' + id);
            },
            update: function() {
                $http.put('[[Url]]/[[Action]]/' + id, this);
            },
        };
        return [[ClassName]];
      }]);";
    // 0 = Class, 1 = server action name

    public NgModelAttribute(){
      
    }

    public string BuildScript(string serviceUrl){
      return modelTemplate.Replace("[[ClassName]]", ClassName)
                          .Replace("[[Action]]", ServiceObject)
                          .Replace("[[Url]]", serviceUrl);
    }

    public string ClassName { get; set; }

    public string ServiceObject { get; set; }

    public string ServiceController { get; set; }

  }
}