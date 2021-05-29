using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace adobe_manager.Controllers
{
    [Route("api")]
    [ApiController]
    public class AdobeController : ControllerBase
    {
        private static readonly HttpClient Client = new()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        [HttpGet("login")]
        public async Task<string> Login([FromQuery] string url, [FromQuery] string email, [FromQuery] string password)
        {
            try
            {
                if (url == null || email == null || password == null)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        error = "لطفا تمامی فیلد ها را وارد نمایید"
                    });
                }

                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    return JsonConvert.SerializeObject(new
                    {
                        error = "آدرس باید با https یا http شروع شود"
                    });
                }

                //send login request to adobe connect
                var response = await Client.GetAsync(
                    $"{url}/api/xml?action=login&login={email}&password={password}");
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var loginParsedXml = XDocument.Parse(responseBody);
                var code = loginParsedXml.Root?.Element("status")?.Attribute("code")?.Value;
                if (code != "ok")
                    return JsonConvert.SerializeObject(new
                    {
                        error = "ورود با خطا مواجه شد"
                    });
                //send request to adobe connect to get cookie value from response
                var res = await Client.GetAsync($"{url}/api/xml?action=common-info");
                res.EnsureSuccessStatusCode();
                var body = await res.Content.ReadAsStringAsync();
                var getSessionParsedXml = XDocument.Parse(body);
                var session = getSessionParsedXml.Root?.Element("common")?.Element("cookie")?.Value;
                return JsonConvert.SerializeObject(new
                {
                    token = session,
                    success = "ورود با موفقیت انجام شد"
                });
            }
            catch (Exception e)
            {
                if (e is TimeoutException or HttpRequestException or TaskCanceledException)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        error = $"سرور {url} در دسترس نیست"
                    });
                }

                Console.WriteLine(e);
                return JsonConvert.SerializeObject(new
                {
                    error = "ورود با خطا مواجه شد"
                });
            }
        }

        [HttpGet("sco_shortcuts")]
        public async Task<string> GetScoShortcuts([FromQuery] string session, [FromQuery] string url)
        {
            try
            {
                //send request to get meetings type sco id
                var response = await Client.GetAsync(
                    $"{url}/api/xml?action=sco-shortcuts&session={session}");
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var scoShortcutsParsedXml = XDocument.Parse(responseBody);
                var code = scoShortcutsParsedXml.Root?.Element("status")?.Attribute("code")?.Value;
                var subcode = scoShortcutsParsedXml.Root?.Element("status")?.Attribute("subcode")?.Value;
                switch (code)
                {
                    case "ok":
                        var SCO_ID = scoShortcutsParsedXml.Root?.Element("shortcuts")?.Elements("sco")!
                            .First(s => s.Attribute("type")?.Value == "meetings").Attribute("sco-id")?.Value;
                        return SCO_ID;
                    case "no-access" when subcode == "no-login":
                        return JsonConvert.SerializeObject(new
                        {
                            error = "no-login"
                        });
                    case "no-access" when subcode == "denied":
                        return JsonConvert.SerializeObject(new
                        {
                            error = "دسترسی مورد نظر را ندارید"
                        });
                    case "no-access":
                        return JsonConvert.SerializeObject(new
                        {
                            error = "دسترسی غیر مجاز می باشد"
                        });
                    default:
                        return JsonConvert.SerializeObject(new
                        {
                            error = "دریافت اطلاعات با خطا مواجه شد"
                        });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return JsonConvert.SerializeObject(new
                {
                    error = "دریافت اطلاعات با خطا مواجه شد"
                });
            }
        }
    }
}