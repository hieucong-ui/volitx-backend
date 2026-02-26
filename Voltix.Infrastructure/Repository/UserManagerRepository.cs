using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Voltix.Domain.Constants;
using Voltix.Domain.Entities;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;

namespace Voltix.Infrastructure.Repository
{
    public class UserManagerRepository : Repository<ApplicationUser>, IUserManagerRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public UserManagerRepository(UserManager<ApplicationUser> userManager, ApplicationDbContext context) : base(context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IdentityResult> AccessFailedAsync(ApplicationUser user)
        {
            return await _userManager.AccessFailedAsync(user);
        }

        public async Task<IdentityResult> ResetAccessFailedAsync(ApplicationUser user)
        {
            return await _userManager.ResetAccessFailedCountAsync(user);
        }

        public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
        {
            return await _userManager.AddToRoleAsync(user, role)
            ?? throw new KeyNotFoundException($"Cannot find user with email: {user.Email} or Role: {role} not exist");
        }

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<ApplicationUser?> GetByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<ApplicationUser?> GetByIdAsync(string id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IList<string>> GetRoleAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<bool> IsEmailExist(string email)
        {
            return await _userManager.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> IsPhoneNumber(string phoneNumber)
        {
            return await (_userManager.Users.AnyAsync(u => u.PhoneNumber == phoneNumber));
        }

        public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
        {
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<IdentityResult> SetPassword(ApplicationUser user, string newPassword)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<IList<ApplicationUser>?> GetUsersInRoleAsync(string roleName)
        {
            return await _userManager.GetUsersInRoleAsync(roleName);
        }

        public async Task<IdentityResult> RemoveAllRole(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return await _userManager.RemoveFromRolesAsync(user, roles);
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            return await _userManager.CreateAsync(user);
        }

        public async Task<IdentityResult> AddLoginGoogleAsync(ApplicationUser user, string googleSub)
        {
            var info = new UserLoginInfo("Google", googleSub, "Google");
            return await _userManager.AddLoginAsync(user, info);
        }

        public async Task<IList<UserLoginInfo>> HasLogin(ApplicationUser user)
        {
            return await _userManager.GetLoginsAsync(user);
        }

        public async Task<ApplicationUser?> GetByUserNameAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }

        public async Task<bool> IsExistUserName(string userName)
        {
            return await _userManager.Users.AnyAsync(u => u.UserName == userName);
        }

        public async Task<int> GetTotalEVMStaffAsync(CancellationToken ct)
        {
            var roleId = await _context.Roles
                .Where(r => r.Name == StaticUserRole.EVMStaff)
                .Select(r => r.Id)
                .FirstOrDefaultAsync(ct);

            if (roleId == null)
                return 0;

            var total = await (
                from ur in _context.UserRoles
                join u in _context.Users on ur.UserId equals u.Id
                where ur.RoleId == roleId && u.LockoutEnabled == false
                select u
            ).CountAsync(ct);

            return total;
        }
    }
}
