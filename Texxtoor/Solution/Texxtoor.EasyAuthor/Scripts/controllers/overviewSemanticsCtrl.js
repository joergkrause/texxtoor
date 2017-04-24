(function() {
    'use strict';

    eap.controller('overviewSemanticsCtrl', ['$scope', 'overviewSvc', '$modal', '$location',
        function ($scope, overviewSvc, $modal, $location) {
          var self = this;

          self.init = function () {

            $scope.termset = [];
            $scope.termtypes = [
              { value: "abbreviation", text: "Abbreviation" },
              { value: "cite", text: "Cite" },
              { value: "idiom", text: "Idiom" },
              { value: "variable", text: "Variable" },
              { value: "definition", text: "Definition" }
            ];
            $scope.type = "";
            $scope.name = "";
            $scope.data = "";
            $scope.inserted = 0;
            $scope.id = self.id;

            overviewSvc.getSemantics(self.id, function (data) {
              $scope.termset = data;
            });

            $scope.myInterval = 5000;

            $scope.addTerm = function() {
              $scope.inserted = {
                type: "abbreviation",
                name: "",
                data: ""
              };
              $scope.termset.push($scope.inserted);
            };

            $scope.saveTerm = function(data, termId) {
              angular.extend(data, { id: termId });
              overviewSvc.addTerm(data);
            };

            $scope.delTerm = function(index) {
              $scope.termset.splice(index, 1);
            };

          };

          self.id = $scope.$stateParams.opusId;

          self.init();

        }
    ]);
})();