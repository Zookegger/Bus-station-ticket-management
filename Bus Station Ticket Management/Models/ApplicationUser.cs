using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bus_Station_Ticket_Management.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [DisplayName("Full Name")]
        public string? FullName { get; set; }

        [DisplayName("Address")]
        public string? Address { get; set; }

        [DisplayName("Gender")]
        public string? Gender { get; set; }

        [DisplayName("Date Of Birth")]
        public DateOnly? DateOfBirth { get; set; }

        [DisplayName("Avatar")]
        public string? Avatar { get; set; }
    
        public static async Task<string?> UploadAvatar(IFormFile image) {
            if (image == null) {
                return null;
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
            var filePath = Path.Combine(directoryPath, fileName);
            
            if (!Directory.Exists(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }

            using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream);

            return fileName;
        }

        public static async Task DeleteAvatar(string? fileName) {
            try {

                if (fileName == null) {
                    return;
                }

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/avatars", fileName);
                if (File.Exists(filePath)) {
                    await Task.Run(() => File.Delete(filePath));
                }
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}
