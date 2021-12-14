using ReverseCustoms.Models.Data;
using ReverseCustoms.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReverseCustoms.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }


        // МЕТОДЫ ВЫВОДА КАТЕГОРИИ МЕНЮ
        public ActionResult CategoryMenuPartial()
        {
            // Объявляем модель типа лист категории VM
            List<CategoryVM> categoryVMList;

            // Инициализируем модель данными
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }

            // Возвращаем частичное представление с моделью
            return PartialView("_CategoryMenuPartial", categoryVMList);
        }


        // Метод вывода товаров по категориям
        public ActionResult Category(string name)
        {
            // объявляем список типа лист
            List<ProductVM> productVMList;

            using (Db db = new Db())
            {
                // Получаем id категории
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                
                int catId = categoryDTO.Id;

                // Инициализируем список данными 
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                // Получаем имя категории
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();

                // Делаем проверку на null
                if (productCat == null)
                {
                    var catName = db.Categories.Where(x => x.Slug == name).Select(x => x.Name).FirstOrDefault();
                    ViewBag.CategoryName = catName;
                }
                else 
                {
                    ViewBag.CategoryName = productCat.CategoryName;
                }

            }
                // Вовзращаем представление с моделью
                return View(productVMList);
        }

        // МЕТОД ДЛЯ КАРТОЧКИ ТОВАРА
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            // Объявляем модели DTO и VM
            ProductDTO dto;
            ProductVM model;

            // Инициализируем id продукта
            int id = 0;

            using (Db db = new Db())
            {
                // Проверяем, доступен ли продукт
                if (!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }

                // Инициализируем productDTO данными
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                // Получаем ID
                id = dto.Id;

                // Инициализируем VM данными
                model = new ProductVM(dto);
            }
            // Получаем изображение из галлереи 
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs")).Select(fn => Path.GetFileName(fn));


                // Возвращаем модель в представление
                return View("ProductDetails", model);
        }
    }
}