'use strict';

angular.module('eap.directives', []);
angular.module('eap.services', []);
angular.module('eap.filters', []);

var eap = angular.module('eap', ['ngRoute', 'ui.bootstrap', 'ui.router', 'eap.directives', 'eap.services', 'eap.filters']);

eap.config(['$stateProvider', '$routeProvider', '$urlRouterProvider', function ($stateProvider, $routeProvider, $urlRouterProvider) {

  //routeConfigurator($urlRouterProvider, getRoutes());
  $urlRouterProvider.when('', '/dashboard');
  $urlRouterProvider.otherwise('/dashboard');

  $stateProvider.state("dashboard",
      {
        url: "/dashboard",
        templateUrl: "/partials/dashboard"
      });

      $stateProvider.state("overview",
      {
        url: "/overview/:opusId",
        templateUrl: "/partials/overview"
      }).state("overview.content",
      {
        url: "/content",
        templateUrl: "/partials/overviewcontent"
      }).state("overview.semantics",
      {
        url: "/semantics",
        templateUrl: "/partials/overviewsemantics"
      }).state("overview.files",
      {
        url: "/files",
        templateUrl: "/partials/overviewfiles"
      });

  $stateProvider.state("preview",
      {
        url: "/preview/:opusId",
        templateUrl: "/partials/preview"
      });
  $stateProvider.state("publish",
      {
        url: "/publish/:opusId",
        templateUrl: "/partials/publish"
      });
  $stateProvider.state("publish.checkandpublish",
      {
        url: "/check",
        parent: "publish",
        templateUrl: "/partials/checkandpublish"
      });
  $stateProvider.state("marketing",
      {
        url: "/marketing/:opusId",
        templateUrl: "/partials/marketing"
      });
  $stateProvider.state("success",
      {
        url: "/success/:opusId",
        templateUrl: "/partials/success"
      });


}]);


eap.run(['$rootScope', '$state', '$stateParams', function ($rootScope, $state, $stateParams) {

  $rootScope.$state = $state;
  $rootScope.$stateParams = $stateParams;


}]);