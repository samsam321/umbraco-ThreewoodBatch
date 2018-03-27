'use strict';
(function () {
    // Create controller variable
    function ThreewoodBatchPhonebookViewController($scope, $routeParams, $http, notificationsService, ThreewoodBatchfileUploadService) {

        $scope.content = { tabs: [{ id: 1, label: "Properties" }, { id: 2, label: "Manager" }] };
        $scope.configuration = {
            departmentNameAlias: "Human Resource",
            phonebookNameAlias: "Telephone Numbers",
            filename2beupload: ""
        };

        $scope.filenamelabel = false;

        $scope.fileSelected = function (files) {
            // In this case, files is just a single path/filename
            $scope.file = files;
            $scope.filenamelabel = true;
            $scope.configuration.filename2beupload = $scope.file.name;
        };               

        $scope.uploadFile = function () {            
            if ($scope.configuration.phonebookNameAlias != "" && $scope.configuration.departmentNameAlias != "") {                
                if (!$scope.isUploading) {                    
                    if ($scope.file) {
                        $scope.showLoader = true;
                        $scope.isUploading = true;
                        ThreewoodBatchfileUploadService.uploadFileToServer($scope.file, $scope.configuration.departmentNameAlias, $scope.configuration.phonebookNameAlias)
                            .then(
                                    function (response) {
                                        if (response) {
                                            $scope.showLoader = false;
                                            notificationsService.success("Success", "Saved to server with the filename " + response);
                                        }
                                        $scope.isUploading = false;
                                    }, function (reason) {
                                        $scope.showLoader = false;
                                        $scope.isUploading = false;
                                        notificationsService.error("Error", "File import failed: " + reason.data);                                        
                                    }
                                  );
                    } else {                        
                        $scope.isUploading = false;
                        notificationsService.error("Error", "You must select a file to upload");                        
                    }
                }
                else {                    
                    notificationsService.warning("Error", "Your File is uploading");
                }
            }
            else
            {
                
                notificationsService.error("Error", "Configuration is empty");
            }
        };
        
        $scope.file = false;
        $scope.isUploading = false;
        $scope.showLoader = false;

    };
    // Register the controller    
    angular.module("umbraco").controller("Threewood.Batch.Phonebook.ViewController", ThreewoodBatchPhonebookViewController);
})();