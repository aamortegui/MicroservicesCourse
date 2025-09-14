using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Utility
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;
        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
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
            
            if (file.Length > (_maxFileSize * 1024 * 1024))
            {
                return new ValidationResult($"Maximum allowed file size is {_maxFileSize} MB.");
            }
            return ValidationResult.Success;
        }

    }
}
