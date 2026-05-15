using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;


namespace WebTH.Areas.Admin.Controllers
{
    public class ChatBotAIController : Controller
    {
        // GET: Admin/ChatBotAI
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<JsonResult> AskAI(string message)
        {
            try
            {
                var apiKey = ConfigurationManager.AppSettings["GroqKey"];
                var client = new HttpClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var requestData = new
                {
                    // Sử dụng model production được Groq hỗ trợ
                    model = "llama-3.1-8b-instant",
                    messages = new[] {
                new { role = "user", content = message }
            }
                };

                var content = new StringContent(JsonConvert.SerializeObject(requestData));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await client.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { reply = $"Lỗi từ OpenRouter: {response.StatusCode} - {responseString}" }, JsonRequestBehavior.AllowGet);
                }

                dynamic json = JsonConvert.DeserializeObject(responseString);
                string reply = json?.choices[0]?.message?.content;

                return Json(new { reply = reply ?? "(AI không trả lời gì cả.)" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { reply = $"Lỗi xử lý: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }
    }

}