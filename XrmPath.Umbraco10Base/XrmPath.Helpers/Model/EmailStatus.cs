namespace XrmPath.Helpers.Model
{
    public class EmailStatus
    {
        public bool EmailSent { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
    }
}