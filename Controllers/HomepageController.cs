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
    public class HomepageController : Controller
    {
        private readonly IHostingEnvironment hostingEnvironment;
        public HomepageController(IHostingEnvironment environment)
        {
            hostingEnvironment = environment;
        }
        public IActionResult Index()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("Username") || HttpContext.Request.Cookies["Username"] == "admin")  //check if user logged in or  not or if he is admin  then redirect to login page or admin page
            {
                return this.RedirectToAction("Index", "Index");
            }
            List<Post> AllPosts = ReadAllPosts();
            return View(AllPosts);
        }
        //about function simply returns about the website
        public IActionResult About()
        {
            if (!HttpContext.Request.Cookies.ContainsKey("Username") || HttpContext.Request.Cookies["Username"] == "admin")  //check if user logged in or  not or if he is admin  then redirect to login page or admin page
            {
                return this.RedirectToAction("Index", "Index");
            }
            return View();
        }
        [HttpGet]
        public IActionResult Profile() // this function returns existing info of user logged in and user can update that info
        {
            if (!HttpContext.Request.Cookies.ContainsKey("Username") || HttpContext.Request.Cookies["Username"] == "admin")  //check if user logged in or  not or if he is admin  then redirect to login page or admin page
            {
                return this.RedirectToAction("Index", "Index");
            }
            User user = GetLogUser();
            return View(user);
        }

        [HttpPost]
        public IActionResult Profile(int id, string name, string username, string email, string password, IFormFile NewPhoto)
        {
            if (NewPhoto != null)
            {

                if (!CheckFileFormat(NewPhoto))
                {
                    ModelState.AddModelError("Error", "Wrong file Fromat");
                    return View(GetLogUser());
                }
            }
            string Conn = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BlogSite;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection sqlConnection = new SqlConnection(Conn);
            SqlParameter p1 = new SqlParameter("name", name);
            SqlParameter p2 = new SqlParameter("username", username);
            SqlParameter p3 = new SqlParameter("email", email);
            SqlParameter p4 = new SqlParameter("password", password);
            string Que = $"Update Users SET Name=@name, Username=@username, Password=@password WHERE Id={id}";
            SqlCommand sqlCommand = new SqlCommand(Que, sqlConnection);
            sqlCommand.Parameters.Add(p1);
            sqlCommand.Parameters.Add(p2);
            sqlCommand.Parameters.Add(p4);
            sqlConnection.Open();
            sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();
            if (NewPhoto != null)
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
            return this.RedirectToAction("Index", "Homepage");
        }
        [HttpGet]
        public IActionResult CreatePost() // this return view to cerate new post
        {
            if (!HttpContext.Request.Cookies.ContainsKey("Username") || HttpContext.Request.Cookies["Username"] == "admin")  //check if user logged in or  not or if he is admin  then redirect to login page or admin page
            {
                return this.RedirectToAction("Index", "Index");
            }
            return View();
        }
        [HttpPost]
        public IActionResult CreatePost(Post post)
        {
            if (ModelState.IsValid)
            {
                CreatNewPost(post);
                return this.RedirectToAction("Index", "Homepage");
            }
            return View();
        }
        public IActionResult Logout() // delete cookies and logging out user
        {
            HttpContext.Response.Cookies.Delete("Username");
            return this.RedirectToAction("Index", "Index");
        }
        public IActionResult DeletePost(int id)
        {
            string Conn = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BlogSite;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection sqlConnection = new SqlConnection(Conn);
            SqlParameter p1 = new SqlParameter("ID", id);
            string Que = $"DELETE FROM Posts WHERE Id = @ID";
            SqlCommand sqlCommand = new SqlCommand(Que, sqlConnection);
            sqlCommand.Parameters.Add(p1);
            sqlConnection.Open();
            sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();
            return this.RedirectToAction("Index", "Homepage");
        }
        public IActionResult ViewPost(int id)
        {
            if (!HttpContext.Request.Cookies.ContainsKey("Username") || HttpContext.Request.Cookies["Username"] == "admin")  //check if user logged in or  not or if he is admin  then redirect to login page or admin page
            {
                return this.RedirectToAction("Index", "Index");
            }
            return View(GetPostByID(id));
        }
        [HttpGet]
        public IActionResult UpdatePost(int id) // if user click update post then this return view to update the post
        {
            if (!HttpContext.Request.Cookies.ContainsKey("Username") || HttpContext.Request.Cookies["Username"] == "admin")  //check if user logged in or  not or if he is admin  then redirect to login page or admin page
            {
                return this.RedirectToAction("Index", "Index");
            }
            Post post = GetPostByID(id);
            return View(post);
        }
        [HttpPost]
        public IActionResult UpdatePost(Post post)
        {
            post.PostedBy = HttpContext.Request.Cookies["Username"];
            string Conn = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BlogSite;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection sqlConnection = new SqlConnection(Conn);
            SqlParameter p1 = new SqlParameter("title", post.Title);
            SqlParameter p2 = new SqlParameter("content", post.Content);
            SqlParameter p3 = new SqlParameter("postedby", post.PostedBy);
            string Que = $"UPDATE Posts SET Title=@title, Content=@content WHERE PostedBy=@postedby";
            SqlCommand sqlCommand = new SqlCommand(Que, sqlConnection);
            sqlCommand.Parameters.Add(p1);
            sqlCommand.Parameters.Add(p2);
            sqlCommand.Parameters.Add(p3);
            sqlConnection.Open();
            sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();
            return this.RedirectToAction("Index", "Homepage");
        }
        bool CreatNewPost(Post post) // this function create new post in database if no errror came return true
        {
            post.PostedBy = HttpContext.Request.Cookies["Username"];
            post.PostingDate = DateTime.Today.ToString("MMM dd, yyyy");
            string Conn = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BlogSite;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection sqlConnection = new SqlConnection(Conn);
            SqlParameter p1 = new SqlParameter("title", post.Title);
            SqlParameter p2 = new SqlParameter("content", post.Content);
            SqlParameter p3 = new SqlParameter("postedby", post.PostedBy);
            SqlParameter p4 = new SqlParameter("postingdate", post.PostingDate);
            string Que2 = $"SELECT ProfileImageName FROM Users WHERE Username = '{post.PostedBy}'";
            SqlCommand sqlCommand = new SqlCommand(Que2, sqlConnection);
            sqlConnection.Open();
            post.ProfileImageName = (string) sqlCommand.ExecuteScalar();
            sqlConnection.Close();
            string Que = $"INSERT INTO Posts (Title, Content, PostedBy, PostingDate, ProfileImageName) VALUES (@title, @content, @postedby, @postingdate, '{post.ProfileImageName}')";
            sqlCommand = new SqlCommand(Que, sqlConnection);
            sqlCommand.Parameters.Add(p1);
            sqlCommand.Parameters.Add(p2);
            sqlCommand.Parameters.Add(p3);
            sqlCommand.Parameters.Add(p4);
            sqlConnection.Open();
            int num = sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();
            if (num == 1)
                return true;
            return false;
        }
        List<Post> ReadAllPosts() //this function reads all posts 
        {
            List<Post> AllPosts = new List<Post>();
            string Conn = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BlogSite;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection sqlConnection = new SqlConnection(Conn);
            string Que = $"SELECT * FROM Posts ORDER BY Id DESC";
            SqlCommand sqlCommand = new SqlCommand(Que, sqlConnection);
            sqlConnection.Open();
            SqlDataReader ReadData = sqlCommand.ExecuteReader();
            while (ReadData.Read())
            {
                Post temp = new Post { ID = ReadData.GetInt32(0), Title = ReadData[1] as string, Content = ReadData[2] as string, PostedBy = ReadData[3] as string, PostingDate = ReadData[4] as string, ProfileImageName = ReadData[5] as string };
                temp.PostedBy = temp.PostedBy.Trim();
                temp.Title = temp.Title.Trim();
                AllPosts.Add(temp);
            }
            sqlConnection.Close();
            return AllPosts;
        }
        Post GetPostByID(int id)
        {
            List<Post> AllPosts = ReadAllPosts();
            return AllPosts.Find(p => p.ID == id);
        }
        User GetLogUser() // this function will return logged in user
        {
            string Conn = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=BlogSite;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection sqlConnection = new SqlConnection(Conn);
            string Que = $"SELECT * FROM Users WHERE Username = '{HttpContext.Request.Cookies["Username"]}'";
            SqlCommand sqlCommand = new SqlCommand(Que, sqlConnection);
            sqlConnection.Open();
            SqlDataReader ReadData = sqlCommand.ExecuteReader();
            ReadData.Read();
            User user = new User { ID = ReadData.GetInt32(0),  Name = ReadData[1] as string, Username = ReadData[2] as string, Email = ReadData[3] as string, Password = ReadData[4] as string, ProfileImageName = ReadData[5] as string, };
            user.Name = user.Name.Trim();
            user.Username = user.Username.Trim();
            user.Email = user.Email.Trim();
            user.Password = user.Password.Trim();
            return user;
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
    }
}
