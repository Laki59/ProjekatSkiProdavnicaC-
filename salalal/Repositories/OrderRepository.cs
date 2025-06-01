using salalal.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace salalal.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        // Vraa sve porudžbine iz baze, zajedno sa korisnikom i skijama,ukljucuje korisnika,stavku porudzbine i skije
        public IEnumerable<Order> GetAllOrders()
        {
            return _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Ski)
                .ToList();
        }

        //Vraća jednu porudbinu po ID-u,zajedno sa povezanim podacima
        public Order GetOrderById(int id)
        {
            return _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Ski)
                .FirstOrDefault(o => o.Id == id);
        }

        // Vraa sve porudžbine koje je napravio odredjeni korisnik
        public IEnumerable<Order> GetOrdersByUserId(int userId)
        {
            return _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Ski)
                .ToList();
        }

        //Vraca sve porudzbine koje su u statusu pending
        public IEnumerable<Order> GetPendingOrders()
        {
            return _context.Orders
                .Where(o => o.Status == "Pending")
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Ski)
                .ToList();
        }

        //Dodaje novu porudžbinu u bazu
        public void AddOrder(Order order)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
        }

        // Azurira status porudzbine (npr iz pending u approved),pronadje po ID-u,
        public void UpdateOrderStatus(int orderId, string status)
        {
            var order = _context.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = status;
                _context.SaveChanges();
            }
        }

        //Brise porudžbinu i sve njene stavke
        public void DeleteOrder(int id)
        {
            var order = _context.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.Id == id);
            if (order != null)
            {
                _context.OrderItems.RemoveRange(order.OrderItems); // Remove associated items
                _context.Orders.Remove(order); // Remove order
                _context.SaveChanges();
            }
        }

        //Metoda za cuvanje
        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
