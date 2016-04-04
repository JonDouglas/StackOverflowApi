using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StackOverflowApi.Models
{
    public class RootComment
    {
        [JsonProperty(PropertyName = "items")]
        public List<Comment> comments { get; set; }
        public bool has_more { get; set; }
        public int quota_max { get; set; }
        public int quota_remaining { get; set; }
    }
}
