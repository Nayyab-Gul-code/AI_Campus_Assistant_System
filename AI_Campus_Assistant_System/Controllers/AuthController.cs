using AI_Campus_Assistant.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace AI_Campus_Assistant.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _auth;

        public AuthController(AuthService auth) => _auth = auth;

        // GET /Auth/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectByRole();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View();
            }

            var user = await _auth.ValidateLoginAsync(email, password);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password. Please try again.");
                return View();
            }

            var principal = _auth.BuildPrincipal(user);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return user.Role switch
            {
                "admin"   => RedirectToAction("Dashboard", "Admin"),
                "teacher" => RedirectToAction("Dashboard", "Teacher"),
                _         => RedirectToAction("Dashboard", "Student")
            };
        }

        // GET /Auth/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string name, string email, string password, string confirmPassword,
            string role, string? phone, string? program, int semesterNo = 1,
            string? designation = null)
        {
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View();
            }
            if (password.Length < 6)
            {
                ModelState.AddModelError("", "Password must be at least 6 characters.");
                return View();
            }

            var (success, message) = await _auth.RegisterAsync(
                name, email, password, role, phone, program, semesterNo, designation);

            if (!success)
            {
                ModelState.AddModelError("", message);
                return View();
            }

            TempData["Success"] = "Account created! Please sign in.";
            return RedirectToAction("Login");
        }

        // POST /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET /Auth/AccessDenied
        public IActionResult AccessDenied() => View();

        private IActionResult RedirectByRole()
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            return role switch
            {
                "admin"   => RedirectToAction("Dashboard", "Admin"),
                "teacher" => RedirectToAction("Dashboard", "Teacher"),
                _       => RedirectToAction("Dashboard", "Student")
            };
        }
    }
}
