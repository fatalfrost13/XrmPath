namespace XrmPath.Helpers.Model
{
    public class MessageModel
    {
        public bool Saved { get; set; } = false;
        public bool Error  {get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public string Description {get;set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public bool TaskRun { get; set; } = false;
        public string ReturnId {get;set; } = string.Empty;
    }
}