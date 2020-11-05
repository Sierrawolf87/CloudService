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

        // GET: api/Disciplines/5
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

        // PUT: api/Disciplines/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDiscipline(Guid id, DisciplineDTO discipline)
        {
            if (id != discipline.Id)
            {
                return BadRequest();
            }

            var find = await _context.Disciplines.FindAsync(id);
            _context.Entry(discipline).State = EntityState.Modified;
            find.Name = discipline.Name;
            find.CreatorId = discipline.CreatorId;
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
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<DisciplineDTO>> PostDiscipline(DisciplineDTO disciplineDto)
        {
            Discipline discipline =new Discipline(disciplineDto.Name, disciplineDto.CreatorId);
            try
            {
                await _context.Disciplines.AddAsync(discipline);
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
