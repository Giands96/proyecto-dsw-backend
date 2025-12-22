using Application.Common.Interfaces;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Infrastructure.Services;

public class QRCoderGenerator : IQRGenerator
{
    public string GenerateQrBase64(string payload)
    {
        // using var generator = new QRCodeGenerator();
        // using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        // using var qrCode = new QRCode(data);
        // using Bitmap qrBitmap = qrCode.GetGraphic(20);
        // using var ms = new MemoryStream();
        // qrBitmap.Save(ms, ImageFormat.Png);
        // return Convert.ToBase64String(ms.ToArray());

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);

        var qrCode = new PngByteQRCode(data);
        byte[] qrBytes = qrCode.GetGraphic(20);

        return Convert.ToBase64String(qrBytes);
    }
}
