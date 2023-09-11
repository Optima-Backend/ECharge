namespace ECharge.Infrastructure.Services.FirebaseNotification
{
    public class FirebasePayload
    {
        public string FCMToken { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}

