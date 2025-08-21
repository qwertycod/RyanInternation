using Homework.Repository.Interfaces;
using Homework.Repository.Models;

public class UnitOfWork : IUnitOfWork
{
    private readonly TestDbContext _context;
    public IStudentRepository StudentRepository { get; }

    public IUserRepository UserRepository { get; }

    public UnitOfWork(TestDbContext context, IStudentRepository studentRepository, IUserRepository userRepository)
    {
        StudentRepository = studentRepository;
        UserRepository = userRepository;
        _context = context;
    }

    public async Task<int> CompleteAsync()
    {
       var res = await _context.SaveChangesAsync();
        return res;
    }

    void IDisposable.Dispose()
    {
        _context.Dispose();
    }
}
