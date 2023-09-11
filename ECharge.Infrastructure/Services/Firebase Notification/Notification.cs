using Newtonsoft.Json;

namespace ECharge.Infrastructure.Services.FirebaseNotification
{
    public class Notification
    {
        public static async void PushNotification(FirebasePayload payload)
        {
            string FirebaseServerKey = "AAAAeM2sEKE:APA91bHl0g1Jijld1nJnyQtq-4WLBbIxHVnhG9w7dvrI1m1lxK-sQTyKFyZ5o9pGC8BsX7NYBiY4STKb8CG5OD673NFxrWSMeaQRROYHnrSCiA-WoBtrFx2BW9suUZ_9rukJ3cxZTR30";

            var initialPayload = new
            {
                to = payload.FCMToken,
                content_available = true,
                priority = "high",
                notification = new
                {
                    title = payload.Title,
                    body = payload.Body,
                    badge = "1",
                    sound = "default"
                }
            };

            string json = JsonConvert.SerializeObject(initialPayload);
            StringContent data = new(json, System.Text.Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {FirebaseServerKey}");
            var response = await client.PostAsync(new Uri("https://fcm.googleapis.com/fcm/send"), data);
        }
    }
}

