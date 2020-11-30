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
    public class GroupUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GroupUsersController> _logger;

        public GroupUsersController(ApplicationDbContext context, ILogger<GroupUsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/GroupUsers
        [Authorize(Roles = "root, admin, network_editor")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupUserDTO>>> GetGroupUsers()
        {
            var find = await _context.GroupUsers.ToListAsync();
            List<GroupUserDTO> userDtos = new List<GroupUserDTO>();
            foreach (var item in find)
            {
                userDtos.Add(item.ToGroupUserDto());
            }

            return userDtos;
        }

        // GET: api/GroupUsers/5
        [Authorize(Roles = "root, admin, network_editor")]
        [HttpGet("{id}")]
        public async Task<ActionResult<GroupUserDTO>> GetGroupUser(Guid id)
        {
            var find = await _context.GroupUsers.FindAsync(id);

            if (find == null)
            {
                return NotFound();
            }

            return find.ToGroupUserDto();
        }

        // PUT: api/GroupUsers/5
        [Authorize(Roles = "root, admin, network_editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGroupUser(Guid id, GroupUserDTO groupUsersDto)
        {
            if (id != groupUsersDto.Id)
            {
                return BadRequest();
            }

            var find = await _context.GroupUsers.FindAsync(id);
            _context.Entry(groupUsersDto).State = EntityState.Modified;
            find.GroupId = groupUsersDto.GroupId;
            find.UserId = groupUsersDto.UserId;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!GroupExists(id))
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

        // POST: api/GroupUsers
        [Authorize(Roles = "root, admin, network_editor")]
        [HttpPost]
        public async Task<ActionResult<GroupUserDTO>> PostGroupUser(GroupUserDTO groupUsersDto)
        {
            GroupUser groupUser = new GroupUser(groupUsersDto.UserId, groupUsersDto.GroupId);
            try
            {
                await _context.GroupUsers.AddAsync(groupUser);
                await _context.SaveChangesAsync();
                return Created("", groupUser.ToGroupUserDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        // DELETE: api/GroupUsers/5
        [Authorize(Roles = "root, admin, network_editor")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<GroupUserDTO>> DeleteGroupUser(Guid id)
        {
            var groupUser = await _context.GroupUsers.FindAsync(id);
            if (groupUser == null)
            {
                return NotFound();
            }

            _context.GroupUsers.Remove(groupUser);
            await _context.SaveChangesAsync();

            return groupUser.ToGroupUserDto();
        }

        private bool GroupExists(Guid id)
        {
            return _context.GroupUsers.Any(e => e.Id == id);
        }
    }
}
