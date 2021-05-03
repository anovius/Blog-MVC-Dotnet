using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Assignment_03.Models
{
    public class Post
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Fill this field")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Fill this field")]
        public string Content { get; set; }
        public string PostedBy { get; set; }
        public string PostingDate { get; set; }
        public string ProfileImageName { get; set; }
    }
}
