using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudService_API.Data;
using CloudService_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CloudService_API.Controllers
{
    [Route("api/[controller]")]
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

        // GET: Discipline/LaboratoryWorks
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

        // GET: Discipline/LaboratoryWorks/5
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

        // PUT: Discipline/LaboratoryWorks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLaboratoryWork(Guid id, LaboratoryWorkDTO laboratoryWorkDto)
        {
            if (id != laboratoryWorkDto.Id)
            {
                return BadRequest();
            }

            _context.Entry(laboratoryWorkDto).State = EntityState.Modified;
            var find = await _context.LaboratoryWorks.FindAsync(id);
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

        // POST: Discipline/LaboratoryWorks
        [HttpPost]
        public async Task<ActionResult<LaboratoryWorkDTO>> PostLaboratoryWork(LaboratoryWorkDTO laboratoryWorkDto)
        {
            LaboratoryWork newlaboratoryWork = new LaboratoryWork(laboratoryWorkDto.Name, laboratoryWorkDto.OwnerId, laboratoryWorkDto.DisciplineId);

            try
            {
                await _context.AddAsync(newlaboratoryWork);
                await _context.SaveChangesAsync();
                return newlaboratoryWork.ToLaboratoryWorkDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        // DELETE: Discipline/LaboratoryWorks/5
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
