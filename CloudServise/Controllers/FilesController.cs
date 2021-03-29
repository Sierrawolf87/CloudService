using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using CloudService_API.Data;
using CloudService_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HeyRed.Mime;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using File = CloudService_API.Models.File;
using Microsoft.AspNetCore.Authorization;

namespace CloudService_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FilesController> _logger;
        private readonly FilePathSettings _filePathSettings;

        public FilesController(ApplicationDbContext context, ILogger<FilesController> logger, FilePathSettings filePathSettings)
        {
            _context = context;
            _logger = logger;
            _filePathSettings = filePathSettings;
        }

        // GET: api/Files
        [Authorize(Roles = "root, admin, network_editor")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FileDTO>>> GetFilesInfo()
        {
            var find = await _context.Files.Include(c => c.Solution).Include(c => c.Requirement).ToListAsync();
            List<FileDTO> dtos = new List<FileDTO>();
            foreach (var item in find)
            {
                dtos.Add(item.ToFileDto());
            }

            return dtos;
        }

        //GET: api/Files/WithPage
        [Authorize(Roles = "root, admin, network_editor")]
        [HttpGet("WithPage")]
        public async Task<IActionResult> GetFilesWithPage([FromQuery] FileParametres fileParametres)
        {
            var find = await _context.Files.Include(c => c.Solution).Include(c => c.Requirement)
                .Where(s =>
               (EF.Functions.Like(s.Id.ToString(), $"%{fileParametres.Text}%") ||
                EF.Functions.Like(s.Name, $"%{fileParametres.Text}%") 
               ) &&
                EF.Functions.Like(s.OwnerId.ToString(), $"%{fileParametres.OwnerId}%") &&
                EF.Functions.Like(s.Requirement.Id.ToString(), $"%{fileParametres.RequirementId}%") &&
                EF.Functions.Like(s.Solution.Id.ToString(), $"%{fileParametres.SolutionId}%")
              ).ToListAsync();

            fileParametres.TotalCount = find.Count;
            if (!fileParametres.Check())
                return NoContent();
            Response.Headers.Add("X-Pagination", fileParametres.PaginationToJson());
            List<FileDTO> dtos = new List<FileDTO>();
            foreach (var item in find)
            {
                dtos.Add(item.ToFileDto());
            }
            return Ok(dtos);
        }

        // GET: api/Files/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<FileDTO>> GetFileInfo(Guid id)
        {
            var file = await _context.Files.FindAsync(id);

            if (file == null)
            {
                return NotFound();
            }

            return file.ToFileDto();
        }

        //GET: api/Files/4/5
        // Возвращает файл из решения
        [HttpGet("DownloadSolutionFile/{solutionId}/{id}")]
        public async Task<FileResult> DownloadSolutionFile(Guid solutionId, Guid id)
        {
            try
            {
                var file = await _context.Files.FindAsync(id);
                var solution = await _context.Solutions.FindAsync(solutionId);
                var laboratoryWork = await _context.LaboratoryWorks.FindAsync(solution.LaboratoryWorkId);
                var discipline = await _context.Disciplines.FindAsync(laboratoryWork.DisciplineId);
                var user = await _context.Users.Include(c => c.Group).FirstOrDefaultAsync(f => f.Id == file.OwnerId);

                FileStream fs = new FileStream(file.PathToFile, FileMode.Open);
                string fileName = $"{user.ToUserDto().Initials} {user.Group.Name} ({laboratoryWork.Name} - {discipline.ShortName}){Auxiliary.GetExtension(file.Name)}";
                return File(fs, MimeTypesMap.GetMimeType(file.Name), fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        //GET: api/Files/4/5
        // Возвращает файл из условий
        [HttpGet("DownloadRequirementsFile/{requirementsId}/{id}")]
        public async Task<FileResult> DownloadRequirementsFile(Guid requirementsId, Guid id)
        {
            try
            {
                var file = await _context.Files.FindAsync(id);
                var requirement = await _context.Requirements.FindAsync(requirementsId);
                var laboratoryWork = await _context.LaboratoryWorks.FindAsync(requirement.LaboratoryWorkId);
                var discipline = await _context.Disciplines.FindAsync(laboratoryWork.DisciplineId);

                FileStream fs = new FileStream(file.PathToFile, FileMode.Open);
                string fileName = $"{laboratoryWork.Name} - {discipline.ShortName}{Auxiliary.GetExtension(file.Name)}";
                return File(fs, MimeTypesMap.GetMimeType(file.Name), fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        //GET: api/Files/DownloadSolution/5
        // Возвращает ZIP решения
        [HttpGet("DownloadSolution/{solutionId}")]
        public async Task<FileResult> DownloadSolution(Guid solutionId)
        {
            await using MemoryStream memoryStream = new MemoryStream();
            using ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true, System.Text.Encoding.GetEncoding("cp866"));
            try
            {
                var solution = await _context.Solutions.Include(c => c.Files).Where(s => s.Id == solutionId).FirstOrDefaultAsync();
                var fileList = solution.Files;
                var user = await _context.Users.Include(c => c.Group).FirstOrDefaultAsync(f => f.Id == solution.OwnerId);
                var laboratoryWork = await _context.LaboratoryWorks.FindAsync(solution.LaboratoryWorkId);
                var discipline = await _context.Disciplines.FindAsync(laboratoryWork.DisciplineId);

                string zipName = $"{user.ToUserDto().Initials} {user.Group.Name} {discipline.ShortName} - {laboratoryWork.Name}.zip";

                foreach (var file in fileList)
                {
                    zipArchive.CreateEntryFromFile(file.PathToFile, file.Name, CompressionLevel.NoCompression);
                }

                zipArchive.Dispose();
                return File(memoryStream.GetBuffer(), MimeTypesMap.GetMimeType("zip"), zipName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        //GET: api/Files/DownloadRequirements/5
        // Возвращает zip условия
        [HttpGet("DownloadRequirements/{solutionId}")]
        public async Task<FileResult> DownloadRequirements(Guid solutionId)
        {
            await using MemoryStream memoryStream = new MemoryStream();
            using ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);
            try
            {
                var requirement = await _context.Requirements.Include(c => c.Files).Where(s => s.Id == solutionId).FirstOrDefaultAsync();
                var fileList = requirement.Files;
                var laboratoryWork = await _context.LaboratoryWorks.FindAsync(requirement.LaboratoryWorkId);
                var discipline = await _context.Disciplines.FindAsync(laboratoryWork.DisciplineId);
                
                string zipName = $"{laboratoryWork.Name} - {discipline.ShortName}.zip";

                foreach (var file in fileList)
                {
                    zipArchive.CreateEntryFromFile(file.PathToFile, file.Name, CompressionLevel.NoCompression);
                }

                zipArchive.Dispose();
                return File(memoryStream.GetBuffer(), MimeTypesMap.GetMimeType("zip"), zipName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        // PUT: api/Files/5
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFileInfo(Guid id, FileDTO file)
        {
            if (id != file.Id)
            {
                return BadRequest();
            }

            _context.Entry(file).State = EntityState.Modified;
            var find = await _context.Files.FindAsync(id);
            find.Name = file.Name;
            find.OwnerId = file.OwnerId;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!FileExists(id))
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

        // Для загрузки фалов нужно отправить файлы в form-data
        // обязательно на HTTPS, иначе возникает ошибка максимального
        // размера body.

        // POST: api/Files/PostSolutionFiles/{solutionId}
        // Загрузка решений студента
        [Authorize]
        [HttpPost("PostSolutionFiles/{solutionId}")]
        public async Task<ActionResult<IEnumerable<FileDTO>>> PostSolutionFiles([FromForm(Name = "file")] IFormFileCollection fileCollection, Guid solutionId)
        {
            List<FileDTO> uploadedList = new List<FileDTO>();
            var findSolution = await _context.Solutions.FindAsync(solutionId);
            foreach (var file in fileCollection)
            {
                File newFile = new File(file.FileName, new Guid(User.Identity.Name), findSolution, _filePathSettings.FolderForFiles);
                if (!Directory.Exists(newFile.PathToDirectory))
                    Directory.CreateDirectory(newFile.PathToDirectory);
                await using (var fileStream = new FileStream(newFile.PathToFile, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                await _context.Files.AddAsync(newFile);
                uploadedList.Add(newFile.ToFileDto());
            }

            await _context.SaveChangesAsync();
            return Created("", uploadedList);
        }

        //POST: api/Files/PostRequirementFiles/{solutionId}
        // Загрузка решений студента
        [Authorize]
        [HttpPost("PostRequirementFiles/{requirementId}")]
        public async Task<ActionResult<IEnumerable<FileDTO>>> PostRequirementFiles([FromForm(Name = "File")] IFormFileCollection fileCollection, Guid requirementId)
        {
            List<FileDTO> UploadedList = new List<FileDTO>();
            var findRequirement = await _context.Requirements.FindAsync(requirementId);
            foreach (var file in fileCollection)
            {
                File newFile = new File(file.FileName, new Guid(User.Identity.Name), findRequirement, _filePathSettings.FolderForFiles);
                if (!Directory.Exists(newFile.PathToDirectory))
                    Directory.CreateDirectory(newFile.PathToDirectory);
                await using (var fileStream = new FileStream(newFile.PathToFile, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                await _context.Files.AddAsync(newFile);
                UploadedList.Add(newFile.ToFileDto());
            }

            await _context.SaveChangesAsync();
            return Created("", UploadedList);
        }

        // DELETE: api/Files/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<FileDTO>> DeleteFile(Guid id)
        {
            var file = await _context.Files.FindAsync(id);
            if (file == null)
            {
                return NotFound();
            }
            FileInfo fileInf = new FileInfo(file.PathToFile);

            if(fileInf.Exists)
                fileInf.Delete();
            else
                _logger.LogWarning($"File {file.PathToFile} not exists");
            
            _context.Files.Remove(file);
            await _context.SaveChangesAsync();

            return file.ToFileDto();
        }

        private bool FileExists(Guid id)
        {
            return _context.Files.Any(e => e.Id == id);
        }
    }
}
