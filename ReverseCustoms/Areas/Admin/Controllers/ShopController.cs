using PagedList;
using ReverseCustoms.Areas.Admin.Models.ViewModels.Shop;
using ReverseCustoms.Models.Data;
using ReverseCustoms.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace ReverseCustoms.Areas.Admin.Controllers
{
        // Доступ админа
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        [Authorize(Roles = "Admin")]
        public ActionResult Categories()
        {
            // Объявляем модель типа List
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                // Инициализируем модель данными
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }

            // Возвращаем List в представление
            return View(categoryVMList);
        }


        // Метод добавления категории
        // POST метод
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public string AddNewCategory(string catName)
        {
            // Объявляем строковую переменную ID
            string id;

            using (Db db = new Db())
            {
                // Проверка имени категории на уникальность
                if (db.Categories.Any(x => x.Name == catName))
                    return "titletaken";

                // Инициализиурем модель DTO
                CategoryDTO dto = new CategoryDTO();

                // Добавляем данные в модель
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                // Сохраняем
                db.Categories.Add(dto);
                db.SaveChanges();

                // Получаем ID для возврата в представление
                id = dto.Id.ToString();

            }

            // Возвращаем ID в представление
            return id;
        }

        // Метод сортировки категорий 
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                // Реализация счетчика
                int count = 1;

                // Инициализируем модель данных
                CategoryDTO dto;

                // Устанавливаем сортировку для каждой страницы
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }

        }

        // МЕТОД УДАЛЕНИЯ КАТЕГОРИИ
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                // Получение категории
                CategoryDTO dto = db.Categories.Find(id);

                // Удаление категории
                db.Categories.Remove(dto);

                // Сохранение изменений в БД
                db.SaveChanges();
            }

            // Сообщение об удачном удалении
            TempData["SM"] = "Категория была успешно удалена!";

            // Переадресация пользователя
            return RedirectToAction("Categories");
        }

        // POST МЕТОД ПЕРЕИМЕНОВАНИЯ КАТЕГОРИИ
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db()) 
            {
                // Проверка названия на уникальность
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";

                // Получение модели DTO
                CategoryDTO dto = db.Categories.Find(id);

                // Редактируем модель DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                // Сохраняем изменения
                db.SaveChanges();
            }

            // Возвращаем слово (просто чтобы работало)
            return "ok";
        }


        // Метод добавления товаров
        [Authorize(Roles = "Admin")]
        public ActionResult AddProduct()
        {
            // Объявляем модель данных
            ProductVM model = new ProductVM();

            // Добавляем список категорий
            using (Db db = new Db()) 
            {
                model.Categories = new SelectList(db.Categories.ToList(), dataValueField:"Id", dataTextField:"Name");
            }

            // Возвращаем модель в представление
            return View(model);
        }

        // МЕТОД POST ДОБАВЛЕНИЯ ТОВАРОВ
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file) 
        {
            // Проверка модели на валидность
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), dataValueField: "Id", dataTextField: "Name");
                    return View(model);
                }
            }

            // Проверка имени на уникальность
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), dataValueField: "Id", dataTextField: "Name");
                    ModelState.AddModelError(key:"", errorMessage:"Такой товар уже существует!");
                    return View(model);
                }
            }

            // Объявляем переменную ProductUD
            int id;

            // Инициализируем и сохраняем в БД модель на основе ProductDTO
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;
            }

            // Добавляем сообщение в TempData
            TempData["SM"] = "Товар был успешно добавлен!";

            #region Upload Image

            // Создаем все необходимые ссылки на дериктории
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            // Проверяем наличие директорий (если нет, то создаем)
            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            // Проверяем, был ли файл загружен
            if (file != null && file.ContentLength > 0)
            {
                // Получаем расширение файла
                string ext = file.ContentType.ToLower();

                // Проверяем расширение файла
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png"
                    ) 
                {
                    using (Db db = new Db())
                    {
                        // Инициализируем список
                        model.Categories = new SelectList(db.Categories.ToList(), dataValueField:"Id", dataTextField: "Name");
                        ModelState.AddModelError(key:"", errorMessage:"Изображение не было загружено - недопустимое расширение.");
                        return View(model);
                    }
                }
            



                // Переменная с именем изображения
                string imageName = file.FileName;

                // Сохраняем имя изображение в модель DTO
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                // Назначаем пути к оригинальному и уменьшенному изображению
                var path = string.Format($"{pathString2}\\{imageName}");
                var path2 = string.Format($"{pathString3}\\{imageName}");
                // Сохраняем оригинальное изображение
                file.SaveAs(path);

                // Создаем и сохраняем уменьшенную копию изображения
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1, 1);
                img.Save(path2);
            }
            #endregion

            // Переадресация пользователя
            return RedirectToAction("AddProduct");
        }

        // МЕТОД СПИСКА ТОВАРОВ
        // GET
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Products(int? page, int? catId)
        {
            // Объявляем модель ProductVM типа List
            List<ProductVM> listOfProductVM;

            // Устанавливаем номер страницы
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                // Инициализируем list и заполняем данными
                listOfProductVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();

                // Заполняем категории данными
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Устанавливаем выбранную категорию
                ViewBag.SelectedCat = catId.ToString();
            }

            // Устанавливаем постраничную навигацию
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 25);
            ViewBag.onePageOfProducts = onePageOfProducts;

            // Возвращаем все это в представление
            return View(listOfProductVM);
        }

        // МЕТОД РЕДАКТИРОВАНИЯ ТОВАРОВ
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult EditProduct(int id)
        {
            // Объявляем модель ProductVM
            ProductVM model;

            
            using (Db db = new Db())
            {
                // Получаем продукт
                ProductDTO dto = db.Products.Find(id);

                // Проверяем доступность продукта
                if (dto == null)
                {
                    return Content("Этот продукт недоступен.");
                }

                // Инициализируем модель данными
                model = new ProductVM(dto);

                // Создаем список категорий
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Получаем все изображения из галереи
                model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }



            // Возвращаем модель в представление

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            // Получаем Id продукта
            int id = model.Id;

            // Заполняем список категориями и изображениями
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            // Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Проверяем имя продукта на уникальность
            using (Db db = new Db()) 
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "Такое имя товара уже существует!");
                    return View(model);
                }
            }

            // Обновляем продукт в БД
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();

            }

            // Устанавливаем сообщение в TempData
            TempData["SM"] = "Вы успешно отредактировали товар!";


            // Логика обработки изображений
            #region Image Upload
            // Проверяем загрузку файла
            if (file != null && file.ContentLength > 0)
            {
                // Получаем расширение файла
                string ext = file.ContentType.ToLower();

                // Проверка расширения файла
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png"
                    )
                {
                    using (Db db = new Db())
                    {
                        // Инициализируем список
                        ModelState.AddModelError(key: "", errorMessage: "Изображение не было загружено - недопустимое расширение.");
                        return View(model);
                    }
                }

                // Устанавливаем пути загрузки 
                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                // Удаляем существующие файлы и директории
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file2 in di1.GetFiles())
                {
                    file2.Delete();
                }

                foreach (var file3 in di2.GetFiles())
                {
                    file3.Delete();
                }

                // Сохраняем изображение
                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                // Сохраняем оригинал и preview версию
                var path = string.Format($"{pathString1}\\{imageName}");
                var path2 = string.Format($"{pathString2}\\{imageName}");
                // Сохраняем оригинальное изображение
                file.SaveAs(path);

                // Создаем и сохраняем уменьшенную копию изображения
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1, 1);
                img.Save(path2);


            }
            #endregion

            // Переадресация пользователя

            return RedirectToAction("EditProduct");
        }



        // МЕТОД УДАЛЕНИЯ ТОВАРА по ID
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteProduct(int id)
        {
            // Удаляем товар из БД
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }
            // Удаляем директории товара (изображения)
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
            var pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);

            // Переадресация
            return RedirectToAction("Products");
        }

        // МЕТОД ДОБАВЛЕНИЯ ИЗОБРАЖЕНИЯ В ГАЛЕРЕЮ POST
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public void SaveGalleryImages(int id)
        {
            // Перебираем все полученные нами файлы
            foreach (string fileName in Request.Files)
            {
                // Инициализируем эти файлы
                HttpPostedFileBase file = Request.Files[fileName];

                // Проверяем на null
                if (file != null && file.ContentLength > 0)
                {
                    // Назначаем все пути к директориям
                    var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");
                    
                    // Назначаем пути изображений
                    var path = string.Format($"{pathString1}\\{file.FileName}");
                    var path2 = string.Format($"{pathString2}\\{file.FileName}");

                    // Сохраняем оригинальную и уменьшенную копию
                    file.SaveAs(path);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200).Crop(1, 1);
                    img.Save(path2);

                }
            }
        }

        // МЕТОД УДАЛЕНИЯ ИЗОБРАЖЕНИЯ ИЗ ГАЛЕРЕИ
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }

        // МЕТОД ПРОСМОТРА ЗАКАЗОВ ДЛЯ АДМИНА GET
        [Authorize(Roles = "Admin,Manager")]
        public ActionResult Orders()
        {
            // Инициализируем модель OrdersForAdminVM
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                // Инициализируем модель заказов OrderVM
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                // Перебираем данные модели OrderVM
                foreach (var order in orders)
                {
                    // Инициализируем словарь товаров
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();

                    // Объявляем переменную общей суммы
                    decimal total = 0m;

                    // Инициализируем лист OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    // Получаем имя пользователя
                    UserDTO user = db.Users.FirstOrDefault(x => x.Id == order.UserId);
                    string username = user.Username;

                    // Перебираем список товаров из OrderDetailDTO
                    foreach (var orderDetails in orderDetailsList)
                    {

                        // Получаем товар, который относится к нашему пользователю
                        //ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);
                        ProductDTO product = db.Products.Find(orderDetails.ProductId);

                        // Получаем цену товара
                        decimal price = product.Price;

                        // Получаем название товара
                        string productName = product.Name;

                        // Добавляем товар в словарь
                        productAndQty.Add(productName, orderDetails.Quantity);

                        // Получаем полную стоимость товаров пользователя
                        total += orderDetails.Quantity * price;
                    }
                    // Добавляем данные в модель OrdersForAdminVM
                    ordersForAdmin.Add(new OrdersForAdminVM()
                        { 
                        OrderNumber = order.OrderId,
                        UserName = username,
                        Total = total,
                        ProductsAndQty = productAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
            // Возвращаем представление с моделью OrdersForAdminVM
            return View(ordersForAdmin);
        }
    }
}