namespace Shared.Models
{
    public class ChatResponse
    {
        public string Model { get; set; }
        public DateTime CreatedAt { get; set; }
        public Message Message { get; set; }
        public string DoneReason { get; set; }
        public bool Done { get; set; }
        public long TotalDuration { get; set; }
        public long LoadDuration { get; set; }
        public int PromptEvalCount { get; set; }
        public long PromptEvalDuration { get; set; }
        public int EvalCount { get; set; }
        public long EvalDuration { get; set; }
    }

    public class Message
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }
}
