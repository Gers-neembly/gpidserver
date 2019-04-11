namespace Neembly.GPIDServer.SharedClasses
{
    public class EmailMessage
    {
        public int OperatorId { get; set; }
        public string Subject { get; set; }
        public bool IsHtml { get; set; }
        public string Message { get; set; }
        public string Receipients { get; set; }
        public string CarbonCopies { get; set; }
        public string BlindCarbonCopies { get; set; }
        public string Sender { get; set; }
        public string Status { get; set; }
        public string FailedMessage { get; set; }
    }
}
