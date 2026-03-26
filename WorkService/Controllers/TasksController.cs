using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkService.Models.DTOs;
using WorkService.Models.Entities;
using WorkService.Models.Enums;
using WorkService.Services;

namespace WorkService.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _service;

        public TasksController(TaskService service)
        {
            _service = service;
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _service.GetAll();
            return Ok(tasks);
        }

        // GET: api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var task = await _service.GetById(id);
            if (task == null) return NotFound();

            return Ok(task);
        }

        // POST: api/tasks
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateTaskDto dto,
            [FromHeader(Name = "X-User-Id")] Guid userId)
        {
            var task = await _service.Create(dto, userId);
            return Ok(task);
        }

        // DELETE: api/tasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.Delete(id);
            return Ok();
        }

        // Patch: api/tasks/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] StatusTask status)
        {
            var success = await _service.UpdateStatus(id, status);
            if (!success) return BadRequest("Task not found");

            return Ok(); 
        }

        [HttpPost("proposal-accepted")]
        public async Task<IActionResult> ProposalAccepted([FromBody] ProposalAcceptedDto dto)
        {
            await _service.HandleProposalAccepted(dto.TaskId);
            return Ok();
        }
    }
}
