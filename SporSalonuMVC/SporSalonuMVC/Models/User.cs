using SporSalonuMVC.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email alanı zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre alanı zorunludur.")]
    public string Password { get; set; } = string.Empty;


    public string Role { get; set; } = "User";

    public List<StudentDocument>? StudentDocuments { get; set; }

    public bool IsStudentVerified => StudentDocuments?.Any(d => d.Status == "Onaylandı") == true;

    public List<UserCard>? UserCards { get; set; }

    public string? ProfilePhoto { get; set; } // ✅ BU SATIRI EKLE

    [NotMapped]
    public IFormFile? ProfilePhotoFile { get; set; }
}
