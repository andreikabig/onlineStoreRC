using ReverseCustoms.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReverseCustoms.Models.ViewModels.Pages
{
    public class SidebarVM
    {
        // Конструктор по умолчанию
        public SidebarVM()
        { 
        }

        public SidebarVM(SidebarDTO row)
        {
            Id = row.Id;
            Body = row.Body;
        }
        public int Id { get; set; }
        [AllowHtml] // Разрешаем HTML теги при редактировании
        public string Body { get; set; }
    }
}