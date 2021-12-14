using ReverseCustoms.Models.Data;
using ReverseCustoms.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace ReverseCustoms.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            // Объявляем лист типа CartVM
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Проверяем не пустая ли корзина
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Ваша корзина пуста.";
                return View();
            }

            // Складываем сумму и записываем во ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            // Возвращаем лист в представление
            return View(cart);
        }

        // МЕТОД ДЛЯ ЧАСТИЧНОГО ПРЕДСТАВЛЕНИЯ
        public ActionResult CartPartial()
        {
            // Объявляем модель CartVM
            CartVM model = new CartVM();

            // Объявляем переменную кол-ва
            int qty = 0;

            // Объявляем переменную цены
            decimal price = 0m;

            // Проверяем сессию корзины
            if (Session["cart"] != null)
            {
                // Получаем общее кол-во товаров и цену
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                // Или устанавливаем кол-во и цену 0
                model.Quantity = 0;
                model.Price = 0m;
            }

            // Возвращаем частичное представление с моделью
            return PartialView("_CartPartial", model);
        }

        // МЕТОД ДЛЯ ДОБАВЛЕНИЯ ТОВАРОВ
        public ActionResult AddToCartPartial(int id)
        {
            // Объявляем лист типа CartVM
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Объявляем модель CartVM
            CartVM model = new CartVM();


            using (Db db = new Db())
            {
                // Получаем товар
                ProductDTO product = db.Products.Find(id);

                // Проверяем, находится ли уже такой товар в корзине
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                // Если нет, то добавляем этот товар
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }

                // Если да, то добавляем еще один
                else
                {
                    productInCart.Quantity++;
                }
            }

            // Получаем общее кол-во, цену и добавляем в модель
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            // Сохраняем состояние корзины в сессию
            Session["cart"] = cart;

            // Частичное представление с моделью 
            return PartialView("_AddToCartPartial", model);
        }


        // ГЕТ МЕТОД ДЛЯ ТОТАЛА В ИНФО ВСЕХ ТОВАРОВ
        public JsonResult IncrementProduct(int productId)
        {
            // Объявляемт List Cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;


            using (Db db = new Db())
            {

                // Получаем модель из листа CartVM
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Добавляем кол-во
                model.Quantity++;

                // Сохраняем необходимые данные
                var result = new { qty = model.Quantity, price = model.Price };

                // Возвращаем JSON объект с данными

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        // МЕТОД ДЛЯ ДЕКРЕМЕНТА УДАЛЕНИЯ ПРОДУКТА
        public ActionResult DecrementProduct(int productId)
        {
            // Объявляемт List Cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;


            using (Db db = new Db())
            {

                // Получаем модель из листа CartVM
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // Отнимаем кол-во
                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                // Сохраняем необходимые данные
                var result = new { qty = model.Quantity, price = model.Price };

                // Возвращаем JSON объект с данными

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // МЕТОД УДАЛЕНИЯ ТОВАРА ИЗ КОРЗИНЫ
        public void RemoveProduct(int productId)
        {
            // Объявляемт List Cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                // Получаем модель из листа CartVM
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                cart.Remove(model);
            }
        }

        // МЕТОД ДЛЯ PAYPAL
        public ActionResult PaypalPartial()
        {
            // Получаем список товаров в корзине
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // Возвращаем частичное представление со списком товаров
            return PartialView(cart);
        }

        // МЕТОД ОБРАБОТКИ ЗАКАЗА POST
        [HttpPost]
        public void PlaceOrder()
        {
            // Получаем список товаров в корзине
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // Получаем имя пользователя
            string userName = User.Identity.Name;

            // Инициализируем orderId
            int orderId = 0;

            using (Db db = new Db()) {
                // Объявляем модель OrderDTO
                OrderDTO orderDTO = new OrderDTO();

                // Получаем Id пользователя
                var q = db.Users.FirstOrDefault(x => x.Username == userName);
                int userId = q.Id;

                // Заполняем модель OrderDTO данными и сохраняем
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);
                db.SaveChanges();

                // Получаем orderId
                orderId = orderDTO.OrderId;

                // Объявляем модель OrderDetailsDTO
                OrderDetailsDTO orderDetailsDto = new OrderDetailsDTO();

                // Инициализируем модель данными OrderDetailsDTO
                foreach (var item in cart)
                {
                    orderDetailsDto.OrderId = orderId;
                    orderDetailsDto.UserId = userId;
                    orderDetailsDto.ProductId = item.ProductId;
                    orderDetailsDto.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDto);
                    db.SaveChanges();
                }
            }
            // Отправляем письмо о заказе на почту администратора
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("04e17073b5462e", "1d17f89262a095"),
                EnableSsl = true
            };
            client.Send("from@example.com", "admin@example.com", "New order", $"Новый заказ! Номер заказа: {orderId}");

            // Обнуляем сессию (обязательное условие для PayPal)
            Session["cart"] = null;
        }
    }
}