using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace QlW_BandoanOnline.Repository.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        private readonly string[] _allowedExtensions;
        
        public FileExtensionAttribute(string[] allowedExtensions = null)
        {
            // Mặc định cho phép các định dạng ảnh phổ biến
            _allowedExtensions = allowedExtensions ?? new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Nếu không có file thì bỏ qua validation
            if (value == null)
            {
                return ValidationResult.Success;
            }

            // Kiểm tra nếu là IFormFile hoặc danh sách IFormFile
            if (value is IFormFile file)
            {
                return ValidateSingleFile(file);
            }

            if (value is IFormFileCollection files)
            {
                foreach (var f in files)
                {
                    var result = ValidateSingleFile(f);
                    if (result != ValidationResult.Success)
                    {
                        return result;
                    }
                }
            }

            return ValidationResult.Success;
        }

        private ValidationResult ValidateSingleFile(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (string.IsNullOrEmpty(extension))
            {
                return new ValidationResult("File không có phần mở rộng");
            }

            if (!_allowedExtensions.Contains(extension))
            {
                return new ValidationResult(GetErrorMessage());
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return $"Chỉ chấp nhận file ảnh với các định dạng: {string.Join(", ", _allowedExtensions)}";
        }
    }
}