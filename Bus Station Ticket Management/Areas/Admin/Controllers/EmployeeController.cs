using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Bus_Station_Ticket_Management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Manager")]
    [Route("Admin/[controller]/[action]")]

    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EmployeeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Employee
        public async Task<IActionResult> Index()
        {
            var usersQuery = _context.Users.AsQueryable();

            var userRolesList = await (from user in _context.Users
                                       join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                       join role in _context.Roles on userRole.RoleId equals role.Id
                                       select new { user.Id, RoleName = role.Name }).ToListAsync();

            var userRolesDict = userRolesList
                .Where(ur => ur.RoleName != "Customer")
                .ToDictionary(x => x.Id, x => x.RoleName);

            ViewBag.UserRoles = userRolesDict;

            // For role filter dropdown
            ViewBag.Roles = await _context.Roles.Select(r => r.Name).Distinct().ToListAsync();

            var userIdsToInclude = userRolesDict.Keys;
            // Contains(List<>) =  IN (list) (SQL)
            return View(await usersQuery.Where(u => userIdsToInclude.Contains(u.Id)).ToListAsync());
        }

        // GET: Employee/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            // Query to get user roles (mapping user Id to role name)
            var userRoles = await (from user in _context.Users
                                   join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                   join role in _context.Roles on userRole.RoleId equals role.Id
                                   select new { user.Id, RoleName = role.Name }).ToListAsync();

            // Create a dictionary mapping user Id to role name
            ViewBag.UserRoles = userRoles.ToDictionary(x => x.Id, x => x.RoleName);

            return View(applicationUser);
        }

        public async Task<IActionResult> DetailsPartial(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (applicationUser == null)
            {
                return NotFound();
            }

            // Query to get user roles (mapping user Id to role name)
            var userRoles = await (from user in _context.Users
                                   join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                   join role in _context.Roles on userRole.RoleId equals role.Id
                                   select new { user.Id, RoleName = role.Name }).ToListAsync();

            // Create a dictionary mapping user Id to role name
            ViewBag.UserRoles = userRoles.ToDictionary(x => x.Id, x => x.RoleName);

            return PartialView("_DetailsPartial", applicationUser);
        }

        // GET: Employee/Create
        public IActionResult Create()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            ViewBag.Roles = roles;
            return View();
        }

        // POST: Employee/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Address,Gender,DateOfBirth,UserName,Email,PhoneNumber")] ApplicationUser applicationUser, string Password, string selectedRole)
        {
            if (!validateEmployee(applicationUser))
            {
                return View(applicationUser);
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = applicationUser.UserName,
                    Email = applicationUser.Email,
                    FullName = applicationUser.FullName,
                    Address = applicationUser.Address,
                    Gender = applicationUser.Gender,
                    DateOfBirth = applicationUser.DateOfBirth,
                    PhoneNumber = applicationUser.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, Password);

                if (result.Succeeded)
                {

                    if (!string.IsNullOrEmpty(selectedRole))
                    {
                        await _userManager.AddToRoleAsync(user, selectedRole);

                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError(string.Empty, "No role selected!");
                }

                // If creation fails, add errors to ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(applicationUser);
        }

        // GET: Employee/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.Users.FindAsync(id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            ViewBag.Roles = roles;
            ViewBag.CurrentRole = userRoles.FirstOrDefault();

            return View(applicationUser);
        }

        // POST: Employee/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            [Bind("FullName,Address,Gender,DateOfBirth,Id,UserName,Email,PhoneNumber,LockoutEnd,LockoutEnabled,AccessFailedCount")] ApplicationUser applicationUser,
            string selectedRole,
            string? newPassword)
        {
            if (id != applicationUser.Id)
            {
                return NotFound();
            }

            if (!validateEmployee(applicationUser))
            {
                return View(applicationUser);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            if (!roles.Any())
            {
                ModelState.AddModelError("", "Error: No roles available.");
                return View(applicationUser);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = roles;
                ViewBag.CurrentRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                return View(applicationUser);
            }

            try
            {
                // Update user properties 
                user.UserName = applicationUser.UserName ?? applicationUser.Email;
                user.Email = applicationUser.Email;
                user.FullName = applicationUser.FullName ?? "Unknown";
                user.Address = applicationUser.Address;
                user.Gender = applicationUser.Gender ?? "Other";
                user.DateOfBirth = applicationUser.DateOfBirth;
                user.PhoneNumber = applicationUser.PhoneNumber;
                user.LockoutEnabled = applicationUser.LockoutEnabled;
                user.LockoutEnd = applicationUser.LockoutEnd;
                user.AccessFailedCount = applicationUser.AccessFailedCount;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    AddErrorsToModelState(updateResult.Errors);
                    return ReloadViewWithRoles(applicationUser, roles, user);
                }

                // Change password if provided
                if (!string.IsNullOrWhiteSpace(newPassword) && newPassword.Length > 0 && !string.IsNullOrEmpty(newPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, newPassword);

                    if (!passwordResult.Succeeded)
                    {
                        AddErrorsToModelState(passwordResult.Errors);
                        return ReloadViewWithRoles(applicationUser, roles, user);
                    }
                }

                // Update role if changed
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (!string.IsNullOrEmpty(selectedRole) && !currentRoles.Contains(selectedRole))
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, selectedRole);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(applicationUser.Id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return ReloadViewWithRoles(applicationUser, roles, user);
            }
        }

        private bool validateEmployee(ApplicationUser applicationUser){
            if (applicationUser == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(applicationUser?.PhoneNumber) &&
            (applicationUser.PhoneNumber.Length < 8 || applicationUser.PhoneNumber.Length > 11))
            {
                ModelState.AddModelError("PhoneNumber", "Invalid phone number!");
                return false;
            }

            if (applicationUser.DateOfBirth.HasValue && (applicationUser.DateOfBirth.Value > DateOnly.FromDateTime(DateTime.Now)))
            {
                ModelState.AddModelError("DateOfBirth", "Date of birth cannot be in the future");
                return false;
            }

            if (applicationUser.DateOfBirth.HasValue && (DateTime.Now.Year - applicationUser.DateOfBirth.Value.Year) < 18)
            {
                ModelState.AddModelError("DateOfBirth", "Employee must be at least 18 years old");
                return false;
            }

            return true;
        }

        private void AddErrorsToModelState(IEnumerable<IdentityError> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult ReloadViewWithRoles(ApplicationUser applicationUser, List<string> roles, ApplicationUser user)
        {
            ViewBag.Roles = roles;
            ViewBag.CurrentRole = user != null ? _userManager.GetRolesAsync(user).Result.FirstOrDefault() : null;
            return View(applicationUser);
        }

        // GET: Employee/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        // POST: Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var applicationUser = await _context.Users.FindAsync(id);
            if (applicationUser != null)
            {
                _context.Users.Remove(applicationUser);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
