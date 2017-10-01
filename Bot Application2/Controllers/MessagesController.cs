using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.AspNetCore.Mvc;
using System;
using BotApplication2;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bot_Application2.Controllers
{
    [Route("api/[controller]")]
    [BotAuthentication]
    public class MessagesController : Controller
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            try
            {
                if (activity?.Type == ActivityTypes.Message)
                {
                    await Conversation.SendAsync(activity, () => new MainDialog());
                    //QnaMakerRespose(activity);
                    // await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
                }
                else
                {
                    HandleSystemMessage(activity);
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }


    private async void QnaMakerRespose(Activity activity)
        {
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            var responsString = string.Empty;
            var responseMsg = string.Empty;
            QnaMarkerResult qnaresponse;

            if (activity.Text.Length > 0)
            {
                var knowLedgeBaseID = "d7366a06-b474-4d91-b9d1-d97d6b196bbd";
                var qnaMakerSubscriptionKey = "f0f079826ae0486bb2bc5b00128f341d";

                Uri qnamakerUriBase = new Uri(@"https://westus.api.cognitive.microsoft.com/qnamaker/v2.0");
                var builder = new UriBuilder($"{qnamakerUriBase}/knowledgebases/{knowLedgeBaseID}/generateAnswer");

                var postBody = $"{{\"question\":\"{ activity.Text}\"}}";

                using (WebClient client = new WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;
                    client.Headers.Add("Ocp-Apim-Subscription-Key", qnaMakerSubscriptionKey);
                    client.Headers.Add("Content-Type", "application/json");
                    responsString = client.UploadString(builder.Uri, postBody);
                }

                try
                {
                    qnaresponse = JsonConvert.DeserializeObject<QnaMarkerResultList>(responsString).Answers[0];
                    responseMsg = qnaresponse.Answer.ToString();
                }
                catch
                {
                    throw new Exception("Error");
                }
            }

            Activity reply = activity.CreateReply(responseMsg);
            await connector.Conversations.ReplyToActivityAsync(reply);
        }
    }
}