using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ProposalService.Services;
using WorkService.Data;
using WorkService.Models.DTOs;
using WorkService.Models.Entities;
using WorkService.Models.Enums;
using WorkService.Repositories;

namespace WorkService.Services
{
    public class TaskService
    {

        private readonly AppDbContext _context;
        private NotificationService _notifyService;
        private readonly HttpClient _httpClient;

        public TaskService(
            AppDbContext context,
            NotificationService notifyService,
            HttpClient httpClient)
        {
            _context = context;
            _notifyService = notifyService;
            _httpClient = httpClient;
        }

        public async Task<List<TaskDto>> GetAll()
        {
            var tasks = await _context.Tasks
                .Include(t => t.TaskTechnologies)
                .ThenInclude(tt => tt.Technology)
                .ToListAsync();

            return tasks.Select(t => new TaskDto
            {
                Id = t.Id,
                CreatedByUserId = t.CreatedByUserId,
                Title = t.Title,
                Description = t.Description,
                Budget = t.Budget,
                Category = t.Category,
                Specialization = t.Specialization,
                CreatedAt = t.CreatedAt,
                Deadline = (DateTime)t.Deadline,
                Status = t.Status,

                Technologies = t.TaskTechnologies
                .Select(tt => new TechnologyDto { Id = tt.Technology.Id, Name = tt.Technology.Name })
                .ToList()
            }).ToList();

        }

        public async Task<TaskDto> GetById(Guid id)
        {
            var task = await _context.Tasks
                .Include(t=>t.TaskTechnologies)
                .ThenInclude(tt=>tt.Technology)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return null;

            var dto = new TaskDto
            {
                Id = task.Id,
                CreatedByUserId = task.CreatedByUserId,
                Title = task.Title,
                Description = task.Description,
                Budget = task.Budget,
                Category = task.Category,
                Specialization = task.Specialization,
                CreatedAt = task.CreatedAt,
                Deadline = task.Deadline ?? default(DateTime),
                Status = task.Status,
                Technologies = task.TaskTechnologies
                .Select(tt => new TechnologyDto { Id = tt.Technology.Id, Name = tt.Technology.Name })
                .ToList()
            };

            return dto;
        }

        public async Task<TaskDto> Create(CreateTaskDto dto, Guid userId)
        {
            var normalizedTechs = dto.Technologies
                .Select(t=>t.Trim().ToLower())
                .Distinct()
                .ToList();

            var existingTechs = await _context.Technologies
                .Where(t => normalizedTechs.Contains(t.Name.ToLower()))
                .ToListAsync();

            var newTechs = normalizedTechs
                .Except(existingTechs.Select(t => t.Name.ToLower()))
                .Select(t => new Technology { Name = t})
                .ToList();

            var task = new TaskEntity
            {
                Title = dto.Title,
                Description = dto.Description,
                Budget = dto.Budget,
                Category = dto.Category,
                Deadline = dto.Deadline.HasValue
                    ? DateTime.SpecifyKind(dto.Deadline.Value, DateTimeKind.Utc)
                    : null,

                Specialization = dto.Specialization,
                CreatedByUserId = userId,

                TaskTechnologies = existingTechs.Concat(newTechs)
                .Select(t=>new TaskTechnology { Technology = t})
                .ToList()
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var taskDto = new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Budget = task.Budget,
                Category = task.Category,
                CreatedAt = task.CreatedAt,
                Deadline = task.Deadline ?? default(DateTime),
                Specialization = task.Specialization,
                CreatedByUserId = task.CreatedByUserId,
                Technologies = task.TaskTechnologies
                .Select(tt => new TechnologyDto
                {
                    Id = tt.Technology.Id,
                    Name = tt.Technology.Name
                }).ToList()
            };


            return taskDto;
        }

        public async Task Delete(Guid id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateStatus(Guid taskId, StatusTask status = StatusTask.InProgress)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return false;

            if(status == StatusTask.TaskReady)
            {
                await _notifyService.SendNotificationAsync(new NotificationDto
                {
                    Type = "TASK_READY",
                    Recipient = new Recipient { UserId = task.CreatedByUserId }
                });
            }

            if (status == StatusTask.Cancelled)
            {
                await _notifyService.SendNotificationAsync(new NotificationDto
                {
                    Type = "TASK_CANCELED",
                    Recipient = new Recipient { UserId = task.CreatedByUserId }
                });
            }

            task.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task HandleProposalAccepted(Guid taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return;

            task.Status = StatusTask.InProgress;
            await _context.SaveChangesAsync();
        }
    }
}
