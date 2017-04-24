(function () {
  'use strict';

  eap.controller('editMetadataCtrl', ['$scope', '$modalInstance',
      function ($scope, $modalInstance) {
        var self = this;

        self.init = function () {
        };

        self.addEventHandlers = function () {
          $scope.close = function (e) {
            $modalInstance.close();
            e.preventDefault();
          };
          $scope.save = function (e) {
            $modalInstance.close();
            e.preventDefault();
          };
        };

        self.init();
        self.addEventHandlers();
      }
  ]);
})();