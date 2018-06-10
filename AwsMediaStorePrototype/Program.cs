using Amazon.MediaStoreData;
using Amazon.MediaStoreData.Model;
using Amazon.Runtime;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AwsMediaStorePrototype
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            var credentials = new BasicAWSCredentials("AKIAJ3MNQL3LXNADQWAA", "fgr1QZQcxQ381wMhL49YrOX9eO+8+MyThTv1axJE");
            var config = new AmazonMediaStoreDataConfig
            {
                ServiceURL = "https://qdzszkjdcpfevy.data.mediastore.eu-central-1.amazonaws.com"
            };

            using (var storeDataClient = new AmazonMediaStoreDataClient(credentials, config))
            {

                var putRequest = new PutObjectRequest();

                using (var sr = new StreamReader(@"e:\big_buck_bunny.mp4"))
                {
                    //putRequest.Body = sr.BaseStream;
                    //putRequest.Path = "id2/new.mp4";
                    //var response =
                    //    storeDataClient.PutObjectAsync(putRequest);

                    //Console.WriteLine(response.Result.HttpStatusCode);
                }

            }

            Console.WriteLine("Guest works start");

            credentials = new BasicAWSCredentials("AKIAJABNRP4XHNM3I7JA", "QqtYmQdxZyeaTw1FcVq15qrz7nl8m6dTI6q5zbee");
            config = new AmazonMediaStoreDataConfig
            {
                ServiceURL = "https://qdzszkjdcpfevy.data.mediastore.eu-central-1.amazonaws.com"
            };

            using (var storeDataClient = new AmazonMediaStoreDataClient(credentials, config))
            {

                var request = new GetObjectRequest
                {
                    Path = "id2/new.mp4"
                };

                try
                {
                    var response = await storeDataClient.GetObjectAsync(request);
                    Console.WriteLine(response.StatusCode);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }


            Console.ReadKey();
        }
    }
}
