using System.Text.Json.Serialization;
using DentBook.Domain.Common.Enums;

namespace DentBook.Application.Common.Results;

/// <summary>
/// Doğrulama hatası bilgilerini temsil eden sınıf
/// </summary>
public class ValidationError : Domain.Models.Error
{
    [JsonInclude] 
    public string PropertyName { get; }

    public ValidationError(string propertyName, string message) 
        : base(DentbookErrorCode.ValidationFailed, message)
    {
        PropertyName = propertyName;
    }
}
