using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.Web.Mvc;
using Umbraco.Web;
using System.Web.Configuration;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using System.Web;
using System.IO;
using System.Net.Http.Headers;
using ThreewoodBatch.Helper;
using Umbraco.Core.Models;
using ThreewoodBatch.Models;

namespace ThreewoodBatch.Controllers
{
    [PluginController("ThreewoodBatch")]
    public class PhonebookController : UmbracoApiController
    {
        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage> UploadFileToServer()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            HttpResponseMessage response;

            string uploadFolder = HttpContext.Current.Server.MapPath("~/App_Data/FileUploads");
            Directory.CreateDirectory(uploadFolder);
            var provider = new PhonebookMultipartFormDataStreamProvider(uploadFolder);
            var result = await Request.Content.ReadAsMultipartAsync(provider);           
            var fileName = result.FileData.First().LocalFileName;

            UmbracoHelper umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            IPublishedContent homePage = umbracoHelper.TypedContentAtRoot().FirstOrDefault();
            PhonebookHelper phonebookHelper = new PhonebookHelper(ApplicationContext, homePage);

            string phonebookNameAlias = result.FormData["phonebookNameAlias"];
            if (string.IsNullOrEmpty(phonebookNameAlias))
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Phonebook Name Alias is not valid");
            }

            string departmentNameAlias = result.FormData["departmentNameAlias"];
            if (string.IsNullOrEmpty(departmentNameAlias))
            {                
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Department Name Alias is not valid"); ;
            }

            try
            {
                Response phonebookResponse = phonebookHelper.ImportPhonebook(fileName, departmentNameAlias, phonebookNameAlias);
                string msg = string.Format("Success Added ({0}), Failure Added ({1}), Success Updated ({2}), Failure Updated ({3}), Success Deleted ({4}), Failure Deleted ({5})",
                                           phonebookResponse.SuccessAddedCount.ToString(),
                                           phonebookResponse.FailAddCount.ToString(),
                                           phonebookResponse.SuccessUpdatedCount.ToString(),
                                           phonebookResponse.FailUpdateCount.ToString(),
                                           phonebookResponse.SuccessDeletedCount.ToString(),
                                           phonebookResponse.FailDeleteCount.ToString());
                response = Request.CreateResponse(HttpStatusCode.OK, msg);
                return response;
            }
            catch (PhonebookValidationException ex)
            {
                LogHelper.Error<Exception>(ex.ExceptionMessage, ex);
                return Request.CreateResponse(ex.StatusCode, ex.ExceptionMessage);
            }
            catch (Exception ex)
            {                
                LogHelper.Error<Exception>(ex.Message, ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
                        
            return response;
        }
    }

    public class PhonebookMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        const string FILENAME_PREFIX = "Phonebook";
        public PhonebookMultipartFormDataStreamProvider(string path) : base(path) { }

        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            try
            {
                string fileExt = Path.GetExtension(headers.ContentDisposition.FileName.Replace("\"", string.Empty));
                return string.Format("{0}_{1}{2}", FILENAME_PREFIX, DateTime.Now.ToString("yyyyMMddhhmmss"), fileExt);
            }
            catch (Exception ex)
            {
                LogHelper.Error<Exception>(ex.Message, ex);
                return null;
            }            
        }
    }
}
