using System.ComponentModel.DataAnnotations;

namespace Treasurer.Api.Contracts.Requests;

public sealed class CsvUploadRequest
{
    [Required]
    public IFormFile File { get; init; } = null!;
}
