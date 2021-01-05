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
    public class DisciplineGroupTeachersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DisciplineGroupTeachersController> _logger;

        public DisciplineGroupTeachersController(ApplicationDbContext context, ILogger<DisciplineGroupTeachersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/DisciplineGroupTeachers
        [Authorize(Roles = "root, admin, network_editor")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DisciplineGroupTeacherDTO>>> GetDisciplineGroupTeacher()
        {
            var find = await _context.DisciplineGroupTeacher.ToListAsync();
            List<DisciplineGroupTeacherDTO> dtos = new List<DisciplineGroupTeacherDTO>();
            foreach (var item in find)
            {
                dtos.Add(item.ToDisciplineGroupTeacherDTO());
            }

            return dtos;
        }

        //GET: api/DisciplineGroupTeachers/WithPage
        [Authorize]
        [HttpGet("WithPage")]
        public async Task<IActionResult> GetDisciplineGroupTeachersWithPage([FromQuery] DisciplineGroupTeacherParameters disciplineGroupTeacherParameters)
        {
            var find = await _context.DisciplineGroupTeacher
                .Where(s =>
                EF.Functions.Like(s.Id.ToString(), $"%{disciplineGroupTeacherParameters.Text}%")
                &&
                (EF.Functions.Like(s.DisciplineId.ToString(), $"%{disciplineGroupTeacherParameters.DisciplineId}%") &&
                 EF.Functions.Like(s.GroupId.ToString(), $"%{disciplineGroupTeacherParameters.GroupId}%") &&
                 EF.Functions.Like(s.TeacherId.ToString(), $"%{disciplineGroupTeacherParameters.TeacherId}%")
                )
              ).ToListAsync();

            disciplineGroupTeacherParameters.TotalCount = find.Count;
            if (!disciplineGroupTeacherParameters.Check())
                return NoContent();
            Response.Headers.Add("X-Pagination", disciplineGroupTeacherParameters.PaginationToJson());
            List<DisciplineGroupTeacherDTO> dtos = new List<DisciplineGroupTeacherDTO>();
            foreach (var item in find)
            {
                dtos.Add(item.ToDisciplineGroupTeacherDTO());
            }
            return Ok(dtos);
        }

        // GET: api/DisciplineGroupTeachers/5
        [Authorize(Roles = "root, admin, network_editor")]
        [HttpGet("{id}")]
        public async Task<ActionResult<DisciplineGroupTeacherDTO>> GetDisciplineGroupTeacher(Guid id)
        {
            var disciplineGroupTeacher = await _context.DisciplineGroupTeacher.FindAsync(id);

            if (disciplineGroupTeacher == null)
            {
                return NotFound();
            }

            return disciplineGroupTeacher.ToDisciplineGroupTeacherDTO();
        }

        // PUT: api/DisciplineGroupTeachers/5
        [Authorize(Roles = "root, admin, network_editor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDisciplineGroupTeacher(Guid id, DisciplineGroupTeacherDTO dto)
        {
            if (id != dto.Id)
            {
                return BadRequest();
            }

            var find = await _context.DisciplineGroupTeacher.FindAsync(id);
            _context.Entry(dto).State = EntityState.Modified;
            find.DisciplineId = dto.DisciplineId;
            find.GroupId = dto.GroupId;
            find.TeacherId = dto.TeacherId;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!DisciplineGroupTeacherExists(id))
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

        // POST: api/DisciplineGroupTeachers
        [Authorize(Roles = "root, admin, network_editor")]
        [HttpPost]
        public async Task<ActionResult<DisciplineGroupTeacherDTO>> PostDisciplineGroupTeacher(DisciplineGroupTeacherDTO dto)
        {
            DisciplineGroupTeacher disciplineGroupTeacher = new DisciplineGroupTeacher(dto.DisciplineId, dto.GroupId, dto.TeacherId);
            try
            {
                await _context.DisciplineGroupTeacher.AddAsync(disciplineGroupTeacher);
                await _context.SaveChangesAsync();
                return Created("", disciplineGroupTeacher.ToDisciplineGroupTeacherDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        // DELETE: api/DisciplineGroupTeachers/5
        [Authorize(Roles = "root, admin, network_editor")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<DisciplineGroupTeacherDTO>> DeleteDisciplineGroupTeacher(Guid id)
        {
            var disciplineGroupTeacher = await _context.DisciplineGroupTeacher.FindAsync(id);
            if (disciplineGroupTeacher == null)
            {
                return NotFound();
            }

            _context.DisciplineGroupTeacher.Remove(disciplineGroupTeacher);
            await _context.SaveChangesAsync();

            return disciplineGroupTeacher.ToDisciplineGroupTeacherDTO();
        }

        private bool DisciplineGroupTeacherExists(Guid id)
        {
            return _context.DisciplineGroupTeacher.Any(e => e.Id == id);
        }
    }
}
