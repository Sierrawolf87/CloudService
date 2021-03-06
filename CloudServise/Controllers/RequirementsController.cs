﻿using System;
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
    [Route("api/Disciplines/LaboratoryWorks/[controller]")]
    [ApiController]
    public class RequirementsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RequirementsController> _logger;

        public RequirementsController(ApplicationDbContext context, ILogger<RequirementsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Disciplines/LaboratoryWorks/Requirements
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequirementDTO>>> GetRequirements()
        {
            var find = await _context.Requirements.Include(c => c.Files).ToListAsync();
            List<RequirementDTO> dtos = new List<RequirementDTO>();
            foreach (var item in find)
            {
                dtos.Add(item.ToRequirementDTO());
            }

            return dtos;
        }

        //GET: api/Disciplines/LaboratoryWorks/Requirements/WithPage
        [Authorize(Roles = "root, admin, network_editor, teacher")]
        [HttpGet("WithPage")]
        public async Task<IActionResult> GetRequirementsWithPage([FromQuery] RequirementParameters requirementParameters)
        {
            var find = await _context.Requirements.Include(c => c.Files)
                .Where(s =>
               (EF.Functions.Like(s.Id.ToString(), $"%{requirementParameters.Text}%") ||
                EF.Functions.Like(s.Description, $"%{requirementParameters.Text}%")
               ) &&
                EF.Functions.Like(s.LaboratoryWorkId.ToString(), $"%{requirementParameters.LaboratoryWorkId}%")
              ).ToListAsync();

            requirementParameters.TotalCount = find.Count;
            if (!requirementParameters.Check())
                return NoContent();
            Response.Headers.Add("X-Pagination", requirementParameters.PaginationToJson());
            List<RequirementDTO> dtos = new List<RequirementDTO>();
            foreach (var item in find)
            {
                dtos.Add(item.ToRequirementDTO());
            }
            return Ok(dtos);
        }

        // GET: api/Disciplines/LaboratoryWorks/Requirements/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<RequirementDTO>> GetRequirement(Guid id)
        {
            var requirement = await _context.Requirements.Include(c => c.Files).FirstOrDefaultAsync(f => f.Id == id);

            if (requirement == null)
            {
                return NotFound();
            }

            return requirement.ToRequirementDTO();
        }

        // GET: api/Disciplines/LaboratoryWorks/Requirements/GetByLaboratory/5
        [Authorize]
        [HttpGet("GetByLaboratory/{id}")]
        public async Task<ActionResult<RequirementDTO>> GetByLaboratory(Guid id)
        {
            var requirement = await _context.Requirements
                .Include(c => c.Files)
                .Where(c => c.LaboratoryWorkId == id)
                .FirstOrDefaultAsync();

            if (requirement == null)
            {
                var newRequirement = new CreateRequirementDTO
                {
                    Id = Guid.NewGuid(),
                    Description = "",
                    LaboratoryWorkId = id
                };
                await PostRequirement(newRequirement);
                await _context.SaveChangesAsync();
                requirement = await _context.Requirements
                .Include(c => c.Files)
                .Where(c => c.LaboratoryWorkId == id)
                .FirstOrDefaultAsync();
            }

            return requirement.ToRequirementDTO();
        }

        // PUT: api/Disciplines/LaboratoryWorks/Requirements/5
        [Authorize(Roles = "root, admin, network_editor, teacher")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRequirement(Guid id, Requirement requirement)
        {
            if (id != requirement.Id)
            {
                return BadRequest();
            }

            _context.Entry(requirement).State = EntityState.Modified;
            var find = await _context.Requirements.FindAsync(id);
            find.Description = requirement.Description;
            find.LaboratoryWorkId = requirement.LaboratoryWorkId;
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!RequirementExists(id))
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

        // POST: api/Disciplines/LaboratoryWorks/Requirements
        [Authorize(Roles = "root, admin, network_editor, teacher")]
        [HttpPost]
        public async Task<ActionResult<RequirementDTO>> PostRequirement(CreateRequirementDTO requirement)
        {
            Requirement newRequirement = new Requirement(requirement.Description, requirement.LaboratoryWorkId);
            try
            {
                await _context.Requirements.AddAsync(newRequirement);
                await _context.SaveChangesAsync();
                return Ok(newRequirement.ToRequirementDTO());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500);
            }
        }

        // DELETE: api/Disciplines/LaboratoryWorks/Requirements/5
        [Authorize(Roles = "root, admin, network_editor, teacher")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<RequirementDTO>> DeleteRequirement(Guid id)
        {
            var requirement = await _context.Requirements.FindAsync(id);
            if (requirement == null)
            {
                return NotFound();
            }

            _context.Requirements.Remove(requirement);
            await _context.SaveChangesAsync();

            return requirement.ToRequirementDTO();
        }

        private bool RequirementExists(Guid id)
        {
            return _context.Requirements.Any(e => e.Id == id);
        }
    }
}
