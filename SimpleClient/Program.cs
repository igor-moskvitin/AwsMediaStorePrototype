using Amazon;
using Amazon.MediaStore;
using Amazon.MediaStoreData;
using Amazon.MediaStoreData.Model;
using Amazon.Runtime;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SimpleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting...");

            Console.WriteLine("Get credentials.");
            //получаем данные для работы с AWS
            var credentials = GetCredentials();

            Console.WriteLine("Put file");
            //Записываем файл в AWS
            await PutFileToAWS(credentials);


            Console.WriteLine("Get file");
            //Читаем файл из AWS
            await GetFileFromAWS(credentials);

            Console.WriteLine("The end.");
            Console.ReadKey();

        }

        /// <summary>
        /// Получаем от сервиса данные для работы с AWS
        /// </summary>
        /// <returns></returns>
        static Credentials GetCredentials()
        {
            var client = new RestClient("http://87.251.89.49:12001/WebServices/User.asmx/GetAwsCredentials");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("undefined", "token=15520-c557394a-031f-42b7-a32c-21087038440d", ParameterType.RequestBody);
            var response = client.Execute(request);
            var jObject = JObject.Parse(response.Content);
            var result = jObject["Result"];

            var creds = new Credentials
            {
                AccessKey = result["AccessKey"].Value<string>(),
                SecretKey = result["SecretKey"].Value<string>(),
                DirctoryPath = result["DirctoryPath"].Value<string>()

            };
            

            return creds;
            
        }

        /// <summary>
        /// Записываем файл в AWS
        /// </summary>
        /// <param name="creds"></param>
        /// <returns></returns>
        static async Task PutFileToAWS(Credentials creds)
        {
            var credentials = new BasicAWSCredentials(creds.AccessKey, creds.SecretKey);

            using (var client = new AmazonMediaStoreClient(credentials, RegionEndpoint.EUCentral1))
            using (var storeDataClient = await CreateStoreDataClientAsync(client: client, containerName: "test", credentials: credentials))
            {
                await PutObjectToContainerAsync(storeDataClient, "path2/sample.mp4");
            }
        }

        /// <summary>
        /// Читаем из AWS
        /// </summary>
        /// <param name="creds"></param>
        /// <returns></returns>
        static async Task GetFileFromAWS(Credentials creds)
        {
            var credentials = new BasicAWSCredentials(creds.AccessKey, creds.SecretKey);

            using (var client = new AmazonMediaStoreClient(credentials, RegionEndpoint.EUCentral1))
            using (var storeDataClient = await CreateStoreDataClientAsync(client: client, containerName: "test", credentials: credentials))
            {
                await GetObjectAsync(storeDataClient, "path2/sample.mp4");
            }
        }

        #region Tools
        private static async Task<AmazonMediaStoreDataClient> CreateStoreDataClientAsync(AmazonMediaStoreClient client, string containerName, BasicAWSCredentials credentials)
        {
            //var request = new ListContainersRequest();
            //var containers = await client.ListContainersAsync(request);
            //var container = containers.Containers.SingleOrDefault(c => c.Name == containerName);
            var config = new AmazonMediaStoreDataConfig
            {
                ServiceURL = "https://qdzszkjdcpfevy.data.mediastore.eu-central-1.amazonaws.com"
            };
            return new AmazonMediaStoreDataClient(credentials, config);
        }

        private static async Task PutObjectToContainerAsync(AmazonMediaStoreDataClient storeDataClient, string path)
        {
            var putRequest = new PutObjectRequest();

            using (var sr = new StreamReader("./SampleVideo/big_buck_bunny.mp4"))
            {
                putRequest.Body = sr.BaseStream;
                putRequest.Path = path;
                var response = await
                    storeDataClient.PutObjectAsync(putRequest);

                Console.WriteLine($"File was sent to AWS with status {response.HttpStatusCode}");
            }
        }

        private static async Task<Stream> GetObjectAsync(AmazonMediaStoreDataClient storeDataClient, string path)
        {
            var request = new GetObjectRequest
            {
                Path = path
            };

            try
            {
                var response = await storeDataClient.GetObjectAsync(request);
                Console.WriteLine($"Get file from AWS with code {response.StatusCode}");
                return response.Body;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Something goes wrong when getting item from AWS: {e.Message}");
            }

            return null;
        }

        private sealed class Credentials
        {
            public string AccessKey { get; set; }
            public string SecretKey { get; set; }
            public string DirctoryPath { get; set; }

        }

        #endregion
    }
}
