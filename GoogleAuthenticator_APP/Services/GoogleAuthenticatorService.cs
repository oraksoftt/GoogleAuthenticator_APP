using OtpNet;
using QRCoder;

 

namespace GoogleAuthenticator.Services
{



    public class GoogleAuthenticatorService
    {
        public string GenerateSecretKey()
        {
            var key = KeyGeneration.GenerateRandomKey(20); // 20 bytes for the key
            return Base32Encoding.ToString(key);
        }

        //public string GenerateQRCodeUri(string secretKey, string accountEmail, string issuer)
        //{
        //    var otpAuthUrl = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountEmail)}" +
        //                     $"?secret={secretKey}&issuer={Uri.EscapeDataString(issuer)}";

        //    return otpAuthUrl;
        //}
        public string GenerateQRCodeUri(string secretKey, string accountEmail, string issuer, int keyIndex)
        {
            var otpAuthUrl = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountEmail)}" +
                             $"?secret={secretKey}&issuer={Uri.EscapeDataString(issuer)}&keyindex={keyIndex}";

            return otpAuthUrl;
        }

        public byte[] GenerateQRCodeImage(string qrCodeUri)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(qrCodeUri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20); // Return PNG image as byte array
        }

        public bool ValidateTOTP(string secretKey, string userCode)
        {
            var totp = new Totp(Base32Encoding.ToBytes(secretKey));
            // return totp.VerifyTotp(userCode, out _, VerificationWindow.RfcSpecifiedNetworkDelay);  // Add grace period for one or two intervals before or after the current one (e.g., ±30 or ±60 seconds).
            return totp.VerifyTotp(userCode, out _, new VerificationWindow(0, 0)); 
        }


    }
}
