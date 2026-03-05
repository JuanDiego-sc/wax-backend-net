using Microsoft.AspNetCore.Http;

namespace UnitTests.Helpers;

public class FormFileMock : IFormFile
{
    private readonly string _fileName;
    private readonly Stream _stream;

    public FormFileMock(string fileName, string content = "fake-image-content")
    {
        _fileName = fileName;
        _stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
    }

    public string ContentType => "image/jpeg";
    public string ContentDisposition => $"form-data; name=\"file\"; filename=\"{_fileName}\"";
    public IHeaderDictionary Headers => new HeaderDictionary();
    public long Length => _stream.Length;
    public string Name => "file";
    public string FileName => _fileName;

    public void CopyTo(Stream target) => _stream.CopyTo(target);
    public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        => _stream.CopyToAsync(target, cancellationToken);
    public Stream OpenReadStream() => _stream;
}
