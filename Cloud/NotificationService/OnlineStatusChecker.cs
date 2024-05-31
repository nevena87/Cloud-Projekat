
using KorisnikService_Data;
using KorisnikService_Data.Models;
using KorisnikService_Data.Queues;
using KorisnikService_Data.Repository;
using Microsoft.WindowsAzure.Storage.Queue;
using NotificationService_Data;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using Notification = KorisnikService_Data.Models.Notification;

namespace NotificationService
{
    public class OnlineStatusChecker
    {
        private static readonly CloudQueue CommentNotificationsQueue = CloudQueueHelper.GetQueue("notifications");
        private static readonly CloudQueue AdminNotificationQueue = CloudQueueHelper.GetQueue("AdminNotificationQueue");

        public static async Task IsCommentAvailableForSend()
        {
            var message = await CloudQueueHelper.ReadMessage(CommentNotificationsQueue);
            if (message == null)
                return;

            var comment = new KomentarRepository().GetKomentarById(message.AsString);
            if (comment == null) return;

            var subs = new PretplataRepository().GetPretplateByTemaId(comment.TemaId);

            var emails = new List<string>();
            foreach (var sub in subs)
            {
                emails.Add(sub.UserEmail); // dodavanje mejla za slanje
            }

            // slanje svih imejlova pretplacenim korisnicima
            int sent = 0;
            foreach (var email in emails)
            {
                await Mailer.Sender.SendEmail(comment.Tekst, email, "Novi komentar je dodat");
                sent++;
            }

            // Save notification info how much emails has been sent
            Notification notification = new Notification(comment.RowKey, sent);
            await new NotificationRepository().InsertNotificationAsync(notification);

            // Remove from queue
            await CloudQueueHelper.DeleteMessage(CommentNotificationsQueue, message);
        }

        public static async Task IsServiceOffline()
        {
            var message = await CloudQueueHelper.ReadMessage(AdminNotificationQueue);
            if (message == null)
                return;

            Trace.TraceError(message.AsString); // ispis koji servis je u ispadu

            try
            {
                if (message == null) return;

                // Send email for each administrator
                foreach (var administrator in (await new AdministratorRepository().ReadAdministratorsAsync()))
                {
                    await Mailer.Sender.SendEmail(message.AsString, administrator.Email, "Informacije o ispadu servisa");
                }

                await CloudQueueHelper.DeleteMessage(AdminNotificationQueue, message);
            }
            catch
            {
                return;
            }
        }
    }
}
