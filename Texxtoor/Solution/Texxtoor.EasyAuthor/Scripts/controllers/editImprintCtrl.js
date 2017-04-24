(function () {
  'use strict';

  eap.controller('editImprintCtrl', ['$scope', 'overviewSvc', '$modalInstance',
      function ($scope, overviewSvc, $modalInstance) {
        var self = this;

        self.init = function () {

          $scope.imprint = {};

          overviewSvc.getImprint(function (data) {
            $scope.imprint = data;
          });
        };

        self.addEventHandlers = function () {
          $scope.close = function () {
            $modalInstance.close();
          };
          $scope.save = function () {
            $modalInstance.close();
          };
        };

        self.init();
        self.addEventHandlers();
      }
  ]);
})();