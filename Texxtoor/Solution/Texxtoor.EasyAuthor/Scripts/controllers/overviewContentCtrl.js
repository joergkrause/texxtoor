(function() {
    'use strict';

    eap.controller('overviewContentCtrl', ['$scope', 'overviewSvc', '$modal', '$location',
        function ($scope, overviewSvc, $modal, $location) {
          var self = this;

          self.init = function () {

            $scope.sections = [];
            $scope.addsection = {};
            $scope.addSectionTitle = "";
            $scope.limit = 2;

            $scope.id = self.id;

            $('button.btn-help').popover();

            overviewSvc.getContent(self.id, function (data) {
              $scope.sections = data;
            });
 
            $scope.myInterval = 5000;
          };

          self.addEventHandlers = function () {
            $scope.addSection = function() {
              // add to end at current level
              overviewSvc.addSection(self.id, $scope.addsection.id, $scope.addSectionTitle, function (data) {

              });
            };
            $scope.insertSection = function () {
              // insert after current same level
              overviewSvc.addSection(self.id, $scope.addsection.id, $scope.addSectionTitle, function (data) {

              });
            };
            $scope.deleteSection = function () {
              // delete and reorganize
              overviewSvc.deleteSection(self.id, sectionId, function (data) {

              });
            };
            $scope.renameSection = function () {
              // rename title
              overviewSvc.renameSection(self.id, sectionId, title, function (data) {

              });
            };
          };

          self.id = $scope.$stateParams.opusId;

          self.init();
          self.addEventHandlers();

        }
    ]);
})();