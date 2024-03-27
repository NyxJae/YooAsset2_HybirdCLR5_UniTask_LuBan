using System;
using System.IO;
using System.Text;
using YooAsset;

/// <summary>
/// 文件偏移加密方式
/// </summary>
public class FileOffsetEncryption : IEncryptionServices
{
    public EncryptResult Encrypt(EncryptFileInfo fileInfo)
    {
        var offset = 32;
        var fileData = File.ReadAllBytes(fileInfo.FilePath);
        var encryptedData = new byte[fileData.Length + offset];
        Buffer.BlockCopy(fileData, 0, encryptedData, offset, fileData.Length);

        var result = new EncryptResult();
        result.Encrypted = true;
        result.EncryptedData = encryptedData;
        return result;
    }
}

/// <summary>
/// 文件流加密方式
/// </summary>
public class FileStreamEncryption : IEncryptionServices
{
    public EncryptResult Encrypt(EncryptFileInfo fileInfo)
    {
        var fileData = File.ReadAllBytes(fileInfo.FilePath);
        for (var i = 0; i < fileData.Length; i++) fileData[i] ^= BundleStream.KEY;

        var result = new EncryptResult();
        result.Encrypted = true;
        result.EncryptedData = fileData;
        return result;
    }
}