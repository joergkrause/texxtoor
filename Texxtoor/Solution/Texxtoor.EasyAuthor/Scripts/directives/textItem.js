(function () {

  'use strict';

  angular.module('eap.directives').directive('textItem', ['$rootScope', function ($rootScope) {

    return {
      restrict: 'E',
      scope: {
        teamMemberRole: '@',
        bookTitle: '@',
        bookDetails: '@',
        author: '@',
        messageCount: '@',
        coverUrl: '@',
        opusId: '@'
      },
      link: function (scope, element) {
        scope.state = $rootScope.state;
      },
      replace: true,
      transclude: true,
      templateUrl: '/directives/textitem'
    };
  }
  ]);

})();