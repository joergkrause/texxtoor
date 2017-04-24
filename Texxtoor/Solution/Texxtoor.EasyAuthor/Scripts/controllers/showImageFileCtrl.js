(function() {
    'use strict';

    eap.controller('overviewCtrl', ['$scope', 'overviewSvc', '$modal', '$location', '$state',
        function ($scope, overviewSvc, $modal, $location, $state) {
          var self = this;

          self.init = function () {

            $scope.image = {};
            $scope.id = self.id;

            overviewSvc.getImage(self.id, function (data) {
              $scope.image = data;
            });

            $scope.myInterval = 5000;
          };

          self.addEventHandlers = function () {
            $scope.close = function () {
              $modalInstance.close();
            };
          };

          // provided from caller
          self.id = $scope.fileId;

          self.init();
          self.addEventHandlers();
        }
    ]);
})();