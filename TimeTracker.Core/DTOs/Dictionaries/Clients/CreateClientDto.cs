using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core.DTOs.Dictionaries.Clients;

public class CreateClientDto : CreateDictionaryDto
{
    [EmailAddress(ErrorMessage = "Некоректний формат email")]
    [StringLength(100, ErrorMessage = "Email не може перевищувати 100 символів")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Некоректний формат телефону")]
    [StringLength(20, ErrorMessage = "Телефон не може перевищувати 20 символів")]
    public string? Phone { get; set; }
}