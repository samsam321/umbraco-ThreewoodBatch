'use strict';
(function () {
    // Create controller variable
    function ThreewoodBatchEditController($scope, $routeParams) {
        // Set a property on the scope equal to the current route id
        $scope.id = $routeParams.id;        
    };
    // Register the controller    
    angular.module("umbraco").controller("Threewood.Batch.EditController", ThreewoodBatchEditController);
})();