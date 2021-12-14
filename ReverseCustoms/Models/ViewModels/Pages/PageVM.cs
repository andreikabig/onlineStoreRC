using ReverseCustoms.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReverseCustoms.Models.ViewModels.Pages
{
    public class PageVM
    {
        // Класс получает данные от DTO и в конструкторе присваивать значения модельке, которая идет на представление
        // Можно считать промежуточным представление (БЕЗОПАСНОСТЬ ЛУЧШЕ)

        public PageVM() 
        {
            // Конструктор по умолчанию (если не будет параметров)
        }
        
        public PageVM(PagesDTO row) 
        {
            // Конструктор класса, который будет присваивать нашей моделе PagesDTO
            Id = row.Id;
            Title = row.Title;
            Slug = row.Slug;
            Body = row.Body;
            Sorting = row.Sorting;
            HasSidebar = row.HasSidebar;
        }

        public int Id { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 3)] // Макс 50, мин 3
        public string Title { get; set; }
        public string Slug { get; set; }
        [Required]
        [StringLength(int.MaxValue, MinimumLength = 3)]
        [AllowHtml]
        public string Body { get; set; }
        public int Sorting { get; set; }
        [Display(Name = "Sidebar")]
        public bool HasSidebar { get; set; }
    }
}