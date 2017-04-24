(function () {

  'use strict';

  angular.module('eap.directives').directive('moreTextItem', ['$rootScope', function ($rootScope) {

    return {
      restrict: 'E',
      scope: {
        Title: '@',
        Details: '@',
      },
      link: function (scope, element) {
        scope.state = $rootScope.state;
      },
      replace: true,
      transclude: true,
      templateUrl: '/directives/moretextitem'
    };
  }
  ]);

})();