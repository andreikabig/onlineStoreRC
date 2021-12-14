using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ReverseCustoms.Models.Data
{
    public class Db : DbContext
    {
        // КОНТЕКСТЫ ДАННЫХ
        public DbSet<PagesDTO> Pages { get; set; } // Связь между БД со страницами
        public DbSet<SidebarDTO> Sidebars { get; set; } // Связь с БД с сайдбарами

        public DbSet<CategoryDTO> Categories { get; set; } // Связь с БД с категориями товаров

        public DbSet<ProductDTO> Products { get; set; } // Связь с БД с категориями товаров

        public DbSet<UserDTO> Users { get; set; } // Связь с БД с пользователями

        public DbSet<RoleDTO> Roles { get; set; } // Связь с БД с ролями

        public DbSet<UserRoleDTO> UserRoles { get; set; } // Связь с БД с ролями пользователей
        public DbSet<OrderDTO> Orders { get; set; } // Связь с БД с заказами
        public DbSet<OrderDetailsDTO> OrderDetails { get; set; } // Связь с БД с деталями заказов

    }
}