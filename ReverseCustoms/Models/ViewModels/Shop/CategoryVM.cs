using ReverseCustoms.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReverseCustoms.Models.ViewModels.Shop
{
    public class CategoryVM
    {
        // Конструктор класса без параметров
        public CategoryVM()
        { 
        }
        public CategoryVM(CategoryDTO row)
        {
            Id = row.Id;
            Name = row.Name;
            Slug = row.Slug;
            Sorting = row.Sorting;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public int Sorting { get; set; }
    }
}