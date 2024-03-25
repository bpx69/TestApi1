using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Moq.EntityFrameworkCore;
using TestApi1.Authentication;
using TestApi1.Controllers;
using TestApi1.Model;
using TestApi1.ViewModel;

namespace TestApi1UnitTests
{
    [TestClass]
    public class TestApi1Tests
    {
        string _client1 = "1AbecedA2Razreda3Klopi4Dni5Sinov6Kolov?";
        Guid _clientGuid1 = new Guid("{617867E5-1B5F-45F4-8BDC-96A9109C3A27}");
        string _client2 = "100MuGromovaIJelenskihMiRogovaTakoMiChuluDedeVrachIBubnjevaDarkw00da!";
        Guid _clientGuid2 = new Guid("{B5B2C0D2-6F18-41AB-B6C0-4E8A361B0765}");
        DefaultHttpContext _httpContext;
        Moq.Mock<UserAPIDbContext> _context;
        UserController _controller;
        TestKeyVerifier _middleware;

        public TestApi1Tests()
        {
            _context = new Moq.Mock<UserAPIDbContext>();
            _middleware = new TestKeyVerifier(new ApiKeyValidation(_context.Object));
            var logger = new Moq.Mock<ILogger<UserController>>();
            _httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext()
            {
                HttpContext = _httpContext
            };
            _controller = new UserController(_context.Object, logger.Object) { ControllerContext = controllerContext };
            var clients = new List<ClientDbRecord>() {
                    { new ClientDbRecord( _clientGuid1, _client1, "Osnova šola") },
                    { new ClientDbRecord( _clientGuid2, _client2, "Duh s sekiro") }
            };
            _context.Setup(x => x.Clients).ReturnsDbSet(clients);
        }

        public void CompareUserDbRecordWithData(UserDbRecord r, UserDTO d)
        {
            Assert.AreEqual(r.Id, new Guid(d.Id));
            Assert.AreEqual(r.UserName, d.UserName);
            Assert.AreEqual(r.FullName, d.FullName);
            Assert.AreEqual(r.EMail, d.EMail);
            Assert.AreEqual(r.MobilePhoneNumber, d.MobilePhoneNumber);
            Assert.AreEqual(r.Language, d.Language);
            Assert.AreEqual(r.Culture, d.Culture);
        }

        [TestMethod]
        public async Task SimpleCreate()
        {
            UserDbRecord? addedRecord = null;
            _httpContext.Request.Headers[Constants.ApiKeyHeaderName] = _client1;
            _context.Setup(x => x.Users).ReturnsDbSet(Enumerable.Empty<UserDbRecord>());
            _context.Setup(x => x.Users.Add(Moq.It.IsAny<UserDbRecord>())).Callback<UserDbRecord>(r => addedRecord = r);
            UserWithPasswordDTO data = new UserWithPasswordDTO(
                "5A1A930F-75D1-4C08-9964-9B410EB1DA85",
                "yoyo",
                "Johnny B. Goode",
                "YoYo@yoyo.org",
                "1234567890",
                "Slovingliš",
                "en-US",
                "MickeyMouse");

            await _middleware.InvokeAsync(_httpContext);

            var result = await _controller.CreateUserDbRecord(data);

            Assert.IsInstanceOfType<OkObjectResult>(result.Result);
            var value = ((ObjectResult)result.Result).Value as UserDTO;
            Assert.IsTrue(value != null && value.Equals(data));
            Assert.IsNotNull(addedRecord);
            CompareUserDbRecordWithData(addedRecord, data);
            Assert.IsNotNull(addedRecord?.PasswordHash);
            Assert.AreEqual(addedRecord?.ClientId, _clientGuid1);
        }

        [TestMethod]
        public async Task DuplicateRecord()
        {
            _httpContext.Request.Headers[Constants.ApiKeyHeaderName] = _client1;
            UserWithPasswordDTO data = new UserWithPasswordDTO(
                "5A1A930F-75D1-4C08-9964-9B410EB1DA85",
                "yoyo",
                "Johnny B. Goode",
                "YoYo@yoyo.org",
                "1234567890",
                "Slovingliš",
                "en-US",
                "MickeyMouse");
            UserDbRecord rec1 = new UserDbRecord(new Guid(data.Id),
                data.UserName, data.FullName, data.EMail, data.MobilePhoneNumber,
                data.Language, data.Culture, "hush", _clientGuid1);
            List<UserDbRecord> recordList = new List<UserDbRecord>() { rec1 };
            _httpContext.Request.Headers[Constants.ApiKeyHeaderName] = _client1;

            _context.Setup(x => x.Users).ReturnsDbSet(recordList);
            _context.Setup(x => x.SaveChangesAsync(Moq.It.IsAny<CancellationToken>())).Throws(new DbUpdateException());

            await _middleware.InvokeAsync(_httpContext);

            var result = await _controller.CreateUserDbRecord(data);

            Assert.IsInstanceOfType<ConflictResult>(result.Result);
        }

        [TestMethod]
        public async Task InvalidData()
        {
            _httpContext.Request.Headers[Constants.ApiKeyHeaderName] = _client1;
            _context.Setup(x => x.Users).ReturnsDbSet(Enumerable.Empty<UserDbRecord>());

            UserWithPasswordDTO data = new UserWithPasswordDTO(
                "5A1A930F-75D1-4C08-9964-9B410EB1DA85",
                "yoyo",
                "Johnny B. Goode",
                "YoYo@yoyo.org",
                "1234567890",
                "Slovingliš",
                "InvalidCulture",
                "MickeyMouse");

            await _middleware.InvokeAsync(_httpContext);

            var result = await _controller.CreateUserDbRecord(data);

            Assert.IsInstanceOfType<BadRequestResult>(result.Result);
        }

        [TestMethod]
        public async Task WrongApiKey()
        {
            _httpContext.Request.Headers[Constants.ApiKeyHeaderName] = "Wrong!";
            _context.Setup(x => x.Users).ReturnsDbSet(Enumerable.Empty<UserDbRecord>());

            UserWithPasswordDTO data = new UserWithPasswordDTO(
                "5A1A930F-75D1-4C08-9964-9B410EB1DA85",
                "yoyo",
                "Johnny B. Goode",
                "YoYo@yoyo.org",
                "1234567890",
                "Slovingliš",
                "en-US",
                "MickeyMouse");

            await _middleware.InvokeAsync(_httpContext);

            Assert.AreEqual(_httpContext.Response.StatusCode, (int)HttpStatusCode.Unauthorized);

            if (_httpContext.Response.StatusCode != (int)HttpStatusCode.Unauthorized)
            {
                var result = await _controller.CreateUserDbRecord(data);

                Assert.IsInstanceOfType<UnauthorizedResult>(result.Result);
            }
        }

        [TestMethod]
        public async Task SimpleUpdate()
        {
            _httpContext.Request.Headers[Constants.ApiKeyHeaderName] = _client1;

            UserWithPasswordDTO data = new UserWithPasswordDTO(
                "5A1A930F-75D1-4C08-9964-9B410EB1DA85",
                "yoyo",
                "Johnny B. Goode",
                "YoYo@yoyo.org",
                "1234567890",
                "Slovingliš",
                "en-US",
                "MickeyMouse");
            UserDbRecord rec1 = new UserDbRecord(new Guid(data.Id),
                    data.UserName, data.FullName, data.EMail, data.MobilePhoneNumber,
                    data.Language, data.Culture, "hush", _clientGuid1);
            List<UserDbRecord> recordList = new List<UserDbRecord>() { rec1 };
            _httpContext.Request.Headers[Constants.ApiKeyHeaderName] = _client1;

            _context.Setup(x => x.Users).ReturnsDbSet(recordList);
            UserDbRecord? updatedRecord = null;

            _context.Setup(x => x.SetAsModified(Moq.It.IsAny<UserDbRecord>())).Callback<UserDbRecord>(r => updatedRecord = r);
            _context.Setup(x => x.Users.FindAsync(Moq.It.IsAny<Guid>())).Returns(ValueTask.FromResult<UserDbRecord>(rec1)!);

            await _middleware.InvokeAsync(_httpContext);

            var result = await _controller.UpdateUserDbRecord("5A1A930F-75D1-4C08-9964-9B410EB1DA85", data);

            Assert.IsInstanceOfType<OkObjectResult>(result.Result);
            Assert.IsNotNull(updatedRecord);
            CompareUserDbRecordWithData(updatedRecord, data);
            Assert.IsNotNull(updatedRecord?.PasswordHash);
            Assert.AreEqual(updatedRecord?.ClientId, _clientGuid1);
        }

        [TestMethod]
        public async Task SimpleGet()
        {
            _httpContext.Request.Headers[Constants.ApiKeyHeaderName] = _client1;

            UserWithPasswordDTO data = new UserWithPasswordDTO(
                "5A1A930F-75D1-4C08-9964-9B410EB1DA85",
                "yoyo",
                "Johnny B. Goode",
                "YoYo@yoyo.org",
                "1234567890",
                "Slovingliš",
                "en-US",
                "MickeyMouse");
            UserDbRecord rec1 = new UserDbRecord(new Guid(data.Id),
                    data.UserName, data.FullName, data.EMail, data.MobilePhoneNumber,
                    data.Language, data.Culture, "hush", _clientGuid1);
            List<UserDbRecord> recordList = new List<UserDbRecord>() { rec1 };
            _httpContext.Request.Headers[Constants.ApiKeyHeaderName] = _client1;

            _context.Setup(x => x.Users).ReturnsDbSet(recordList);
            _context.Setup(x => x.Users.FindAsync(Moq.It.IsAny<Guid>())).Returns(ValueTask.FromResult<UserDbRecord>(rec1)!);

            await _middleware.InvokeAsync(_httpContext);

            var result = await _controller.GetUserDbRecord(new Guid("5A1A930F-75D1-4C08-9964-9B410EB1DA85"));

            Assert.IsInstanceOfType<OkObjectResult>(result.Result);
            var value = ((ObjectResult)result.Result).Value as UserDTO;
            Assert.IsTrue(value != null && value.Equals(data));
        }

        [TestMethod]
        public async Task SimpleDelete()
        {
            _httpContext.Request.Headers[Constants.ApiKeyHeaderName] = _client1;

            UserWithPasswordDTO data = new UserWithPasswordDTO(
                "5A1A930F-75D1-4C08-9964-9B410EB1DA85",
                "yoyo",
                "Johnny B. Goode",
                "YoYo@yoyo.org",
                "1234567890",
                "Slovingliš",
                "en-US",
                "MickeyMouse");
            UserDbRecord rec1 = new UserDbRecord(new Guid(data.Id),
                    data.UserName, data.FullName, data.EMail, data.MobilePhoneNumber,
                    data.Language, data.Culture, "hush", _clientGuid1);
            List<UserDbRecord> recordList = new List<UserDbRecord>() { rec1 };
            _httpContext.Request.Headers[Constants.ApiKeyHeaderName] = _client1;

            _context.Setup(x => x.Users).ReturnsDbSet(recordList);
            UserDbRecord? removedRecord = null;

            _context.Setup(x => x.Users.Remove(Moq.It.IsAny<UserDbRecord>())).Callback<UserDbRecord>(r => removedRecord = r);
            _context.Setup(x => x.Users.FindAsync(Moq.It.IsAny<Guid>())).Returns(ValueTask.FromResult<UserDbRecord>(rec1)!);

            await _middleware.InvokeAsync(_httpContext);

            var result = await _controller.DeleteUserDbRecord(new Guid("5A1A930F-75D1-4C08-9964-9B410EB1DA85"));

            Assert.IsInstanceOfType<NoContentResult>(result);
            Assert.IsNotNull(removedRecord);
            CompareUserDbRecordWithData(removedRecord, data);
            Assert.IsNotNull(removedRecord?.PasswordHash);
            Assert.AreEqual(removedRecord?.ClientId, _clientGuid1);
        }


        [TestMethod]
        public async Task SimpleVerifyUser()
        {
            _httpContext.Request.Headers[Constants.ApiKeyHeaderName] = _client1;

            UserWithPasswordDTO data = new UserWithPasswordDTO(
                "5A1A930F-75D1-4C08-9964-9B410EB1DA85",
                "yoyo",
                "Johnny B. Goode",
                "YoYo@yoyo.org",
                "1234567890",
                "Slovingliš",
                "en-US",
                "MickeyMouse");

            VerifyDTO testParameters = new VerifyDTO("yoyo", "testPassword");

            Guid idGuid = new Guid(data.Id);
            UserDbRecord rec1 = new UserDbRecord(idGuid,
                    data.UserName, data.FullName, data.EMail, data.MobilePhoneNumber,
                    data.Language, data.Culture, _controller.GetPasswordHash(idGuid, "testPassword"), _clientGuid1);
            List<UserDbRecord> recordList = new List<UserDbRecord>() { rec1 };


            _context.Setup(x => x.Users).ReturnsDbSet(recordList);
  
            await _middleware.InvokeAsync(_httpContext);

            var result = await _controller.VerifyUserPassword(testParameters);

            Assert.IsInstanceOfType<OkObjectResult>(result.Result);
            var value = ((ObjectResult)result.Result).Value as UserDTO;
            Assert.IsTrue(value != null && value.Equals(data));
        }
    }
}