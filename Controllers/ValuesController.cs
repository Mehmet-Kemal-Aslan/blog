using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System;
using System.Linq;



namespace test
{
    public class DtoBlogArticleList
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Date { get; set; }
        public string Category { get; set; }
        public string Summary { get; set; }
        public string Id { get; set; }
    }

    public class DtoBlogCategoryList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }

    }

    public class DtoBlogTagList
    {
        public string Name { get; set; }
        public int Id { get; set; }

    }

    public class DtoBlogGetArticle
    {
        public int Id { get; set; }
    }

    public class DtoBlogCommentList
    {
        public string CommenterId { get; set; }
        public int CommentId { get; set; }
        public string UserName { get; set; }
        public string UserMail { get; set; }
        public string UserComment { get; set; }
        public string ComDate { get; set; }
    }

    public class DtoBlogGetComment
    {
        public int FollowerId { get; set; }
        public string UComment { get; set; }
        public int ArticleId { get; set; }
    }

    public class DtoBlogAddFollower
    {
        public string UName { get; set; }
        public string UMail { get; set; }
        public string UPassword { get; set; }
    }

    public class DtoBlogEnteringAccount
    {
        public int MailCount { get; set; }
        public int FollowerId { get; set; }
    }

    public class DtoBlogSetComment
    {
        public string NewComment { get; set; }
        public int Yorum_Id { get; set; }
    }


    namespace test.Controllers
    {

        [ApiController]
        public class BlogController : ControllerBase

        {
            string connStr = "Server=.\\SQLExpress;Database=Blog;Trusted_Connection=Yes;";

            [Route("api/CategoryList")]
            [HttpGet]
            public object Categories()
            {

                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    // Kategoriler dönecek
                    var result = conn.Query<DtoBlogCategoryList>("SELECT Id, Name, (select COUNT(*) from Article A where A.Category_Id = c.Id) AS Count  FROM Category C");
                    return result;
                }
            }

            [Route("api/TagList")]
            [HttpGet]
            public object Tags()
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    // Taglar dönecek
                    var result = conn.Query<DtoBlogTagList>("SELECT Id, Name  FROM Tag");
                    return result;
                }
            }

            [Route("api/ArticleList")]
            [HttpGet]
            public object Articles(int categoryID, int tagId)
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT TOP 10 A.Id as Id, A.Title, A.Body, A.summary, CONVERT(varchar(10), A.Date, 103) AS Date, C.Name  as Category  FROM Article A JOIN Category C ON A.Category_Id = C.Id";
                    if (categoryID > 0)
                    {
                        query += " where A.CAtegory_Id =" + categoryID;
                    }
                    if (tagId > 0)
                    {
                        query += " WHERE A.Id IN (SELECT Article_Id FROM ArticleTags WHERE Tag_Id= " + tagId + ")";
                    }

                    query += " order by Date desc";
                    //query += " order by A.Date desc";
                    // MAkaleler dönecek
                    var result = conn.Query<DtoBlogArticleList>(query);
                    return result;
                }
            }

            [Route("api/GetArticle")]
            [HttpGet]
            public object Articles(int articleId)
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = "SELECT TOP 1  A.Id as Id, A.Title, A.Body, CONVERT(varchar(10), A.Date, 103) AS Date, C.Name  as Category  FROM Article A JOIN Category C ON A.Category_Id = C.Id where A.Id =" + articleId;

                    var result = conn.Query<DtoBlogArticleList>(query).FirstOrDefault();
                    return result;
                }
            }

            [Route("api/CommentList")]
            [HttpGet]
            public object Articles1(int articleId)
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = "SELECT  Com.Id as CommentId, Com.Article_Id, Com.Follower_Id as CommenterId, CONVERT(varchar(10), Com.Date, 103) as ComDate, Com.Usercomment, F.Username, F.Usermail FROM Followers F JOIN Comment Com ON Com.Follower_Id = F.Id WHERE Com.Article_Id = " + articleId;

                    var result = conn.Query<DtoBlogCommentList>(query).ToList();
                    return result;
                }
            }

            [Route("api/GetComment")]
            [HttpPost]
            public object Comment([FromBody]DtoBlogGetComment comment)
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    var result = conn.Execute("INSERT INTO Comment(Follower_Id, Usercomment, Article_Id) VALUES('" + comment.FollowerId + "', '" + comment.UComment + "','" + comment.ArticleId + "')");

                    return result;
                }
            }

            [Route("api/AddFollower")]
            [HttpPost]
            public object Comment([FromBody] DtoBlogAddFollower follower)
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    var result = conn.Execute("INSERT INTO Followers(Username, Usermail, Password) VALUES('" + follower.UName + "', '" + follower.UMail + "','" + follower.UPassword + "')");

                    return result;
                }
            }

            [Route("api/AccountPage")]
            [HttpGet]
            public object Articles2(int followerId)
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = "SELECT Followers.Username FROM Followers WHERE Followers.Id = " + followerId;

                    var result = conn.Query<DtoBlogCommentList>(query).FirstOrDefault();
                    return result;
                }
            }

            [Route("api/EnteringAccount")]
            [HttpGet]
            public object Articles3(string umail, string upassword)
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    //string query = "SELECT Id AS followerId, COUNT (Usermail) AS MailCount FROM Followers WHERE Usermail LIKE '" + umail.TrimEnd().TrimStart() ;
                    //query += "' AND Password LIKE '" + upassword + "' GROUP BY Id";

                    string query = "EXEC MAILCOUNT @mailCount = 0, @umail = '" + umail.TrimEnd().TrimStart();
                    query += "' , @upassword = '" + upassword;
                    query += "'";

                    var result = conn.Query<DtoBlogEnteringAccount>(query).FirstOrDefault();
                    return result;
                }
            }

            [Route("api/SetComment")]
            [HttpPost]
            public object Comment([FromBody] DtoBlogSetComment comment)
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    var result = conn.Execute("UPDATE Comment SET Usercomment = '" + comment.NewComment + "' WHERE Id= '" + comment.Yorum_Id + "'");

                    return result;
                }
            }
        }
    }
}
