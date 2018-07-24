using Amazon;
using Amazon.IdentityManagement.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using System;
using System.Threading.Tasks;

namespace AwsMediaStorePrototype
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            var credentials = new BasicAWSCredentials("censored", "censored");

            var client = new AmazonSecurityTokenServiceClient(credentials);

            var request = new GetSessionTokenRequest {DurationSeconds = 900};
            var response = await client.GetSessionTokenAsync(request);

            Console.WriteLine(response.Credentials.AccessKeyId);
            Console.WriteLine(response.Credentials.SecretAccessKey);


            var credentials2 = response.Credentials;

            var client2 = new AmazonSecurityTokenServiceClient(credentials2);

            var req = new AssumeRoleRequest();
            req.DurationSeconds = 900;
            req.RoleArn = "arn:aws:s3:::my-test-bucket-coolrocket";
            req.RoleSessionName = "user1";

            //var x = await client2.AssumeRoleAsync(req);


            
            var s3client = new AmazonS3Client(response.Credentials, RegionEndpoint.EUCentral1);


            

            var r = new GetObjectRequest();
            r.BucketName = "my-test-bucket-coolrocket";
            r.Key = "user1/big_buck_bunny.mp4";
            //r.Path = "user1/big_buck_bunny.mp4";


            var temp = await s3client.GetObjectAsync(r);
            

            //using (var client = new AmazonMediaStoreClient(credentials, RegionEndpoint.EUCentral1))
            //using (var storeDataClient = await CreateStoreDataClientAsync(client: client, containerName: "test", credentials: credentials))
            //{
            //    //var user = await CreateIamUserAsync(credentials, "zero");
            //    //await DeleteIamUserAsync(credentials, "zero");
            //    //await DeleteObjectFromContainerAsync(storeDataClient, "id3/sample.mp4");

            //    var t = await GetContainerPolicyAsync(client, "test");
            //    //await PutObjectToContainerAsync(storeDataClient, "path2/sample.mp4");
            //    //var stream = await GetObjectAsync(storeDataClient, "path1/sample.mp4");

            //}

            Console.ReadKey();
        }


        private static async Task<User> CreateIamUserAsync(AWSCredentials credentials, string userName)
        {
            using (var client = new Amazon.IdentityManagement.AmazonIdentityManagementServiceClient(credentials,
                RegionEndpoint.EUCentral1))
            {
                var request = new CreateUserRequest(userName);
                var response = await client.CreateUserAsync(request);
                Console.WriteLine($"User {userName} was created.");


                //create creds for user
                var createKeyRequest = new CreateAccessKeyRequest { UserName = response.User.UserName };
                var accessKeyResponse = await client.CreateAccessKeyAsync(createKeyRequest);
                //add policy to user (demo)
                var attachUserPolicyRequest = new AttachUserPolicyRequest();
                attachUserPolicyRequest.UserName = userName;
                attachUserPolicyRequest.PolicyArn = "arn:aws:iam::aws:policy/AWSElementalMediaStoreFullAccess";
                await client.AttachUserPolicyAsync(attachUserPolicyRequest);


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
