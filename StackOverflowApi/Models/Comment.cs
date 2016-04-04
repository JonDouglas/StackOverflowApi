using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackOverflowApi.Models
{
    public class Comment
    {
        public Owner owner { get; set; }
        public bool edited { get; set; }
        public int score { get; set; }
        public int creation_date { get; set; }
        public int post_id { get; set; }
        public int comment_id { get; set; }
        public ReplyToUser reply_to_user { get; set; }
    }
}
