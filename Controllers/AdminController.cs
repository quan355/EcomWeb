using EComWeb.Models;
using EComWeb.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EComWeb.Controllers
{
    public class AdminController : Controller
    {
        private MyContext _context;
        private IWebHostEnvironment _evn;
        private readonly ICloudinaryService _cloudinaryService;

        public AdminController(MyContext context,IWebHostEnvironment env, ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
            _context = context;
            _evn = env;
        }
        public IActionResult Index()
        {
            string admin_session = HttpContext.Session.GetString("admin_session");
            if (admin_session == null)
            {
                return RedirectToAction("login");
            }
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string adminEmail,string adminPassword)
        {
            
                var row = _context.tbl_admin.FirstOrDefault(a => a.admin_email == adminEmail);
                if (row != null && row.admin_password == adminPassword)
                {
                    HttpContext.Session.SetString("admin_session", row.admin_id.ToString());
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.message = "Tài khoản hoặc mật khẩu không chính xác";
                }
                return View();
            
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("admin_session");
            return RedirectToAction("login");
        }
        public IActionResult Profile()
        {
            string admin_session = HttpContext.Session.GetString("admin_session");
            if (admin_session == null)
            {
                return RedirectToAction("login");
            }
            else
            {
                var adminId = HttpContext.Session.GetString("admin_session");
                var row = _context.tbl_admin.Where(a => a.admin_id == int.Parse(adminId)).ToList();
                return View(row);
            }
            
        }
        [HttpPost]
        public IActionResult Profile(Admin admin)
        {
            _context.tbl_admin.Update(admin);
            _context.SaveChanges();
            return RedirectToAction("Profile");
        }
        [HttpPost]
        public IActionResult ChangeProfileImage(IFormFile admin_image,Admin admin)
        {
            string  ImagePath = Path.Combine(_evn.WebRootPath,"admin_image", admin_image.FileName);
            FileStream fs = new FileStream(ImagePath,FileMode.Create);
            admin_image.CopyTo(fs);
            admin.admin_image = admin_image.FileName;
            _context.tbl_admin.Update(admin);
            _context.SaveChanges();
            return RedirectToAction("Profile");
        }
        public IActionResult fetchCustomer()
        {
            string admin_session = HttpContext.Session.GetString("admin_session");
            if (admin_session == null)
            {
                return RedirectToAction("login");
            }
            else
            {
                return View(_context.tbl_customer.ToList());
            }
                
        }
        public IActionResult customerDetails(int id)
        {
            return View(_context.tbl_customer.FirstOrDefault(c => c.customer_id == id));
        }
       
        [HttpPost]
        public IActionResult updateCustomer(Customer customer,IFormFile customer_image)
        {
            string ImagePath = Path.Combine(_evn.WebRootPath,"customer_image",customer_image.FileName);
            FileStream fs = new FileStream(ImagePath, FileMode.Create);
            customer_image.CopyTo(fs);
            customer.customer_image = customer_image.FileName;
            _context.tbl_customer.Update(customer);
            _context.SaveChanges();
            return RedirectToAction("fetchCustomer");
        }
        public IActionResult deletePermission(int id)
        {
            return View(_context.tbl_customer.FirstOrDefault(c => c.customer_id == id));
        }
        public IActionResult deleteCustomer(int id)
        {
            var customer = _context.tbl_customer.Find(id);
            _context.tbl_customer.Remove(customer);
            _context.SaveChanges();
            return RedirectToAction("fetchCustomer");
        }
        public IActionResult fetchCategory()
        {
            string admin_session = HttpContext.Session.GetString("admin_session");
            if (admin_session == null)
            {
                return RedirectToAction("login");
            }
            else
            {
                return View(_context.tbl_category.ToList());
            }    
                
        }
        public IActionResult addCategory() 
        {
            return View();
        }
        [HttpPost]
        public IActionResult addCategory(Category cat)
        {
            _context.tbl_category.Add(cat);
            _context.SaveChanges();
            return RedirectToAction("fetchCategory");
        }
        public IActionResult updateCategory(int id) 
        { 
            var category=_context.tbl_category.Find(id);
            return View(category);
        }
        [HttpPost]
        public IActionResult updateCategory(Category cat) 
        { 
            _context.tbl_category.Update(cat);
            _context.SaveChanges();
            return RedirectToAction("fetchCategory");
        }
        public IActionResult deletePermissionCategory(int id)
        {
            return View(_context.tbl_category.FirstOrDefault(c => c.category_id == id));
        }
        public IActionResult deleteCategory(int id) 
        {
            var category = _context.tbl_category.Find(id);
            _context.tbl_category.Remove(category);
            _context.SaveChanges();
            return RedirectToAction("fetchCategory");
        }
        public IActionResult fetchProduct()
        {
            string admin_session = HttpContext.Session.GetString("admin_session");
            if (admin_session == null)
            {
                return RedirectToAction("login");
            }
            else
            {
                return View(_context.tbl_product.ToList());
            }
                
        }
        public IActionResult addProduct()
        {
            List<Category> categories = _context.tbl_category.ToList();
            ViewData["category"] = categories;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> addProduct(Product prod, IFormFile products_image)
        {
            // Lưu vào DB trước để có products_id
            _context.tbl_product.Add(prod);
            await _context.SaveChangesAsync(); 

            // Upload lên Cloudinary với products_id
            var uploadResult = await _cloudinaryService.UploadImageAsync(products_image, prod.products_id);

            // Cập nhật URL ảnh vào DB
            prod.products_image = uploadResult.SecureUrl.ToString();
            _context.tbl_product.Update(prod);
            await _context.SaveChangesAsync();

            return RedirectToAction("fetchProduct");
        }
        public IActionResult productDetails(int id)
        {
            return View(_context.tbl_product.Include(p => p.Category).FirstOrDefault(p => p.products_id == id));
        }
        public IActionResult deletePermissionProduct(int id)
        {
            return View(_context.tbl_product.FirstOrDefault(c => c.products_id == id));
        }
        public IActionResult deleteProduct(int id)
        {
            var product = _context.tbl_product.Find(id);
            _context.tbl_product.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("fetchProduct");
        }
        public IActionResult updateProduct(int id)
        {
            List<Category> categories = _context.tbl_category.ToList();
            ViewData["category"] = categories;
            var product = _context.tbl_product.Find(id);
            ViewBag.selectedCategoryId = product.cat_id;
            return View(product);
        }
        [HttpPost]
        public IActionResult updateProduct(Product prod)
        {
            _context.tbl_product.Update(prod);
            _context.SaveChanges();
            return RedirectToAction("fetchProduct");
        }
        [HttpPost]
        public async Task<IActionResult> ChangeProductImage(IFormFile products_image, Product products)
        {
            // Xóa ảnh cũ trên Cloudinary theo products_id
            await _cloudinaryService.DeleteImageAsync($"products/{products.products_id}");

            // Upload ảnh mới với publicId = "products/{products_id}"
            var uploadResult = await _cloudinaryService.UploadImageAsync(products_image, products.products_id);

            // Cập nhật URL ảnh mới
            products.products_image = uploadResult.SecureUrl.ToString();

            _context.tbl_product.Update(products);
            await _context.SaveChangesAsync();

            return RedirectToAction("fetchProduct");
        }
        public IActionResult fetchFeedback()
        {
            string admin_session = HttpContext.Session.GetString("admin_session");
            if (admin_session == null)
            {
                return RedirectToAction("login");
            }
            else
            {
                return View(_context.tbl_feedback.ToList());
            }
               
        }
        public IActionResult deletePermissionFeedback(int id)
        {
            return View(_context.tbl_feedback.FirstOrDefault(f => f.feedback_id == id));
        }
        public IActionResult deleteFeedback(int id)
        {
            var feedback = _context.tbl_feedback.Find(id);
            _context.tbl_feedback.Remove(feedback);
            _context.SaveChanges();
            return RedirectToAction("fetchFeedback");
        }
        public IActionResult fetchCart()
        {
            string admin_session = HttpContext.Session.GetString("admin_session");
            if (admin_session == null)
            {
                return RedirectToAction("login");
            }
            else
            {
                var cart = _context.tbl_cart.Include(c => c.products).Include(c => c.customers).ToList();
                return View(cart);
            }
                
        }
        public IActionResult deletePermissionCart(int id)
        {
            return View(_context.tbl_cart.FirstOrDefault(c => c.cart_id == id));
        }
        public IActionResult deleteCart(int id)
        {
            var cart = _context.tbl_cart.Find(id);
            _context.tbl_cart.Remove(cart);
            _context.SaveChanges();
            return RedirectToAction("fetchCart");
        }
        public IActionResult updateCart(int id)
        {
            var cart = _context.tbl_cart.Find(id);
            return View(cart);
        }
        [HttpPost]
        public IActionResult updateCart(int cart_status,Cart cart)
        {
            cart.cart_status = cart_status;
            _context.tbl_cart.Update(cart);
            _context.SaveChanges() ;
            return RedirectToAction("fetchCart");
        }
        [HttpGet]
        public IActionResult fetchOrder()
        {
            string admin_session = HttpContext.Session.GetString("admin_session");
            if (admin_session == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                var orders = _context.tbl_order
               .Include(o => o.products)
               .Include(o => o.customers)
               .OrderByDescending(o => o.order_date)
               .ToList();

                return View(orders);
            }
        }
        public IActionResult deleteOrder(int id)
        {
            var order = _context.tbl_order.Find(id);
            _context.tbl_order.Remove(order);
            _context.SaveChanges();
            return RedirectToAction("fetchOrder");
        }
        public IActionResult deletePermissionOrder(int id)
        {
            return View(_context.tbl_order.FirstOrDefault(c => c.order_id == id));
        }
        // GET: Hiển thị form cập nhật trạng thái đơn hàng
        [HttpGet]
        public IActionResult updateOrder(int id)
        {
            var order = _context.tbl_order
                .Include(o => o.products)
                .FirstOrDefault(o => o.order_id == id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("fetchOder");
            }

            return View(order);
        }

        // POST: Xử lý cập nhật trạng thái đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrder(int order_id, int order_status)
        {
            var order = _context.tbl_order
                .FirstOrDefault(o => o.order_id == order_id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("fetchOrder");
            }

            // Chỉ cập nhật order_status
            order.order_status = order_status;

            try
            {
                _context.tbl_order.Update(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật trạng thái đơn hàng thành công!";
                return RedirectToAction("fetchOrder");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật đơn hàng: " + ex.Message;
                return RedirectToAction("fetchOrder", new { id = order_id });
            }
        }
    }
}
