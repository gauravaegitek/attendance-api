// using System.ComponentModel.DataAnnotations;

// namespace attendance_api.DTOs
// {
//     public class RegisterDto
//     {
//         [Required]
//         public string UserName { get; set; } = string.Empty;


//         [Required]
//         [EmailAddress]
//         public string Email { get; set; } = string.Empty;

//         [Required]
//         [MinLength(6)]
//         public string Password { get; set; } = string.Empty;

//         [Required]
//         [Compare(nameof(Password), ErrorMessage = "Password and ConfirmPassword do not match")]
//         public string ConfirmPassword { get; set; } = string.Empty;
//         [Required]
//         public string Role { get; set; } = string.Empty;
//     }

//     public class LoginDto
//     {
//         [Required]
//         [EmailAddress]
//         public string Email { get; set; } = string.Empty;

//         [Required]
//         public string Password { get; set; } = string.Empty;

//         [Required]
//         public string DeviceId { get; set; } = string.Empty;
//     }

//     public class LoginResponseDto
//     {
//         public int UserId { get; set; }
//         public string UserName { get; set; } = string.Empty;
//         public string Email { get; set; } = string.Empty;
//         public string Role { get; set; } = string.Empty;
//         public string Token { get; set; } = string.Empty;
//         public string Message { get; set; } = string.Empty;
//     }

//     public class ClearDeviceDto
//     {
//         [Required]
//         public int UserId { get; set; }
//     }

//     public class ApiResponse<T>
//     {
//         public bool Success { get; set; }
//         public string Message { get; set; } = string.Empty;
//         public T? Data { get; set; }
//         public List<string>? Errors { get; set; }
//     }
// }










// using System.ComponentModel.DataAnnotations;

// namespace attendance_api.DTOs
// {
//     public class RegisterDto
//     {
//         [Required]
//         public string UserName { get; set; } = string.Empty;

//         [Required, EmailAddress]
//         public string Email { get; set; } = string.Empty;

//         [Required, MinLength(6)]
//         public string Password { get; set; } = string.Empty;

//         [Required, Compare(nameof(Password), ErrorMessage = "Password and ConfirmPassword do not match")]
//         public string ConfirmPassword { get; set; } = string.Empty;

//         [Required]
//         public string Role { get; set; } = string.Empty;
//     }

//     public class LoginDto
//     {
//         [Required, EmailAddress]
//         public string Email { get; set; } = string.Empty;

//         [Required]
//         public string Password { get; set; } = string.Empty;

//         // keep required if you want strict device binding
//         [Required]
//         public string DeviceId { get; set; } = string.Empty;
//     }

//     public class LoginResponseDto
//     {
//         public int UserId { get; set; }
//         public string UserName { get; set; } = string.Empty;
//         public string Email { get; set; } = string.Empty;
//         public string Role { get; set; } = string.Empty;
//         public string Token { get; set; } = string.Empty;
//         public string Message { get; set; } = string.Empty;
//     }

//     public class ClearDeviceDto
//     {
//         [Required]
//         public int UserId { get; set; }
//     }

//     public class UserListDto
//     {
//         public int UserId { get; set; }
//         public string UserName { get; set; } = string.Empty;
//         public string Email { get; set; } = string.Empty;
//         public string Role { get; set; } = string.Empty;
//         public string? DeviceId { get; set; }
//         public DateTime? LastSeen { get; set; }
//         public DateTime CreatedOn { get; set; }
//         public bool IsActive { get; set; }
//     }

//     public class ApiResponse<T>
//     {
//         public bool Success { get; set; }
//         public string Message { get; set; } = string.Empty;
//         public T? Data { get; set; }
//         public List<string>? Errors { get; set; }
//     }
// }







using System.ComponentModel.DataAnnotations;

namespace attendance_api.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password), ErrorMessage = "Password and ConfirmPassword do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        public int RoleId { get; set; } // ✅ ADDED — Flutter se roleId aayega
    }

    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string DeviceId { get; set; } = string.Empty;

        public string? DeviceType { get; set; }   // <-- NEW
        public string? DeviceName { get; set; }   // <-- NEW

    }

    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int LoginHistoryId { get; set; }
    }

    public class ClearDeviceDto
    {
        [Required]
        public int UserId { get; set; }
    }

    public class UserListDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? DeviceId { get; set; }
        public DateTime? LastSeen { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
    }

    // public class ApiResponse<T>
    // {
    //     public bool Success { get; set; }
    //     public string Message { get; set; } = string.Empty;
    //     public T? Data { get; set; }
    //     public List<string>? Errors { get; set; }
    // }
}