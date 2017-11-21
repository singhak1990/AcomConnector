﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using ICMSConnector.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ICMSConnector
{
    class Program
    {
        public static void Main(string[] args)
        {
            //Initialize object for authentication and authorization
            AuthController auth = new AuthController();

            IcmsClient icmsClient = new IcmsClient();
            HttpClient httpClient = auth.HttpClient;
            try
            {
                //Ping the translation service API
                var responseText = icmsClient.Ping(httpClient).Result;

                //Write result to test file
                File.WriteAllText(@"Files\Output\PingOutput.txt", responseText);

                //Upload file and write result to a text file
                var uploadResponse = icmsClient.UploadFile(httpClient).Result;
                File.WriteAllText(@"Files\Output\FileUploadOutput.txt", uploadResponse);

                //Get Handback response
                //If the file hasn't gone through HO and HB process the response will be empty
                var handbackResponse = icmsClient.GetLocFilesWithContent(httpClient, true, 4001072).Result;             
                if (handbackResponse.Equals("Not Found"))
                {
                    File.WriteAllText(@"Files\Output\HandbackOutput.txt", "Localized file not found");
                }
                else
                {
                    var hbJson = (JObject)JsonConvert.DeserializeObject(handbackResponse);
                    File.WriteAllBytes(@"Files\Output\HandbackOutput.txt",Convert.FromBase64String(hbJson["Content"].ToString()));
                }
                var fileIds = new List<long?> {4001072};
                var resultOfUpdate = icmsClient.SetLocFileStatus(httpClient, "CheckedIn", fileIds);
            }
            catch (Exception exception)
            {
                Console.WriteLine(string.Format(@"Exception: {0} has occurred",exception.Message));
            }                      
        }
    }
}
