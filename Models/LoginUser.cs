using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment_03.Models
{
    public class LoginUser
    {
        [Required(ErrorMessage = "Fill this field")]
        [StringLength(50)]
        public string Username { get; set; }
        [Required(ErrorMessage = "Fill this field")]
        [StringLength(50)]
        public string Password { get; set; }
    }
}
