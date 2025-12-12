namespace Application.Common.Interfaces;

public interface IQRGenerator
{
    string GenerateQrBase64(string payload);
}
