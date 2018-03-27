'use strict';

var umbracoResources = angular.module("umbraco.resources");
    
function ThreewoodBatchfileUploadService($http) {
    return {
        uploadFileToServer: function (file, departmentNameAlias, phonebookNameAlias) {

            var API_ROOT = 'ThreewoodBatch/Phonebook/'

            var request = {
                file: file,
                departmentNameAlias: departmentNameAlias,
                phonebookNameAlias: phonebookNameAlias
            };
            return $http({
                method: 'POST',
                url: API_ROOT + "UploadFileToServer",
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    var formData = new FormData();
                    formData.append("file", data.file);
                    formData.append("departmentNameAlias", data.departmentNameAlias);
                    formData.append("phonebookNameAlias", data.phonebookNameAlias);
                    return formData;
                },
                data: request
            }).then(function (response) {
                if (response) {
                    var fileName = response.data;
                    return fileName;
                } else {
                    return false;
                }
            });
        }
    }
};

// Register directive
umbracoResources.factory('ThreewoodBatchfileUploadService', ThreewoodBatchfileUploadService);