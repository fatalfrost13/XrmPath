namespace XrmPath.CRM.DataAccess.Model
{
    public class MessageModel
    {
        public bool Saved { get; set; } = false;
        public bool Error  {get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public string Description {get;set; } = string.Empty;
        public string ReturnId {get;set; } = string.Empty;
    }
}