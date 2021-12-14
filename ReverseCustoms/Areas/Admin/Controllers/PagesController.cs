using ReverseCustoms.Models.Data;
using ReverseCustoms.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ReverseCustoms.Areas.Admin.Controllers
{
        // Доступ админа
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        [Authorize(Roles = "Admin,ContentManager")]
        public ActionResult Index()
        {
            // Объявляем список для представления (PageVM)
            List<PageVM> pageList;

            // Инициализация списка (DB)
            using (Db db = new Db())
            { 
                pageList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }

                // Вовзращаем список в представление
                return View(pageList);
        }


        // Метод создания страниц
        // GET: Admin/Pages/AddPage
        [HttpGet]
        [Authorize(Roles = "Admin,ContentManager")]
        public ActionResult AddPage()
        {
            return View();
        }

        // POST: Admin/Pages/AddPage
        [HttpPost]
        [Authorize(Roles = "Admin,ContentManager")]
        public ActionResult AddPage(PageVM model) 
        {
            // Проверка модели на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            else
            {
                using (Db db = new Db())
                {
                    // Объявляем переменную для краткого описания (slug)
                    string slug;

                    // Инициализируем класс PageDTO
                    PagesDTO dto = new PagesDTO();

                    // Присваиваем заголовок модели
                    dto.Title = model.Title.ToUpper();

                    // Проверка на наличие краткого описания, если нет, присваиваем его
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }

                    // Проверка заголовка и краткого описания на уникальность
                    if (db.Pages.Any(x => x.Title == model.Title))
                    {
                        ModelState.AddModelError("", "Этот заголовок уже используется.");
                        return View(model);
                    }
                    else if (db.Pages.Any(x => x.Slug == model.Slug))
                    {
                        ModelState.AddModelError("", "Это описание уже используется.");
                        return View(model);
                    }

                    // Присваиваем оставшиеся значения модели
                    dto.Slug = slug;
                    dto.Body = model.Body;
                    dto.HasSidebar = model.HasSidebar;
                    dto.Sorting = 100; // При добавлении 100 она добавляется в конец списка

                    // Сохраняем модель в БД
                    db.Pages.Add(dto);
                    db.SaveChanges();
                }
            }

            // Передаем сообщение через TempData
            TempData["SM"] = "Страница успешно добавлена!";

            // Переадресация пользователя на метод INDEX
            return RedirectToAction("Index");

        }


        // Редактирование страницы
        [HttpGet]
        [Authorize(Roles = "Admin,ContentManager")]
        public ActionResult EditPage(int id)
        {
            // Объявление модели PageVM
            PageVM model;

            using (Db db = new Db())
            {
                // Получение страницы
                PagesDTO dto = db.Pages.Find(id);

                // Проверка на доступность страницы
                if (dto == null)
                {
                    return Content("Данная страница не доступна.");
                }

                // Инициализируем модель данными
                model = new PageVM(dto);

            }
           
            // Возвращаем модель в представление

            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "Admin,ContentManager")]
        public ActionResult EditPage(PageVM model) 
        {
            // Проверка модели на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                // Получаем id страницы
                int id = model.Id;

                // Объявляем переменную карткого описания заголовка
                string slug = "home";

                // Получаем страницу (по Id)
                PagesDTO dto = db.Pages.Find(id);

                // Присваиваем название из полученной модели в DTO
                dto.Title = model.Title;

                // Проверяем краткое описание и присваиваем его, если это необходимо
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else 
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                // Проверяем slug и title на уникальность
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "Этот заголовок не доступен.");
                    return View(model);

                }
                else if (db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug)) 
                {
                    ModelState.AddModelError("", "Это краткое описание не доступно.");
                    return View(model);
                }

                // Записываем остальные значения в класс DTO
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;


                // Сохраняем измненения в БД
                db.SaveChanges();
            }


            // Устанавливаем сообщение в TempData
            TempData["SM"] = "Вы успешно отредактировали данную страницу!";

            // Переадресация пользователя 
            return RedirectToAction("EditPage");
        }

        // Метод просмотра информации о странице
        [Authorize(Roles = "Admin,ContentManager")]
        public ActionResult PageDetails(int id) 
        {
            // Объявляем модель PageVM
            PageVM model;
            using (Db db = new Db())
            {
                // Получение страницы
                PagesDTO dto = db.Pages.Find(id);

                // Подтверждаем, что страница доступна
                if (dto == null)
                {
                    return Content("Эта страница недоступна.");
                }

                // Присваиваем модели информацию из БД
                model = new PageVM(dto);
            }

            // Возвращаем модель в представление
            return View(model);
        }


        // Метод удаления страницы
        [Authorize(Roles = "Admin,ContentManager")]
        public ActionResult DeletePage(int id) 
        {
            using (Db db = new Db())
            {
                // Получение страницы
                PagesDTO dto = db.Pages.Find(id);

                // Удаление страницы
                db.Pages.Remove(dto);

                // Сохранение изменений в БД
                db.SaveChanges();
            }

            // Сообщение об удачном удалении
            TempData["SM"] = "Страница была успешно удалена!";

            // Переадресация пользователя
            return RedirectToAction("Index");
        }

        // Метод сортировки страниц (перетаскиванием пользователя)
        [HttpPost]
        [Authorize(Roles = "Admin,ContentManager")]
        public void ReorderPages(int [] id)
        {
            using (Db db = new Db()) 
            {
                // Реализация счетчика
                int count = 1;

                // Инициализируем модель данных
                PagesDTO dto;

                // Устанавливаем сортировку для каждой страницы
                foreach (var pageId in id) 
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
            
        }

        // Метод EditSidebar
        [HttpGet]
        [Authorize(Roles = "Admin,ContentManager")]
        public ActionResult EditSidebar()
        {
            // Объявляем модель
            SidebarVM model;

            using (Db db = new Db()) 
            {
                // Получаем данные из DTO
                SidebarDTO dto = db.Sidebars.Find(1);    // Жесткие значения в код не желательны !!!!!!!!!!!!!!

                // Заполняем модель данными
                model = new SidebarVM(dto);
            }

            // Возвращаем представление с моделью
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "Admin,ContentManager")]
        public ActionResult EditSidebar(SidebarVM model)
        {

            using (Db db = new Db())
            {
                // Получение данных из DTO (БД)
                SidebarDTO dto = db.Sidebars.Find(1);     // Жесткие значения в код не желательны !!!!!!!!!!!!!!

                // Присваиваем данные в тело (в свойство Body)
                dto.Body = model.Body;

                // Сохраняем
                db.SaveChanges();
            }

            // Присваиваем сообщение в TempDate
            TempData["SM"] = "Вы успешно отредактировали сайдбар!";

            // Переадресация
            return RedirectToAction("EditSidebar");
        }
    }
}