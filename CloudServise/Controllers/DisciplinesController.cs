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
    public class DisciplinesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DisciplinesController> _logger;

        public DisciplinesController(ApplicationDbContext context, ILogger<DisciplinesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Disciplines
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DisciplineDTO>>> GetDisciplines()
        {
            var find = await _context.Disciplines.ToListAsync();
            List<DisciplineDTO> disciplineDtos = new List<DisciplineDTO>();
            foreach (var item in find)
            {
                disciplineDtos.Add(item.ToDisciplineDto());
            }

            return disciplineDtos;
        }

        //GET: api/Disciplines/WithPage
        [Authorize]
        [HttpGet("WithPage")]
        public async Task<IActionResult> GetDisciplinesWithPage([FromQuery] DisciplineParametres disciplineParametres)
        {
            var find = await _context.Disciplines
                .Where(s =>
               (EF.Functions.Like(s.Id.ToString(), $"%{disciplineParametres.Text}%") ||
                EF.Functions.Like(s.Name, $"%{disciplineParametres.Text}%")
               ) &&
                EF.Functions.Like(s.OwnerId.ToString(), $"%{disciplineParametres.OwnerId}%")
              ).ToListAsync();

            disciplineParametres.TotalCount = find.Count;
            if (!disciplineParametres.Check())
                return NoContent();
            Response.Headers.Add("X-Pagination", disciplineParametres.PaginationToJson());
            List<DisciplineDTO> dtos = new List<DisciplineDTO>();
            foreach (var item in find)
            {
                dtos.Add(item.ToDisciplineDto());
            }
            return Ok(dtos);
        }


        // GET: api/Disciplines/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<DisciplineDTO>> GetDiscipline(Guid id)
        {
            var discipline = await _context.Disciplines.FindAsync(id);

            if (discipline == null)
            {
                return NotFound();
            }

            return discipline.ToDisciplineDto();
        }

        [Authorize]
        [HttpGet("GetStudentDiscipline")]
        public async Task<IActionResult> GetStudentDiscipline([FromQuery] string search)
        {
            var groupId = await _context.Users
                .Include(c => c.Group)
                .Where(c => c.Id.Equals(new Guid(User.Identity.Name)))
                .Select(c => c.Group.Id)
                .FirstOrDefaultAsync();
            var discipline = await _context.DisciplineGroupTeacher
                .Include(c => c.Discipline)
                .Include(c => c.Group)
                .Where(c => c.Group.Id.Equals(groupId) &&
                        (EF.Functions.Like(c.Discipline.Name, $"%{search}%") ||
                        EF.Functions.Like(c.Discipline.ShortName, $"%{search}%")))
                .Select(c => c.Discipline.ToDisciplineDto())
                .ToListAsync();

            if (discipline == null)
            {
                return NotFound();
            }

            return Ok(discipline);
        }

        [Authorize]
        [HttpGet("GetTeacherDiscipline")]
        public async Task<IActionResult> GetTeacherDiscipline([FromQuery] string search)
        {
            var discipline = await _context.DisciplineGroupTeacher
                .Include(c => c.Discipline)
                .Include(c => c.Group)
                .Include(c => c.Teacher)
                .Where(c => c.Teacher.Id.Equals(new Guid(User.Identity.Name)) &&
                        EF.Functions.Like(c.Discipline.Name, $"%{search}%") ||
                        EF.Functions.Like(c.Discipline.ShortName, $"%{search}%") ||
                        EF.Functions.Like(c.Group.Name, $"%{search}%"))
                .Select(c => new 
                {
                    discipline = c.Discipline.ToDisciplineDto(),
                    group = c.Group.ToGroupDto()
                })
                .ToListAsync();

            if (discipline == null)
            {
                return NotFound();
            }

            return Ok(discipline);
        }

        // PUT: api/Disciplines/5
        [Authorize(Roles = "root, admin, network_editor, teacher")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDiscipline(Guid id, DisciplineDTO discipline)
        {
            if (id != discipline.Id)
            {
                return BadRequest();
            }

            var find = await _context.Disciplines.FindAsync(id);
            _context.Entry(find).State = EntityState.Modified;
            find.Name = discipline.Name;
            find.OwnerId = discipline.OwnerId;
            find.ShortName = discipline.ShortName;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!DisciplineExists(id))
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

        // POST: api/Disciplines
        [Authorize(Roles = "root, admin, network_editor, teacher")]
        [HttpPost]
        public async Task<ActionResult<DisciplineDTO>> PostDiscipline(CreateDisciplineDTO disciplineDto)
        {
            Discipline discipline = new Discipline(Guid.NewGuid(), disciplineDto.Name, new Guid(User.Identity.Name), disciplineDto.ShortName);
            DisciplineGroupTeacher disciplineGroupTeacher =
                new DisciplineGroupTeacher(discipline.Id, disciplineDto.GroupId, new Guid(User.Identity.Name));
            try
            {
                await _context.Disciplines.AddAsync(discipline);
                await _context.DisciplineGroupTeacher.AddAsync(disciplineGroupTeacher);
                await _context.SaveChangesAsync();
                return Created("", discipline.ToDisciplineDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        // DELETE: api/Disciplines/5
        [Authorize(Roles = "root, admin, network_editor, teacher")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<DisciplineDTO>> DeleteDiscipline(Guid id)
        {
            var discipline = await _context.Disciplines.FindAsync(id);
            if (discipline == null)
            {
                return NotFound();
            }

            _context.Disciplines.Remove(discipline);
            await _context.SaveChangesAsync();

            return discipline.ToDisciplineDto();
        }

        private bool DisciplineExists(Guid id)
        {
            return _context.Disciplines.Any(e => e.Id == id);
        }
    }
}
