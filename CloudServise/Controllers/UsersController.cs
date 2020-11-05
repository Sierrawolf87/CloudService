using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudServise_API.Data;
using CloudServise_API.Models;
using Microsoft.Extensions.Logging;

namespace CloudServise_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            List<UserDTO> userDtos = new List<UserDTO>();
            var actionResult = await _context.Users.ToListAsync();
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
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return user.ToUserDto();
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<UserDTO>> PostUser(User user)
        {
            var role = await _context.Roles.FindAsync(user.Role.Id);
            if (role == null)
            {
                return BadRequest();
            }

            var newUser = new User(user.Name, user.Surname, user.Patronymic, user.ReportCard, role);

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
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
