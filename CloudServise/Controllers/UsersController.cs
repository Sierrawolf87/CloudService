using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CloudService_API.Data;
using CloudService_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CloudService_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;
        private readonly PasswordHashSettings _passwordHashSettings;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger, PasswordHashSettings passwordHashSettings)
        {
            _context = context;
            _logger = logger;
            _passwordHashSettings = passwordHashSettings;
        }

        // GET: api/Users
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            List<UserDTO> userDtos = new List<UserDTO>();
            var actionResult = await _context.Users.Include(c => c.Role).ToListAsync();
            foreach (var user in actionResult)
            {
                userDtos.Add(user.ToUserDto());
            }
            return userDtos;
        }


        // GET: api/Users/5
            [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(Guid id)
        {
            var user = await _context.Users.Include(c => c.Role).FirstOrDefaultAsync(i => i.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return user.ToUserDto();
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(Guid id, UserDTO user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }
            
            var find = await _context.Users.FindAsync(id);
            _context.Entry(user).State = EntityState.Modified;

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

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserDTO>> PostUser(UserRegisterDTO user)
        {
            var role = await _context.Roles.FindAsync(user.Role.Id);
            if (role == null)
            {
                return BadRequest();
            }

            var newUser = new User(user.Name, user.Surname, user.Patronymic, user.ReportCard, role, _passwordHashSettings.HashKey);

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

        // DELETE: api/Users/5
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
        
        [HttpPost("GetToken")]
        public async Task<IActionResult> GetToken([FromForm] string username, [FromForm] string password)
        {
            var identity = await GetIdentity(username, password);
            if (identity == null)
            {
                return BadRequest(new { errorText = "Invalid username or password." });
            }

            var now = DateTime.UtcNow;
            // создаем JWT-токен
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
                access_token = encodedJwt,
                username = identity.Name
            };

            return Ok(response);
        }

        private async Task<ClaimsIdentity> GetIdentity(string username, string password)
        {
            var task = await Task.Run(() =>
            {
                var user = _context.Users.Include(c => c.Role)
                    .FirstOrDefault(x => x.UserName == username && x.Password == Auxiliary.GenerateHashPassword(password, _passwordHashSettings.HashKey));
                if (user == null) return null;
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserName),
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
    }
}
