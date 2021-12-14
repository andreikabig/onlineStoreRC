using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReverseCustoms.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Manager,ContentManager")]    // Доступ админа
    public class DashboardController : Controller
    {
        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            return View();
        }
    }
}