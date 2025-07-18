﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using salalal.Models;
using System.IO;
using System.Linq;
using System.Security.Claims;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly ISkiRepository _skiRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IWebHostEnvironment _webHostEnvironment; // za hendlovanje file uploadova

    public AdminController(IUserRepository userRepository, ISkiRepository skiRepository, IOrderRepository orderRepository, IWebHostEnvironment webHostEnvironment)
    {
        _userRepository = userRepository;
        _skiRepository = skiRepository;
        _orderRepository = orderRepository;
        _webHostEnvironment = webHostEnvironment;
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    private IActionResult RedirectToHomeIfNotAdmin()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Index", "Home");
        }
        return null;
    }

    // -------------- USERS -----------------
    public IActionResult ManageUsers()
    {
        var redirect = RedirectToHomeIfNotAdmin();
        if (redirect != null) return redirect;

        //Uzima sve korisnike iz DB i vraca ih na ManageUsers view
        var users = _userRepository.GetAllUsers();
        return View("ManageUsers", users);
    }

    [HttpGet]
    public IActionResult EditUser(int id)
    {
        var redirect = RedirectToHomeIfNotAdmin();
        if (redirect != null) return redirect;

        //Za edit sam uzeo da korisnika nadje po ID-u,ako ga ne nadje,vraca 404
        var user = _userRepository.GetUserById(id);
        if (user == null) return NotFound();

        return View("EditUser", user);
    }

    [HttpPost]
    public IActionResult EditUser(User user)
    {
        var redirect = RedirectToHomeIfNotAdmin();
        if (redirect != null) return redirect;

        if (!ModelState.IsValid) return View(user);

        //Updatuje korisnika u DB
        _userRepository.UpdateUser(user);
        return RedirectToAction("ManageUsers");
    }

    [HttpPost]
    public IActionResult DeleteUser(int id)
    {
        var redirect = RedirectToHomeIfNotAdmin();
        if (redirect != null) return redirect;

        //Deletuje korisnika i refreshuje stranicu
        _userRepository.DeleteUser(id);
        return RedirectToAction("ManageUsers");
    }

    // -------------- SKIS ----------------

    public IActionResult ManageSkis(string searchTerm)
    {
        var redirect = RedirectToHomeIfNotAdmin();
        if (redirect != null) return redirect;

        var skis = _skiRepository.GetAllSkis(); //uzima sve skije

        if (!string.IsNullOrEmpty(searchTerm))// ako je searchterm unesen filtrira po imenu(case sensitive(bug?))
        {
            skis = skis.Where(s => s.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        else//ako nema search terma sve prikazi
        {
            skis = skis.ToList();
        }

        return View("ManageSkis", skis);
    }



    [HttpGet]
    public IActionResult AddSki()
    {
        var redirect = RedirectToHomeIfNotAdmin();
        if (redirect != null) return redirect;

        return View("AddSki");
    }

    [HttpPost]
    public IActionResult AddSki(Ski ski, IFormFile imageFile)
    {
        var redirect = RedirectToHomeIfNotAdmin();
        if (redirect != null) return redirect;

        if (ModelState.IsValid)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                // Folder za sliku
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                string uniqueFileName = Path.GetFileName(imageFile.FileName); // Oreginalni fajl ime
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Cuvaj u wwwroot/Images
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(fileStream);
                }

                // Cuvati u DB
                ski.ImagePath = "/Images/" + uniqueFileName;
            }

            _skiRepository.AddSki(ski);
            _skiRepository.SaveChanges();
            return RedirectToAction("ManageSkis");
        }
        return View("AddSki", ski);
    }

    [HttpGet]
    public IActionResult EditSki(int id)
    {
        var redirect = RedirectToHomeIfNotAdmin();
        if (redirect != null) return redirect;

        var ski = _skiRepository.GetSkiById(id);
        if (ski == null) return NotFound();

        return View("EditSki", ski);
    }

    [HttpPost]
    public IActionResult EditSki(Ski ski, IFormFile imageFile)
    {
        var redirect = RedirectToHomeIfNotAdmin();
        if (redirect != null) return redirect;

        if (ModelState.IsValid)
        {
            var existingSki = _skiRepository.GetSkiById(ski.Id);
            if (existingSki == null)
            {
                return NotFound();
            }

            existingSki.Name = ski.Name;
            existingSki.Model = ski.Model;
            existingSki.Price = ski.Price;
            existingSki.StockQuantity = ski.StockQuantity;

            if (imageFile != null && imageFile.Length > 0)
            {
                // Ime foldera
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                string uniqueFileName = Path.GetFileName(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Cuvaj slikui
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    imageFile.CopyTo(fileStream);
                }

                // Update putanju slike
                existingSki.ImagePath = "/Images/" + uniqueFileName;
            }

            _skiRepository.UpdateSki(existingSki);
            _skiRepository.SaveChanges();
            return RedirectToAction("ManageSkis");
        }
        return View("EditSki", ski);
    }

    public IActionResult DeleteSki(int id)
    {
        var redirect = RedirectToHomeIfNotAdmin();
        if (redirect != null) return redirect;

        _skiRepository.DeleteSki(id);
        _skiRepository.SaveChanges();
        return RedirectToAction("ManageSkis");
    }

    public IActionResult AdjustStock(int id, int quantity)
    {
        var redirect = RedirectToHomeIfNotAdmin();
        if (redirect != null) return redirect;

        var ski = _skiRepository.GetSkiById(id);
        if (ski == null) return NotFound();

        ski.StockQuantity += quantity;
        _skiRepository.UpdateSki(ski);
        _skiRepository.SaveChanges();

        return RedirectToAction("ManageSkis");
    }

    // -------------- Orderi '----------------

    public IActionResult ViewOrders(string searchTerm)
    {
        var redirect = RedirectToHomeIfNotAdmin();
        if (redirect != null) return redirect;

        var orders = _orderRepository.GetAllOrders();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            orders = orders
                .Where(o =>
                    o.User.Username.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    o.Id.ToString().Contains(searchTerm))
                .ToList();
        }

        return View("ViewOrders", orders);
    }

    [HttpPost]
    public IActionResult DeleteOrder(int id)
    {
        var redirect = RedirectToHomeIfNotAdmin();
        if (redirect != null) return redirect;

        _orderRepository.DeleteOrder(id);
        return RedirectToAction("ViewOrders");
    }


}
