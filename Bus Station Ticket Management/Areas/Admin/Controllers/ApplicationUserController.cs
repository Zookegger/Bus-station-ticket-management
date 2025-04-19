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
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]

    public class ApplicationUserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationUserController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: ApplicationUser
        public async Task<IActionResult> Index(string? searchString, string? sortBy, string? roleFilter)
        {
            var usersQuery = _context.Users.AsQueryable();

            var userRolesList = await (from user in _context.Users
                                       join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                       join role in _context.Roles on userRole.RoleId equals role.Id
                                       select new { user.Id, RoleName = role.Name }).ToListAsync();

            var userRolesDict = userRolesList.ToDictionary(x => x.Id, x => x.RoleName);
            ViewBag.UserRoles = userRolesDict;

            // Role filter
            if (!string.IsNullOrEmpty(roleFilter))
            {
                var userIdsInRole = userRolesList
                    .Where(x => x.RoleName == roleFilter)
                    .Select(x => x.Id)
                    .ToHashSet();

                usersQuery = usersQuery.Where(u => userIdsInRole.Contains(u.Id));
            }

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u =>
                    (u.FullName != null && u.FullName.Contains(searchString)) ||
                    (u.UserName != null && u.UserName.Contains(searchString)) ||
                    (u.Email != null && u.Email.Contains(searchString)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchString))
                );
            }

            // Sorting
            usersQuery = sortBy switch
            {
                "name_asc" => usersQuery.OrderBy(u => u.FullName),
                "name_desc" => usersQuery.OrderByDescending(u => u.FullName),
                "username_asc" => usersQuery.OrderBy(u => u.UserName),
                "username_desc" => usersQuery.OrderByDescending(u => u.UserName),
                "email_asc" => usersQuery.OrderBy(u => u.Email),
                "email_desc" => usersQuery.OrderByDescending(u => u.Email),
                _ => usersQuery.OrderBy(u => u.Id)
            };

            // For role filter dropdown
            ViewBag.Roles = await _context.Roles.Select(r => r.Name).Distinct().ToListAsync();
            ViewBag.SearchString = searchString;
            ViewBag.SortBy = sortBy;
            ViewBag.RoleFilter = roleFilter;

            return View(await usersQuery.ToListAsync());
        }

        // GET: ApplicationUser/Details/5
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

        // GET: ApplicationUser/Create
        public IActionResult Create()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            ViewBag.Roles = roles;
            return View();
        }

        // POST: ApplicationUser/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Address,Gender,DateOfBirth,UserName,Email,PhoneNumber")] ApplicationUser applicationUser, string Password, string selectedRole)
        {

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

        // GET: ApplicationUser/Edit/5
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

        // POST: ApplicationUser/Edit/5
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

            if (!string.IsNullOrEmpty(applicationUser?.PhoneNumber) &&
            (applicationUser.PhoneNumber.Length < 8 || applicationUser.PhoneNumber.Length > 11))
            {
                ModelState.AddModelError("PhoneNumber", "Invalid phone number!");
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

            if (applicationUser == null) {
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
                if (!string.IsNullOrWhiteSpace(newPassword))
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
                if (!ApplicationUserExists(applicationUser.Id))
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
            ViewBag.CurrentRole = (user != null ? _userManager.GetRolesAsync(user).Result.FirstOrDefault() : null);
            return View(applicationUser);
        }

        // GET: ApplicationUser/Delete/5
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

        // POST: ApplicationUser/Delete/5
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

        private bool ApplicationUserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
