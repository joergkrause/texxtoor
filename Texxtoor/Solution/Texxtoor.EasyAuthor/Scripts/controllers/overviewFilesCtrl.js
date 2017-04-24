(function() {
    'use strict';

    eap.controller('overviewFilesCtrl', ['$scope', 'overviewSvc', '$modal', '$location',
        function ($scope, overviewSvc, $modal, $location) {
          var self = this;

          self.init = function () {

            $scope.overview = {};
            $scope.overview.active = {};
            $scope.overview.active.tab = 'content';

            $scope.files = {};
            $scope.id = self.id;

            overviewSvc.getFiles(self.id, function (data) {
              $scope.files = data;
            });

            $('button.btn-help').popover();
 
            $scope.myInterval = 5000;
          };

          self.addEventHandlers = function() {
            $scope.saveFile = function (fileId) {
              var file = _.find($scope.files, function(f) {
                return f.id == fileId;
              });
              overviewSvc.saveFile(file);
            }
            $scope.copyFile = function (fileId) {

            }
            $scope.deleteFile = function (fileId) {

            }
            $scope.downloadFile = function (fileId) {

            }
            $scope.setVolume = function (fileId, volume) {

            }
            $scope.showImage = function (fileId) {
              $scope.fileId = fileId;
              var modalInstance = $modal.open({
                templateUrl: "/dialogs/showImageFile",
                controller: "showImageFileCtrl",
                scope: $scope,
                width: "770px",
              });
              modalInstance.result.then(function () {
                // TODO: logic on closing dialog
              });

            }
          };

          self.id = $scope.$stateParams.opusId;

          self.init();
          self.addEventHandlers();

        }
    ]);
})();