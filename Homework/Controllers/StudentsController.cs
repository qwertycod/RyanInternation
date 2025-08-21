using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Castle.Core.Resource;
using Homework;
using Homework.Models;
using Homework.Repository.Interfaces;
using Homework.Repository.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Data;
using System.Text.Json;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StudentsController> _logger;
    private readonly IConfiguration _configuration;

    private readonly string _storageConnectionString;
    private readonly string _servicBusConnectionString;
    string containerName = "homework210625";
    string serviceBusQueue = "homeworkqueue";

    private static RateLimiter _limiter = new RateLimiter(3, TimeSpan.FromSeconds(10));

    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public StudentsController(IUnitOfWork unitOfWork, ILogger<StudentsController> logger, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _configuration = configuration;
        _storageConnectionString = _configuration.GetConnectionString("AzureStorageConnectionString");
        _servicBusConnectionString = _configuration.GetConnectionString("ServicBusConnectionString");

        _client = new ServiceBusClient(_servicBusConnectionString);
        _sender = _client.CreateSender(serviceBusQueue);
    }

    private  static List<Homework.Models.Student> GetDummyStudents()
    {
        var s = new List<Homework.Models.Student>();
        s.Add(new Homework.Models.Student { Name = "Alex", Age = 10 });
        s.Add(new Homework.Models.Student { Name = "Peter", Age = 12 });
        s.Add(new Homework.Models.Student { Name = "Bob" , Age = 21 });

        return s;
    }
    
    [Authorize]
    [HttpGet]
    //[ResponseCache(Duration = 60)]
    public async Task<IActionResult> GetStudents()
    {
        try
        {
            var res = await _unitOfWork.StudentRepository.GetAllStudentsAsync();

            //var s9 = res.FirstOrDefault(s => s.StudentId == 9);
            //var sb = s9.StudentBooks;

           // var jsonData = JsonSerializer.Serialize(res);

            return Ok(res);
          //  var pic = await GetStudentProfilePic();
            //return Ok(new { res = res, pic = pic });
        }
        catch (Exception ex)
        {
            // Log the exception for debugging (optional but recommended)
            _logger.LogError(ex, "Error occurred while fetching students.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    //// Get pic in file format
    //[HttpGet("GetStudentProfilePic")]
    //public async Task<IActionResult> GetStudentProfilePic()
    //{

    //    BlobServiceClient blobServiceClient = new BlobServiceClient(_storageConnectionString);
    //    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    //    BlobClient blobClient = containerClient.GetBlobClient(blobName);

    //    // Download or access the blob
    //    Stream blobStream = await blobClient.OpenReadAsync();
    //    if (await blobClient.ExistsAsync())
    //    {
    //        var stream = await blobClient.OpenReadAsync();
    //        return File(blobStream, "image/jpeg");
    //    }

    //    return NotFound("Student image not found");
    //}

    // get student iamge via SAS url (temporary)
    [HttpGet("GetStudentProfileUrl")]
    public async Task<string> GetSasUrl([FromQuery] string blobName)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            return "Blob name is required.";

        try
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_storageConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            // Ensure blob exists before generating SAS
            if (!await blobClient.ExistsAsync())
                return "Blob not found.";

            // Generate SAS
            BlobSasBuilder sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b", // 'b' = blob
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(130)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);

            return sasUri.ToString();
        }
        catch (Exception ex)
        {
            // Log error
            _logger.LogError($"Error generating SAS URL: {ex.Message}");
            return "Generating SAS URL";
        }
    }


    [Authorize]
    [HttpPost]
    //[ValidateAntiForgeryToken]  // Will not work, Web APIs typically do not use anti-forgery tokens.
    public async Task<IActionResult> AddStudents([FromForm] Homework.Models.Student student)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (student.Image == null || student.Image.Length == 0)
            return BadRequest("Image is required.");

        // Upload image to Azure Blob Storage
        string blobUrl;
        using (var stream = student.Image.OpenReadStream())
        {
            var blobClient = new BlobServiceClient(_storageConnectionString);
            var containerClient = blobClient.GetBlobContainerClient("homework210625");
            await containerClient.CreateIfNotExistsAsync();
            await containerClient.SetAccessPolicyAsync(PublicAccessType.None); // secure

            var blobName = $"{Guid.NewGuid()}_{student.Image.FileName}";
            var blob = containerClient.GetBlobClient(blobName);
            await blob.UploadAsync(stream, overwrite: true);
            blobUrl = blob.Uri.ToString();
        }

        Homework.Repository.Models.Student studentRepo = new Homework.Repository.Models.Student()
        {
            Name = student.Name,
            Age = student.Age,
            Course = student.Course,
            Gender = student.Gender,
            Gpa = student.Gpa,
            PhotoUrl = blobUrl,
            PaymentStatus = student.PaymentStatus,
            StudentId = student.StudentId,
            Year = student.Year
        };

        await _unitOfWork.StudentRepository.AddAsync(studentRepo);
        await _unitOfWork.CompleteAsync();
        return Ok();
    }

    [HttpGet("/TestAzureServiceBusQueue")]
    public async Task TestAzureServiceBusQueue()
    {
        string connectionString = "";
        string queueName = "topic1";

        var client = new ServiceBusClient(connectionString);
        var sender = client.CreateSender(queueName);

        await sender.SendMessageAsync(new ServiceBusMessage("Hello from Azure Service Bus"));
    }


    [Authorize]
    [HttpGet("GetStudentByID/{id}")]
    //[ValidateAntiForgeryToken]  // Will not work, Web APIs typically do not use anti-forgery tokens.
    [ResponseCache(Duration = 30)]
    public async Task<IActionResult> GetStudentByID(int id)
    {
        var student = await _unitOfWork.StudentRepository.GetByIdAsync(id);
        if (student == null)
            return NotFound();
        if(student.PhotoUrl != null)
        {
            var names = student.PhotoUrl.Split('/');
            var image_name = names[names.Length - 1];
            var thumb_image = $"thumb_{image_name}";
            var bloburl = await GetSasUrl(thumb_image);
            student.PhotoUrl = bloburl;
        }
        
        return Ok(student);
    }
 //  https://storage210625r.blob.core.windows.net/homework210625/22fcb0b1-f38d-442d-a509-552f9f81f708_s3.jpg
    
    [Authorize]
    [HttpPut]
    //[ValidateAntiForgeryToken]  // Will not work, Web APIs typically do not use anti-forgery tokens.
    public async Task<IActionResult> UpdateStudents([FromBody] Homework.Models.Student updatedStudent)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var student = await _unitOfWork.StudentRepository.GetByIdAsync(updatedStudent.StudentId);
        if (student == null)
            return NotFound();

        if (student.RowVersion == null || student.RowVersion.Length == 0)
            return BadRequest("Missing RowVersion for concurrency control.");

        // Map updated fields (if needed)

        // one way of updating the student is to map all the properties and use saveChange() method.
        student.Name = updatedStudent.Name;
        student.Course   = updatedStudent.Course;
        student.Gender   = updatedStudent.Gender;
        student.Age = updatedStudent.Age;
        student.Gpa  = updatedStudent.Gpa;
        student.Year = updatedStudent.Year;

        // ...other fields

        // other way
        // Set original RowVersion for concurrency check
        // updatedStudent.RowVersion = student.RowVersion;
        //_unitOfWork.StudentRepository.Update(updatedStudent);

        // Set original RowVersion for concurrency check
        _unitOfWork.StudentRepository.UpdateStudentWithConcurrency(student, student.RowVersion);

        try
        {
            await _unitOfWork.CompleteAsync();
            return Ok(new { updatedRowVersion = student.RowVersion });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("The record was modified by another user.");
        }
    }

    // method to show transaction on students with 2 operation AddFeePayment and UpdateStudentAccount
    [Authorize]
    [HttpPost("/submitFee/{studentId}/{amount}")]
    public async Task<IActionResult> SubmitFeeAsync(int studentId, decimal amount)
    {
        using var transaction = await _unitOfWork.StudentRepository.BeginTransactionAsync(IsolationLevel.ReadUncommitted);

        try
        {
            // 1. Add fee payment record
            var payment = new Homework.Repository.Models.FeePayment
            {
                StudentId = studentId,
                Amount = amount,
                PaymentDate = DateTime.UtcNow
            };
            _unitOfWork.StudentRepository.AddFeePayment(payment);

            //// 2. Update student's account balance and student PaymentStatus
            ///
            await _unitOfWork.StudentRepository.UpdateStudentAccount(studentId, amount);

            await _unitOfWork.CompleteAsync();

            await transaction.CommitAsync();

            return Ok(new { message = "Fee submitted successfully" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error submitting fee");
            return StatusCode(500, "An error occurred while submitting fee");
        }
    }

    [ResponseCache(Duration = 30)]
    [HttpGet("GetBooks")]
    public async Task<IActionResult> GetBooks()
    {
        try
        {
            var data = await _unitOfWork.StudentRepository.GetBooks();
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting fee");
            throw;
        }      
    }

    [HttpPost("BuyBook/{bookId}/{StudentId}")]
    public async Task<IActionResult> BuyBook(int bookId, int StudentId)
    {
        try
        {
            var studentBook = new Homework.Repository.Models.StudentBook
            {
                BookId = bookId,
                PurchaseDate = DateTime.UtcNow,
                StudentId = StudentId
            };
            var data = await _unitOfWork.StudentRepository.BuyBook(studentBook);
            await _unitOfWork.CompleteAsync();

            var message = new
            {
                BookId = studentBook.BookId,
                StudentId = studentBook.StudentId,
                Email = "abcd@gmail.com",
                PurchaseDate = DateTime.UtcNow,
                EventType = "Buy",
                UpdateId = studentBook.UpdateId > 0 ? studentBook.UpdateId : -1,
            };

            string json = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(json);
            await _sender.SendMessageAsync(serviceBusMessage);

            return Ok(new { status = "Pending", updateId = studentBook.UpdateId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Buy Book");
            throw;
        }
    }

   

  //  [Authorize]
    [HttpGet("/GetDummyStudent")]
    public async Task<IActionResult> GetDummyStudent(int id)
    {
        try
        {
            if (!_limiter.TryExecute())
            {
                return StatusCode(429, "Rate limit exceeded");
            }
            if (id != 0)
            {
                var data = Convert.ToInt32("Abc123");
            }
            return Ok(GetDummyStudents());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting fee");
            throw;
        }
    }
}