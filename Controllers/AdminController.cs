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
    public class AdminController : Controller
    {
        private readonly IHostingEnvironment hostingEnvironment; // for uploading files
        public AdminController(IHostingEnvironment environment)
        {
            hostingEnvironment = environment;
        }
        public IActionResult Index()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("Username") || HttpContext.Request.Cookies["Username"]!= "admin") // checking if admin logged in or not
                return this.RedirectToAction("Index", "Index");
            return View("Index", ReadAllUsers()); // retun a page containg all users
        }
        [HttpGet]
        public IActionResult CreateUser()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("Username") || HttpContext.Request.Cookies["Username"] != "admin") // checking if admin logged in or not
                return this.RedirectToAction("Index", "Index");
            return View();
        }
        [HttpPost]
        public IActionResult CreateUser(User user) // function to create new user by admin
        {
            if (ModelState.IsValid)
            {
                if (CheckFileFormat(user.ProfileImage))
                {
                    if (CreateNewUser(user))
                    {
                        return this.RedirectToAction("Index", "Admin");
                    }
                    ModelState.AddModelError("Username", "Username already exists"); // if user already exists no new account will be created
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
        public IActionResult DeleteUser(int id) //function to delete user by admin
        {
            if (!HttpContext.Request.Cookies.ContainsKey("Username") || HttpContext.Request.Cookies["Username"] != "admin") // checking if admin logged in or not
                return this.RedirectToAction("Index", "Index");
            User user = ReadAllUsers().Find(u => u.ID == id);
            string Conn = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BlogSite;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection sqlConnection = new SqlConnection(Conn);
            SqlParameter p1 = new SqlParameter("ID", id);
            string Que = $"DELETE FROM Users WHERE Id = @ID";
            SqlCommand sqlCommand = new SqlCommand(Que, sqlConnection);
            sqlCommand.Parameters.Add(p1);
            sqlConnection.Open();
            sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();
            Que = $"DELETE FROM Posts WHERE PostedBy='{user.Username}'";
            sqlCommand = new SqlCommand(Que, sqlConnection);
            sqlConnection.Open();
            sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();
            return RedirectToAction("Index", "Admin");
        }
        [HttpGet]
        public IActionResult UpdateUser(int id) // function to upadte existing user by admin
        {
            if (!HttpContext.Request.Cookies.ContainsKey("Username") || HttpContext.Request.Cookies["Username"] != "admin") // checking if admin logged in or not
                return this.RedirectToAction("Index", "Index");
            return View(ReadAllUsers().Find(u => u.ID == id));
        }
        [HttpPost]
        public IActionResult UpdateUser(int id, string name, string username, string email, string password, IFormFile NewPhoto)
        {
            if (NewPhoto != null)
            {

                if (!CheckFileFormat(NewPhoto))
                {
                    ModelState.AddModelError("Error", "Wrong file Fromat");
                    return View(ReadAllUsers().Find(u => u.ID == id));
                }
            }
            string Conn = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BlogSite;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection sqlConnection = new SqlConnection(Conn);
            SqlParameter p1 = new SqlParameter("name", name);
            SqlParameter p2 = new SqlParameter("username", username);
            SqlParameter p3 = new SqlParameter("email", email);
            SqlParameter p4 = new SqlParameter("password",password);
            string Que = $"Update Users SET Name=@name, Username=@username, Password=@password WHERE Id={id}";
            SqlCommand sqlCommand = new SqlCommand(Que, sqlConnection);
            sqlCommand.Parameters.Add(p1);
            sqlCommand.Parameters.Add(p2);
            sqlCommand.Parameters.Add(p4);
            sqlConnection.Open();
            sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();
            if(NewPhoto != null)
            {
                string[] file = NewPhoto.FileName.Split('.'); //To get extension of file
                string fileName = string.Format(@"{0}.{1}", Guid.NewGuid(), file[1]); //Guid used to generate unique filename
                string folderName = Path.Combine(hostingEnvironment.WebRootPath, "ProfileImages");
                string filePath = Path.Combine(folderName, fileName);
                NewPhoto.CopyTo(new FileStream(filePath, FileMode.Create));
                Que = $"UPDATE Users SET ProfileImageName = '{fileName}' WHERE Id = {id}";
                sqlCommand = new SqlCommand(Que, sqlConnection);
                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
                Que = $"UPDATE Posts SET ProfileImageName = '{fileName}' WHERE PostedBy = '{username}'";
                sqlCommand = new SqlCommand(Que, sqlConnection);
                sqlConnection.Open();
                sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
            }
            return RedirectToAction("Index", "Admin");
        }
        List<User> ReadAllUsers() // this function will read all users from database and return list from it
        {
            List<User> AllUser = new List<User>();
            string Conn = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BlogSite;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection sqlConnection = new SqlConnection(Conn);
            string Que = $"SELECT * FROM Users";
            SqlCommand sqlCommand = new SqlCommand(Que, sqlConnection);
            sqlConnection.Open();
            SqlDataReader ReadData = sqlCommand.ExecuteReader();
            while (ReadData.Read())
            {
                User temp = new User { ID = ReadData.GetInt32(0), Name = ReadData[1] as string, Username = ReadData[2] as string, Email = ReadData[3] as string, Password = ReadData[4] as string, ProfileImageName = ReadData[5] as string };
                temp.Name = temp.Name.Trim();
                temp.Username = temp.Username.Trim();
                temp.Email = temp.Email.Trim();
                temp.Password = temp.Password.Trim();
                AllUser.Add(temp);
            }
            sqlConnection.Close();
            return AllUser;
        }
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
        bool CreateNewUser(User user) // create new user if username not availble then no user created and return false
        {
            string Conn = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BlogSite;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection sqlConnection = new SqlConnection(Conn);
            SqlParameter p1 = new SqlParameter("name", user.Name);
            SqlParameter p2 = new SqlParameter("username", user.Username);
            SqlParameter p3 = new SqlParameter("email", user.Email);
            SqlParameter p4 = new SqlParameter("password", user.Password);
            string Que = $"SELECT * FROM Users WHERE Username = @username";
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

    }
}
