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
    [Route("api/Disciplines/[controller]")]
    [ApiController]
    public class LaboratoryWorksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LaboratoryWorksController> _logger;

        public LaboratoryWorksController(ApplicationDbContext context, ILogger<LaboratoryWorksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Disciplines/LaboratoryWorks
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LaboratoryWorkDTO>>> GetLaboratoryWorks()
        {
            var find = await _context.LaboratoryWorks.ToListAsync();
            List<LaboratoryWorkDTO> dtos = new List<LaboratoryWorkDTO>();
            foreach (var item in find)
            {
                dtos.Add(item.ToLaboratoryWorkDto());
            }
            return dtos;
        }

        //GET: api/Disciplines/LaboratoryWorks/WithPage
        [Authorize(Roles = "root, admin, network_editor, teacher")]
        [HttpGet("WithPage")]
        public async Task<IActionResult> GetLaboratoryWorksWithPage([FromQuery] LaboratoryWorkParameters laboratoryWorkParameters)
        {
            var find = await _context.LaboratoryWorks
                .Where(s =>
               (EF.Functions.Like(s.Id.ToString(), $"%{laboratoryWorkParameters.Text}%") ||
                EF.Functions.Like(s.Name, $"%{laboratoryWorkParameters.Text}%")
               ) &&
                EF.Functions.Like(s.DisciplineId.ToString(), $"%{laboratoryWorkParameters.DisciplineId}%") &&
                EF.Functions.Like(s.OwnerId.ToString(), $"%{laboratoryWorkParameters.OwnerId}%")
              ).ToListAsync();

            laboratoryWorkParameters.TotalCount = find.Count;
            if (!laboratoryWorkParameters.Check())
                return NoContent();
            Response.Headers.Add("X-Pagination", laboratoryWorkParameters.PaginationToJson());
            List<LaboratoryWorkDTO> dtos = new List<LaboratoryWorkDTO>();
            foreach (var item in find)
            {
                dtos.Add(item.ToLaboratoryWorkDto());
            }
            return Ok(dtos);
        }

        // GET: Disciplines/LaboratoryWorks/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<LaboratoryWorkDTO>> GetLaboratoryWork(Guid id)
        {
            var laboratoryWork = await _context.LaboratoryWorks.FindAsync(id);

            if (laboratoryWork == null)
            {
                return NotFound();
            }

            return laboratoryWork.ToLaboratoryWorkDto();
        }

        [Authorize(Roles = "student")]
        [HttpGet("GetLaboratoryListByDisciplineStudent/{id}")]
        public async Task<IActionResult> GetLaboratoryListByDisciplineStudent(Guid id, [FromQuery]string search)
        {
            var marks = await _context.Solutions
                .Where(c => c.OwnerId == new Guid(User.Identity.Name))
                .Select(c => new
                {
                    laboratoryWorkId = c.LaboratoryWorkId,
                    mark = c.Mark
                })
                .ToListAsync();
            var laboratoryList = await _context.LaboratoryWorks
                .Include(c => c.Discipline)
                .Where(c => c.DisciplineId == id &&
                    EF.Functions.Like(c.Name, $"%{search}%"))
                .ToListAsync();
            var requirement = await _context.Requirements.ToListAsync();
            var result = laboratoryList.Select(c => new
            {
                laboratory = c.ToLaboratoryWorkDto(),
                discipline = c.Discipline.ToDisciplineDto(),
                requirement = requirement.Where(r => r.LaboratoryWorkId == c.Id).Select(r => r.Description).FirstOrDefault(),
                mark = marks.Where(m => m.laboratoryWorkId == c.Id).Select(m => m.mark).FirstOrDefault()
            })
                .ToList();
            return Ok(result);
        }

        [Authorize(Roles = "teacher")]
        [HttpGet("GetLaboratoryListByDisciplineTeacher/{id}")]
        public async Task<IActionResult> GetLaboratoryListByDisciplineTeacher(Guid id, [FromQuery] string search)
        {
            var marks = await _context.Solutions
                .Select(c => new
                {
                    laboratoryWorkId = c.LaboratoryWorkId,
                    mark = c.Mark
                })
                .ToListAsync();
            var laboratoryList = await _context.LaboratoryWorks
                .Include(c => c.Discipline)
                .Where(c => c.DisciplineId == id &&
                    EF.Functions.Like(c.Name, $"%{search}%"))
                .OrderBy(c => c.Name)
                .ToListAsync();
            var requirement = await _context.Requirements.ToListAsync();
            var solutions = await _context.Solutions.ToListAsync();
            var result = laboratoryList.Select(c => new
            {
                laboratory = c.ToLaboratoryWorkDto(),
                discipline = c.Discipline.ToDisciplineDto(),
                requirement = requirement.Where(r => r.LaboratoryWorkId == c.Id).Select(r => r.Description).FirstOrDefault(),
                solutionsCount = solutions.Count(s => s.LaboratoryWorkId == c.Id)
                //,markAvg = Math.Round(marks.Where(m => m.laboratoryWorkId == c.Id).Select(m => m.mark).Average(), 1),
            })
                .ToList();
            return Ok(result);
        }

        [Authorize(Roles = "teacher")]
        [HttpGet("GetStudentLaboratoryListForTeatcher/{id}")]
        public async Task<IActionResult> GetStudentLaboratoryListForTeatcher(Guid id, [FromQuery] string search)
        {
            var users = await _context.Users.ToListAsync();
            var solutionsList = await _context.Solutions
                .Where(c => c.LaboratoryWorkId.Equals(id))
                .ToListAsync();
            var result = solutionsList.Select(c => new
            {
                initials = users.FirstOrDefault(f => f.Id.Equals(c.OwnerId)).ToUserDto().Initials,
                solutionId = c.Id,
                laboratoryWorkId = c.LaboratoryWorkId,
                mark = c.Mark
            })
                .ToList();
            return Ok(result);
        }

        // PUT: Disciplines/LaboratoryWorks/5
        [Authorize(Roles = "root, admin, network_editor, teacher")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLaboratoryWork(Guid id, LaboratoryWorkDTO laboratoryWorkDto)
        {
            if (id != laboratoryWorkDto.Id)
            {
                return BadRequest();
            }

            var find = await _context.LaboratoryWorks.FindAsync(id);
            _context.Entry(find).State = EntityState.Modified;
            find.Name = laboratoryWorkDto.Name;
            find.DisciplineId = laboratoryWorkDto.DisciplineId;
            find.OwnerId = laboratoryWorkDto.OwnerId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!LaboratoryWorkExists(id))
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

        // POST: Disciplines/LaboratoryWorks
        [Authorize(Roles = "root, admin, network_editor, teacher")]
        [HttpPost]
        public async Task<ActionResult<LaboratoryWorkDTO>> PostLaboratoryWork(CreateLaboratoryWorkDTO laboratoryWorkDto)
        {
            LaboratoryWork newLaboratoryWork = new LaboratoryWork(Guid.NewGuid(), laboratoryWorkDto.Name, new Guid(User.Identity.Name), laboratoryWorkDto.DisciplineId);
            try
            {
                await _context.AddAsync(newLaboratoryWork);
                await _context.SaveChangesAsync();
                return newLaboratoryWork.ToLaboratoryWorkDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        // DELETE: Disciplines/LaboratoryWorks/5
        [Authorize(Roles = "root, admin, network_editor, teacher")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<LaboratoryWorkDTO>> DeleteLaboratoryWork(Guid id)
        {
            var laboratoryWork = await _context.LaboratoryWorks.FindAsync(id);
            if (laboratoryWork == null)
            {
                return NotFound();
            }

            _context.LaboratoryWorks.Remove(laboratoryWork);
            await _context.SaveChangesAsync();

            return Ok(laboratoryWork.ToLaboratoryWorkDto());
        }

        private bool LaboratoryWorkExists(Guid id)
        {
            return _context.LaboratoryWorks.Any(e => e.Id == id);
        }
    }
}
