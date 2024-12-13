using Basic_CRUD.Models;
using Basic_CRUD.Models.Services;
using Microsoft.AspNetCore.Mvc;

namespace Basic_CRUD.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment) {
            this.context = context;
            this.environment = environment;
        }
        public IActionResult Index()
        {
            var products = context.Products.OrderByDescending(p => p.Id).ToList();
            return View(products);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductDto productDto)
        {
            if (productDto.ImageFileName == null)
            {
                ModelState.AddModelError("ImageFile", "The image file is required");
            }

            if (!ModelState.IsValid)
            {
                return View(productDto);
            }

            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(productDto.ImageFileName!.FileName);

            string imageFullPath = Path.Combine(environment.WebRootPath, "Image/Products", newFileName);

            using (var stream = System.IO.File.Create(imageFullPath))
            {
                productDto.ImageFileName.CopyTo(stream);
            }

            Product product = new Product()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = newFileName,
                CreateAt = DateTime.Now,
            };

            context.Products.Add(product);
            context.SaveChanges();

            return RedirectToAction("Index", "Products");
        }

        public IActionResult Edit(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }
            var productDto = new ProductDto()
            {
                Name= product.Name,
                Brand= product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description,
            };
            ViewData["Id"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreateAt"] = product.CreateAt.ToString("MM/dd/yyyy");
            return View(productDto);
        }

        [HttpPost]
        public IActionResult Edit(int id, ProductDto productDto)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }
            if(!ModelState.IsValid)
            {
                ViewData["Id"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreateAt"] = product.CreateAt.ToString("MM/dd/yyyy");

                return View(productDto);
            }
            string newFileName = product.ImageFileName;
            if(productDto.ImageFileName != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(productDto.ImageFileName.FileName);

                string imageFullPath = Path.Combine(environment.WebRootPath, "Image/Products", newFileName);
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDto.ImageFileName.CopyTo(stream);
                }
                string oldImageFullPath = environment.WebRootPath + "Image/Products" + product.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }
            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Description = productDto.Description;
            product.ImageFileName = newFileName;

            context.SaveChanges();

            return RedirectToAction("Index","Products");
        }

        public IActionResult Delete(int id)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }
            string imageFullPath = environment.WebRootPath + "/Image/Products/" + product.ImageFileName;
            System.IO.File.Delete(imageFullPath);

            context.Products.Remove(product);
            context.SaveChanges();

            return RedirectToAction("Index", "Products");
        }
    }
}
