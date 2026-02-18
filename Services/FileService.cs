namespace attendance_api.Services
{
    public interface IFileService
    {
        Task<string> SaveSelfieAsync(IFormFile file, string folderPath);
        bool DeleteSelfie(string filePath);
        string GetSelfieUrl(string fileName);
    }

    public class FileService : IFileService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public FileService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task<string> SaveSelfieAsync(IFormFile file, string folderPath)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty");

                var maxSizeKB = int.Parse(_configuration["AppSettings:MaxSelfieFileSizeKB"] ?? "500");
                if (file.Length > maxSizeKB * 1024)
                    throw new ArgumentException($"File size exceeds {maxSizeKB}KB limit");

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    throw new ArgumentException("Only JPG, JPEG, and PNG files are allowed");

                // Create upload directory if it doesn't exist
                var uploadPath = Path.Combine(_environment.WebRootPath, folderPath);
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative path for database storage
                return Path.Combine(folderPath, fileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving file: {ex.Message}");
            }
        }

        public bool DeleteSelfie(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return false;

                var fullPath = Path.Combine(_environment.WebRootPath, filePath);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetSelfieUrl(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            return $"/{fileName.Replace("\\", "/")}";
        }
    }
}
