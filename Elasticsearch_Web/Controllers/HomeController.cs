using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Elasticsearch_Web.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            var key = Request.QueryString["key"] ?? "";
            Helper helper = new Helper();
            //helper.DeleteIndexAll("db_student");
            //helper.DeleteDocument<Student>(1, "db_student");
            //helper.CreateIndexAutoMap<Student>("db_student",3,0);
            //IEnumerable<Student> students = new List<Student> { new Student { Id = 1, Name = "张三", Description = "张三爱中国", DateTime = DateTime.Now },
            //new Student { Id = 2, Name = "李四", Description = "李四爱中国", DateTime = DateTime.Now },
            //new Student { Id = 3, Name = "王五", Description = "王五爱日本", DateTime = DateTime.Now },
            //new Student { Id = 4, Name = "李凡", Description = "李凡爱睡觉", DateTime = DateTime.Now },
            //new Student { Id = 5, Name = "王伟", Description = "张三爱中国", DateTime = DateTime.Now },
            //new Student { Id = 6, Name = "姜大卫", Description = "发展中国家", DateTime = DateTime.Now },
            //new Student { Id = 7, Name = "李大傻", Description = "撒各单位过去额", DateTime = DateTime.Now },
            //new Student { Id = 8, Name = "武则天", Description = "爱我中华", DateTime = DateTime.Now },
            //new Student { Id = 9, Name = "李毅大帝", Description = "我是中国人，我爱中国，我宣誓永远忠于党，忠于国家，忠于人民，中国万岁", DateTime = DateTime.Now }};
            //helper.CreateDocument<Student>(students, "db_student");

            var result = helper.Query("db_student", key);
            return View(result);
        }
    }
}