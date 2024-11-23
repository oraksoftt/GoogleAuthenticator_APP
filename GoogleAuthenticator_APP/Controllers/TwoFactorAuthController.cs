using GoogleAuthenticator.Services;
using GoogleAuthenticator_APP;
using GoogleAuthenticator_APP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

public class TwoFactorAuthController : Controller
{
    private readonly GoogleAuthenticatorService _authenticatorService;
    private readonly DBOperations _dbOperations;
    private readonly IConfiguration _configuration;

    public TwoFactorAuthController(GoogleAuthenticatorService authenticatorService, DBOperations dbOperations, IConfiguration configuration)
    {
        _authenticatorService = authenticatorService;
        _dbOperations = dbOperations;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Setup2FA()
    {
        var userEmail = _configuration["AppSettings:DefaultEmail"];
        var issuer = _configuration["AppSettings:Issuer"];

        // Retrieve existing keys for the user
        var existingKeys = _dbOperations.GetUser2FAKeys(userEmail);

        if (existingKeys.Count > 0)
        {
            var qrCodes = new List<byte[]>();
            foreach (var user2FA in existingKeys)
            {
                var qrCodeUri = _authenticatorService.GenerateQRCodeUri(user2FA.SecretKey, user2FA.Email, user2FA.Issuer, user2FA.KeyIndex);
                var qrCodeImage = _authenticatorService.GenerateQRCodeImage(qrCodeUri);
                qrCodes.Add(qrCodeImage);
            }

            return View("Setup2FA", qrCodes);
        }

        // Generate and save new keys
        var qrCodesNew = new List<byte[]>();

        for (int i = 1; i <= 3; i++)
        {
            var user2FA = new User2FA
            {
                Id = Guid.NewGuid(),
                Email = userEmail,
                Issuer = issuer,
                SecretKey = _authenticatorService.GenerateSecretKey(),
                Is2FAEnabled = false,
                KeyIndex = i,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save the user with the KeyIndex in DB
            _dbOperations.SaveUser2FA(user2FA);

            // Generate the QR code URI for the specific secret key and KeyIndex
            var qrCodeUri = _authenticatorService.GenerateQRCodeUri(user2FA.SecretKey, user2FA.Email, user2FA.Issuer, i);
            var qrCodeImage = _authenticatorService.GenerateQRCodeImage(qrCodeUri);

            qrCodesNew.Add(qrCodeImage);
        }

        return View("Setup2FA", qrCodesNew);
    }

    //[HttpGet]
    //public IActionResult Validate2FA()
    //{
    //    return View();
    //}
    [HttpPost]
    public IActionResult Validate2FA(Request request)
    {
        int keyIndex = request.KeyIndex;

        // Ensure KeyIndex is valid
        if (keyIndex <= 0)
        {
            TempData[$"ValidationResult_{keyIndex}"] = "Invalid KeyIndex. Please try again.";
            return RedirectToAction("Setup2FA");
        }

        if (string.IsNullOrEmpty(request.Code))
        {
            TempData[$"ValidationResult_{keyIndex}"] = "Please enter your 6-digit code.";
            return RedirectToAction("Setup2FA");
        }

        // Fetch the secret key for this user and KeyIndex
        var userEmail = _configuration["AppSettings:DefaultEmail"];
        var user2FA = _dbOperations.GetUser2FAByEmailAndKeyIndex(userEmail, keyIndex);

        if (user2FA == null)
        {
            TempData[$"ValidationResult_{keyIndex}"] = "No secret key found for this index. Please try setting up 2FA again.";
            return RedirectToAction("Setup2FA");
        }

        // Validate the OTP code
        var isValid = _authenticatorService.ValidateTOTP(user2FA.SecretKey, request.Code);

        if (isValid)
        {
            user2FA.Is2FAEnabled = true;

            _dbOperations.SaveUser2FA(user2FA);
            TempData[$"ValidationResult_{keyIndex}"] = "2FA validated successfully!";
        }
        else
        {
            TempData[$"ValidationResult_{keyIndex}"] = "Invalid code. Please try again.";
        }

        return RedirectToAction("Setup2FA");
    }

    /*
    [HttpPost]
    public IActionResult Validate2FA(Request request)
    {
        int keyIndex = request.KeyIndex;

        // Ensure that KeyIndex is valid (between 1 and 3 for this example)
        if (keyIndex <= 0 || keyIndex > 3 )
        {
            TempData[$"ValidationResult_{keyIndex}"] = "Invalid KeyIndex. Please try again.";
            return RedirectToAction("Setup2FA");
        }
        if (request.Code ==null)
        {
            TempData[$"ValidationResult_{keyIndex}"] = "Please Enter your 6 digit code";
            return RedirectToAction("Setup2FA");
        }

        // Retrieve the SecretKey from the session based on the KeyIndex
        var secretKey = HttpContext.Session.GetString($"SecretKey_{keyIndex}");

        // Check if SecretKey is found in the session
        if (string.IsNullOrEmpty(secretKey))
        {
            TempData[$"ValidationResult_{keyIndex}"] = "SecretKey not found in session. Please try scanning the QR code again.";
            return RedirectToAction("Setup2FA");
        }

        // Validate the OTP code entered by the user using the SecretKey
        var isValid = _authenticatorService.ValidateTOTP(secretKey, request.Code);

        if (isValid)
        {
            // Mark 2FA as validated (you can add additional logic for enabling 2FA for this user)
            TempData[$"ValidationResult_{keyIndex}"] = "success";

        }
        else
        {
            TempData[$"ValidationResult_{keyIndex}"] = "Invalid code. Please try again.";
        }

        return RedirectToAction("Setup2FA"); // Redirect to the same page to display the result
    }
    */
    //[HttpPost]
    //public IActionResult Validate2FA(string userCode)
    //{
    //    var secretKey = HttpContext.Session.GetString("SecretKey_1"); // Assuming the first key for validation, change accordingly.

    //    if (_authenticatorService.ValidateTOTP(secretKey, userCode))
    //    {
    //        TempData["ValidationResult"] = "2FA validated successfully!";
    //    }
    //    else
    //    {
    //        TempData["ValidationResult"] = "Invalid code. Please try again.";
    //    }

    //    return RedirectToAction("Validate2FA");
    //}
}
