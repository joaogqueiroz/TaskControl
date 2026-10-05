using System.ComponentModel.DataAnnotations;

namespace TaskControl.UnitTests
{
    // Runs the same DataAnnotations checks MVC runs before ModelState.IsValid.
    public static class ModelValidation
    {
        public static List<string> Errors(object model)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
            return results.Select(r => r.ErrorMessage!).ToList();
        }
    }
}
