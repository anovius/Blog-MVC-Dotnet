using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Assignment_03.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Assignment_03.Controllers
{
    public class IndexController : Controller
    {
        private readonly IHostingEnvironment hostingEnvironment;
        public IndexController(IHostingEnvironment environment)
        {
            hostingEnvironment = environment; // For uploading file
        }
        [HttpGet]
        public IActionResult Index()
        {
            //Checking weather user already logged in or not and is he admin or not
            if (HttpContext.Request.Cookies.ContainsKey("Username"))
            {
                if(HttpContext.Request.Cookies["Username"].Equals("admin"))
                    return this.RedirectToAction("Index", "Admin");
                return this.RedirectToAction("Index", "Homepage");
            }
            return View();
        }
        [HttpPost]
        public IActionResult Index(LoginUser user)
        {
            if (ModelState.IsValid)
            {
                //check if user is an admin or not 
                if(user.Username.Equals("admin") && user.Password.Equals("admin"))
                {
                    HttpContext.Response.Cookies.Append("Username", user.Username);
                    return this.RedirectToAction("Index", "Admin");
                }
                //checking if user exists in database or not
                if (LoginUser(user))
                {
                    HttpContext.Response.Cookies.Append("Username", user.Username);// maintaining cookies for loggedin user
                    return this.RedirectToAction("Index", "Homepage");
                }
                ModelState.AddModelError("Password", "Invalid Username or Password");
                return View();
            }
            else
            {
                return View();
            }
        }
        [HttpGet]
        public IActionResult SignUp()
        {
            //Checking weather user already logged in or not and is he admin or not
            if (HttpContext.Request.Cookies.ContainsKey("Username"))
            {
                if (HttpContext.Request.Cookies["Username"].Equals("admin"))
                    return this.RedirectToAction("Index", "Admin");
                return this.RedirectToAction("Index", "Homepage");
            }
            return View();
        }
        [HttpPost]
        public IActionResult SignUp(User user)
        {
            if (ModelState.IsValid)
            {
                //checking profile photo format for correct file type
                if (CheckFileFormat(user.ProfileImage))
                {
                    if (CreateUser(user))
                    {
                        HttpContext.Response.Cookies.Append("Username", user.Username); // maintaining cookies for loggedin user
                        return this.RedirectToAction("Index","Homepage");
                    }
                    //if user not created means username already exists
                    ModelState.AddModelError("Username", "Username already exists");
                    return View();
                }
                else
                {
                    ModelState.AddModelError("ProfileImage", "Not an image file");
                    return View();
                }
            }
            return View();
        }
        //function taking a file and checking if file is image file or not
        bool CheckFileFormat(IFormFile postedFile)
        {
            if (!string.Equals(postedFile.ContentType, "image/jpg", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(postedFile.ContentType, "image/jpeg", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(postedFile.ContentType, "image/pjpeg", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(postedFile.ContentType, "image/gif", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(postedFile.ContentType, "image/x-png", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(postedFile.ContentType, "image/png", StringComparison.OrdinalIgnoreCase))
            return false;
            return true;
        }
        //function take a user and create a new user in database if no error came
        bool CreateUser( User user)
        {
            string Conn = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BlogSite;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection sqlConnection = new SqlConnection(Conn);
            SqlParameter p1 = new SqlParameter("name", user.Name);
            SqlParameter p2 = new SqlParameter("username", user.Username);
            SqlParameter p3 = new SqlParameter("email", user.Email);
            SqlParameter p4 = new SqlParameter("password", user.Password);
            string Que = $"SELECT * FROM Users WHERE Username = @username"; // checking username already exists or not
            SqlCommand sqlCommand = new SqlCommand(Que, sqlConnection);
            sqlCommand.Parameters.Add(p2);
            sqlConnection.Open();
            SqlDataReader ReadData = sqlCommand.ExecuteReader();
            if (ReadData.HasRows)
            {
                sqlConnection.Close();
                return false;
            }
            sqlConnection.Close();
            string[] file = user.ProfileImage.FileName.Split('.'); //To get extension of file
            string fileName = string.Format(@"{0}.{1}", Guid.NewGuid(), file[1]); //Guid used to generate unique filename
            string folderName = Path.Combine(hostingEnvironment.WebRootPath, "ProfileImages");
            string filePath = Path.Combine(folderName, fileName);
            user.ProfileImage.CopyTo(new FileStream(filePath, FileMode.Create));
            user.ProfileImageName = fileName;
            Que = $"INSERT INTO Users (Name, Username, Email, Password, ProfileImageName) VALUES (@name, @username, @email, @password, @path)";
            sqlCommand = new SqlCommand(Que, sqlConnection);
            SqlParameter p5 = new SqlParameter("username", user.Username);
            SqlParameter p6 = new SqlParameter("path", user.ProfileImageName);
            sqlCommand.Parameters.Add(p1);
            sqlCommand.Parameters.Add(p5);
            sqlCommand.Parameters.Add(p3);
            sqlCommand.Parameters.Add(p4);
            sqlCommand.Parameters.Add(p6);
            sqlConnection.Open();
            sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();
            return true;
        }
        // function takes a user and check weather user exists or not
        bool LoginUser(LoginUser user)
        {
            string Conn = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BlogSite;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection sqlConnection = new SqlConnection(Conn);
            string Que = $"SELECT * FROM Users WHERE Username = @user AND Password = @pass";
            SqlParameter p1 = new SqlParameter("user", user.Username);
            SqlParameter p2 = new SqlParameter("pass", user.Password);
            SqlCommand sqlCommand = new SqlCommand(Que, sqlConnection);
            sqlCommand.Parameters.Add(p1);
            sqlCommand.Parameters.Add(p2);
            sqlConnection.Open();
            SqlDataReader ReadData = sqlCommand.ExecuteReader();
            if (ReadData.HasRows)
            {
                sqlConnection.Close();
                return true;
            }
            sqlConnection.Close();
            return false;
        }
    }
}
