using System;
using System.ComponentModel.DataAnnotations;

namespace SporSalonuMVC.Attributes
{
    public class DateInNext7DaysAttribute : ValidationAttribute
    {
        public DateInNext7DaysAttribute()
        {
            ErrorMessage = "Tarih bugünden başlayarak en fazla 7 gün sonrası olmalıdır.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateValue)
            {
                var today = DateTime.Today;
                var maxDate = today.AddDays(7);

                if (dateValue >= today && dateValue <= maxDate)
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(ErrorMessage);
        }
    }
}
