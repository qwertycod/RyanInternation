using Azure.Messaging.ServiceBus;
using Homework.Models;
using Homework.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Homework.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StudentsController> _logger;
        private readonly IConfiguration _configuration;

        private readonly string _storageConnectionString;
        private readonly string _servicBusConnectionString;
        string containerName = "homework210625";
        string serviceBusQueue = "homeworkqueue";

        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;

        public InventoryController(IUnitOfWork unitOfWork, ILogger<StudentsController> logger, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
            _storageConnectionString = _configuration.GetConnectionString("AzureStorageConnectionString");
            _servicBusConnectionString = _configuration.GetConnectionString("ServicBusConnectionString");

            _client = new ServiceBusClient(_servicBusConnectionString);
            _sender = _client.CreateSender(serviceBusQueue);
        }

        [HttpGet("BookPurchaseStatus")]
        public async Task<IActionResult> BookPurchaseStatus(int bookId, int studentId, int updateId)
        {
            var sb = new Repository.Models.StudentBook { StudentId = studentId, BookId = bookId, UpdateId = updateId };
            var status = await _unitOfWork.StudentRepository.BuyBookStatus(sb);
            return Ok(status);
        }


        [HttpPut("UpdateBuyBook1")]
        public async Task<IActionResult> UpdateBuyBook1(StudentBookDto studentBook)
        {
            return   Ok("ok");
        }

        [HttpPut]   
        public async Task<IActionResult> UpdateBuyBook(StudentBookDto studentBook)
        {
            try
            {
                //Convert.ToInt16("a");
                var res = await _unitOfWork.StudentRepository.UpdateInventoryBuyBook(studentBook.BookId, studentBook.StudentId, studentBook.UpdateId);
                if(res > 0)
                {
                    await _unitOfWork.CompleteAsync();
                    return Ok(new { updateId = res, message = "Book Inventory record updated after buy." });
                }
                return BadRequest(new { updateId = -1, message = "Book not found." });
            }
            catch (Exception ex)
            {
                var message = new
                {
                    BookId = studentBook.BookId,
                    StudentId = studentBook.StudentId,
                    Email = "27rajdeep@gmail.com",
                    PurchaseDate = DateTime.UtcNow,
                    EventType = "Delete",
                    UpdateId = studentBook.UpdateId
                };

                string json = JsonSerializer.Serialize(message);
                var serviceBusMessage = new ServiceBusMessage(json);
                await _sender.SendMessageAsync(serviceBusMessage);
                return Ok("Book inventory record deleted after inventory record error.");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBookBuyRecord(int bookId, int studentId, int updateId)
        {
            var studentBook = new Homework.Repository.Models.StudentBook
            {
                BookId = bookId,
                StudentId = studentId
            };
            var result = await _unitOfWork.StudentRepository.DeleteBookRecord(bookId, studentId, updateId);

            var undoBuyBook = await _unitOfWork.StudentRepository.UpdateInventoryUndoBuyBook(bookId);
            if (!result && !undoBuyBook)
            {
                return NotFound("Record not found or already deleted.");
            }
            await _unitOfWork.CompleteAsync();
            return Ok("Book Buy record undo successfully.");
        }
    }
}
