
using Homework.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework.Repository.Interfaces
{
    public interface IStudentRepository
    {
        Task<IEnumerable<Student>> GetAllStudentsAsync();
        Task<Student> GetByIdAsync(int id);
        Task AddAsync(Student student);
        void Remove(Student student);

        void Update(Student student);

        void UpdateStudentWithConcurrency(Student existingStudent, byte[] originalRowVersion);
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel);
        void AddFeePayment(FeePayment feePayment);
        Task<string> UpdateStudentAccount(int studentId, decimal amount);
        Task<IList<Book>> GetBooks();
        Task<bool> BuyBook(StudentBook studentBook);
        Task<bool> BuyBookStatus(StudentBook studentBook);
        Task<int> UpdateInventoryBuyBook(int bookId, int studentId, int updateId);
        Task<bool> DeleteBookRecord(int bookId, int studentId, int updateId);
        Task<bool> UpdateInventoryUndoBuyBook(int bookId);
    }
}
