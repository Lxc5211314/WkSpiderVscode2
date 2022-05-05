using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace TiSpider
{
    public static class CsvHelper
    {
        /// <summary>
        /// 将json数组转为csv文件
        /// </summary>
        /// <param name="jsonArray"></param>
        /// <param name="filePath"></param>
        public static void JsonArrayToCsv(string jsonArray, string filePath)
        {
            var csv = new StringBuilder();
            var json = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonArray);
            var keys = json[0].Keys.ToList();
            foreach (var key in keys)
            {
                csv.Append(key);
                csv.Append(",");
            }
            csv.AppendLine();
            foreach (var item in json)
            {
                foreach (var key in keys)
                {
                    csv.Append(item[key]);
                    csv.Append(",");
                }
                csv.AppendLine();
            }
            
            // 写入文件
            System.IO.File.WriteAllText("/Users/xinchangli/Downloads/"+filePath, csv.ToString());
        }
    }
}