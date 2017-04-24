(function () {
  'use strict';

  angular.module('eap.services').factory('dashboardSvc', ['$http', function ($http) {
    var url = [];
    url['all'] = '/api/texts/all';
    url['published'] = '/api/texts/published';

    return {
      getAll: function (callback) {
        $http({ method: 'GET', url: url['all'], cache: false })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      },
      getPublished: function (callback) {
        $http({ method: 'GET', url: url['published'], cache: false })
            .success(function (data, status, headers, config) {
              callback(data);
            }).error(function (data, status, headers, config) {

            });
      }
    };
  }
  ]);
})();