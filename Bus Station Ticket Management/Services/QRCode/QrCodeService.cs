using QRCoder;

namespace Bus_Station_Ticket_Management.Services.QRCode
{
    public interface IQrCodeService
    {
        string GenerateQrCode(string ticketId);
        string GenerateQrCode(string ticketId, int size);
    }

    public class QrCodeService : IQrCodeService
    {
        public string GenerateQrCode(string ticketId)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(ticketId, QRCodeGenerator.ECCLevel.Q);
                var pngQrCode = new PngByteQRCode(qrCodeData);
                var qrCodeBytes = pngQrCode.GetGraphic(20);
                return Convert.ToBase64String(qrCodeBytes);
            }
        }
        
        public string GenerateQrCode(string ticketId, int size)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(ticketId, QRCodeGenerator.ECCLevel.Q);
                var pngQrCode = new PngByteQRCode(qrCodeData);
                var qrCodeBytes = pngQrCode.GetGraphic(size);
                return Convert.ToBase64String(qrCodeBytes);
            }
        }
    }
}
