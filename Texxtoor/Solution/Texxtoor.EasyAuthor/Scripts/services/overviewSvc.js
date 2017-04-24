(function () {
  'use strict';

  angular.module('eap.services').factory('overviewSvc', ['$http', function ($http) {
    var url = {};
    url['gettext'] = '/api/texts/overview/:id/gettext';
    url['content'] = '/api/texts/overview/:id/content';
    url['semantics'] = '/api/texts/overview/:id/semantics';
    url['files'] = '/api/texts/overview/:id/files';
    url['file'] = '/api/texts/overview/:id/file';
    url['image'] = '/api/texts/overview/:id/image';
    url['metadata'] = '/api/texts/overview/:id/metadata';
    url['imprint'] = '/api/texts/overview/:id/imprint';
    url.getUrl = function (name, id) {
      return this[name].replace(':id', id);
    };

    return {
      getText: function (id, callback) {
        $http({ method: 'GET', url: url.getUrl('gettext', id), cache: false })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      },
      saveMetaData: function (id, metadata, callback) {
        $http({ method: 'PUT', url: url.getUrl('metadata', id), data: metadata, cache: false })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      },


      // Content
      getContent: function (id, callback) {
        $http({ method: 'GET', url: url.getUrl('content', id), cache: false })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      },
      addSection: function (id, parentId, title, callback) {
        $http({ method: 'POST', url: url.getUrl('content', id), data: { parentId: parentId, title: title } })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      },
      deleteSection: function (id, sectionId, callback) {
        $http({ method: 'DELETE', url: url.getUrl('content', id), data: { sectionId: sectionId } })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      },
      renameSection: function (id, sectionId, title, callback) {
        $http({ method: 'PUT', url: url.getUrl('content', id), data: { sectionId: sectionId, title: title } })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      },
      getImprint: function(id, callback) {
        $http({ method: 'GET', url: url.getUrl('imprint', id), cache: false })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      },
      saveImprint: function(imprint, callback) {
        $http({ method: 'PUT', url: url.getUrl('imprint', id), data: imprint })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      },

      // Semantik Data
      getSemantics: function (id, callback) {
        $http({ method: 'GET', url: url.getUrl('semantics', id), cache: false })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      },
      addTerm: function (term, callback) {
        $http({ method: 'POST', url: url.getUrl('semantics', id), data: term, cache: false })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      },
      delTerm: function (id, callback) {
        $http({ method: 'DELETE', url: url.getUrl('semantics', id), cache: false })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      },
      saveTerm: function (term, callback) {
        $http({ method: 'PUT', url: url.getUrl('semantics', id), data: term, cache: false })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      },

      // Ressources
      getFiles: function (id, callback) {
        $http({ method: 'GET', url: url.getUrl('files', id), cache: false })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      },
      addFile: function (id, callback) {
        // TODO: Implement upload
        $http({ method: 'POST', url: url.getUrl('files', id), cache: false })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      },
      delFile: function (id, callback) {
        $http({ method: 'DELETE', url: url.getUrl('files', id), cache: false })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      },
      getImage: function(id, callback) {
        $http({ method: 'GET', url: url.getUrl('image', id), cache: false })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      },
      saveFile: function(file, callback) {
        $http({ method: 'PUT', url: url.getUrl('file'), data: file })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      }

    };
  }
  ]);
})();