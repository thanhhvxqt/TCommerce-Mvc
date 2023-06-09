﻿using T.Library.Model.Response;
using T.Library.Model;
using T.Library.Model.Users;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using T.WebApi.Database.ConfigurationDatabase;
using T.WebApi.Extensions;
using Microsoft.AspNetCore.Identity;
using T.Library.Model.ViewsModel;
using App.Utilities;
using System.Text;

namespace T.WebApi.Services.UserServices
{
    public interface IUserService
    {
        Task<List<UserModel>> GetAllAsync();
        Task<List<Role>> GetAllRolesAsync();
        Task<ServiceResponse<UserModel>> Get(Guid id);
        Task<ServiceResponse<bool>> CreateOrEditAsync(UserModel model);
        Task<ServiceResponse<bool>> DeleteAsync(Guid id);
    }
    public class UserService : IUserService
    {
        private readonly DatabaseContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public UserService(IMapper mapper, DatabaseContext context, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _mapper = mapper;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<ServiceResponse<bool>> CreateOrEditAsync(UserModel model)
        {
            
            {
                var userTable = await _context.Users.FirstOrDefaultAsync(x => x.Id == model.Id);

                if (!AppUtilities.IsValidEmail(model.Email))
                    return new ServiceErrorResponse<bool>("Cần nhập đúng định dạng email");

                if ((await _userManager.FindByEmailAsync(model.Email) != null) && userTable?.Email != model.Email)
                {
                    return new ServiceErrorResponse<bool>("Email đã tồn tại");
                }

                if ((await _context.Users.FirstOrDefaultAsync(x => x.PhoneNumber == model.PhoneNumber) != null) && userTable?.PhoneNumber != model.PhoneNumber)
                    return new ServiceErrorResponse<bool>("Số điện thoại đã được đăng ký");

                model.Password = model.ConfirmPassword = GenerateRandomPassword(length: 6);

                
                if (userTable == null)
                {
                    var user = new User();
                    user.Email = model.Email;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.UserName = model.UserName;
                    user.CreatedDate = AppExtensions.GetDateTimeNow();
                    user.RequirePasswordChange = true;
                    user.Deleted = user.Deleted;
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (model.RoleNames != null && model.RoleNames.Any())
                    {
                        var userRoles = await _userManager.GetRolesAsync(user);
                        await _userManager.RemoveFromRolesAsync(user, userRoles);

                        var selectedRoles = model.RoleNames.Distinct();
                        try
                        {
                            await _userManager.AddToRolesAsync(user, selectedRoles);
                        }
                        catch(Exception ex)
                        {
                            return new ServiceErrorResponse<bool>(ex.Message);
                        }
                    }
                }
                else
                {
                    var userMapped = _mapper.Map<User>(model);

                    if (_context.IsRecordUnchanged(userTable, userMapped))
                    {
                        return new ServiceErrorResponse<bool>("Data is unchanged");
                    }

                    await _userManager.FindByIdAsync(model.Id.ToString());
                    //userTable.Dob = model.Dob;
                    userTable.Email = model.Email;
                    userTable.FirstName = model.FirstName;
                    userTable.LastName = model.LastName;
                    userTable.PhoneNumber = model.PhoneNumber;
                    userTable.UserName = model.UserName;
                    userTable.RequirePasswordChange = true;
                    userTable.Deleted = userTable.Deleted;
                    if (model.RoleNames != null && model.RoleNames.Any())
                    {
                        var userRoles = await _userManager.GetRolesAsync(userTable);
                        await _userManager.RemoveFromRolesAsync(userTable, userRoles);

                        var selectedRoles = model.RoleNames.Distinct();
                        try
                        {
                            await _userManager.AddToRolesAsync(userTable, selectedRoles);
                        }
                        catch (Exception ex)
                        {
                            return new ServiceErrorResponse<bool>(ex.Message);
                        }
                    }
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        userTable.PasswordHash = _userManager.PasswordHasher.HashPassword(userTable, model.Password);

                    }
                    var result = await _userManager.UpdateAsync(userTable);
                    if (!result.Succeeded)
                    {
                        return new ServiceErrorResponse<bool>("Update user failed");
                    }
                }
                
                return new ServiceSuccessResponse<bool>();
            }
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString()); ;

            if (user == null) { throw new Exception($"Cannot find user: {id}"); }

            user.Deleted = true;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new ServiceErrorResponse<bool>("Delete user failed");
            }
            return new ServiceSuccessResponse<bool>();
        }

        public async Task<ServiceResponse<UserModel>> Get(Guid id)
        {
            
            {
                var user = await _userManager.FindByIdAsync(id.ToString());

                var roles = await _userManager.GetRolesAsync(user);

                var model = _mapper.Map<UserModel>(user);

                model.RoleNames = roles;

                var response = new ServiceResponse<UserModel>
                {
                    Data = model,
                    Success = true
                };
                return response;
            }
        }

        public async Task<List<UserModel>> GetAllAsync()
        {
            
            {
                var model = _mapper.Map<List<UserModel>>(await _context.Users.Where(x=>x.Deleted == false).ToListAsync());
                return model;
            }
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            
            {
                return await _roleManager.Roles.ToListAsync();
            }
        }

        public string GenerateRandomPassword(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            StringBuilder password = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < length; i++)
            {
                int randomIndex = random.Next(validChars.Length);
                password.Append(validChars[randomIndex]);
            }

            return password.ToString();
        }
    }
}
