using Amazon;
using Amazon.IdentityManagement.Model;
using Amazon.Runtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaStoreUserController : ControllerBase
    {
        public AwsOptions AwsOptions { get; set; }


        public MediaStoreUserController(IOptions<AwsOptions> options)
        {
            AwsOptions = options.Value;
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            return Ok("Running...");
        }

        [HttpPost]
        public async Task<UserDto> Post([FromBody] UserDto dto)
        {
            var credentials = new BasicAWSCredentials(AwsOptions.Key, AwsOptions.Password);
            var response = await CreateIamUserAsync(credentials, dto.Name);
            return new UserDto
                {
                    Name = response.Item1.UserName,
                    Key = response.Item2.AccessKeyId,
                    Pass = response.Item2.SecretAccessKey
                };
            
        }

        private static async Task<(User, AccessKey)> CreateIamUserAsync(AWSCredentials credentials, string userName)
        {
            using (var client = new Amazon.IdentityManagement.AmazonIdentityManagementServiceClient(credentials,
                RegionEndpoint.EUCentral1))
            {
                var request = new CreateUserRequest(userName);
                var response = await client.CreateUserAsync(request);
                Console.WriteLine($"User {userName} was created.");

                //create creds for user
                var createKeyRequest = new CreateAccessKeyRequest {UserName = response.User.UserName};
                var accessKeyResponse = await client.CreateAccessKeyAsync(createKeyRequest);
                //add policy to user (demo)
                var attachUserPolicyRequest = new AttachUserPolicyRequest();
                attachUserPolicyRequest.UserName = userName;
                attachUserPolicyRequest.PolicyArn = "arn:aws:iam::aws:policy/AWSElementalMediaStoreFullAccess";
                await client.AttachUserPolicyAsync(attachUserPolicyRequest);

                return (response.User, accessKeyResponse.AccessKey);
            }
        }

        public class UserDto
        {
            public string Name { get; set; }
            public string Key { get; set; }
            public string Pass { get; set; }
        }
    }
}
