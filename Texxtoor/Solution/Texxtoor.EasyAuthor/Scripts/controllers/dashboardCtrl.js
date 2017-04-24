(function () {
  'use strict';

  eap.controller('dashboardCtrl', ['$scope', 'dashboardSvc', '$modal', '$location', '$state',
      function ($scope, dashboardSvc, $modal, $location, $state) {
        var self = this;

        self.init = function () {
          $scope.texts = [];
          $scope.published = [];
          $scope.slides = [];

          dashboardSvc.getAll(function (data) {
            $scope.texts = data;
          });

          dashboardSvc.getPublished(function (data) {
            $scope.published = data;
          });

          $scope.myInterval = 5000;
        };

        self.addEventHandlers = function () {
          // new text dlg
          $scope.createNewText = function () {
            var modalInstance = $modal.open({
              templateUrl: "/dialogs/createNewText",
              controller: "createNewTextCtrl",
              scope: $scope,
              width: "770px",
            });
            modalInstance.result.then(function () {
              // TODO: logic on closing dialog
            });
          };
          // forward to all text view
          $scope.showAllTexts = function() {
            $location.path('/alltexts');
          };

          $scope.overview = function () {
            var id = this.text.id;
            $state.transitionTo('overview', { opusId: id });
          };
        };

        self.init();
        self.addEventHandlers();
      }
  ]);
})();