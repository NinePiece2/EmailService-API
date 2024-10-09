namespace EmailService_API.Models
{
    public class EnqueueIncomingMessage
    {
        public string? UserName { get; set; }
        public string? Title { get; set; }
        public string? CreatedEmail { get; set; }
        public string? CreatedName { get; set; }
        public bool? IsSecure { get; set; }
        public string? BodyHtml { get; set; }
        public string? MessageType { get; set; }
        public bool? IsImportantTag { get; set; }
        public string? CcEmail { get; set; }
        public string? BccEmail { get; set; }
    }
}
