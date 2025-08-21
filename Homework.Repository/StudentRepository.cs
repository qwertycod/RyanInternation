using Homework.Repository.Interfaces;
using Homework.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Homework.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly TestDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IDistributedCache _redisCache;

        public StudentRepository(TestDbContext context, IMemoryCache cache, IDistributedCache redisCache)
        {
            this._context = context;
            _cache = cache;
            _redisCache = redisCache;
        }

        public async Task AddAsync(Student student)
        {
            var res = await _context.Students.AddAsync(student);
        }

        //public async Task<IEnumerable<Student>> GetAllStudentsAsync() //In memory cache
        //{
        //    const string cacheKey = "all_students";

        //    if (!_cache.TryGetValue(cacheKey, out IEnumerable<Student> students))
        //    {
        //        students = await _context.Students.ToListAsync();

        //        var cacheOptions = new MemoryCacheEntryOptions()
        //            .SetSlidingExpiration(TimeSpan.FromMinutes(10));

        //        _cache.Set(cacheKey, students, cacheOptions);
        //    }

        //    return students;
        //}

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()   //redis need some server/docker to run for serving request
        {
            try
            {
                const string cacheKey = "all_students_redis";

                //var isAlive = await _redisCache.GetAsync("ping_test_key");

                //if (isAlive == null)
                //{
                //    var cachedData = await _redisCache.GetStringAsync(cacheKey);
                //    if (cachedData != null)
                //    {
                //        return JsonSerializer.Deserialize<IEnumerable<Student>>(cachedData);
                //    }
                //}
          
                // Fallback to database
                //var students = await _context.Students.Include(s=>s.StudentBooks).ToListAsync();    // Implicit loading using Include(s=>s.StudentBooks)
                var students = await _context.Students.ToListAsync();    

                var cacheEntryOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                };

              //  var jsonData = JsonSerializer.Serialize(students);

              //  await _redisCache.SetStringAsync(cacheKey, jsonData, cacheEntryOptions);

                return students;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Student> GetByIdAsync(int id)
        {
            var res = await _context.Students.SingleAsync(s => s.StudentId == id);
            return res;
        }

        public void Remove(Student student)
        {
            var res = _context.Students.Remove(student);
        }
        public void Update(Student student)
        {
            _context.Entry(student).Property("RowVersion").OriginalValue = student.RowVersion;
            _context.Entry(student).State = EntityState.Modified;
        }

        public void UpdateStudentWithConcurrency(Student existingStudent, byte[] originalRowVersion)
        {
            _context.Entry(existingStudent).Property(s => s.RowVersion).OriginalValue = originalRowVersion;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            return await _context.Database.BeginTransactionAsync(isolationLevel);
        }

        public void AddFeePayment(FeePayment feePayment)
        {
            _context.FeePayments.Add(feePayment);
        }

        public async Task<string> UpdateStudentAccount(int studentId, decimal amount)
        {
            var account = await _context.Accounts
          .FirstOrDefaultAsync(a => a.StudentId == studentId);

            if (account == null)
            {
                return "Account not found";
            }

            account.TotalPaid += amount;
            account.LastPaymentDate = DateTime.UtcNow;
            _context.Accounts.Update(account);

            // 3. Optionally, update Students table (e.g., PaymentStatus)
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                return "Student not found";
            }
            student.PaymentStatus = "Paid";  // example

            return "Ok";
        }

        public async Task<IList<Book>> GetBooks()
        {
            var x = new List<Book>();
            var books = await _context.Books.Include(s => s.StudentBooks).ToListAsync();    // add  .Include(s => s.StudentBooks) to inlcude it while fetching
            return books;
        }

        public async Task<bool> BuyBook(StudentBook studentBook)
        {
            var res = await _context.StudentBooks.AddAsync(studentBook);
        //    await _context.SaveChangesAsync();

            // EF Core will now populate the identity key
            var res1 = studentBook.UpdateId;
            return true;
        }


        public async Task<bool> BuyBookStatus(StudentBook studentBook)
        {
           // var sbRecord = await _context.StudentBooks.FirstOrDefaultAsync(b => b.StudentId == studentBook.StudentId && b.BookId == studentBook.BookId && b.UpdateId == studentBook.UpdateId);
            var inventoryRecord = await _context.Inventories.SingleOrDefaultAsync(b => b.UpdateId == studentBook.UpdateId && ( b.ChangedByStudentId == studentBook.StudentId));
            var res = inventoryRecord != null;
            return res;
        }

        public async Task<bool> DeleteBookRecord(int bookId, int studentId, int updateId)
        {
            try
            {
                var book = await _context.StudentBooks.FirstOrDefaultAsync(b => b.UpdateId == updateId);
                if (book != null)
                {
                    _context.StudentBooks.Remove(book);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> UpdateInventoryBuyBook(int bookId, int studentId, int updateId)
        {
            try
            {
                var count = _context.Inventories.Count();
                var inv = _context.Inventories.ToList();
                var book = _context.Inventories.FirstOrDefault(b => b.BookId == bookId);
                if(book == null || (book.Count <= 0))
                {
                    return -1;
                }
                book.Count--;   // buy book so count reduce
                book.ChangedByStudentId = studentId;
                book.UpdateId = updateId;
                await _context.SaveChangesAsync();

                return updateId;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<bool> UpdateInventoryUndoBuyBook(int bookId)
        {
            try
            {
                var book = _context.Inventories.FirstOrDefault(b => b.BookId == bookId);
                if (book == null || (book.Count <= 0))
                {
                    return false;
                }
                book.Count++;     // Undo Buy book so count increase

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
