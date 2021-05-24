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
            catch (TaskCanceledException)
            {
                return JsonConvert.SerializeObject(new
                {
                    error = $"سرور {url} در دسترس نیست"
                });
            }
            catch (TimeoutException)
            {
                return JsonConvert.SerializeObject(new
                {
                    error = $"سرور {url} در دسترس نیست"
                });
            }
            catch (HttpRequestException)
            {
                return JsonConvert.SerializeObject(new
                {
                    error = $"سرور {url} در دسترس نیست"
                });
            }
            catch (Exception e)
            {
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

        // [HttpGet("sco_expanded_contents")]
        // public async Task<string> GetScoExpandedContents([FromQuery] string session, [FromQuery] string url,
        //     [FromQuery] string scoId, [FromQuery] string from_date, [FromQuery] string to_date)
        // {
        //     try
        //     {
        //         var response = await Client.GetAsync(
        //             $"{url}/api/xml?action=sco-expanded-contents&filter-type=content&sco-id={scoId}&session={session}&counters=true&filter-gt-date-created={from_date}&filter-lt-date-created={to_date}");
        //         response.EnsureSuccessStatusCode();
        //         var responseBody = await response.Content.ReadAsStringAsync();
        //         var scoContentsParsedXml = XDocument.Parse(responseBody);
        //         var code = scoContentsParsedXml.Root?.Element("status")?.Attribute("code")?.Value;
        //         var subcode = scoContentsParsedXml.Root?.Element("status")?.Attribute("subcode")?.Value;
        //         switch (code)
        //         {
        //             case "ok":
        //                 var expandedScosXml = scoContentsParsedXml.Root?.Element("expanded-scos")?.ToString();
        //                 var doc = new XmlDocument();
        //                 doc.LoadXml(expandedScosXml!);
        //                 var json = JsonConvert.SerializeXmlNode(doc);
        //                 var array = JObject.Parse(json).First?.First?.First?.First;
        //                 var expandedScos = JsonConvert.DeserializeObject<List<SCO>>(array?.ToString()!)!.Select(o =>
        //                     new
        //                     {
        //                         o.id, o.title, o.type, o.size, o.folder_id, o.date, o.depth
        //                     });
        //                 var sizes = await ScoSizes(session, url, scoId);
        //                 var sizesJSON = JArray.Parse(sizes);
        //                 var scos = new List<SCO>();
        //                 foreach (var t in expandedScos)
        //                 {
        //                     string path = "";
        //                     var folderId = t.folder_id;
        //                     for (var i = 0; i < t.depth; i++)
        //                     {
        //                         var scoInfo = await ScoInfo(session, url, folderId);
        //                         var scoObj = JsonConvert.DeserializeObject<SCOINFO>(scoInfo);
        //                         folderId = scoObj?.folder_id;
        //                         path = $"/{scoObj?.name}{path}";
        //                     }
        //
        //                     var size = sizesJSON.Where(s => s["id"].ToString() == t.id).Select(s => s["size"]).First();
        //                     var sco = new SCO
        //                     {
        //                         id = t.id,
        //                         date = DateTime.Parse(t.date).ToPersianDateTime().ToString("yyyy/MM/dd HH:mm"),
        //                         folder_id = path,
        //                         size = (Convert.ToUInt64(size) / 1000).ToString(),
        //                         title = t.title,
        //                         type = t.type
        //                     };
        //                     scos.Add(sco);
        //                 }
        //
        //                 return JsonSerializer.Serialize(scos);
        //             case "operation-size-error":
        //                 return JsonConvert.SerializeObject(new
        //                 {
        //                     error = "حجم اطلاعات دریافتی بیش از حد مجاز است"
        //                 });
        //             case "no-access" when subcode == "no-login":
        //                 return JsonConvert.SerializeObject(new
        //                 {
        //                     error = "شما وارد نشده اید"
        //                 });
        //             case "no-access" when subcode == "denied":
        //                 return JsonConvert.SerializeObject(new
        //                 {
        //                     error = "دسترسی مورد نظر را ندارید"
        //                 });
        //             case "no-access":
        //                 return JsonConvert.SerializeObject(new
        //                 {
        //                     error = "دسترسی غیر مجاز می باشد"
        //                 });
        //             default:
        //                 return JsonConvert.SerializeObject(new
        //                 {
        //                     error = "دریافت اطلاعات با خطا مواجه شد"
        //                 });
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e);
        //         return JsonConvert.SerializeObject(new
        //         {
        //             error = "دریافت اطلاعات با خطا مواجه شد"
        //         });
        //     }
        // }

        // [HttpDelete("sco_delete")]
        // public async Task<string> ScoDelete([FromBody] DeleteSCOViewModel vm)
        // {
        //     try
        //     {
        //         foreach (var id in vm.ids)
        //         {
        //             var response = await Client.GetAsync(
        //                 $"https://{vm.url}/api/xml?action=sco-delete&sco-id={id}&session={vm.session}");
        //             response.EnsureSuccessStatusCode();
        //             var responseBody = await response.Content.ReadAsStringAsync();
        //             var scoDeleteParsedXml = XDocument.Parse(responseBody);
        //             var code = scoDeleteParsedXml.Root?.Element("status")?.Attribute("code")?.Value;
        //             if (code != "ok")
        //             {
        //                 return JsonConvert.SerializeObject(new
        //                 {
        //                     error = "حذف فایل ها با خطا مواجه شد"
        //                 });
        //             }
        //         }
        //
        //         return JsonConvert.SerializeObject(new
        //         {
        //             success = "حذف فایل ها با موفقیت انجام شد"
        //         });
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e);
        //         return JsonConvert.SerializeObject(new
        //         {
        //             error = "حذف فایل ها با خطا مواجه شد"
        //         });
        //     }
        // }

        // [HttpGet("sco_sizes")]
        // public async Task<string> ScoSizes([FromQuery] string session, [FromQuery] string url,
        //     [FromQuery] string scoId)
        // {
        //     try
        //     {
        //         var response = await Client.GetAsync(
        //             $"{url}/api/xml?action=sco-sizes&sco-id={scoId}&session={session}");
        //         response.EnsureSuccessStatusCode();
        //         var responseBody = await response.Content.ReadAsStringAsync();
        //         var scoSizesParsedXml = XDocument.Parse(responseBody);
        //         var code = scoSizesParsedXml.Root?.Element("status")?.Attribute("code")?.Value;
        //         var subcode = scoSizesParsedXml.Root?.Element("status")?.Attribute("subcode")?.Value;
        //         switch (code)
        //         {
        //             case "ok":
        //                 var scoSize = scoSizesParsedXml.Root?.Element("scos")?.ToString();
        //                 var doc = new XmlDocument();
        //                 doc.LoadXml(scoSize!);
        //                 var json = JsonConvert.SerializeXmlNode(doc);
        //                 var array = JObject.Parse(json).First?.First?.First?.First;
        //                 var sizes =
        //                     JsonConvert.DeserializeObject<List<SCO>>(array?.ToString()!)!.Select(
        //                         o => new {o.id, o.size});
        //                 var result = JsonConvert.SerializeObject(sizes);
        //                 return result;
        //             case "operation-size-error":
        //                 return JsonConvert.SerializeObject(new
        //                 {
        //                     error = "حجم اطلاعات دریافتی بیش از حد مجاز است"
        //                 });
        //             case "no-access" when subcode == "no-login":
        //                 return JsonConvert.SerializeObject(new
        //                 {
        //                     error = "شما وارد نشده اید"
        //                 });
        //             case "no-access" when subcode == "denied":
        //                 return JsonConvert.SerializeObject(new
        //                 {
        //                     error = "دسترسی مورد نظر را ندارید"
        //                 });
        //             case "no-access":
        //                 return JsonConvert.SerializeObject(new
        //                 {
        //                     error = "دسترسی غیر مجاز می باشد"
        //                 });
        //             default:
        //                 return JsonConvert.SerializeObject(new
        //                 {
        //                     error = "دریافت اطلاعات با خطا مواجه شد"
        //                 });
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e);
        //         return JsonConvert.SerializeObject(new
        //         {
        //             error = "دریافت اطلاعات با خطا مواجه شد"
        //         });
        //     }
        // }
        //
        // [HttpGet("sco_info")]
        // public async Task<string> ScoInfo([FromQuery] string session, [FromQuery] string url,
        //     [FromQuery] string scoId)
        // {
        //     try
        //     {
        //         var response = await Client.GetAsync(
        //             $"{url}/api/xml?action=sco-info&sco-id={scoId}&session={session}");
        //         response.EnsureSuccessStatusCode();
        //         var responseBody = await response.Content.ReadAsStringAsync();
        //         var scoInfoParsedXml = XDocument.Parse(responseBody);
        //         var code = scoInfoParsedXml.Root?.Element("status")?.Attribute("code")?.Value;
        //         var subcode = scoInfoParsedXml.Root?.Element("status")?.Attribute("subcode")?.Value;
        //         switch (code)
        //         {
        //             case "ok":
        //                 var sco = scoInfoParsedXml.Root?.Element("sco");
        //                 var scoName = sco?.Element("name")?.Value;
        //                 var scoFolderID = sco?.Attribute("folder-id")?.Value;
        //                 return JsonConvert.SerializeObject(new
        //                 {
        //                     name = scoName,
        //                     folder_id = scoFolderID
        //                 });
        //             case "operation-size-error":
        //                 return JsonConvert.SerializeObject(new
        //                 {
        //                     error = "حجم اطلاعات دریافتی بیش از حد مجاز است"
        //                 });
        //             case "no-access" when subcode == "no-login":
        //                 return JsonConvert.SerializeObject(new
        //                 {
        //                     error = "شما وارد نشده اید"
        //                 });
        //             case "no-access" when subcode == "denied":
        //                 return JsonConvert.SerializeObject(new
        //                 {
        //                     error = "دسترسی مورد نظر را ندارید"
        //                 });
        //             case "no-access":
        //                 return JsonConvert.SerializeObject(new
        //                 {
        //                     error = "دسترسی غیر مجاز می باشد"
        //                 });
        //             default:
        //                 return JsonConvert.SerializeObject(new
        //                 {
        //                     error = "دریافت اطلاعات با خطا مواجه شد"
        //                 });
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e);
        //         return JsonConvert.SerializeObject(new
        //         {
        //             error = "دریافت اطلاعات با خطا مواجه شد"
        //         });
        //     }
        // }
    }
}