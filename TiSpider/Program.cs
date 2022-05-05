using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Newtonsoft.Json;

namespace TiSpider
{
    class Program
    {
        private static List<ModelField> ModelFields = new List<ModelField>();
        
        private static List<ModelAndField> modelAndFields = new List<ModelAndField>();

        private static List<Model> Models = new List<Model>();
        
        private static List<string> ErrModel = new List<string>();

        private static List<string> ModelCountMsg = new List<string>();
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("ti爬虫程序启动");
            await Spider();
            JsonToCsv();
        }

        private static async Task Spider()
        {
            string baseUrl = "https://www.ti.com.cn";

            using (var playwright = await Playwright.CreateAsync())
            await using (var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions()
                         {
                             Timeout = 0,
                             Headless = false,
                             Devtools = true
                         }))
            {
                var context = await browser.NewContextAsync();
                context.SetDefaultTimeout(0);

                var pageIndex = await context.NewPageAsync();

                // go to https://www.ti.com.cn
                await pageIndex.GotoAsync("https://www.ti.com.cn");

                await pageIndex.Locator(".ti_p-responsiveHeader-nav-bar-item.mod-link .ti_p-responsiveHeader-nav-bar-link")
                    .First.ClickAsync();

                var productUl = pageIndex.Locator("//*[@id=\"responsiveHeader-panel-products\"]/div/div[1]/ul");
                var productLis = productUl.Locator("li[class='ti_p-megaMenu-nav-list-item']");
                var liCount = await productLis.CountAsync();

                // 便利所有的产品
                for (int i = 0; i < liCount; i++)
                {
                    var productLi = productLis.Nth(i + 4);
                    var navTarget = await productLi.Locator("a").GetAttributeAsync("data-nav-target");

                    // 判断a标签下是否存在元素
                    if (false == (await productLi.Locator($"a").Locator("svg").CountAsync() > 0))
                    {
                        var overFiewUrl = await productLi.Locator("a").GetAttributeAsync("href");
                        overFiewUrl = overFiewUrl.Trim("//".ToCharArray());
                        overFiewUrl = $"https://{overFiewUrl}";
                        var productPage = await context.NewPageAsync();

                        await productPage.GotoAsync(overFiewUrl);
                        await productPage.Locator("text=parametric-filter查看所有产品").ClickAsync();

                        SetModel(productPage);

                        await productPage.CloseAsync();

                        continue;
                        ;
                    }

                    // 点击产品
                    await pageIndex.Locator($"text={productLi.TextContentAsync().Result}").First.ClickAsync();

                    // 不是不是第一个元素则会有懒加载
                    if (i != 0)
                    {
                        Task.Delay(1000).Wait();
                    }

                    var classUl = pageIndex.Locator($"#{navTarget}-sub");
                    var classLi = classUl.Locator("li");

                    // 便利所有的类别
                    for (int j = 0; j < await classLi.CountAsync(); j++)
                    {
                        var classLiItem = classLi.Nth(j);
                        var roleValue = await classLiItem.GetAttributeAsync("role");

                        if ((!string.IsNullOrEmpty(roleValue) && roleValue.Length > 2) == false)
                        {
                            continue;
                        }

                        var productA = classLiItem.Locator("div").First.Locator("a");
                        var href = await productA.GetAttributeAsync("href");
                        var productUrl = baseUrl + href;

                        var productPage = await context.NewPageAsync();
                        await productPage.GotoAsync(productUrl);
                        SetModel(productPage);
                        await productPage.CloseAsync();
                    }
                }
            }
        }
        
        static void SetModel(IPage productPage,string modelParaent="Texas Instruments")
        {
            
        }
        
        static void JsonToCsv()
        {
            while (true)
            {
                try
                {
                    // 将M三个集合转为Json数组
                    var modelFieldJson = JsonConvert.SerializeObject(ModelFields);
                    var modelAndFieldJson = JsonConvert.SerializeObject(modelAndFields);
                    var modelJson = JsonConvert.SerializeObject(Models);
            
                    // 将json数组转为Csv
                    CsvHelper.JsonArrayToCsv(modelFieldJson, "ModelFields.csv");
                    CsvHelper.JsonArrayToCsv(modelAndFieldJson, "ModelFields.csv");
                    CsvHelper.JsonArrayToCsv(modelJson, "ModelFields.csv");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }
            }
        }
        
    }
}