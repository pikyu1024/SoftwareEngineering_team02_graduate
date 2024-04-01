using Moq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using project_manage_system_backend.Dtos;
using project_manage_system_backend.Services;
using project_manage_system_backend.Shares;
using project_manage_system_backend.Models;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Net;
using System.Data.Common;
using System.Net.Http;
using System;
using Xunit;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using project_manage_system_backend.Dtos.Gitlab;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Xml.Linq;
using System.Linq;
using Isopoh.Cryptography.Argon2;


namespace PMS_test.ServicesTest
{
    [TestCaseOrderer("XUnit.Project.Orderers.AlphabeticalOrderer", "XUnit.Project")]
    public class TrelloServiceTest
    {
        private readonly PMSContext _dbContext;
        private readonly HttpClient _client;
        private readonly TrelloService _trelloService;
        private readonly TrelloService _trelloServiceFail;

        // public Mock<TrelloService> mockTrelloService = new Mock<TrelloService>();

        private readonly IConfiguration _configuration;

        private const string _owner = "shark";
        private const string _name = "Billie";
        //private const string _failFakeRepository = "https://github.com/" + _owner + "/" + _name;
        //private const string _successFakeRepository = "https://github.com/" + _owner + "/testRepo";
        //private const string _successFakeSonarqube = "http://192.168.0.1/api/project_analyses/search?project=ppp";


        TrelloLoginDto trelloLoginDto = new TrelloLoginDto
        {
            Name = "My Trello",
            BoardUrl = "https://trello.com/b/2G4Mm0Fj",
            ProjectId = 1
        };

        TrelloLoginDto dtoForFail = new TrelloLoginDto
        {
            Name = "My Trello",
            BoardUrl = "https://trello.com/fail",
            ProjectId = 1
        };

        TrelloLoginDto trelloLoginDtoNoData = new TrelloLoginDto
        {
            Name = "",
            BoardUrl = "",
            ProjectId = 1
        };

        TrelloLoginDto trelloLoginDtoNull = new TrelloLoginDto();

        TrelloBoardAuthorizeDto trelloBoardAuthorizeDto = new TrelloBoardAuthorizeDto()
        {
            BoardId = "6551cf2e6345bd69b9762e54",
            Account = $"github_{_owner}",
        };

        TrelloBoardAuthorizeDto trelloBoardAuthorizeDtoFail = new TrelloBoardAuthorizeDto()
        {
            BoardId = "123333",
            Account = $"github_{_owner}",
        };

        TrelloListAuthorizeDto trelloListAuthorizeDto = new TrelloListAuthorizeDto()
        {
            ListId = "658a93dac515eb754e1896fc",
            Account = "123"
        };

        TrelloListAuthorizeDto trelloListAuthorizeDtoFail = new TrelloListAuthorizeDto()
        {
            ListId = "false",
            Account = "123"
        };

        TrelloInboundAddCardDto trelloAddCardDto = new TrelloInboundAddCardDto()
        {
            ListId = "658a93dac515eb754e1896fc",
            Name = "new_card"
        };

        TrelloInboundAddCardDto trelloAddCardDtoFail = new TrelloInboundAddCardDto()
        {
            ListId = "falseId",
            Name = "false new card"
        };

        TrelloCardUpdateDto trelloCardUpdateDtoInvalidCardId = new TrelloCardUpdateDto()
        {
            CardId = "InvalidCardId",
            Desc = "",
            ListId = "1",
            CardName = "Name"
        };


        List<BoardDetailDto> boardDetailDtos = new List<BoardDetailDto>();


        private Project _project1;

        public TrelloServiceTest()
        {
            /*
            var input = "TrelloAPI:APIUrl";
            var output = "https://api.trello.com/1";

            Func<string, string> GetValue = x => output;

            Mock<IConfiguration> _mockIConfiguration = new Mock<IConfiguration>();
            _mockIConfiguration.Setup(GetValue).Returns(output);
            IConfiguration configuration = _mockIConfiguration.Object;
            */

            var _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var _configurationfail = new ConfigurationBuilder().AddJsonFile("appsettingsfail.json").Build();


            //string clientId = _configuration.GetValue<string>("githubConfig:client_id");
            //string clientSecret = _configuration.GetValue<string>("githubConfig:client_secret");

            _dbContext = new PMSContext(new DbContextOptionsBuilder<PMSContext>()
               .UseSqlite(CreateInMemoryDatabase()).Options);
            _dbContext.Database.EnsureCreated();
            InitialDatabase();
            // _client = CreateMockClient();
            _client = new HttpClient();
            _trelloService = new TrelloService(_configuration, _dbContext, _client);
            _trelloServiceFail = new TrelloService(_configurationfail, _dbContext, _client);


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

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("DataSource=:memory:");

            connection.Open();

            return connection;
        }

      

        private HttpResponseMessage getResponseOfBoardDetailFail() {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
            return httpResponseMessage;
        }

        private HttpClient CreateMockClient()
        {
            var mockHttp = new MockHttpMessageHandler();

            // mockHttp.When(HttpMethod.Get, "https://DomainURL.atlassian.net/rest/agile/1.0/board")
            //     .Respond("application/json", getResponseOfBoardDetail());
            // mockHttp.Fallback.WithAny().Respond(HttpStatusCode.BadRequest);


            return mockHttp.ToHttpClient();
        }

        
        // CheckTrelloBindInfoExist
        [Fact]
        public async void TestCheckTrelloBindInfoExistFail()
        {
            // fake fail key
            string key = "264ddae6871ae99fbc62b5003674d5eh";
            string token = "ATTA410a593e9a2a3eb220ac09419e70df56b327ab92346b74af064cde092f12f2e5230AA84E";
            bool response = await _trelloService.CheckTrelloBindInfoExist(key, token);

            Assert.False(response);
        }

        [Fact]
        public async void TestCheckTrelloBindInfoExistSuccess()
        {
            // true key
            string key = "264ddae6871ae99fbc62b5003674d5ef";
            string token = "ATTA410a593e9a2a3eb220ac09419e70df56b327ab92346b74af064cde092f12f2e5230AA84E";
            bool response = await _trelloService.CheckTrelloBindInfoExist(key, token);

            Assert.True(response);
        }


        // GetBoardIdByUrl
        [Fact]
        public async void TestGetBoardIdByUrlSuccess()
        {
            string key = "264ddae6871ae99fbc62b5003674d5ef";
            string token = "ATTA410a593e9a2a3eb220ac09419e70df56b327ab92346b74af064cde092f12f2e5230AA84E";
            string response = await _trelloService.GetBoardIdByUrl("https://trello.com/b/2G4Mm0Fj", key, token);
            Assert.IsType<string>(response);
            Assert.Equal("6551cf2e6345bd69b9762e54", response);

        }

        [Fact]
        public async void TestGetBoardIdByUrlFail()
        {
            // fake fail key
            string key = "264ddae6871ae99fbc62b5003674d5eh";
            string token = "ATTA410a593e9a2a3eb220ac09419e70df56b327ab92346b74af064cde092f12f2e5230AA84E";
            string response = await _trelloService.GetBoardIdByUrl("https://trello.com/b/2G4Mm0Fj", key, token);
            Assert.IsType<string>(response);
            Assert.Equal("None", response);

        }

        //CheckTrelloRepoIsExist
        [Fact]
        public async void TestCheckTrelloRepoIsExistSuccess()
        {
            // true id
            string boardId = "659a70e2ef817012a212bb2a";
            string key = "264ddae6871ae99fbc62b5003674d5ef";
            string token = "ATTA410a593e9a2a3eb220ac09419e70df56b327ab92346b74af064cde092f12f2e5230AA84E";
            bool response = await _trelloService.CheckTrelloRepoIsExist(boardId, key, token);
            Assert.True(response);
        }

        [Fact]
        public async void TestCheckTrelloRepoIsExistFail()
        {
            // fail id
            string boardId = "659a70";
            string key = "264ddae6871ae99fbc62b5003674d5ef";
            string token = "ATTA410a593e9a2a3eb220ac09419e70df56b327ab92346b74af064cde092f12f2e5230AA84E";
            bool response = await _trelloService.CheckTrelloRepoIsExist(boardId, key, token);
            Assert.False(response);
        }

        // CheckTrelloRepo
        [Fact]
        public async void TestCheckTrelloRepoSuccess()
        {
            string user_id = "github_shark";
            bool result = await _trelloService.CheckTrelloRepo(trelloLoginDto, user_id);
            Assert.True(result);
        }

        [Fact]
        public async Task TestCheckTrelloRepoNoUser()
        {
            string user_id = "55688";

            var exception = await Assert.ThrowsAsync<Exception>(() => _trelloService.CheckTrelloRepo(trelloLoginDto, user_id));
            Assert.Equal("Can't Find This User", exception.Message);
        }

        [Fact]
        public async Task TestCheckTrelloRepoInvalidBoardUrl()
        {
            string user_id = "github_shark";
            var exception = await Assert.ThrowsAsync<Exception>(() => _trelloService.CheckTrelloRepo(dtoForFail, user_id));
            Assert.Equal("Invalid BoardUrl", exception.Message);
        }

        [Fact]
        public async Task TestCheckTrelloRepoInvalidRepo()
        {
            string user_id = "fake";
            var exception = await Assert.ThrowsAsync<Exception>(() => _trelloService.CheckTrelloRepo(dtoForFail, user_id));
            Assert.Equal("Invalid BoardUrl", exception.Message);
        }

        // create Trello Repo
        [Fact]
        public async void TestCreateTrelloRepoSuccess()
        {
            string user_id = "github_shark";
            bool result = await _trelloService.CreateTrelloRepo(trelloLoginDto, user_id);
            Assert.True(result);
        }

        [Fact]
        public async Task TestCreateTrelloRepoNoUser()
        {
            string user_id = "happyhappyhappy";
            var exception = await Assert.ThrowsAsync<Exception>(() => _trelloService.CheckTrelloRepo(trelloLoginDto, user_id));
            Assert.Equal("Can't Find This User", exception.Message);
        }

        [Fact]
        public async Task TestCreateTrelloRepoInvalidBoardId()
        {
            string user_id = "github_shark";
            var exception = await Assert.ThrowsAsync<Exception>(() => _trelloService.CheckTrelloRepo(dtoForFail, user_id));
            Assert.Equal("Invalid BoardUrl", exception.Message);
        }

        // skip the CreateTrelloRepo: Create Repository Fail


        // GetListsInfo
        [Fact]
        public async Task TestGetListsInfoSuccess()
        {
            string user_id = "github_shark";
            var result = await _trelloService.GetListsInfo(trelloBoardAuthorizeDto, user_id);
            Assert.NotNull(result);

        }

        [Fact]
        public async Task TestGetListsInfoFail()
        {
            string user_id = "github_shark";
            var result = _trelloService.GetListsInfo(trelloBoardAuthorizeDtoFail, user_id);
            Assert.Empty(await result.ConfigureAwait(false));

        }

        // AddCardsInList
        [Fact]
        public async Task TestGetCardsInListSuccessAsync()
        {
            string user_id = "github_shark";
            var result = await _trelloService.GetCardsInList(trelloListAuthorizeDto, user_id);
            // Assert.NotNull(result);
            Assert.Equal(1, result.Count);
        }

        [Fact]
        public async Task TestGetCardsInListFail ()
        {
            string user_id = "github_shark";
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>_trelloService.GetCardsInList(trelloListAuthorizeDtoFail, user_id));
            Assert.Equal("get trello cards fail", exception.Message);
        }

        [Fact]
        public async Task TestAddCardsInListSuccessAsync()
        {
            IConfiguration config = new ConfigurationBuilder().Build();
            //Assert.True(config);
            string user_id = "github_shark";
            var result = await _trelloService.AddCardsInList(trelloAddCardDto, user_id);
            Assert.True(result);
        }

        [Fact]
        public async Task TestAddCardsInListInvalidList()
        {
            string user_id = "github_shark";
            var result = await _trelloService.AddCardsInList(trelloAddCardDtoFail, user_id);
            Assert.False(result);
        }

        [Fact]
        public async Task TestAddCardsInListFail()
        {
            string user_id = "github_shark";
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _trelloServiceFail.AddCardsInList(trelloAddCardDtoFail, user_id));
            Assert.Equal("add card in list fail", exception.Message);
        }
        
        [Fact]
        public async Task TestDeleteCardsInListFail()
        {
            IConfiguration config = new ConfigurationBuilder().Build();
            //Assert.True(config);
            string user_id = "github_shark";
            var result = await _trelloService.DeleteTrelloCardInfo(trelloCardUpdateDtoInvalidCardId, user_id);
            Assert.False(result);

        }

        /*
        public async void TestCheckTrelloRepoNoUser()
        {

            Assert.False(response);
        }
        public async void TestCheckTrelloRepoNoBoardId()
        {

            Assert.True(response);
        }
        */

        /*[Fact]
        public void Testnouser_CheckUserBindStatus()
        {
            String user_id = "github_zoezou9";
            //var exception = await Record.ExceptionAsync(() => _trelloService.CheckUserBindStatus(user_id));
            var exception = Assert.Throws<Exception>(() => _trelloService.CheckUserBindStatus(user_id));
            Assert.Equal("Can't Find This User", exception.Message);
            //Assert.Null(exception);

        }
        */


    }
}