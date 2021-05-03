using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace Assignment_03.Models
{
    public class User
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Fill this field")]
        [StringLength(50)]
        public string Name { get; set; }
        [Required(ErrorMessage = "Fill this field")]
        [StringLength(50)]
        public string Username { get; set; }
        [Required(ErrorMessage = "Fill this field")]
        [StringLength(50)]
        public string Email { get; set; }
        [Required(ErrorMessage = "Fill this field")]
        [StringLength(50)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Select a profile photo")]
        public IFormFile ProfileImage { get; set; }
        public string ProfileImageName { get; set; }
    }
}
