(function() {
    'use strict';
    eap.controller('successCtrl', ['$scope', 'successSvc', '$modal', '$location',
        function ($scope, successSvc, $modal, $location) {
          var self = this;

          self.init = function () {
            $scope.text = {};
            $scope.content = {};
            $scope.semantic = {};
            $scope.files = {};
            $scope.metadata = {};
            $scope.id = self.id;

            $scope.myInterval = 5000;
          };

          self.id = $scope.$stateParams.opusId;

          self.init();

        }
    ]);
})();