using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bus_Station_Ticket_Management.Models
{
    public enum DiscountType
    {
        Percentage,

        [Display(Name = "Fixed Amount")]
        FixedAmount
    }

    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Coupon Code")]
        public string CouponString { get; set; }

        [Required]
        [DisplayName("Discount Type")]
        public DiscountType DiscountType { get; set; }

        [Required]
        [DisplayName("Amount")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        [Range(0.01, 1000000.00, ErrorMessage = "Amount must be between 0.01 and 1000000.00")]
        public decimal DiscountAmount { get; set; }

        [Required]
        [DisplayName("Start Period")]
        public DateTime? StartPeriod { get; set; }

        [Required]
        [DisplayName("End Period")]
        public DateTime? EndPeriod { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool IsActive { get; set; }

        [DisplayName("Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [DisplayName("Last Updated")]
        public DateTime? LastUpdated { get; set; } = DateTime.Now;

        [DisplayName("Image")]
        public string? ImageUrl { get; set; }

        [Required]
        [DisplayName("Title")]
        public string? Title { get; set; }

        [Required]
        [DisplayName("Description")]
        public string? Description { get; set; }

        public async Task<string?> UploadImage(IFormFile file)
        {
            try
            {

                if (file == null || file.Length == 0)
                {
                    return null;
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "coupons");
                var filePath = Path.Combine(directoryPath, fileName);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                return fileName;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task DeleteImage(string? fileName)
        {
            try
            {

                if (fileName == null)
                {
                    return;
                }

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/coupons", fileName);
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}