using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BotApplication2
{
    //    [LuisModel("73198ae7-86b7-4f44-beb6-73e64a2208c2", "d8a285498d484265a88a1f30686a2670", domain: "westus.api.cognitive.microsoft.com", apiVersion: LuisApiVersion.V2)]
    [LuisModel("73198ae7-86b7-4f44-beb6-73e64a2208c2", "d8a285498d484265a88a1f30686a2670")]
    [Serializable]
    public class MainDialog : LuisDialog<object>
    {
        int _saludos = 1;

        public MainDialog() { }
        public MainDialog(ILuisService service) : base(service) { }

        [LuisIntent("Greet")]
        public async Task Greet(IDialogContext context, LuisResult result)
        {
            if (_saludos == 1)
                await context.PostAsync("Hola Yo soy un bot. ¿En qué puedo ayudarte?");
            else if (_saludos == 3)
            {
                _saludos = 0;
                await context.PostAsync("Esta charla se esta tornando aburrida.");
            }
            else
                await context.PostAsync("Hola");

            _saludos++;
        }

        [LuisIntent("Ask")]
        public async Task Ask(IDialogContext context, LuisResult result)
        {
            QnaMakerRespose(context);
            // await context.PostAsync("Usted me esta preguntando");
        }

        [LuisIntent("Laugh")]
        public async Task Laugh(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Que risa!!");
            context.Wait(MessageReceived);
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("No se che.");
            context.Wait(MessageReceived);
        }


        private async void QnaMakerRespose(IDialogContext context)
        {
            var activity = (Activity)context.Activity;
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

                    if (double.Parse(qnaresponse.Score, System.Globalization.CultureInfo.InvariantCulture) > 50)
                        responseMsg = qnaresponse.Answer.ToString();
                    else
                    {
                        responseMsg = "La verdad es que no estoy muy seguro. Voy a buscar en Internet \n\r";
                        responseMsg += SearchInBing(activity.Text.ToString());
                    }
                }
                catch
                {
                    throw new Exception("Error");
                }
            }
            // return responseMsg;
            Activity reply = activity.CreateReply(responseMsg);
            await connector.Conversations.ReplyToActivityAsync(reply);
        }

        private string SearchInBing(string pregunta)
        {
            string responsString = string.Empty;
            var userAgent = "Mozilla/5.0";
            var bingApiKey = "941dc587b4f04f678bdd24fde9845f14";

            Uri bingUriBase = new Uri(@"https://api.cognitive.microsoft.com/bing/v5.0/search?");
            var q = $"q={pregunta}";
            var mkt = "es-AR HTTP/1.1";
            var builder = new UriBuilder($"{bingUriBase }{q}&{mkt}");

            using (WebClient client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                client.Headers.Add("Ocp-Apim-Subscription-Key", bingApiKey);
                client.Headers.Add("User-Agent", userAgent);

                Stream data = client.OpenRead(builder.ToString());
                StreamReader reader = new StreamReader(data);
                string s = reader.ReadToEnd();
                Random rnd = new Random();

                var bingSearchReponse = JsonConvert.DeserializeObject<BingSearchReponse>(s).webPages.value[rnd.Next(0, 3)];
                responsString = $"Esta es una web interesante que encontre \n\r{bingSearchReponse.ToString()}";
            }
            return responsString;
            //  await context.PostAsync(responsString);
        }
    }
}
