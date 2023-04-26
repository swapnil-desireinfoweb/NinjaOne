using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace NinjaOne.DataExtractUtility
{
    internal class Program
    {
        static void Main(string[] args)
        {
            JToken responseObj = null;
            try
            {
                //Read configuration
                string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"EntityConfig.json");
                string configJsonString = System.IO.File.ReadAllText(filePath);
                var deSerializedEntityList = JsonConvert.DeserializeObject<List<Entity>>(configJsonString);
                
                Console.WriteLine("---Data movement activity completed.---\n");
                foreach (Entity entity in deSerializedEntityList)
                {
                    Console.WriteLine($"---Activity for {entity.Name} list Started.---\n");
                    Console.WriteLine($"Fetching {entity.Name} list.\n");
                    NinjaApi ninja = new NinjaApi();
                    ninja.AccessKeyId = Convert.ToString(ConfigurationManager.AppSettings["NINJA:ACCESS_KEY_ID"]);
                    ninja.SecretAccessKey = Convert.ToString(ConfigurationManager.AppSettings["NINJA:SECRET_ACCESS_KEY"]);
                    string response = ninja.GetData(entity.SourceAPIUrl);

                    Console.WriteLine($"Saving {entity.Name} list.\n");
                    if (!string.IsNullOrEmpty(response))
                    {
                        SharePointService sharePointService = new SharePointService();
                        sharePointService.Url = Convert.ToString(ConfigurationManager.AppSettings["SHAREPOINT:URL"]);
                        sharePointService.UserName = Convert.ToString(ConfigurationManager.AppSettings["SHAREPOINT:USER_NAME"]);
                        sharePointService.Password = Convert.ToString(ConfigurationManager.AppSettings["SHAREPOINT:PASSWORD"]);
                        if (!string.IsNullOrEmpty(entity.ResponseNode))
                        {
                            JObject jsonObject = JObject.Parse(response);
                            responseObj = jsonObject.SelectToken(entity.ResponseNode);
                        }
                        else
                        {
                            responseObj = JValue.Parse(response);
                        }
                        
                        sharePointService.Execute(entity.Name, entity.MappingColumns, responseObj);
                    }
                    Console.WriteLine($"---Activity for {entity.Name} list ended.---\n");
                }

                Console.WriteLine("---Data movement activity completed.---\n");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error:- " + ex.ToString());
            }

            Console.ReadLine();
        }
    }
}
