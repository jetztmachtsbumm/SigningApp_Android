using System.Security.Cryptography; // Needed for SHA256
using System.Diagnostics;
using System.Text.RegularExpressions;

// --- New Function: GetPackageChecksum ---
string GetPackageChecksum(string apkPath)
{
    // The package checksum is the Base64 URL-safe SHA-256 hash of the entire APK file.
    using var sha256 = SHA256.Create();
    using var stream = File.OpenRead(apkPath);
    
    // 1. Calculate the raw SHA-256 hash of the file content
    var bytes = sha256.ComputeHash(stream);

    // 2. Convert to Base64 URL-safe string
    var base64 = Convert.ToBase64String(bytes)
        .Replace('+', '-')
        .Replace('/', '_')
        .TrimEnd('='); // Remove Base64 padding (required by Android provisioning)

    return base64;
}

// --- Existing Function: GetSignatureChecksum (Modified to be a bit cleaner) ---
string GetSignatureChecksum(string apkPath, string apksignerPath)
{
    // example: "C:\\Android\\build-tools\\34.0.0\\apksigner.bat"
    var output = RunProcess(apksignerPath, $"verify --print-certs \"{apkPath}\"");

    // find the SHA-256 digest line
    // e.g. "SHA-256 digest: 12 AB CD EF ..."
    var match = Regex.Match(output, @"SHA-256 digest:\s*([0-9A-Fa-f: ]+)");
    if (!match.Success)
        throw new Exception("Could not parse SHA-256 digest from apksigner output. Output was: " + output);

    var hexFingerprint = match.Groups[1].Value;

    // Remove spaces and colons
    hexFingerprint = hexFingerprint.Replace(" ", "").Replace(":", "");

    // Convert hex → bytes
    var bytes = Enumerable.Range(0, hexFingerprint.Length / 2)
        .Select(i => Convert.ToByte(hexFingerprint.Substring(i * 2, 2), 16))
        .ToArray();

    // Convert to Base64 URL-safe
    var base64 = Convert.ToBase64String(bytes)
        .Replace('+', '-')
        .Replace('/', '_')
        .TrimEnd('=');

    return base64;
}

// --- Existing Function: RunProcess ---
string RunProcess(string fileName, string args)
{
    var startInfo = new ProcessStartInfo
    {
        FileName = fileName,
        Arguments = args,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true // Added for cleaner execution
    };

    using var process = Process.Start(startInfo);
    var output = process?.StandardOutput.ReadToEnd() ?? string.Empty;
    var error = process?.StandardError.ReadToEnd() ?? string.Empty;
    process?.WaitForExit();

    return output + Environment.NewLine + error;
}

// --- Main Execution ---

// Define the paths for your specific setup
string apkFilePath = @"C:\Users\JulianKellner\Downloads\app-release.apk";
string signerPath = @"C:\Users\JulianKellner\AppData\Local\Android\Sdk\build-tools\35.0.0\apksigner.bat";

// 1. Calculate and display the Package Checksum
try
{
    string packageChecksum = GetPackageChecksum(apkFilePath);
    Console.WriteLine("--- Package Checksum (File Integrity) ---");
    Console.WriteLine("Key: android.app.extra.PROVISIONING_DEVICE_ADMIN_PACKAGE_CHECKSUM");
    Console.WriteLine($"Value: \"{packageChecksum}\"");
    Console.WriteLine("------------------------------------------");

    // 2. Calculate and display the Signature Checksum
    string signatureChecksum = GetSignatureChecksum(apkFilePath, signerPath);
    Console.WriteLine("--- Signature Checksum (Key Integrity) ---");
    Console.WriteLine("Key: android.app.extra.PROVISIONING_DEVICE_ADMIN_SIGNATURE_CHECKSUM");
    Console.WriteLine($"Value: \"{signatureChecksum}\"");
    Console.WriteLine("------------------------------------------");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
    Console.WriteLine("Check your file paths for the APK and apksigner.bat.");
}