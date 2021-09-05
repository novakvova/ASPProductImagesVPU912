using AppProductImages.Data;
using AppProductImages.Data.Entities;
using AppProductImages.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AppProductImages.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppEFContext _context;

        public HomeController(ILogger<HomeController> logger,
            AppEFContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var model = _context.Products
                .Include(i=>i.ProductImages)
                .Select(x => new ProductItemViewModel
                {
                    Name=x.Name,
                    Price=x.Price,
                    Images = x.ProductImages.Select(t=>new ProductImageItemVM
                    {
                        Path="/images/"+t.Name
                    }).ToList()
                });
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductAddViewModel model)
        {
            List<string> fileNames = new List<string>();
            foreach (var item in model.Images)
            {
                string fileName = "";
                if (item != null)
                {
                    var ext = Path.GetExtension(item.FileName);
                    fileName = Path.GetRandomFileName() + ext;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(),
                        "products", fileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        item.CopyTo(stream);
                    }
                    fileNames.Add(fileName);
                }
            }
            Product product = new Product()
            {
                Name = model.Name,
                Price = model.Price
            };
            _context.Products.Add(product);
            _context.SaveChanges();
            int counter = 1;
            foreach (var img in fileNames)
            {
                ProductImage productImage = new ProductImage()
                {
                    Name = img,
                    Priority = counter++,
                    ProductId = product.Id
                };
                _context.ProductImages.Add(productImage);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

    }
}
