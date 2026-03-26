using Microsoft.EntityFrameworkCore;
using WorkService.Data;
using WorkService.Models.Entities;

namespace WorkService.Repositories
{
    public class TaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskEntity>> GetAll()
        {
            return await _context.Tasks.ToListAsync();
        }

        public async Task<TaskEntity> GetById(int id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task Add(TaskEntity task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(TaskEntity task)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
        }
    }
}
