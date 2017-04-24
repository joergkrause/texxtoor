(function() {
    'use strict';

    eap.controller('overviewCtrl', ['$scope', 'overviewSvc', '$modal', '$location', '$state',
        function ($scope, overviewSvc, $modal, $location, $state) {
          var self = this;

          self.init = function () {

            $scope.overview = {};
            $scope.overview.active = {};
            $scope.overview.active.tab = 'content';

            $scope.text = {};
            $scope.id = self.id;

            overviewSvc.getText(self.id, function (data) {
              $scope.text = data;
            });

            $('button.btn-help').popover();

            $scope.myInterval = 5000;
          };

          self.addEventHandlers = function() {
            $scope.editMetadata = function () {
              var modalInstance = $modal.open({
                templateUrl: "/dialogs/editMetadata",
                controller: "editMetadataCtrl",
                scope: $scope,
                width: "770px",
              });
              modalInstance.result.then(function () {
                // TODO: logic on closing dialog
              });
            }
            $scope.editImprint = function () {
              var modalInstance = $modal.open({
                templateUrl: "/dialogs/editImprint",
                controller: "editImprintCtrl",
                scope: $scope,
                width: "770px",
              });
              modalInstance.result.then(function () {
                // TODO: logic on closing dialog
              });
            }
            $scope.authorAssignRole = function (id, e) {
              e.preventDefault();
              alert(id);
              return false;
            }
            $scope.authorRemove = function (id, e) {
              alert(id);
              e.preventDefault();
              return false;
            }
            $scope.authorRate = function (id, e) {
              alert(id);
              e.preventDefault();
              return false;
            }
          };

          self.id = $scope.$stateParams.opusId;

          self.init();
          self.addEventHandlers();
        }
    ]);
})();