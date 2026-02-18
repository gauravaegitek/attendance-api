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
    [Authorize(Roles = "admin")]
    public class HolidayController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HolidayController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/holiday
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<HolidayListDto>>>> GetAllHolidays()
        {
            try
            {
                var holidays = await _context.Holidays
                    .OrderBy(h => h.HolidayDate)
                    .Select(h => new HolidayListDto
                    {
                        HolidayId = h.HolidayId,
                        HolidayName = h.HolidayName,
                        HolidayDate = h.HolidayDate,
                        Description = h.Description,
                        IsActive = h.IsActive,
                        CreatedOn = h.CreatedOn
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<HolidayListDto>>
                {
                    Success = true,
                    Message = "Holidays retrieved successfully",
                    Data = holidays
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<HolidayListDto>>
                {
                    Success = false,
                    Message = "Failed to retrieve holidays",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // GET: api/holiday/upcoming
        [HttpGet("upcoming")]
        public async Task<ActionResult<ApiResponse<List<HolidayListDto>>>> GetUpcomingHolidays()
        {
            try
            {
                var today = DateTime.Now.Date;
                var holidays = await _context.Holidays
                    .Where(h => h.HolidayDate >= today && h.IsActive)
                    .OrderBy(h => h.HolidayDate)
                    .Select(h => new HolidayListDto
                    {
                        HolidayId = h.HolidayId,
                        HolidayName = h.HolidayName,
                        HolidayDate = h.HolidayDate,
                        Description = h.Description,
                        IsActive = h.IsActive,
                        CreatedOn = h.CreatedOn
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<HolidayListDto>>
                {
                    Success = true,
                    Message = "Upcoming holidays retrieved successfully",
                    Data = holidays
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<HolidayListDto>>
                {
                    Success = false,
                    Message = "Failed to retrieve upcoming holidays",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // GET: api/holiday/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<HolidayListDto>>> GetHoliday(int id)
        {
            try
            {
                var holiday = await _context.Holidays.FindAsync(id);

                if (holiday == null)
                {
                    return NotFound(new ApiResponse<HolidayListDto>
                    {
                        Success = false,
                        Message = "Holiday not found"
                    });
                }

                var holidayDto = new HolidayListDto
                {
                    HolidayId = holiday.HolidayId,
                    HolidayName = holiday.HolidayName,
                    HolidayDate = holiday.HolidayDate,
                    Description = holiday.Description,
                    IsActive = holiday.IsActive,
                    CreatedOn = holiday.CreatedOn
                };

                return Ok(new ApiResponse<HolidayListDto>
                {
                    Success = true,
                    Message = "Holiday retrieved successfully",
                    Data = holidayDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<HolidayListDto>
                {
                    Success = false,
                    Message = "Failed to retrieve holiday",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // POST: api/holiday
        [HttpPost]
        public async Task<ActionResult<ApiResponse<HolidayListDto>>> CreateHoliday([FromBody] HolidayDto dto)
        {
            try
            {
                // Check if holiday already exists for the same date
                var existingHoliday = await _context.Holidays
                    .FirstOrDefaultAsync(h => h.HolidayDate.Date == dto.HolidayDate.Date);

                if (existingHoliday != null)
                {
                    return BadRequest(new ApiResponse<HolidayListDto>
                    {
                        Success = false,
                        Message = "A holiday already exists for this date"
                    });
                }

                var holiday = new Holiday
                {
                    HolidayName = dto.HolidayName,
                    HolidayDate = dto.HolidayDate.Date,
                    Description = dto.Description,
                    IsActive = dto.IsActive,
                    CreatedOn = DateTime.Now
                };

                _context.Holidays.Add(holiday);
                await _context.SaveChangesAsync();

                var responseDto = new HolidayListDto
                {
                    HolidayId = holiday.HolidayId,
                    HolidayName = holiday.HolidayName,
                    HolidayDate = holiday.HolidayDate,
                    Description = holiday.Description,
                    IsActive = holiday.IsActive,
                    CreatedOn = holiday.CreatedOn
                };

                return CreatedAtAction(nameof(GetHoliday), new { id = holiday.HolidayId }, new ApiResponse<HolidayListDto>
                {
                    Success = true,
                    Message = "Holiday created successfully",
                    Data = responseDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<HolidayListDto>
                {
                    Success = false,
                    Message = "Failed to create holiday",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // PUT: api/holiday/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<HolidayListDto>>> UpdateHoliday(int id, [FromBody] HolidayDto dto)
        {
            try
            {
                var holiday = await _context.Holidays.FindAsync(id);

                if (holiday == null)
                {
                    return NotFound(new ApiResponse<HolidayListDto>
                    {
                        Success = false,
                        Message = "Holiday not found"
                    });
                }

                // Check if another holiday exists for the new date
                var existingHoliday = await _context.Holidays
                    .FirstOrDefaultAsync(h => h.HolidayDate.Date == dto.HolidayDate.Date && h.HolidayId != id);

                if (existingHoliday != null)
                {
                    return BadRequest(new ApiResponse<HolidayListDto>
                    {
                        Success = false,
                        Message = "A holiday already exists for this date"
                    });
                }

                holiday.HolidayName = dto.HolidayName;
                holiday.HolidayDate = dto.HolidayDate.Date;
                holiday.Description = dto.Description;
                holiday.IsActive = dto.IsActive;

                await _context.SaveChangesAsync();

                var responseDto = new HolidayListDto
                {
                    HolidayId = holiday.HolidayId,
                    HolidayName = holiday.HolidayName,
                    HolidayDate = holiday.HolidayDate,
                    Description = holiday.Description,
                    IsActive = holiday.IsActive,
                    CreatedOn = holiday.CreatedOn
                };

                return Ok(new ApiResponse<HolidayListDto>
                {
                    Success = true,
                    Message = "Holiday updated successfully",
                    Data = responseDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<HolidayListDto>
                {
                    Success = false,
                    Message = "Failed to update holiday",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // DELETE: api/holiday/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteHoliday(int id)
        {
            try
            {
                var holiday = await _context.Holidays.FindAsync(id);

                if (holiday == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Holiday not found"
                    });
                }

                _context.Holidays.Remove(holiday);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Holiday deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to delete holiday",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}