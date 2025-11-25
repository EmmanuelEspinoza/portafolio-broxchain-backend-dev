using System.Security.Cryptography;

public class AesEncryptionService : IEncryptionService
{
    // private readonly IKeyManagementService _keyManagementService;
    private readonly int _keySize;
    private readonly int _blockSize;
    private readonly IConfiguration _configuration;

    public AesEncryptionService(
        IConfiguration configuration,
        // IKeyManagementService keyManagementService, 
        int keySize = 256, int blockSize = 128)
    {
        // _keyManagementService = keyManagementService;
        _keySize = keySize;
        _blockSize = blockSize;
        _configuration = configuration;
    }

    public async Task<string> EncryptAsync(string plainText)
    {
        // var key = await _keyManagementService.GetEncryptionKeyAsync();
        var keyBase64 = _configuration["SecretAes"] ?? "XWs1V2cnRzVmQztvQVJiMw==";
        byte[] key = Convert.FromBase64String(keyBase64);
        
        
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.GenerateKey();
            aesAlg.KeySize = _keySize;
            aesAlg.BlockSize = _blockSize;
            aesAlg.Key = key;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.GenerateIV();

            using (var encryptor = aesAlg.CreateEncryptor())
            using (var msEncrypt = new MemoryStream())
            {
                msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    await swEncrypt.WriteAsync(plainText);
                }

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    public async Task<string> DecryptAsync(string cipherText)
    {
        // var key = await _keyManagementService.GetEncryptionKeyAsync();
        var fullCipher = Convert.FromBase64String(cipherText);
        
        var keyBase64 = _configuration["SecretAes"] ?? "XWs1V2cnRzVmQztvQVJiMw==";
        byte[] key = Convert.FromBase64String(keyBase64);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.KeySize = _keySize;
            aesAlg.BlockSize = _blockSize;
            aesAlg.Key = key;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            byte[] iv = new byte[16];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            aesAlg.IV = iv;

            using (var decryptor = aesAlg.CreateDecryptor())
            using (var msDecrypt = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (var srDecrypt = new StreamReader(csDecrypt))
            {
                return await srDecrypt.ReadToEndAsync();
            }
        }
    }

    public async Task<byte[]> GenerateKeyAsync()
    {
        using (var aes = Aes.Create())
        {
            aes.KeySize = _keySize;
            aes.GenerateKey();
            return aes.Key;
        }
    }
}