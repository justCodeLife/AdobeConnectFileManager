using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace adobe_manager.Hubs
{
    public class AdobeHub : Hub
    {
        private static readonly HttpClient Client = new()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        public async Task ScoDelete(DeleteSCOViewModel vm)
        {
            try
            {
                var counter = 0.0;
                var percent = 0;
                foreach (var id in vm.ids)
                {
                    var response = await Client.GetAsync(
                        $"{vm.url}/api/xml?action=sco-delete&sco-id={id}&session={vm.session}");
                    response.EnsureSuccessStatusCode();
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var scoDeleteParsedXml = XDocument.Parse(responseBody);
                    var code = scoDeleteParsedXml.Root?.Element("status")?.Attribute("code")?.Value;
                    if (code != "ok")
                    {
                        await Clients.Caller.SendAsync("ScoDelete",
                            new {error = $"حذف فایل با شناسه {id} با خطا مواجه شد"});
                        return;
                    }

                    if (percent != (int) ((counter + 1) / vm.ids.Count * 100))
                    {
                        percent = (int) ((counter + 1) / vm.ids.Count * 100);
                        await Clients.Caller.SendAsync("ShowProgress", percent);
                    }

                    counter++;
                }

                await Clients.Caller.SendAsync("ScoDelete", vm.ids);
            }
            catch (HttpRequestException)
            {
                await Clients.Caller.SendAsync("ScoDelete", new
                {
                    error = $"سرور {vm.url} در دسترس نیست"
                });
            }
            catch (TaskCanceledException)
            {
                await Clients.Caller.SendAsync("ScoDelete", new
                {
                    error = $"سرور {vm.url} در دسترس نیست"
                });
            }
            catch (TimeoutException)
            {
                await Clients.Caller.SendAsync("ScoDelete", new
                {
                    error = $"سرور {vm.url} در دسترس نیست"
                });
            }
            catch (Exception e)
            {
                await Clients.Caller.SendAsync("ScoDelete",
                    new {error = "حذف فایل ها با خطا مواجه شد"});
                Console.WriteLine(e);
            }
        }

        public async Task GetScoExpandedContents(string session, string url, string scoId, string from_date,
            string to_date)
        {
            try
            {
                if (session == null || url == null || scoId == null || from_date == null || to_date == null)
                {
                    await Clients.Caller.SendAsync("GetScoExpandedContents", new
                    {
                        error = "لطفا تمامی فیلد ها را وارد نمایید"
                    });
                    return;
                }

                var response = await Client.GetAsync(
                    $"{url}/api/xml?action=sco-expanded-contents&filter-type=content&sco-id={scoId}&session={session}&filter-gte-date-created={from_date}&filter-lte-date-created={to_date}");
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var scoContentsParsedXml = XDocument.Parse(responseBody);
                var code = scoContentsParsedXml.Root?.Element("status")?.Attribute("code")?.Value;
                var subcode = scoContentsParsedXml.Root?.Element("status")?.Attribute("subcode")?.Value;
                switch (code)
                {
                    case "ok":
                        var expandedScosXml = scoContentsParsedXml.Root?.Element("expanded-scos")?.ToString();
                        var doc = new XmlDocument();
                        doc.LoadXml(expandedScosXml!);
                        var json = JsonConvert.SerializeXmlNode(doc);
                        var token = JObject.Parse(json);
                        if (token["expanded-scos"]?.ToString() != "")
                        {
                            var array = token.First?.First?.First?.First;
                            var expandedScos = JsonConvert.DeserializeObject<List<SCO>>(array?.ToString()!)!.Select(o =>
                                new
                                {
                                    o.id, o.title, o.type, o.size, o.folder_id, o.date, o.depth
                                });
                            await Clients.Caller.SendAsync("FilesCount", expandedScos.Count());

                            var res = await Client.GetAsync(
                                $"{url}/api/xml?action=sco-sizes&sco-id={scoId}&session={session}&filter-type=content&filter-gte-date-created={from_date}&filter-lte-date-created={to_date}");
                            res.EnsureSuccessStatusCode();
                            var resBody = await res.Content.ReadAsStringAsync();
                            var scoSizesParsedXml = XDocument.Parse(resBody);
                            var scoSizesCode = scoSizesParsedXml.Root?.Element("status")?.Attribute("code")?.Value;

                            if (scoSizesCode == "ok")
                            {
                                var scoSize = scoSizesParsedXml.Root?.Element("scos")?.ToString();
                                var sizesDoc = new XmlDocument();
                                sizesDoc.LoadXml(scoSize!);
                                var sizesJson = JsonConvert.SerializeXmlNode(sizesDoc);
                                var sizesArray = JObject.Parse(sizesJson).First?.First?.First?.First;
                                var sizesObj =
                                    JsonConvert.DeserializeObject<List<SCO>>(sizesArray?.ToString()!)!.Select(
                                        o => new {o.id, o.size});
                                var sizes = JsonConvert.SerializeObject(sizesObj);
                                var sizesJSON = JArray.Parse(sizes);
                                var scos = new List<SCO>();
                                var counter = 0.0;
                                var percent = 0;
                                foreach (var t in expandedScos)
                                {
                                    var size = sizesJSON.Where(s => s["id"].ToString() == t.id).Select(s => s["size"])
                                        .First()?.ToString();

                                    var sco = new SCO
                                    {
                                        id = t.id,
                                        date = DateTime.Parse(t.date).ToPersianDateTime().ToString("yyyy/MM/dd HH:mm"),
                                        size = ulong.TryParse(size, out var i) ? (i / 1024).ToString() : "",
                                        title = t.title,
                                        type = t.type,
                                        depth = t.depth
                                    };
                                    scos.Add(sco);
                                    if (percent != (int) ((counter + 1) / expandedScos.Count() * 100))
                                    {
                                        percent = (int) ((counter + 1) / expandedScos.Count() * 100);
                                        await Clients.Caller.SendAsync("ShowProgress", percent);
                                    }

                                    counter++;
                                }

                                await Clients.Caller.SendAsync("GetScoExpandedContents", scos);
                                // foreach (var t in expandedScos)
                                // {
                                //     var path = "";
                                //     var folderId = t.folder_id;
                                //     for (var i = 0; i < t.depth; i++)
                                //     {
                                //         var infoResponse = await Client.GetAsync(
                                //             $"{url}/api/xml?action=sco-info&sco-id={folderId}&session={session}");
                                //         infoResponse.EnsureSuccessStatusCode();
                                //         var infoResponseBody = await infoResponse.Content.ReadAsStringAsync();
                                //         var scoInfoParsedXml = XDocument.Parse(infoResponseBody);
                                //         var infoCode = scoInfoParsedXml.Root?.Element("status")?.Attribute("code")
                                //             ?.Value;
                                //         if (infoCode != "ok")
                                //         {
                                //             await Clients.Caller.SendAsync("FilesPath",
                                //                 new {error = "دریافت مسیر فایل ها با خطا مواجه شد"});
                                //             return;
                                //         }
                                //
                                //         var sco = scoInfoParsedXml.Root?.Element("sco");
                                //         var scoName = sco?.Element("name")?.Value;
                                //         var scoFolderID = sco?.Attribute("folder-id")?.Value;
                                //         folderId = scoFolderID;
                                //         path = $"/{scoName}{path}";
                                //     }
                                //
                                //     await Clients.Caller.SendAsync("FilesPath", new
                                //     {
                                //         path,
                                //         id = t.id
                                //     });
                                // }
                            }
                        }
                        else
                        {
                            await Clients.Caller.SendAsync("GetScoExpandedContents", new List<SCO>());
                        }

                        break;
                    case "operation-size-error":
                        await Clients.Caller.SendAsync("GetScoExpandedContents", new
                        {
                            error = "حجم اطلاعات دریافتی بیش از حد مجاز است"
                        });
                        break;
                    case "no-access" when subcode == "no-login":
                        await Clients.Caller.SendAsync("GetScoExpandedContents", new
                        {
                            error = "no-login"
                        });
                        break;
                    case "no-access" when subcode == "denied":
                        await Clients.Caller.SendAsync("GetScoExpandedContents", new
                        {
                            error = "دسترسی مورد نظر را ندارید"
                        });
                        break;
                    case "no-access":
                        await Clients.Caller.SendAsync("GetScoExpandedContents", new
                        {
                            error = "دسترسی غیر مجاز می باشد"
                        });
                        break;
                    default:
                        await Clients.Caller.SendAsync("GetScoExpandedContents", new
                        {
                            error = "دریافت اطلاعات با خطا مواجه شد"
                        });
                        break;
                }
            }
            catch (HttpRequestException)
            {
                await Clients.Caller.SendAsync("GetScoExpandedContents", new
                {
                    error = $"سرور {url} در دسترس نیست"
                });
            }
            catch (TaskCanceledException)
            {
                await Clients.Caller.SendAsync("GetScoExpandedContents", new
                {
                    error = $"سرور {url} در دسترس نیست"
                });
            }
            catch (TimeoutException)
            {
                await Clients.Caller.SendAsync("GetScoExpandedContents", new
                {
                    error = $"سرور {url} در دسترس نیست"
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Clients.Caller.SendAsync("GetScoExpandedContents", new
                {
                    error = "دریافت اطلاعات با خطا مواجه شد"
                });
            }
        }

        public async Task FilePath(FilePathViewModel vm)
        {
            try
            {
                var path = "";
                var folderId = vm.id;
                for (var i = 0; i < vm.depth; i++)
                {
                    var infoResponse = await Client.GetAsync(
                        $"{vm.url}/api/xml?action=sco-info&sco-id={folderId}&session={vm.session}");
                    infoResponse.EnsureSuccessStatusCode();
                    var infoResponseBody = await infoResponse.Content.ReadAsStringAsync();
                    var scoInfoParsedXml = XDocument.Parse(infoResponseBody);
                    var infoCode = scoInfoParsedXml.Root?.Element("status")?.Attribute("code")?.Value;
                    if (infoCode != "ok")
                    {
                        await Clients.Caller.SendAsync("FilesPath",
                            new {error = $"دریافت مسیر فایل با شناسه {vm.id} با خطا مواجه شد"});
                        return;
                    }

                    var sco = scoInfoParsedXml.Root?.Element("sco");
                    var scoName = sco?.Element("name")?.Value;
                    var scoFolderID = sco?.Attribute("folder-id")?.Value;
                    folderId = scoFolderID;
                    path = $"/{scoName}{path}";
                }

                await Clients.Caller.SendAsync("FilePath", new
                {
                    path,
                    id = vm.id
                });
            }
            catch (HttpRequestException)
            {
                await Clients.Caller.SendAsync("FilePath", new
                {
                    error = $"سرور {vm.url} در دسترس نیست"
                });
            }
            catch (TaskCanceledException)
            {
                await Clients.Caller.SendAsync("FilePath", new
                {
                    error = $"سرور {vm.url} در دسترس نیست"
                });
            }
            catch (TimeoutException)
            {
                await Clients.Caller.SendAsync("FilePath", new
                {
                    error = $"سرور {vm.url} در دسترس نیست"
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Clients.Caller.SendAsync("FilePath", new
                {
                    error = "دریافت مسیر فایل با خطا مواجه شد"
                });
            }
        }

        public async Task Folders(string session, string url, string scoId)
        {
            try
            {
                var response = await Client.GetAsync(
                    $"{url}/api/xml?action=sco-contents&filter-type=folder&sco-id={scoId}&session={session}");
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var scoContentsParsedXml = XDocument.Parse(responseBody);
                var code = scoContentsParsedXml.Root?.Element("status")?.Attribute("code")?.Value;
                var subcode = scoContentsParsedXml.Root?.Element("status")?.Attribute("subcode")?.Value;
                switch (code)
                {
                    case "ok":
                        var scoContentsXml = scoContentsParsedXml.Root?.Element("scos")?.ToString();
                        var doc = new XmlDocument();
                        doc.LoadXml(scoContentsXml!);
                        var json = JsonConvert.SerializeXmlNode(doc);
                        var array = JObject.Parse(json).First?.First?.First?.First;
                        var scoContents = JsonConvert.DeserializeObject<List<Folder>>(array?.ToString()!)!.Select(o =>
                            new
                            {
                                o.id, o.name
                            });
                        await Clients.Caller.SendAsync("Folders", scoContents);
                        break;
                    case "operation-size-error":
                        await Clients.Caller.SendAsync("Folders", new
                        {
                            error = "حجم اطلاعات دریافتی بیش از حد مجاز است"
                        });
                        break;
                    case "no-access" when subcode == "no-login":
                        await Clients.Caller.SendAsync("Folders", new
                        {
                            error = "no-login"
                        });
                        break;
                    case "no-access" when subcode == "denied":
                        await Clients.Caller.SendAsync("Folders", new
                        {
                            error = "دسترسی مورد نظر را ندارید"
                        });
                        break;
                    case "no-access":
                        await Clients.Caller.SendAsync("Folders", new
                        {
                            error = "دسترسی غیر مجاز می باشد"
                        });
                        break;
                    default:
                        await Clients.Caller.SendAsync("Folders", new
                        {
                            error = "دریافت اطلاعات با خطا مواجه شد"
                        });
                        break;
                }
            }
            catch (HttpRequestException)
            {
                await Clients.Caller.SendAsync("Folders", new
                {
                    error = $"سرور {url} در دسترس نیست"
                });
            }
            catch (TaskCanceledException)
            {
                await Clients.Caller.SendAsync("Folders", new
                {
                    error = $"سرور {url} در دسترس نیست"
                });
            }
            catch (TimeoutException)
            {
                await Clients.Caller.SendAsync("Folders", new
                {
                    error = $"سرور {url} در دسترس نیست"
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Clients.Caller.SendAsync("Folders", new
                {
                    error = "دریافت اطلاعات با خطا مواجه شد"
                });
            }
        }
    }
}