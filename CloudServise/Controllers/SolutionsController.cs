﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudService_API.Data;
using CloudService_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CloudService_API.Controllers
{
    [Route("api/Disciplines/LaboratoryWorks/[controller]")]
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

        // GET: api/Disciplines/LaboratoryWorks/Solutions
        [Authorize(Roles = "root, admin, network_editor, teacher")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SolutionDTO>>> GetSolutions()
        {
            var find = await _context.Solutions.Include(c => c.Files).ToListAsync();
            List<SolutionDTO> dtos = new List<SolutionDTO>();
            foreach (var item in find)
            {
                dtos.Add(item.ToSolutionDto());
            }

            return dtos;
        }

        //GET: api/Disciplines/LaboratoryWorks/Solutions/WithPage
        [Authorize(Roles = "root, admin, network_editor, teacher")]
        [HttpGet("WithPage")]
        public async Task<IActionResult> GetSolutionsWithPage([FromQuery] SolutionParameters solutionParameters)
        {
            var find = await _context.Solutions.Include(c => c.Files)
                .Where(s =>
               (EF.Functions.Like(s.Id.ToString(), $"%{solutionParameters.Text}%") ||
                EF.Functions.Like(s.Description, $"%{solutionParameters.Text}%")
               ) &&
                EF.Functions.Like(s.LaboratoryWorkId.ToString(), $"%{solutionParameters.LaboratoryWorkId}%") &&
                EF.Functions.Like(s.OwnerId.ToString(), $"%{solutionParameters.OwnerId}%")
              ).ToListAsync();

            solutionParameters.TotalCount = find.Count;
            if (!solutionParameters.Check())
                return NoContent();
            Response.Headers.Add("X-Pagination", solutionParameters.PaginationToJson());
            List<SolutionDTO> dtos = new List<SolutionDTO>();
            foreach (var item in find)
            {
                dtos.Add(item.ToSolutionDto());
            }
            return Ok(dtos);
        }


        // GET: api/Disciplines/LaboratoryWorks/Solutions/5
        [Authorize(Roles = "root, admin, network_editor, teacher")]
        [HttpGet("{id}")]
        public async Task<ActionResult<SolutionDTO>> GetSolution(Guid id)
        {
            var solution = await _context.Solutions.Include(c => c.Files).FirstOrDefaultAsync(f => f.Id == id);

            if (solution == null)
            {
                return NotFound();
            }

            return solution.ToSolutionDto();
        }

        [Authorize(Roles = "student")]
        [HttpGet("{id}/GetMySolution")]
        public async Task<ActionResult<SolutionDTO>> GetMySolution(Guid id)
        {
            var solution = await _context.Solutions.Include(c => c.LaboratoryWork).Include(c => c.Files)
                .Where(c => c.LaboratoryWorkId == id && c.OwnerId == new Guid(User.Identity.Name)).FirstOrDefaultAsync();

            if (solution == null)
            {
                var newSolution = new CreateSolutionDTO
                {
                    Id = Guid.NewGuid(),
                    Description = "",
                    LaboratoryWorkId = id,
                    Mark = 0,
                };
                await PostSolution(newSolution);
                await _context.SaveChangesAsync();
                solution = await _context.Solutions.Include(c => c.LaboratoryWork).Include(c => c.Files)
                .Where(c => c.LaboratoryWorkId == id && c.OwnerId == new Guid(User.Identity.Name)).FirstOrDefaultAsync();
            }

            return Ok(solution.ToSolutionDto());
        }

        // PUT: api/Disciplines/LaboratoryWorks/Solutions/5
        [Authorize(Roles = "root, admin, network_editor, teacher, student")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSolution(Guid id, SolutionDTO solution)
        {
            if (id != solution.Id)
            {
                return BadRequest();
            }

            var find = await _context.Solutions.FindAsync(id);
            _context.Entry(find).State = EntityState.Modified;
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

        [Authorize(Roles = "root, admin, network_editor, teacher, student")]
        [HttpPut("{id}/UdpateMark/{mark}")]
        public async Task<IActionResult> UdpateMark(Guid id, int mark)
        {
            var find = await _context.Solutions.FindAsync(id);
            _context.Entry(find).State = EntityState.Modified;
            find.Mark = mark;
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

        // POST: api/Disciplines/LaboratoryWorks/Solutions
        [Authorize(Roles = "root, admin, network_editor, student")]
        [HttpPost]
        public async Task<ActionResult<SolutionDTO>> PostSolution(CreateSolutionDTO solution)
        {
            Solution newSolution = new Solution(solution.Description, new Guid(User.Identity.Name), solution.Mark, solution.LaboratoryWorkId);
            try
            {
                await _context.Solutions.AddAsync(newSolution);
                await _context.SaveChangesAsync();
                return Ok(newSolution.ToSolutionDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        // DELETE: api/Disciplines/LaboratoryWorks/Solutions/5
        [Authorize(Roles = "root, admin, network_editor, teacher, student")]
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
