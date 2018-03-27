using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreewoodBatch.Helper;

namespace ThreewoodBatch.Models
{

    public class Response
    {
        public List<string> SuccessAdded { get; set; }
        public List<string> FailAdded { get; set; }
        public List<string> SuccessUpdated { get; set; }
        public List<string> FailUpdated { get; set; }
        public List<string> SuccessDeleted { get; set; }
        public List<string> FailDeleted { get; set; }

        public int SuccessAddedCount { get { return SuccessAdded.Count; } }
        public int SuccessUpdatedCount { get { return SuccessUpdated.Count; } }
        public int SuccessDeletedCount { get { return SuccessDeleted.Count; } }
        public int FailAddCount { get { return FailAdded.Count; } }
        public int FailUpdateCount { get { return FailUpdated.Count; } }
        public int FailDeleteCount { get { return FailDeleted.Count; } }
        public string message { get; set; }

        public Response()
        {
            SuccessAdded = new List<string>();
            SuccessUpdated = new List<string>();
            SuccessDeleted = new List<string>();
            FailAdded = new List<string>();
            FailUpdated = new List<string>();
            FailDeleted = new List<string>();
        }
    }
}
