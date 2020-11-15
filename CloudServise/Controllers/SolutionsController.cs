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
    public class SolutionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SolutionsController> _logger;
        
        public SolutionsController(ApplicationDbContext context, ILogger<SolutionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Solutions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SolutionDTO>>> GetSolutions()
        {
            var find = await _context.Solutions.ToListAsync();
            List<SolutionDTO> dtos = new List<SolutionDTO>();
            foreach (var item in find)
            {
                dtos.Add(item.ToSolutionDto());
            }

            return dtos;
        }

        // GET: api/Solutions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SolutionDTO>> GetSolution(Guid id)
        {
            var solution = await _context.Solutions.FindAsync(id);

            if (solution == null)
            {
                return NotFound();
            }

            return solution.ToSolutionDto();
        }

        // PUT: api/Solutions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSolution(Guid id, SolutionDTO solution)
        {
            if (id != solution.Id)
            {
                return BadRequest();
            }

            _context.Entry(solution).State = EntityState.Modified;
            var find = await _context.Solutions.FindAsync(id);
            find.Description = solution.Description;
            find.LaboratoryWorkId = solution.LaboratoryWorkId;
            find.Mark = solution.Mark;
            find.OwnerId = solution.OwnerId;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SolutionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex.Message);
                    return StatusCode(500);
                }
            }

            return Ok();
        }

        // POST: api/Solutions
        [HttpPost]
        public async Task<ActionResult<SolutionDTO>> PostSolution(CreateSolutionDTO solution)
        {
            Solution newsolution = new Solution(solution.Description, solution.OwnerId, solution.Mark, solution.LaboratoryWorkId);
            try
            {
                await _context.Solutions.AddAsync(newsolution);
                await _context.SaveChangesAsync();
                return Ok(newsolution.ToSolutionDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        // DELETE: api/Solutions/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<SolutionDTO>> DeleteSolution(Guid id)
        {
            var solution = await _context.Solutions.FindAsync(id);
            if (solution == null)
            {
                return NotFound();
            }

            _context.Solutions.Remove(solution);
            await _context.SaveChangesAsync();

            return solution.ToSolutionDto();
        }

        private bool SolutionExists(Guid id)
        {
            return _context.Solutions.Any(e => e.Id == id);
        }
    }
}
