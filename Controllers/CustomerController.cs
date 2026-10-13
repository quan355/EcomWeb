using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using BCrypt.Net;
using EComWeb.Models;
using EComWeb.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace EComWeb.Controllers
{
    public class CustomerController : Controller
    {
        private MyContext _context;
        private IWebHostEnvironment _evn;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        public CustomerController(MyContext context, IWebHostEnvironment evn, IConfiguration configuration, IUserRepository userRepository)
        {
            _context = context;
            _evn = evn;
            _configuration = configuration;
            _userRepository = userRepository;
        }
        public IActionResult Index()
        {

            ViewBag.checkSession = HttpContext.Session.GetString("customerSession");
            return View();
        } 
        public IActionResult customerLogin()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CustomerLogin(string customerEmail, string customerPassword)
        {
            if (string.IsNullOrEmpty(customerEmail) || string.IsNullOrEmpty(customerPassword))
            {
                ViewBag.Message = "Vui lòng nhập email và mật khẩu!";
                return View();
            }

            var customer = _userRepository.GetByEmail(customerEmail);
            if (customer == null || !BCrypt.Net.BCrypt.Verify(customerPassword, customer.customer_password))
            {
                ViewBag.Message = "Tài khoản hoặc mật khẩu không chính xác!";
                return View();
            }

            HttpContext.Session.SetString("customerSession", customer.customer_id.ToString());
            return RedirectToAction("index");
        }
        public IActionResult customerRegistration()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CustomerRegistration(Customer customer)
        {
            // Kiểm tra customer null hoặc email/mật khẩu trống
            if (customer == null || string.IsNullOrEmpty(customer.customer_email) || string.IsNullOrEmpty(customer.customer_password))
            {
                ViewBag.Message = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            // Kiểm tra độ dài mật khẩu tối thiểu (ít nhất 6 ký tự)
            if (customer.customer_password.Length < 6)
            {
                ViewBag.Message = "Mật khẩu phải có ít nhất 6 ký tự!";
                return View();
            }

            // Kiểm tra định dạng email hợp lệ bằng regex
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(customer.customer_email, emailPattern))
            {
                ViewBag.Message = "Email không hợp lệ! Vui lòng nhập email đúng định dạng.";
                return View();
            }

            // Kiểm tra các thuộc tính khác của customer (ví dụ: customer_name không được trống)
            if (string.IsNullOrEmpty(customer.customer_name))
            {
                ViewBag.Message = "Tên khách hàng không được để trống!";
                return View();
            }

            // Kiểm tra xem email đã tồn tại hay chưa
            if (_userRepository.EmailExists(customer.customer_email))
            {
                ViewBag.Message = "Tài khoản đã tồn tại!";
                return View();
            }

            try
            {
                // Hash mật khẩu và thêm khách hàng
                customer.customer_password = BCrypt.Net.BCrypt.HashPassword(customer.customer_password);
                _userRepository.Add(customer);
                _userRepository.SaveChanges();
                return RedirectToAction("customerLogin");
            }
            catch (Exception)
            {
                // Xử lý ngoại lệ khi lưu dữ liệu thất bại
                ViewBag.Message = "Đã xảy ra lỗi khi đăng ký. Vui lòng thử lại!";
                return View();
            }
        }
        public IActionResult customerLogout()
        {
            HttpContext.Session.Remove("customerSession");
            return RedirectToAction("index");
        }
        public IActionResult customerUProfile()
        {
            ViewBag.checkSession = HttpContext.Session.GetString("customerSession");
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("customerSession")))
            {
                return RedirectToAction("customerLogin");
            }
            else
            {
                List<Category> category = _context.tbl_category.ToList();
                ViewData["category"] = category;
                var customerId = HttpContext.Session.GetString("customerSession");
                var row = _context.tbl_customer.Where(a => a.customer_id == int.Parse(customerId)).ToList();
                return View(row);
            }
        }
        [HttpPost]
        public IActionResult updateCustomerProfile(Customer customer, IFormFile customer_image)
        {
            var existingCustomer = _context.tbl_customer.Find(customer.customer_id);

            if (existingCustomer == null)
            {
                return NotFound();
            }

            if (customer_image != null)
            {
                string ImagePath = Path.Combine(_evn.WebRootPath, "customer_image", customer_image.FileName);
                using (var fs = new FileStream(ImagePath, FileMode.Create))
                {
                    customer_image.CopyTo(fs);
                }
                existingCustomer.customer_image = customer_image.FileName;
            }

            // Cập nhật các thuộc tính khác
            existingCustomer.customer_name = customer.customer_name;
            existingCustomer.customer_email = customer.customer_email;
            existingCustomer.customer_phone = customer.customer_phone;
            existingCustomer.customer_address = customer.customer_address;
            existingCustomer.customer_city = customer.customer_city;
            existingCustomer.customer_country = customer.customer_country;
            existingCustomer.customer_password = customer.customer_password;
            existingCustomer.customer_gender = customer.customer_gender;
            // ... cập nhật các thuộc tính khác nếu cần

            _context.SaveChanges();
            return RedirectToAction("customerUProfile");
        }
        public IActionResult feedback()
        {
            ViewBag.checkSession = HttpContext.Session.GetString("customerSession");
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;
            return View();
        }
        [HttpPost]
        public IActionResult feedback(Feedback feedback)
        {
            
            _context.tbl_feedback.Add(feedback);
            _context.SaveChanges();
            return RedirectToAction("feedback");
        }
        public IActionResult allfetchProduct()
        {
            ViewBag.checkSession = HttpContext.Session.GetString("customerSession");
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;
            List<Product> product = _context.tbl_product.ToList();
            ViewData["product"] = product;
            return View();
        }
        public IActionResult productDetails(int id)
        {
            ViewBag.checkSession = HttpContext.Session.GetString("customerSession");
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;
            var product =  _context.tbl_product.Where(p => p.products_id == id).ToList();
            return View(product);
        }
        public IActionResult addtoCart(int product_id, int quantity, int size)
        {
            string customerId = HttpContext.Session.GetString("customerSession");
            int custId;
            if (!int.TryParse(customerId, out custId))
            {
                TempData["ErrorMessage"] = "Phiên đăng nhập không hợp lệ!";
                return RedirectToAction("customerLogin");
            }

            // Kiểm tra xem sản phẩm với product_id và size đã tồn tại trong giỏ hàng chưa
            var existingCartItem = _context.tbl_cart
                .FirstOrDefault(c => c.cust_id == custId &&
                                   c.product_id == product_id &&
                                   c.product_size == size &&
                                   c.cart_status == 0);

            if (existingCartItem != null)
            {
                // Nếu sản phẩm đã tồn tại (cùng product_id và size), tăng số lượng
                existingCartItem.product_quantity += quantity;
                _context.Update(existingCartItem);
            }
            else
            {
                // Nếu sản phẩm chưa tồn tại, tạo bản ghi mới
                var cart = new Cart
                {
                    cust_id = custId,
                    product_id = product_id,
                    product_quantity = quantity,
                    product_size = size,
                    cart_status = 0 // Chờ đợi
                };
                _context.tbl_cart.Add(cart);
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Sản phẩm đã được thêm vào giỏ hàng!";
            return RedirectToAction("fetchCart", new { id = custId });
        }
        public IActionResult fetchCart()
        {
            string customerId = HttpContext.Session.GetString("customerSession");
            ViewBag.checkSession = customerId;

            // Kiểm tra đăng nhập
            if (string.IsNullOrEmpty(customerId))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem giỏ hàng!";
                return RedirectToAction("customerLogin");
            }

            // Parse customerId
            int custId;
            if (!int.TryParse(customerId, out custId))
            {
                TempData["ErrorMessage"] = "Phiên đăng nhập không hợp lệ!";
                return RedirectToAction("customerLogin");
            }

            // Lấy danh sách danh mục (category)
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;

            // Lấy danh sách giỏ hàng
            var cart = _context.tbl_cart
                .Where(c => c.cust_id == custId && c.cart_status == 0)
                .Include(c => c.customers)
                .Include(c => c.products)
                .ToList();

            // Tính tổng tiền
            decimal total = 0;
            foreach (var item in cart)
            {
                string cleanedPrice = new string(item.products.products_price.Where(char.IsDigit).ToArray());
                if (decimal.TryParse(cleanedPrice, out decimal price))
                {
                    total += price * item.product_quantity;
                }
            }

            ViewBag.TotalPrice = total; // Lưu tổng tiền vào ViewBag

            return View(cart);
        }
        [HttpPost]
        public IActionResult deleteProduct(int id)
        {
            var product = _context.tbl_cart.Find(id);
            if (product == null)
            {
                return NotFound("Sản Phẩm Không Tồn Tại.");
            }
            _context.tbl_cart.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("fetchCart");
        }
        [HttpGet]
        public async Task<IActionResult> AllFetchProduct(string q, int[] categoryIds)
        {
            ViewBag.checkSession = HttpContext.Session.GetString("customerSession");
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;
            // Lấy danh sách tất cả danh mục để hiển thị checkbox
            var categories = await _context.tbl_category.ToListAsync();
            ViewData["categories"] = categories;

            // Lấy danh sách sản phẩm
            IQueryable<Product> query = _context.tbl_product
                .Include(p => p.Category);

            // Lọc theo từ khóa tìm kiếm (nếu có)
            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(p => p.products_name.Contains(q));
            }

            // Lọc theo danh mục (nếu có danh mục được chọn)
            if (categoryIds != null && categoryIds.Any())
            {
                query = query.Where(p => categoryIds.Contains(p.cat_id));
            }

            // Lấy danh sách sản phẩm sau khi lọc
            var products = await query.ToListAsync();

            ViewData["product"] = products;
            ViewData["q"] = q;
            ViewData["selectedCategoryIds"] = categoryIds ?? new int[0]; // Lưu danh mục được chọn để giữ trạng thái checkbox
            return View("allfetchProduct");
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string customerEmail)
        {
            if (string.IsNullOrEmpty(customerEmail) || !new EmailAddressAttribute().IsValid(customerEmail))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập email hợp lệ!";
                return View();
            }

            var customer = await _context.tbl_customer
                .FirstOrDefaultAsync(c => c.customer_email == customerEmail);

            if (customer == null)
            {
                TempData["ErrorMessage"] = "Email không tồn tại!";
                return View();
            }

            customer.ResetPasswordToken = Guid.NewGuid().ToString();
            customer.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1);

            _context.Update(customer);
            await _context.SaveChangesAsync();

            string resetLink = Url.Action(
                 action: "ResetPassword",
                 controller: "Customer",
                 values: new { token = customer.ResetPasswordToken },
                 protocol: HttpContext.Request.Scheme // Lấy scheme (http hoặc https) từ request
    );
            try
            {
                await SendResetPasswordEmail(customerEmail, resetLink);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi khi gửi email: {ex.Message}";
                return View();
            }

            TempData["SuccessMessage"] = "Liên kết đặt lại mật khẩu đã được gửi đến email của bạn!";
            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        private async Task SendResetPasswordEmail(string toEmail, string resetLink)
        {
            // Đọc cấu hình từ EmailSettings
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPortString = _configuration["EmailSettings:SmtpPort"];
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"];

            // Kiểm tra các giá trị cấu hình
            if (string.IsNullOrEmpty(smtpServer))
                throw new ArgumentNullException(nameof(smtpServer), "SmtpServer không được cấu hình trong appsettings.json.");
            if (string.IsNullOrEmpty(smtpPortString) || !int.TryParse(smtpPortString, out var smtpPort))
                throw new ArgumentException("SmtpPort không hợp lệ trong appsettings.json.");
            if (string.IsNullOrEmpty(smtpUsername))
                throw new ArgumentNullException(nameof(smtpUsername), "SmtpUsername không được cấu hình trong appsettings.json.");
            if (string.IsNullOrEmpty(smtpPassword))
                throw new ArgumentNullException(nameof(smtpPassword), "SmtpPassword không được cấu hình trong appsettings.json.");
            if (string.IsNullOrEmpty(senderEmail))
                throw new ArgumentNullException(nameof(senderEmail), "SenderEmail không được cấu hình trong appsettings.json.");
            if (string.IsNullOrEmpty(senderName))
                throw new ArgumentNullException(nameof(senderName), "SenderName không được cấu hình trong appsettings.json.");

            using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = "Đặt lại mật khẩu",
                    Body = $"Nhấn vào liên kết sau để đặt lại mật khẩu: <a href='{resetLink}'>Đặt lại mật khẩu</a>",
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Liên kết không hợp lệ!";
                return RedirectToAction("ForgotPassword");
            }

            var customer = _context.tbl_customer
                .FirstOrDefault(c => c.ResetPasswordToken == token && c.ResetPasswordTokenExpiry > DateTime.UtcNow);

            if (customer == null)
            {
                TempData["ErrorMessage"] = "Liên kết không hợp lệ hoặc đã hết hạn!";
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string token, string newPassword)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            if (newPassword.Length < 8)
            {
                TempData["ErrorMessage"] = "Mật khẩu phải có ít nhất 8 ký tự!";
                return View();
            }

            var customer = await _context.tbl_customer
                .FirstOrDefaultAsync(c => c.ResetPasswordToken == token && c.ResetPasswordTokenExpiry > DateTime.UtcNow);

            if (customer == null)
            {
                TempData["ErrorMessage"] = "Liên kết không hợp lệ hoặc đã hết hạn!";
                return RedirectToAction("ForgotPassword");
            }

            customer.customer_password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            customer.ResetPasswordToken = null;
            customer.ResetPasswordTokenExpiry = null;

            _context.Update(customer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập.";
            return RedirectToAction("customerLogin");
        }
        [HttpGet]
        public IActionResult Checkout()
        {
            string customerId = HttpContext.Session.GetString("customerSession");
            if (customerId == null)
            {
                return RedirectToAction("customerLogin");
            }

            int custId;
            if (!int.TryParse(customerId, out custId))
            {
                TempData["ErrorMessage"] = "Phiên đăng nhập không hợp lệ!";
                return RedirectToAction("customerLogin");
            }

            // Lấy thông tin khách hàng từ database
            var customer = _context.tbl_customer.FirstOrDefault(c => c.customer_id == custId);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng!";
                return RedirectToAction("customerLogin");
            }

            // Kiểm tra thông tin giao hàng
            if (string.IsNullOrWhiteSpace(customer.customer_country) ||
                string.IsNullOrWhiteSpace(customer.customer_city) ||
                string.IsNullOrWhiteSpace(customer.customer_address) ||
                string.IsNullOrWhiteSpace(customer.customer_phone))
            {
                TempData["ErrorMessage"] = "Thông tin giao hàng của bạn chưa đầy đủ. Vui lòng cập nhật thông tin cá nhân!";
                return RedirectToAction("customerUProfile", new { id = custId });
            }

            // Lấy danh sách giỏ hàng
            var cartItems = _context.tbl_cart
                .Where(c => c.cust_id == custId && c.cart_status == 0)
                .Include(c => c.products)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Không có đơn hàng nào để đặt!";
                return RedirectToAction("fetchCart", new { id = custId });
            }

            // Tính tổng tiền
            decimal total = 0;
            foreach (var item in cartItems)
            {
                string cleanedPrice = new string(item.products.products_price.Where(char.IsDigit).ToArray());
                if (decimal.TryParse(cleanedPrice, out decimal price))
                {
                    total += price * item.product_quantity;
                }
            }

            // Truyền dữ liệu vào view
            ViewBag.Customer = customer;
            ViewBag.CartItems = cartItems;
            ViewBag.TotalPrice = total;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCheckout()
        {
            string customerId = HttpContext.Session.GetString("customerSession");
            if (customerId == null)
            {
                TempData["ErrorMessage"] = "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại!";
                return RedirectToAction("customerLogin");
            }

            int custId;
            if (!int.TryParse(customerId, out custId))
            {
                TempData["ErrorMessage"] = "Phiên đăng nhập không hợp lệ!";
                return RedirectToAction("customerLogin");
            }
            var customer = _context.tbl_customer.FirstOrDefault(c => c.customer_id == custId);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng!";
                return RedirectToAction("customerLogin");
            }

            if (string.IsNullOrWhiteSpace(customer.customer_country) ||
                string.IsNullOrWhiteSpace(customer.customer_city) ||
                string.IsNullOrWhiteSpace(customer.customer_address) ||
                string.IsNullOrWhiteSpace(customer.customer_phone))
            {
                TempData["ErrorMessage"] = "Thông tin giao hàng của bạn chưa đầy đủ. Vui lòng cập nhật thông tin cá nhân!";
                return RedirectToAction("EditProfile", new { id = custId });
            }

            var cartItems = _context.tbl_cart
                .Where(c => c.cust_id == custId && c.cart_status == 0)
                .Include(c => c.products)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"Số lượng cartItems: {cartItems.Count}");
            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Không có đơn hàng nào để đặt!";
                return RedirectToAction("fetchCart");
            }

            try
            {
                var orders = new List<Order>();
                foreach (var item in cartItems)
                {
                    System.Diagnostics.Debug.WriteLine($"Product ID: {item.product_id}, Product Price: {item.products.products_price}");

                    string cleanedPrice = new string(item.products.products_price.Where(char.IsDigit).ToArray());
                    if (!decimal.TryParse(cleanedPrice, out decimal price))
                    {
                        System.Diagnostics.Debug.WriteLine($"Không thể parse giá: {cleanedPrice}");
                        TempData["ErrorMessage"] = $"Không thể tính giá cho sản phẩm {item.products.products_name}! Giá: {item.products.products_price}";
                        return RedirectToAction("Checkout");
                    }

                    var order = new Order
                    {
                        cust_id = item.cust_id,
                        product_id = item.product_id,
                        product_size = item.product_size,
                        product_quantity = item.product_quantity,
                        total_price = price * item.product_quantity,
                        order_date = DateTime.Now,
                        order_status = 0,
                    };
                    orders.Add(order);
                }

                _context.tbl_order.AddRange(orders);
                System.Diagnostics.Debug.WriteLine($"Số đơn hàng sẽ thêm: {orders.Count}");

                _context.tbl_cart.RemoveRange(cartItems);
                System.Diagnostics.Debug.WriteLine($"Số mục sẽ xóa trong tbl_cart: {cartItems.Count}");

                int affectedRows = await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"Số bản ghi được cập nhật: {affectedRows}");

                var recentTime = DateTime.Now.AddMinutes(-1);
                var newOrders = _context.tbl_order
                    .Where(o => o.cust_id == custId && o.order_date >= recentTime)
                    .ToList();
                System.Diagnostics.Debug.WriteLine($"Số đơn hàng mới trong tbl_order: {newOrders.Count}");

                if (!newOrders.Any())
                {
                    TempData["ErrorMessage"] = "Không thể tạo đơn hàng. Vui lòng thử lại!";
                    return RedirectToAction("Checkout");
                }

                TempData["SuccessMessage"] = "Đặt hàng thành công! Đơn hàng của bạn đã được xác nhận.";
                return RedirectToAction("CheckoutConfirmation");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi đặt hàng: " + ex.Message;
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lưu thay đổi: {ex.Message}");
                return RedirectToAction("Checkout");
            }
        }
        public IActionResult CheckoutConfirmation()
        {
            ViewBag.checkSession = HttpContext.Session.GetString("customerSession");
            return View();
        }

        public IActionResult OrderHistory()
        {
            ViewBag.checkSession = HttpContext.Session.GetString("customerSession");
            string customerId = HttpContext.Session.GetString("customerSession");
            if (string.IsNullOrEmpty(customerId))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem lịch sử đơn hàng!";
                return RedirectToAction("customerLogin");
            }

            int custId;
            if (!int.TryParse(customerId, out custId))
            {
                TempData["ErrorMessage"] = "Phiên đăng nhập không hợp lệ!";
                return RedirectToAction("customerLogin");
            }

            var orders = _context.tbl_order
                .Where(o => o.cust_id == custId)
                .Include(o => o.products)
                .OrderByDescending(o => o.order_date)
                .ToList();

            return View(orders);
        }
    }
}
