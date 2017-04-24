var TEXXTOOR = (function (app) {
  
  app.controller('Index', ['$scope', '$http', function ($scope, $http) {
      $scope.angular = "Hallo Angular in Jade";
      $scope.data = {};
      
      (function getData() {
        $http.get('/users', { cache: false } ).
        success(function (data, status, headers, cfg) {
          $scope.data = data;
        }).
        error(function (data, status, headers, cfg) {
          $scope.error = status;
        });
      })();

    }]);
  
  return app;
	
})(TEXXTOOR || {});