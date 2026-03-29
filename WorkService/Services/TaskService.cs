using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        private readonly ServiceProposal _proposalService;

        public TaskService(
            AppDbContext context,
            NotificationService notifyService,
            HttpClient httpClient,
            ServiceProposal proposalService)
        {
            _context = context;
            _notifyService = notifyService;
            _httpClient = httpClient;
            _proposalService = proposalService;
        }

        public async Task<PagedResultDto<TaskDto>> GetAll(int page, int limit)
        {
            var query = _context.Tasks
                .Include(t => t.TaskTechnologies)
                .ThenInclude(tt => tt.Technology)
                .AsQueryable();

            var total = await query.CountAsync();

            var tasks = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

            var data = tasks.Select(t => new TaskDto
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
                .Select(tt => new TechnologyDto
                {
                    Id = tt.Technology.Id,
                    Name = tt.Technology.Name
                }).ToList()
            }).ToList();

            return new PagedResultDto<TaskDto>
            {
                Data = data,
                Total = total,
                Page = page,
                Limit = limit
            };
        }

        public async Task<PagedResultDto<MyTasksDto>> GetMyTasks(Guid id, int page, int limit)
        {
            var query = _context.Tasks
                .Where(t => t.CreatedByUserId == id);

            var total = await query.CountAsync();

            var tasks = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(t => new MyTasksDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Budget = (float)t.Budget,
                    Category = t.Category,
                    Specialization = t.Specialization,
                    Deadline = (DateTime)t.Deadline,
                    Status = t.Status,
                    Technologies = t.TaskTechnologies
                    .Select(tt => new TechnologyDto
                    {
                        Id = tt.Technology.Id,
                        Name = tt.Technology.Name
                    }).ToList()
                }).ToListAsync();

            var taskIds = tasks.Select(t => t.Id).ToList();
            var executorsMap = await _proposalService.GetExecutorsByTaskIds(taskIds);

            foreach (var task in tasks)
            {
                if (executorsMap.TryGetValue(task.Id, out var executors))
                {
                    task.Executors = executors;
                }
            }

            return new PagedResultDto<MyTasksDto>
            {
                Data = tasks,
                Total = total,
                Page = page,
                Limit = limit
            };
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

        public async Task<List<FilteredTaskDto>> GetFiltered()
        {
            var tasks = await _context.Tasks
                .Include(t => t.TaskTechnologies)
                .ThenInclude(tt => tt.Technology)
                .ToListAsync();

            return tasks.Select(t => new FilteredTaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Budget = t.Budget,
                Specialization = t.Specialization,
                Technologies = t.TaskTechnologies
                .Select(tt=>new TechnologyDto
                {
                    Id = tt.Technology.Id,
                    Name = tt.Technology.Name
                }).ToList()
            }).ToList();
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

        public async Task<UpdateTaskDto?> UpdateTask(Guid taskId, UpdateTaskDto dto)
        {
            var task = await _context.Tasks
                .Include(t => t.TaskTechnologies)
                .ThenInclude(tt => tt.Technology)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return null;

            //частичное обновление полей
            if (!string.IsNullOrWhiteSpace(dto.Title))
                task.Title = dto.Title;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                task.Description = dto.Description;

            if (dto.Budget.HasValue)
                task.Budget = dto.Budget.Value;

            if (!string.IsNullOrWhiteSpace(dto.Category))
                task.Category = dto.Category;

            if (!string.IsNullOrWhiteSpace(dto.Specialization))
                task.Specialization = dto.Specialization;

            if (dto.Deadline.HasValue)
                task.Deadline = dto.Deadline;

            if (dto.Status.HasValue)
                task.Status = dto.Status.Value;

            //технологии
            if (dto.Technologies != null && dto.Technologies.Any())
            {
                var normalizedTechs = dto.Technologies
                    .Select(t => t.Trim().ToLower())
                    .Distinct()
                    .ToList();

                var oldTechs = task.TaskTechnologies.ToList();

                var existingTechs = await _context.Technologies
                    .Where(t => normalizedTechs.Contains(t.Name.ToLower()))
                    .ToListAsync();

                var newTechs = normalizedTechs
                    .Except(existingTechs.Select(t => t.Name.ToLower()))
                    .Select(t => new Technology { Name = t })
                    .ToList();

                //добавление новых технологий в базу
                _context.Technologies.AddRange(newTechs);
                await _context.SaveChangesAsync();

                task.TaskTechnologies.Clear();
                foreach (var tech in existingTechs.Concat(newTechs))
                {
                    task.TaskTechnologies.Add(new TaskTechnology
                    {
                        TaskId = task.Id,
                        Technology = tech
                    });
                }
            }

            await _context.SaveChangesAsync();

            return new UpdateTaskDto
            {
                Title = task.Title,
                Description = task.Description,
                Budget = task.Budget,
                Category = task.Category,
                Specialization = task.Specialization,
                Deadline = task.Deadline,
                Status = task.Status,
                Technologies = task.TaskTechnologies.Select(tt => tt.Technology.Name).ToList()
            };

        }

        
    }
}
