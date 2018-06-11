using Amazon.MediaStoreData;
using Amazon.MediaStoreData.Model;
using Amazon.Runtime;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.IdentityManagement.Model;
using Amazon.MediaStore;
using Amazon.MediaStore.Model;

namespace AwsMediaStorePrototype
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            var credentials = new BasicAWSCredentials("CENSORED", "CENSORED");

            using (var client = new AmazonMediaStoreClient(credentials, RegionEndpoint.EUCentral1))
            using (var storeDataClient = await CreateStoreDataClientAsync(client: client, containerName: "test", credentials: credentials))
            {
                //var user = await CreateIamUserAsync(credentials, "zero");
                //await DeleteIamUserAsync(credentials, "zero");
                //await DeleteObjectFromContainerAsync(storeDataClient, "id3/sample.mp4");

                await PutObjectToContainerAsync(storeDataClient, "id3/sample.mp4");
                var stream = await GetObjectAsync(storeDataClient, "id3/sample.mp4");

            }

            Console.ReadKey();
        }

        private static async Task<AmazonMediaStoreDataClient> CreateStoreDataClientAsync(AmazonMediaStoreClient client, string containerName, BasicAWSCredentials credentials)
        {
            var request = new ListContainersRequest();
            var containers = await client.ListContainersAsync(request);
            var container = containers.Containers.SingleOrDefault(c => c.Name == containerName);
            var config = new AmazonMediaStoreDataConfig
            {
                ServiceURL = container.Endpoint
            };
            return new AmazonMediaStoreDataClient(credentials, config);
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

        private static async Task DeleteObjectFromContainerAsync(AmazonMediaStoreDataClient storeDataClient, string path)
        {
            var deleteRequest = new DeleteObjectRequest {Path = path};
            try
            {
                var result = await storeDataClient.DeleteObjectAsync(deleteRequest);
                Console.WriteLine($"File was deleted from AWS with status {result.HttpStatusCode}");
            }
            catch (Amazon.MediaStoreData.Model.ObjectNotFoundException ex)
            {
                Console.WriteLine($"File was not found in container ({ex.Message})");
                
            }
        }

        /// <summary>
        /// creating container take several minutes!
        /// </summary>
        /// <param name="client"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static async Task<Container> CreateContainerAsync(AmazonMediaStoreClient client, string name)
        {
            var request = new CreateContainerRequest {ContainerName = name};
            Console.WriteLine($"Creating container {name}...");
            try
            {
                var response = await client.CreateContainerAsync(request);
                Console.WriteLine($"Container {name} was created with status code {response.HttpStatusCode}");
                return response.Container;
            }
            catch (Exception)
            {

                return null;
            }
        }

        private static async Task DeleteContainerAsync(AmazonMediaStoreClient client, string name)
        {
            var request = new DeleteContainerRequest { ContainerName = name };
            Console.WriteLine($"Deleteing container {name}...");
            try
            {
                var response = await client.DeleteContainerAsync(request);
                Console.WriteLine($"Container {name} was created with status code {response.HttpStatusCode}");
            }
            catch (ContainerInUseException ex)
            {
                Console.WriteLine($"Container is in use ({ex.Message})");
            }
            catch (ObjectNotFoundException ex)
            {
                Console.WriteLine($"Container was not found ({ex.Message})");
            }
            
        }

        private static async Task PutContainerPolicyAsync(AmazonMediaStoreClient client, string containerName, string policy)
        {
            var request = new PutContainerPolicyRequest
            {
                ContainerName = containerName,
                Policy = policy
            };
            var response = await client.PutContainerPolicyAsync(request);
            Console.WriteLine($"Policy for container {containerName} was updated with status code {response.HttpStatusCode}");
        }


        /// <summary>
        /// На данный момент не ясно почему метод падает с <see cref="PolicyNotFoundException"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        private static async Task<string> GetContainerPolicyAsync(AmazonMediaStoreClient client, string containerName)
        {
            var request = new GetContainerPolicyRequest {ContainerName = containerName};

            try
            {
                var response = await client.GetContainerPolicyAsync(request);
                Console.WriteLine($"Policy from container {containerName} was retrived with status code {response.HttpStatusCode}");
                return response.Policy;
            }
            catch (PolicyNotFoundException ex)
            {
                Console.WriteLine($"Policy was not found {ex.Message}");
                return String.Empty;
            }
        }


        private static async Task<User> CreateIamUserAsync(AWSCredentials credentials, string userName)
        {
            using (var client = new Amazon.IdentityManagement.AmazonIdentityManagementServiceClient(credentials,
                RegionEndpoint.EUCentral1))
            {
                var request = new CreateUserRequest(userName);
                var response = await client.CreateUserAsync(request);
                Console.WriteLine($"User {userName} was created.");
                return response.User;
            }
        }

        private static async Task DeleteIamUserAsync(AWSCredentials credentials, string userName)
        {
            using (var client = new Amazon.IdentityManagement.AmazonIdentityManagementServiceClient(credentials,
                RegionEndpoint.EUCentral1))
            {
                var request = new DeleteUserRequest(userName);
                await client.DeleteUserAsync(request);
                Console.WriteLine($"User {userName} was deleted.");
            }
        }
    }
}
