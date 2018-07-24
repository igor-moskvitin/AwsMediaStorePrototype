using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;

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
        public Dto Post([FromBody] Dto dto)
        {
            var credentials = new BasicAWSCredentials(AwsOptions.Key, AwsOptions.Password);
            using (var s3Client = new AmazonS3Client(credentials, RegionEndpoint.EUCentral1))
            {
                var key = Guid.NewGuid();
                var path = "users/" + key + ".ext";
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = "my-test-bucket-coolrocket",
                    Key = path,
                    Expires = DateTime.Now.AddHours(1)
                };

                var url = s3Client.GetPreSignedURL(request);
                var response = new Dto
                {
                    FileKey = key.ToString(),
                    FileExtension = dto.FileExtension,
                    FileUrl = url
                };

                return response;
            }
        }


        public class Dto
        {
            public string FileKey { get; set; }
            public string FileExtension { get; set; }
            public string FileUrl { get; set; }
        }
    }
}
