(function() {
    'use strict';

    eap.controller('publishCtrl', ['$scope', 'publishSvc', '$modal', '$location', 
        function ($scope, publishSvc, $modal, $location) {
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