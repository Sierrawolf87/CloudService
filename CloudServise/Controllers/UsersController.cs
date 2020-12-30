using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CloudService_API.Data;
using CloudService_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CloudService_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;
        private readonly PasswordHashSettings _passwordHashSettings;
        private readonly MailSettings _mailSettings;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger, PasswordHashSettings passwordHashSettings, MailSettings mailSettings)
        {
            _context = context;
            _logger = logger;
            _passwordHashSettings = passwordHashSettings;
            _mailSettings = mailSettings;
        }

        // GET: api/Users
        [Authorize(Roles = "root, admin, network_editor")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            List<UserDTO> userDtos = new List<UserDTO>();
            var actionResult = await _context.Users.Include(c => c.Role).Include(c => c.Group).ToListAsync();
            foreach (var user in actionResult)
            {
                userDtos.Add(user.ToUserDto());
            }
            return userDtos;
        }

        [Authorize(Roles = "root, network_editor")]
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> SearchUsers([FromQuery] string text, [FromQuery] string role, [FromQuery] string group)
        {
            List<UserDTO> userDtos = new List<UserDTO>();
            var actionResult = await _context.Users.Include(c => c.Role).Include(c => c.Group)
                .Where(u => 
               (EF.Functions.Like(u.Id.ToString(), $"%{text}%") ||
                EF.Functions.Like(u.UserName , $"%{text}%") ||
                EF.Functions.Like(u.Name , $"%{text}%") ||
                EF.Functions.Like(u.Surname , $"%{text}%") ||
                EF.Functions.Like(u.Patronymic, $"%{text}%") ||
                EF.Functions.Like(u.ReportCard , $"%{text}%") ||
                EF.Functions.Like(u.Email, $"%{text}%") ||
                EF.Functions.Like(u.Group.Name, $"%{text}%")
               ) &&
               EF.Functions.Like(u.Role.Id.ToString(), $"%{role}%") &&
               EF.Functions.Like(u.Group.Id.ToString(), $"%{group}%")
               )
               .ToListAsync();

            foreach (var user in actionResult)
            {
                userDtos.Add(user.ToUserDto());
            }
            return userDtos;
        }

        // GET: api/Users/WithPage
        [Authorize(Roles = "root, admin, network_editor")]
        [HttpGet("WithPage")]
        public async Task<IActionResult> GetUsersWithPage([FromQuery] RequestParameters requestParameters)
        {
            requestParameters.TotalCount = await _context.Users.CountAsync();
            if (!requestParameters.Check())
                return NoContent();
            Response.Headers.Add("X-Pagination", requestParameters.ToJson());
            List<UserDTO> userDtos = new List<UserDTO>();
            var actionResult = await _context.Users.Include(c => c.Role).Skip(requestParameters.Skip).Take(requestParameters.Take).ToListAsync();
            foreach (var user in actionResult)
            {
                userDtos.Add(user.ToUserDto());
            }
            return Ok(userDtos);
        }

        // GET: api/Users/5
        [Authorize(Roles = "root, admin, network_editor")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _context.Users.Include(c => c.Role).Include(c => c.Group).FirstOrDefaultAsync(i => i.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user.ToUserDto());
        }

        // PUT: api/Users/5
        [Authorize(Roles = "root, admin, network_editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(Guid id, UserDTO user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            var find = await _context.Users.FindAsync(id);
            _context.Entry(find).State = EntityState.Modified;

            try
            {
                find.Email = user.Email;
                find.UserName = user.UserName;
                find.Name = user.Name;
                find.Surname = user.Surname;
                find.Patronymic = user.Patronymic;
                find.ReportCard = user.ReportCard;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex.Message);
                    return StatusCode(500);
                }
            }

            return NoContent();
        }

        // POST: api/Users/auth/SignUp
        [HttpPost("auth/SignUp")]
        public async Task<ActionResult<UserDTO>> SignUp(UserRegisterDTO user)
        {
            var find = await _context.Users.Where(c => c.ReportCard == user.ReportCard).FirstOrDefaultAsync();
            if (find != null)
            {
                return BadRequest("Пользователь с таким учётным номером уже существует");
            }
            var role = await _context.Roles.FindAsync(user.RoleId);
            if (role == null)
            {
                return BadRequest("Invalid role Id");
            }

            var group = await _context.Groups.FindAsync(user.GroupId);
            if (group == null)
            {
                return BadRequest("Invalid group Id");
            }

            var newUser = new User(user.Name, user.Surname, user.Patronymic, user.ReportCard, role, group, _passwordHashSettings.HashKey);

            try
            {
                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();
                return Created("", newUser.ToUserDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        //POST: api/users/auth/signin
        [HttpPost("auth/SignIn")]
        public async Task<IActionResult> Signin([FromForm] string username, [FromForm] string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return BadRequest("Введите логин и пароль");
            }
            var identity = await GetIdentity(username, password);
            if (identity == null)
            {
                return NotFound("Неверный логин или пароль");
            }

            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                token = encodedJwt,
                user_id = identity.Name
            };

            return Ok(response);
        }


        //GET: api/users/auth/GetUserRole
        [Authorize]
        [HttpGet("auth/GetUserRole")]
        public async Task<IActionResult> GetUserRole()
        {
            var find = await _context.Users.Include(c => c.Role).FirstOrDefaultAsync(c => c.Id == new Guid(User.Identity.Name));
            var role = find.Role.Name;
            if (role == null)
                return NotFound();
            else
                return Ok(role);
        }


        //POST: api/users/current/ChangeEmail
        [Authorize]
        [HttpPost("current/ChangeEmail")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmail changeEmail)
        {
            var find = await _context.Users.FindAsync(new Guid(User.Identity.Name));
            var findEmail = await _context.Users.FirstOrDefaultAsync(c => c.Email == changeEmail.NewEmail);
            if (findEmail != null)
            {
                return BadRequest("Пользователь с такой почтой уже зарегестрирован");
            }
            find.Email = changeEmail.NewEmail;
            await _context.SaveChangesAsync();

            return Ok();
        }
        

        //POST: api/users/auth/ForgotPassword
        [HttpPost("auth/ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromForm] string username)
        {
            var find = await _context.Users.Where(c => c.UserName == username).FirstOrDefaultAsync();
            if (find == null)
            {
                return NotFound("Пользователь не найден");
            }
            if (string.IsNullOrEmpty(find.Email))
            {
                return BadRequest("У вас нет электронной почты. Обратитесь к администратору");
            }
            ForgotPassword forgotPassword = new ForgotPassword(find.Id, DateTime.Now.AddHours(2));
            var code = Convert.ToBase64String(SerializeForgotPassword(forgotPassword));

            await Auxiliary.SendEmailAsync(find.Email, "Восстановление пароля", $"Для восстановления пароля перейдите по ссылке <br> http://localhost:3000/auth/ResetPassword/{code}", _mailSettings);

            return Ok("На вашу почту отправлено письмо с инструкцией");
        }

        //POST: api/users/auth/ResetPassword
        [HttpPost("auth/ResetPassword/{code}")]
        public async Task<IActionResult> ResetPassword(string code, [FromForm] ResetPassword resetPassword)
        {
            if (resetPassword.NewPassword != resetPassword.ConfimPassword)
                return BadRequest("Пароли не совпадают");
            ForgotPassword forgotPassword = new ForgotPassword();
            try
            {
                forgotPassword = DesserializeForgotPassword(Convert.FromBase64String(code));
            }
            catch
            {
                return BadRequest("Неверная ссылка");
            }

            if (forgotPassword.DateTime <= DateTime.Now)
            {
                return BadRequest("Время жизни ссылки истекло. Повторите запрос сброса пароля");
            }

            var find = await _context.Users.FindAsync(forgotPassword.Id);
            find.Password = Auxiliary.GenerateHashPassword(resetPassword.NewPassword, _passwordHashSettings.HashKey);
            await _context.SaveChangesAsync();
            
            return Ok("Пароль успешно изменён. Можете перейти ко входу");
        }

        //POST: api/users/auth/ResetPasswordSelf
        [Authorize]
        [HttpPost("auth/ResetPasswordSelf")]
        public async Task<IActionResult> ResetPasswordSelf([FromBody] ResetPasswordSelf resetPassword)
        {
            if (resetPassword.NewPassword != resetPassword.ConfimPassword)
                return BadRequest("Пароли не совпадают");
            var find = await _context.Users.FindAsync(new Guid(User.Identity.Name));
            if (Auxiliary.GenerateHashPassword(resetPassword.OldPassword, _passwordHashSettings.HashKey) != find.Password)
            {
                return BadRequest("Неверный старый пароль");
            }

            find.Password = Auxiliary.GenerateHashPassword(resetPassword.NewPassword, _passwordHashSettings.HashKey);

            if (!string.IsNullOrEmpty(find.Email))
            {
                await Auxiliary.SendEmailAsync(find.Email, "Сброс пароля", "Вы изменили пароль в личном кабинете. Если это были не вы - выполните сброс пароля по почте", _mailSettings);
            }
             
            return Ok();
        }
         
        // DELETE: api/Users/5
        [Authorize(Roles = "root, admin, network_editor")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return Ok(user.ToUserDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }
        
        
        private async Task<ClaimsIdentity> GetIdentity(string username, string password)
        {
            var task = await Task.Run(async () =>
            {
                var user = await _context.Users.Include(c => c.Role)
                    .FirstOrDefaultAsync(x => x.UserName == username && x.Password == Auxiliary.GenerateHashPassword(password, _passwordHashSettings.HashKey));
                if (user == null) return null;
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Id.ToString()),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.Name)
                };
                ClaimsIdentity claimsIdentity =
                    new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                        ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            });

            return task;
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        private byte[] SerializeForgotPassword(ForgotPassword forgotPassword)
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(forgotPassword.Id.ToString());
                    writer.Write(Convert.ToString(forgotPassword.DateTime));
                }
                return m.ToArray();
            }
        }

        private ForgotPassword DesserializeForgotPassword(byte[] data)
        {
            ForgotPassword result = new ForgotPassword();
            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    result.Id = new Guid(reader.ReadString());
                    result.DateTime = Convert.ToDateTime(reader.ReadString());
                }
            }
            return result;
        }
    }
}
