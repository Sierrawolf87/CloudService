using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudService_API.Data;
using CloudService_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CloudService_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RolesController> _logger;

        public RolesController(ApplicationDbContext context, ILogger<RolesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Roles
        [Authorize(Roles = "root, admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDTO>>> GetRoles()
        {
            var find = await _context.Roles.ToListAsync();
            List<RoleDTO> roleDtos = new List<RoleDTO>();
            foreach (var item in find)
            {
                roleDtos.Add(item.ToRoleDTO());
            }

            return roleDtos;
        }

        // GET: api/Roles/5
        [Authorize(Roles = "root, admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDTO>> GetRole(Guid id)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            return role.ToRoleDTO();
        }

        // PUT: api/Roles/5
        [Authorize(Roles = "root, admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(Guid id, RoleDTO role)
        {
            if (id != role.Id)
            {
                return BadRequest();
            }


            var find = await _context.Roles.FindAsync(role.Id);
            _context.Entry(find).State = EntityState.Modified;
            find.Name = role.Name;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!RoleExists(id))
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

        // POST: api/Roles
        [Authorize(Roles = "root, admin")]
        [HttpPost]
        public async Task<ActionResult<Role>> PostRole(RoleDTO role)
        {
            var newRole = new Role(role.Name);
            
            try
            {
                await _context.Roles.AddAsync(newRole);
                await _context.SaveChangesAsync();
                return Created("", newRole.ToRoleDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        // DELETE: api/Roles/5
        [Authorize(Roles = "root, admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<RoleDTO>> DeleteRole(Guid id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return role.ToRoleDTO();
        }

        private bool RoleExists(Guid id)
        {
            return _context.Roles.Any(e => e.Id == id);
        }
    }
}
