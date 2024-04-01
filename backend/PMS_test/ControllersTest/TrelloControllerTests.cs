using project_manage_system_backend.Dtos;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using project_manage_system_backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using project_manage_system_backend.Services;
using project_manage_system_backend.Models;
using RichardSzalay.MockHttp;
using System.Net;
using Microsoft.Extensions.Configuration;
using project_manage_system_backend.Shares;
using Castle.Core.Configuration;
using Moq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using System.Security.Policy;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using project_manage_system_backend.Dtos.Gitlab;


namespace PMS_test.ControllersTest
{
    [TestCaseOrderer("XUnit.Project.Orderers.AlphabeticalOrderer", "XUnit.Project")]
    public class TrelloControllerTests : BaseControllerTests
    {

        private readonly TrelloController _trelloController;
        private readonly new HttpClient _client;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        // private readonly TrelloService _trelloService;
        private readonly Mock<TrelloService> _trelloService;

        private const string _owner = "shark";
        private const string _name = "Billie";




        TrelloLoginDto trelloLoginDto = new TrelloLoginDto{
            Name = "My Trello",
            BoardUrl = "https://trello.com/b/2G4Mm0Fj",
            ProjectId = 1
        };

        TrelloLoginDto dtoForFail = new TrelloLoginDto{
            Name = "My Trello",
            BoardUrl = "https://trello.com/fail",
            ProjectId = 1
        };


        private Project _project1;  

        
        public TrelloControllerTests() : base()
        {
            _dbContext.Database.EnsureCreated();
            InitialDatabase();
            _client = CreateMockClient();
            var _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var _configurationfail = new ConfigurationBuilder().AddJsonFile("appsettingsfail.json").Build();

            //var expectedUrl = "https://api.trello.com/1/search/members/?query=1&key=264ddae6871ae99fbc62b5003674d5ef&token=ATTA410a593e9a2a3eb220ac09419e70df56b327ab92346b74af064cde092f12f2e5230AA84E";
            //var expectedUrl = "https://api.trello.com/1/boards/6551cf2e6345bd69b9762e54?key=264ddae6871ae99fbc62b5003674d5ef&token=ATTA410a593e9a2a3eb220ac09419e70df56b327ab92346b74af064cde092f12f2e5230AA84E";
            //var _client = CreateMockClient(expectedUrl);
            // _trelloService = new TrelloService(_configuration,_dbContext, _client); //(IConfiguration configuration, PMSContext dbContext, HttpClient client = null) : base(dbContext)

            Mock<TrelloService> _mockService = new Mock<TrelloService>();
            _mockService.Setup(x => x.CreateTrelloRepo(trelloLoginDto, _owner)).ReturnsAsync(true);
            //TrelloService mockService = _mockService.Object;

            //_trelloService = new Mock<TrelloService>(_configuration, _dbContext, _client);
            //_trelloService.Setup(service => service.CreateTrelloRepo(It.IsAny<TrelloLoginDto>(), It.IsAny<string>())).ReturnsAsync(true);

            _trelloController = new TrelloController(_configuration, _dbContext);

            // 將模擬對象的實例設置到控制器中
            _trelloController.SetTrelloService(_mockService.Object); // 注意這裡使用了 .Object

            var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser")
            }));

            _trelloController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = fakeUser }
            };

            _trelloController = new TrelloController(_configuration, _dbContext);
            //_trelloService = new JiraService(_dbContext, _client);
            //_trelloController.setTrelloService(_trelloService);

        }

        private void InitialDatabase()
        {
            var user = new project_manage_system_backend.Models.User
            {
                Account = $"github_{_owner}",
                Authority = "User",
                AvatarUrl = null,
                Name = _owner,
                TrelloKey = "264ddae6871ae99fbc62b5003674d5ef",
                TrelloToken = "ATTA410a593e9a2a3eb220ac09419e70df56b327ab92346b74af064cde092f12f2e5230AA84E",
            };

            var userfake = new project_manage_system_backend.Models.User
            {
                Account = $"fake",
                Authority = "User",
                AvatarUrl = null,
                Name = _owner,
                TrelloKey = "123123132",
                TrelloToken = "ATTA410a593e9a2a3eb220ac09419e70df56b327ab92346b74af064cde092f12f2e5230AA84E",
            };

            var repo = new Repo
            {
                Name = _name,
                Owner = _owner,
                Url = $"https://github.com/{_owner}/{_name}"
            };

            var project = new Project
            {
                ID = 1,
                Name = "AAAA",
                //Owner = project_manage_system_backend.Models.User._owner,
                Repositories = new List<Repo>() { repo },
                Users = new List<UserProject>()
            };

            user.Projects.Add(new UserProject { Project = project });
            userfake.Projects.Add(new UserProject { Project = project });
            _dbContext.Users.Add(user);
            _dbContext.Users.Add(userfake);
            _dbContext.SaveChanges();
        }

        private HttpClient CreateMockClient() //string expectedUrl
        {
            var mockHttp = new MockHttpMessageHandler();
            string key = "264ddae6871ae99fbc62b5003674d5ef";
            string token = "ATTA410a593e9a2a3eb220ac09419e70df56b327ab92346b74af064cde092f12f2e5230AA84E";
            string expectedUrl = "https://api.trello.com/1/boards/6551cf2e6345bd69b9762e54?key=264ddae6871ae99fbc62b5003674d5ef&token=ATTA410a593e9a2a3eb220ac09419e70df56b327ab92346b74af064cde092f12f2e5230AA84E";
            mockHttp.When(HttpMethod.Get, expectedUrl)
                 .Respond("6551cf2e6345bd69b9762e54", ""); //application/json
            mockHttp.Fallback.WithAny().Respond(HttpStatusCode.BadRequest);

            //string jsonUrl = url + ".json?" + $"key={key}&token={token}";
            return mockHttp.ToHttpClient();
        }

        private string getResponseOfBoardDetail()
        {
            string responseData = @"{
                
            }";

            return responseData;
        }

        /*
        [Fact]

        public void TestCheckUserBindStatusSuccess()
        {
            IActionResult iActionResult = _trelloController.CheckUserBindStatus();
            var okResult = iActionResult as OkObjectResult;
            var responseDto = okResult.Value as ResponseDto;

            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal("Added Success", responseDto.message);
            Assert.True(responseDto.success);

        }
        */

        /*
        [Fact]
        public async void TestCreateTrelloRepoSuccess() {

            // Arrange
            // var trelloLoginDto = new TrelloLoginDto();

            // Stubbing（偽造） CreateTrelloRepo 方法
            //_trelloService.Setup(service => service.CreateTrelloRepo(It.IsAny<TrelloLoginDto>(), It.IsAny<string>())).ReturnsAsync(true);

            // Act
            // Console.WriteLine("Some serious debugging happening here...");
            // IActionResult iActionResult = await _trelloController.CreateTrelloRepo(trelloLoginDto);

            // Assert
            // var okResult = Assert.IsType<OkObjectResult>(iActionResult);
            // var responseDto = Assert.IsType<ResponseDto>(okResult.Value);

            //var okResult = iActionResult as OkObjectResult;
            //var responseDto = okResult.Value as ResponseDto;

            // Assert.Equal(200, okResult.StatusCode);
            // Assert.Equal("Added Success", responseDto.message);
            // Assert.True(responseDto.success);
        }

        */
        /*

        [Fact]
        public void TestCreateTrelloRepoFail() {
            IActionResult iActionResult = _trelloController.CreateTrelloRepo(dtoForFail);
            var okResult = iActionResult as OkObjectResult;
            var responseDto = okResult.Value as ResponseDto;

            Assert.Equal(200, okResult.StatusCode);
            Assert.False(responseDto.success);
            Assert.True(responseDto.message.Contains("Added Error: One or more errors occurred."));
        }
        
        [Fact]
        public void TestGetBoardInfo() {l
            Task<IActionResult> iActionResult = _trelloController.GetBoardInfo(trelloLoginDto);
            var okResult = iActionResult.Result as OkObjectResult;
            Assert.Equal(200, okResult.StatusCode);
        }
        */

        /*
        [Fact]
        public void CheckUserBindStatusReturnsOkResponse()
        {
            // Act
            var result = _trelloController.CheckUserBindStatus();

            // Assert
            // var okResult = Assert.IsType<OkObjectResult>(result);
            // _trelloService.Verify(service => service.CheckUserBindStatus("testuser"), Times.Once);
            // Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public void CheckUserBindStatus_ReturnsErrorResponseOnException()
        {
            // Arrange
            _trelloService.Setup(service => service.CheckUserBindStatus("testuser"))
                .Throws(new System.Exception("Some error"));

            // Act
            var result = _trelloController.CheckUserBindStatus();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseDto = okResult.Value as ResponseDto;
            Assert.False(responseDto.success);
            Assert.Equal("Checked Error: Some error", responseDto.message);
        }
        */
    }

}