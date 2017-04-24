(function () {
  'use strict';

  eap.controller('createNewTextCtrl', ['$scope', '$modalInstance',
      function ($scope, $modalInstance) {
        var self = this;

        self.init = function () {
        };

        self.addEventHandlers = function () {
          $scope.close = function () {
            $modalInstance.close();
          };
          $scope.edit = function () {
            $modalInstance.close();
          };
          $scope.word = function () {
            $modalInstance.close();
          };
        };

        self.init();
        self.addEventHandlers();
      }
  ]);
})();