﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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

namespace CloudService_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FilesController> _logger;

        public FilesController(ApplicationDbContext context, ILogger<FilesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Files
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FileDTO>>> GetFilesInfo()
        {
            var find = await _context.Files.ToListAsync();
            List<FileDTO> dtos = new List<FileDTO>();
            foreach (var item in find)
            {
                dtos.Add(item.ToFileDto());
            }

            return dtos;
        }

        // GET: api/Files/5
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
                var user = await _context.Users.FindAsync(file.OwnerId);
                var group = await (from contextGroups in _context.Groups
                    join contextGroupUser in _context.GroupUsers on contextGroups.Id equals contextGroupUser.GroupId
                    join contextUser in _context.Users on contextGroupUser.UserId equals contextUser.Id
                    where contextUser.Id == user.Id
                    select contextGroups).ToListAsync();
                FileStream fs = new FileStream(file.PathToFile, FileMode.Open);
                string fileName = $"{user.ToUserDto().Initials} {group.First().Name} ({laboratoryWork.Name} - {discipline.ShortName}){Auxiliary.GetExtension(file.Name)}";
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
                var user = await _context.Users.FindAsync(file.OwnerId);
                var group = await (from contextGroups in _context.Groups
                    join contextGroupUser in _context.GroupUsers on contextGroups.Id equals contextGroupUser.GroupId
                    join contextUser in _context.Users on contextGroupUser.UserId equals contextUser.Id
                    where contextUser.Id == user.Id
                    select contextGroups).ToListAsync();
                FileStream fs = new FileStream(file.PathToFile, FileMode.Open);
                string fileName = $"{laboratoryWork.Name} {group.First().Name} {discipline.ShortName}{Auxiliary.GetExtension(file.Name)}";
                return File(fs, MimeTypesMap.GetMimeType(file.Name), fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        //GET: api/Files/5
        // Возвращает ZIP архив решения
        [HttpGet("DownloadSolution/{solutionId}")]
        public async Task<FileResult> DownloadSolution(Guid solutionId)
        {
            string zipDirectory = @"C:\CloudService\ZIPOut";
            if (!Directory.Exists(zipDirectory))
                Directory.CreateDirectory(zipDirectory);
            string zipPath = @$"C:\CloudService\ZIPOut\{Guid.NewGuid()}.zip";

            //MemoryStream memoryStream = new MemoryStream();
            FileStream fsOut = new FileStream(zipPath, FileMode.Create);
            
            try
            {
                var solution = await _context.Solutions.Include(c => c.Files).FirstOrDefaultAsync();
                var fileList = solution.Files;
                var user = await _context.Users.FindAsync(solution.OwnerId);
                var laboratoryWork = await _context.LaboratoryWorks.FindAsync(solution.LaboratoryWorkId);
                var discipline = await _context.Disciplines.FindAsync(laboratoryWork.DisciplineId);
                var group = await (from contextGroups in _context.Groups
                    join contextGroupUser in _context.GroupUsers on contextGroups.Id equals contextGroupUser.GroupId
                    join contextUser in _context.Users on contextGroupUser.UserId equals contextUser.Id
                    where contextUser.Id == user.Id
                    select contextGroups).ToListAsync();

                ZipArchive zipArchive = new ZipArchive(fsOut, ZipArchiveMode.Update);
                foreach (var file in fileList)
                {   
                    zipArchive.CreateEntryFromFile(file.PathToFile, file.Name, CompressionLevel.NoCompression);
                }

                await fsOut.DisposeAsync();
                fsOut = new FileStream(zipPath, FileMode.Open);
                string zipName = $"{user.ToUserDto().Initials} {group.First().Name} {discipline.ShortName} - {laboratoryWork.Name}.zip";
                //memoryStream.Position = 0;
                return File(fsOut, MimeTypesMap.GetMimeType("zip"), zipName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }


        // Сделать zip условий

        // PUT: api/Files/5
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

        // POST: api/Files
        // Загрузка решений студента
        [HttpPost("PostSolutionFiles/{solutionId}")]
        public async Task<ActionResult<IEnumerable<FileDTO>>> PostSolutionFiles([FromForm] FileUploadDTO fileInfo, [FromForm(Name = "File")] IFormFileCollection fileCollection, Guid solutionId)
        {
            List<FileDTO> uploadedList = new List<FileDTO>();
            var findSolution = await _context.Solutions.FindAsync(solutionId);
            foreach (var file in fileCollection)
            {
                Models.File newFile = new Models.File(file.FileName, fileInfo.OwnerId, findSolution);
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

        // Загрузка решений студента
        [HttpPost("PostRequirementFiles/{requirementId}")]
        public async Task<ActionResult<IEnumerable<FileDTO>>> PostRequirementFiles([FromForm] FileUploadDTO fileInfo, [FromForm(Name = "File")] IFormFileCollection fileCollection, Guid requirementId)
        {
            List<FileDTO> UploadedList = new List<FileDTO>();
            var findRequirement = await _context.Requirements.FindAsync(requirementId);
            foreach (var file in fileCollection)
            {
                Models.File newFile = new Models.File(file.FileName, fileInfo.OwnerId, findRequirement);
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