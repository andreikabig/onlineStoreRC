using ReverseCustoms.Models.Data;
using ReverseCustoms.Models.ViewModels.Account;
using ReverseCustoms.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ReverseCustoms.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        // МЕТОД СОЗДАНИЯ АККАУНТА
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {

            return View("CreateAccount");
        }
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            // Проверка на валидность модели
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            // Проверяем соответствие пароля
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Пароли не совпадают!");
                return View("CreateAccount", model);
            }

            using (Db db = new Db()) {
                // Проверяем имя на уникальность
                if (db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", $"Имя пользователя {model.Username} уже занято.");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                // Создаем экземпляр класса UserDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAdress = model.EmailAdress,
                    Username = model.Username,
                    Password = model.Password
                };

                // Добавляем все данные в экземпляр класса
                db.Users.Add(userDTO);

                // Сохраняем данные
                db.SaveChanges();

                // Добавляем роль пользователю
                int id = userDTO.Id;

                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();

            }

            // Записываем сообщение в TempData
            TempData["SM"] = "Вы успешно зарегистрировались!";

            // Переадресация пользователя
            return RedirectToAction("Login");
        }

        // ГЕТ МЕТОД АВТОРИЗАЦИИ
        [HttpGet]
        public ActionResult Login()
        {
            // Подтвердить, что пользователь не авторизован
            string userName = User.Identity.Name;
            if (!string.IsNullOrEmpty(userName))
                return RedirectToAction("user-profile");
            // Возвращаем представление
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            // Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Проверяем пользователя на валидность
            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
                if (!isValid)
                {
                    ModelState.AddModelError("", "Неправильное имя пользователя или пароль!");
                    return View(model);
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
                }
            }
        }

        // МЕТОД ВЫХОДА GET
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
        [Authorize]
        public ActionResult UserNavPartial()
        {
            // Получаем имя пользователя
            string userName = User.Identity.Name;

            // Объявляем модель
            UserNavPartialVM model;

            using (Db db = new Db()) {
                // Получаем пользователя
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                // Заполняем модель данными из контекста DTO
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }
            // Возвращаем частичное представление с моделью
            return PartialView(model);
        }

        // МЕТОД ПРОФИЛЯ ПОЛЬЗОВАТЕЛЯ ГЕТ
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            // Получаем имя пользователя
            string userName = User.Identity.Name;

            // Объявляем модель
            UserProfileVM model;

            using (Db db = new Db()) {
                // Получаем пользователя
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                // Инициализируем модель данными
                model = new UserProfileVM(dto);
            }

            // Возвращаем модель в представление
            return View("UserProfile", model);
        }
        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            bool userNameIsChanged = false;

            // Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            // Проверяем пароль (если пользователь его меняет)
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Пароли не совпадают.");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db()) {
                // Получаем имя пользователя
                string userName = User.Identity.Name;

                // Проверяем, сменилось ли имя пользователя
                if (userName != model.Username)
                {
                    userName = model.Username;
                    userNameIsChanged = true;
                }

                // Проверяем имя на уникальность
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == userName))
                {
                    ModelState.AddModelError("", $"Имя пользователя {model.Username} уже занято.");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                // Изменяем модель контекста данных
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAdress = model.EmailAdress;
                dto.Username = model.Username;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;

                }
                // Сохраняем изменения
                db.SaveChanges();
            }
            // Устанавливаем сообщения в TempData
            TempData["SM"] = "Вы успешно отредактировали ваш профиль!";

            // Возвращаем представление с моделью

            if (!userNameIsChanged)
                return View("UserProfile", model);
            else
                return RedirectToAction("Logout");
        }

        // МЕТОД ПОЛУЧЕНИЯ ЗАКАЗОВ ДЛЯ ПОЛЬЗОВАТЕЛЯ
        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            // Инициализируем модель OrdersForUserVM
            List<OrdersForUserVM> orderForUser = new List<OrdersForUserVM>();

            using (Db db = new Db()) {
                // Получаем Id пользователя
                UserDTO user = db.Users.FirstOrDefault(x => x.Username == User.Identity.Name);
                int userId = user.Id;

                // Инициализируем модель OrderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

                // Перебираем список товаров в OrderVM
                foreach (var order in orders)
                {

                    // Инициализируем словарь товаров
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();

                    // Объявляем переменную конечной суммы
                    decimal total = 0m;

                    // Инициализируем модель OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsDto = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    // Перебираем список OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsDto) {

                        // Получаем товар
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);
                        // Получаем цену товара
                        decimal price = product.Price;

                        // Получаем имя товара
                        string productName = product.Name;

                        // Добавляем товар в словарь
                        productAndQty.Add(productName, orderDetails.Quantity);

                        // Получаем конечную стоимость товара
                        total += orderDetails.Quantity * price;
                    }
                    // Добавляем полученные данные в модель
                    orderForUser.Add(new OrdersForUserVM(){
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
            // Возвращаем представление с моделью
            return View(orderForUser);
        }
    }
}