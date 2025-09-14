using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Utility
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;
        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return new ValidationResult("File cannot be null.");
            }
            var file = value as IFormFile;
            if (file == null)
            {
                return new ValidationResult("Invalid file type.");
            }
            //var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_extensions.Contains(fileExtension))
            {
                return new ValidationResult($"File extension '{fileExtension}' is not allowed. Allowed extensions are: {string.Join(", ", _extensions)}");
            }
            return ValidationResult.Success;
        }

    }
}
