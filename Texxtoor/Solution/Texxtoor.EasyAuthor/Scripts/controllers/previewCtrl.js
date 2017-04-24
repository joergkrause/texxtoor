(function() {
    'use strict';

    eap.controller('previewCtrl', ['$scope', 'previewSvc', '$modal', '$location',
        function ($scope, previewSvc, $modal, $location) {
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