using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ThreewoodBatch.Models
{    
    public class PhonebookException : Exception
    {
        public string ExceptionMessage { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public PhonebookException(string message, HttpStatusCode code)
        {
            ExceptionMessage = message;
            StatusCode = code;
        }
    }

    public class SecurityException : PhonebookException
    {
        public SecurityException(string message, HttpStatusCode code) : base(message, code)
        {
        }
    }

    public class PhonebookValidationException : PhonebookException
    {
        public PhonebookValidationException(string message, HttpStatusCode code) : base(message, code)
        {
            //Can pre-define your status code here or pass in as a parameter depending on how generic your exception is   
        }
    }
}





