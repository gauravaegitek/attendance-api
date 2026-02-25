// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using attendance_api.Data;
// using attendance_api.DTOs;
// using attendance_api.Models;

// namespace attendance_api.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class HelpSupportController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public HelpSupportController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // ✅ FAQs fetch karo
//         [HttpGet("faqs")]
//         public async Task<ActionResult<ApiResponse<List<FaqDto>>>> GetFaqs()
//         {
//             try
//             {
//                 var faqs = await _context.Faqs
//                     .Where(f => f.IsActive)
//                     .OrderBy(f => f.SortOrder)
//                     .Select(f => new FaqDto
//                     {
//                         FaqId = f.FaqId,
//                         Question = f.Question,
//                         Answer = f.Answer,
//                         Category = f.Category
//                     })
//                     .ToListAsync();

//                 return Ok(new ApiResponse<List<FaqDto>>
//                 {
//                     Success = true,
//                     Message = "FAQs fetched successfully",
//                     Data = faqs
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<List<FaqDto>>
//                 {
//                     Success = false,
//                     Message = "Failed to fetch FAQs",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }

//         // ✅ Admin — FAQ add karo
//         [Authorize(Roles = "admin")]
//         [HttpPost("faqs")]
//         public async Task<ActionResult<ApiResponse<string>>> AddFaq([FromBody] AddFaqDto dto)
//         {
//             try
//             {
//                 var faq = new Faq
//                 {
//                     Question = dto.Question,
//                     Answer = dto.Answer,
//                     Category = dto.Category,
//                     SortOrder = dto.SortOrder,
//                     IsActive = true
//                 };

//                 _context.Faqs.Add(faq);
//                 await _context.SaveChangesAsync();

//                 return Ok(new ApiResponse<string> { Success = true, Message = "FAQ added successfully" });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<string>
//                 {
//                     Success = false,
//                     Message = "Failed to add FAQ",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }

//         // ✅ Contact Us — Message submit karo
//         [Authorize]
//         [HttpPost("contact")]
//         public async Task<ActionResult<ApiResponse<string>>> ContactUs([FromBody] ContactUsDto dto)
//         {
//             try
//             {
//                 var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
//                 var userId = int.Parse(userIdClaim!.Value);

//                 var contact = new ContactMessage
//                 {
//                     UserId = userId,
//                     Subject = dto.Subject,
//                     Message = dto.Message,
//                     CreatedAt = DateTime.Now,
//                     Status = "pending"
//                 };

//                 _context.ContactMessages.Add(contact);
//                 await _context.SaveChangesAsync();

//                 return Ok(new ApiResponse<string>
//                 {
//                     Success = true,
//                     Message = "Your message has been submitted. We will get back to you soon.",
//                     Data = "submitted"
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<string>
//                 {
//                     Success = false,
//                     Message = "Failed to submit message",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }

//         // ✅ Admin — Saari contact messages dekho
//         [Authorize(Roles = "admin")]
//         [HttpGet("contact/messages")]
//         public async Task<ActionResult<ApiResponse<List<ContactMessageDto>>>> GetContactMessages()
//         {
//             try
//             {
//                 var messages = await _context.ContactMessages
//                     .Include(c => c.User)
//                     .OrderByDescending(c => c.CreatedAt)
//                     .Select(c => new ContactMessageDto
//                     {
//                         ContactId = c.ContactId,
//                         UserId = c.UserId,
//                         UserName = c.User!.UserName ?? string.Empty,  // ✅ Fixed CS8602
//                         Email = c.User!.Email ?? string.Empty,        // ✅ Fixed CS8602
//                         Subject = c.Subject,
//                         Message = c.Message,
//                         Status = c.Status,
//                         CreatedAt = c.CreatedAt
//                     })
//                     .ToListAsync();

//                 return Ok(new ApiResponse<List<ContactMessageDto>>
//                 {
//                     Success = true,
//                     Message = "Messages fetched successfully",
//                     Data = messages
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<List<ContactMessageDto>>
//                 {
//                     Success = false,
//                     Message = "Failed to fetch messages",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }

//         // ✅ Admin — Message resolve karo
//         [Authorize(Roles = "admin")]
//         [HttpPut("contact/resolve/{contactId}")]
//         public async Task<ActionResult<ApiResponse<string>>> ResolveMessage(int contactId)
//         {
//             try
//             {
//                 var message = await _context.ContactMessages.FindAsync(contactId);
//                 if (message == null)
//                     return NotFound(new ApiResponse<string> { Success = false, Message = "Message not found" });

//                 message.Status = "resolved";
//                 await _context.SaveChangesAsync();

//                 return Ok(new ApiResponse<string> { Success = true, Message = "Message resolved" });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new ApiResponse<string>
//                 {
//                     Success = false,
//                     Message = "Failed",
//                     Errors = new List<string> { ex.Message }
//                 });
//             }
//         }
//     }
// }








using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using attendance_api.Data;
using attendance_api.DTOs;
using attendance_api.Models;

namespace attendance_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelpSupportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HelpSupportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ FAQs fetch karo
        [HttpGet("faqs")]
        public async Task<ActionResult<ApiResponse<List<FaqDto>>>> GetFaqs()
        {
            try
            {
                var faqs = await _context.Faqs
                    .Where(f => f.IsActive)
                    .OrderBy(f => f.SortOrder)
                    .Select(f => new FaqDto
                    {
                        FaqId    = f.FaqId,
                        Question = f.Question,
                        Answer   = f.Answer,
                        Category = f.Category
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<FaqDto>>
                {
                    Success = true,
                    Message = "FAQs fetched successfully",
                    Data    = faqs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<FaqDto>>
                {
                    Success = false,
                    Message = "Failed to fetch FAQs",
                    Errors  = new List<string> { ex.Message }
                });
            }
        }

        // ✅ Admin — FAQ add karo
        [Authorize(Roles = "admin")]
        [HttpPost("faqs")]
        public async Task<ActionResult<ApiResponse<string>>> AddFaq([FromBody] AddFaqDto dto)
        {
            try
            {
                var faq = new Faq
                {
                    Question  = dto.Question,
                    Answer    = dto.Answer,
                    Category  = dto.Category,
                    SortOrder = dto.SortOrder,
                    IsActive  = true
                };

                _context.Faqs.Add(faq);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string> { Success = true, Message = "FAQ added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to add FAQ",
                    Errors  = new List<string> { ex.Message }
                });
            }
        }

        // ✅ Contact Us — Message submit karo
        [Authorize]
        [HttpPost("contact")]
        public async Task<ActionResult<ApiResponse<string>>> ContactUs([FromBody] ContactUsDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim!.Value);

                var contact = new ContactMessage
                {
                    UserId    = userId,
                    Subject   = dto.Subject,
                    Message   = dto.Message,
                    CreatedAt = DateTime.Now,
                    Status    = "pending"
                };

                _context.ContactMessages.Add(contact);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Your message has been submitted. We will get back to you soon.",
                    Data    = "submitted"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed to submit message",
                    Errors  = new List<string> { ex.Message }
                });
            }
        }

        // ✅ NEW — User: Apne messages aur status dekho
        [Authorize]
        [HttpGet("contact/my-messages")]
        public async Task<ActionResult<ApiResponse<List<ContactMessageDto>>>> GetMyMessages()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim!.Value);

                var messages = await _context.ContactMessages
                    .Include(c => c.User)
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new ContactMessageDto
                    {
                        ContactId = c.ContactId,
                        UserId    = c.UserId,
                        UserName  = c.User!.UserName ?? string.Empty,
                        Email     = c.User!.Email    ?? string.Empty,
                        Subject   = c.Subject,
                        Message   = c.Message,
                        Status    = c.Status,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<ContactMessageDto>>
                {
                    Success = true,
                    Message = "Messages fetched successfully",
                    Data    = messages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<ContactMessageDto>>
                {
                    Success = false,
                    Message = "Failed to fetch messages",
                    Errors  = new List<string> { ex.Message }
                });
            }
        }

        // ✅ Admin — Saari contact messages dekho
        [Authorize(Roles = "admin")]
        [HttpGet("contact/messages")]
        public async Task<ActionResult<ApiResponse<List<ContactMessageDto>>>> GetContactMessages()
        {
            try
            {
                var messages = await _context.ContactMessages
                    .Include(c => c.User)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new ContactMessageDto
                    {
                        ContactId = c.ContactId,
                        UserId    = c.UserId,
                        UserName  = c.User!.UserName ?? string.Empty,
                        Email     = c.User!.Email    ?? string.Empty,
                        Subject   = c.Subject,
                        Message   = c.Message,
                        Status    = c.Status,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<ContactMessageDto>>
                {
                    Success = true,
                    Message = "Messages fetched successfully",
                    Data    = messages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<ContactMessageDto>>
                {
                    Success = false,
                    Message = "Failed to fetch messages",
                    Errors  = new List<string> { ex.Message }
                });
            }
        }

        // ✅ Admin — Message resolve karo
        [Authorize(Roles = "admin")]
        [HttpPut("contact/resolve/{contactId}")]
        public async Task<ActionResult<ApiResponse<string>>> ResolveMessage(int contactId)
        {
            try
            {
                var message = await _context.ContactMessages.FindAsync(contactId);
                if (message == null)
                    return NotFound(new ApiResponse<string> { Success = false, Message = "Message not found" });

                message.Status = "resolved";
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<string> { Success = true, Message = "Message resolved" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Failed",
                    Errors  = new List<string> { ex.Message }
                });
            }
        }
    }
}