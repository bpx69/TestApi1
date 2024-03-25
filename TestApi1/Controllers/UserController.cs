using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Swashbuckle.AspNetCore.Annotations;

using TestApi1.Model;
using TestApi1.ViewModel;
using TestApi1.Authentication;
using TestApi1.Logging;

using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Net;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.OpenApi.Models;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Data.SqlClient;


namespace TestApi1.Controllers
{
    /// <summary>
    /// API Controller implementing the complete User API
    /// Because of simplicity not special Business Logic layer has been created
    /// </summary>
    [Route("api/User")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserAPIDbContext _context;
        private readonly ApiLogger _logger;

        // password hashing method
        const int keySize = 32;
        const int iterations = 150000;
        HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;


        /// <summary>
        /// Not API Function, used in tests to create test password hashes and therefore public.
        /// </summary>
        /// <param name="saltGuid"></param>
        /// <param name="plainTextPassword"></param>
        /// <returns>password hash to be stored for password comparisons</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        public string GetPasswordHash(Guid saltGuid, string plainTextPassword)
        {
            var salt = saltGuid.ToByteArray();

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(plainTextPassword),
                salt,
                iterations,
                hashAlgorithm,
                keySize);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Compares plain text password with stored password hash
        /// </summary>
        /// <param name="password"></param>
        /// <param name="hash"></param>
        /// <param name="saltGuid"></param>
        /// <returns>true if passwords match, false if not</returns>
        bool VerifyPassword(string password, string hash, Guid saltGuid)
        {
            var salt = saltGuid.ToByteArray();
            var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithm, keySize);
            return CryptographicOperations.FixedTimeEquals(hashToCompare, Convert.FromBase64String(hash));
        }

        /// <summary>
        /// Password comparison has non trivial execution time and it can be run asynchronously in a task
        /// </summary>
        /// <param name="password"></param>
        /// <param name="hash"></param>
        /// <param name="saltGuid"></param>
        /// <returns>Task returning a bool. See synchronous function for semantics.</returns>
        protected Task<bool> VerifyPasswordAsync(string password, string hash, Guid saltGuid)
        {
            return Task.Run<bool>(() => VerifyPassword(password, hash, saltGuid));
        }

        /// <summary>
        /// Creates a UserDbRecord from user DTO 
        /// </summary>
        /// <param name="userData"></param>
        /// <param name="userRecord"></param>
        /// <returns>true if ok, false if inconsistent or missing data</returns>
        protected bool ValidateAndPrepareData(UserWithPasswordDTO userData, out UserDbRecord? userRecord)
        {
            
            userRecord = null;
            if (!Guid.TryParse(userData.Id, out var parsedIdGuid)) return false;
            if (String.IsNullOrEmpty(userData.UserName)) return false;
            if (String.IsNullOrEmpty(userData.Language)) return false;
            if (String.IsNullOrEmpty(userData.Culture)) return false;
            if (!Guid.TryParse(Request.Headers[Constants.ApiClientIdHederName], out var parsedClientGuid)) return false;
            try
            {
                if (CultureInfo.GetCultureInfo(userData.Culture) == null) return false;
            }
            catch(CultureNotFoundException)
            {
                return false;
            }

            userRecord = new UserDbRecord(
                parsedIdGuid,
                userData.UserName,
                userData.FullName,
                userData.EMail,
                userData.MobilePhoneNumber,
                userData.Language,
                userData.Culture,
                GetPasswordHash(parsedIdGuid, userData.Password),
                parsedClientGuid
            );
            return true;
         }

        public UserController(UserAPIDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = new ApiLogger(logger, "User");
        }

        /// <summary>
        /// Debugging function, 
        /// </summary>
        /// <returns>list of all Users of a Client</returns>
        // GET: api/User
        [HttpGet]
        [SwaggerOperation(Summary = "Get a specific user by ID")]
        [SwaggerHeader(Constants.ApiKeyHeaderName, "API Key of Client")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Returns the list of", typeof(TestApi1.ViewModel.UserDTO))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Invalid API Key")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, "Problem accessing database")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            _logger.LogEntry(Request);

            if (!_context.Database.CanConnect())
                return _logger.LogExit(StatusCode((int)HttpStatusCode.ServiceUnavailable));
            if (!Guid.TryParse(Request.Headers[Constants.ApiClientIdHederName], out var clientId)) return _logger.LogExit(Unauthorized());
            var result = await _context.Users
                .Where((rec) => rec.ClientId == clientId)
                .Select<UserDbRecord, UserDTO>(user => new UserDTO(user))
                .ToListAsync();
            return _logger.LogExit<IEnumerable<UserDTO>>(result);
        }

        /// <summary>
        /// API Function, Retrieve a specific User 
        /// </summary>
        /// <returns>a DTO containing specific user data if found, the below responses if problems</returns>
        // GET: api/User/5
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get a specific user by ID")]
        [SwaggerHeader(Constants.ApiKeyHeaderName, "API Key of Client")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Returns the requested user", typeof(TestApi1.ViewModel.UserDTO))]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Invalid API Key")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "Item not found")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, "Problem accessing database")]
        public async Task<ActionResult<UserDTO>> GetUserDbRecord(Guid id)
        {
            _logger.LogEntry(Request);

            var userDbRecord = await _context.Users.FindAsync(id);
            if (!Guid.TryParse(Request.Headers[Constants.ApiClientIdHederName], out var clientId)) return _logger.LogExit(Unauthorized());
            
            if (userDbRecord == null || userDbRecord.ClientId != clientId) return _logger.LogExit(NotFound());
 
            return Ok(_logger.LogOk<UserDTO>(new UserDTO(userDbRecord)));
        }
        /// <summary>
        /// Updates a user record in the database
        /// </summary>
        /// <param name="id">id of the record</param>
        /// <param name="userData">new user data DTO</param>
        /// <returns>a DTO containing specific user data  or below responses on error</returns>
        // PUT: api/User/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Change the existing user data")]
        [SwaggerHeader(Constants.ApiKeyHeaderName, "API Key of Client")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Done")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "User with Id not in database")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Invalid API Key")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "Problem with data i.e. UserName is non-unique")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, "Problem accessing database")]
        public async Task<ActionResult<UserDTO>> UpdateUserDbRecord(string id, UserWithPasswordDTO userData)
        {
            _logger.LogEntry(Request);
            _logger.LogData(userData);
          
            // Bad data
            if (id != userData.Id) return _logger.LogExit(BadRequest());
            // Data validation

            if (!ValidateAndPrepareData(userData, out var user)) return _logger.LogExit(BadRequest());
            if (!Guid.TryParse(Request.Headers[Constants.ApiClientIdHederName], out var clientId)) return _logger.LogExit(Unauthorized());

            var existingRecord = await _context.Users.FindAsync(user!.Id);
            if (existingRecord == null || existingRecord.ClientId != user.ClientId) return _logger.LogExit(NotFound());


            _context.SetAsModified(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException e)
            {

                _logger.LogError("Update Concurrency Exception", e);
                return _logger.LogExit(Conflict());
            }

            return Ok(_logger.LogOk(new UserDTO(user)));
        }

        /// <summary>
        /// Creates a new user record in a database
        /// </summary>
        /// <param name="userData">DTO containing user data</param>
        /// <returns>DTO with the created user data on success, below responses on errors</returns>
        // POST: api/User
        [HttpPost]
        [SwaggerOperation(Summary = "Create a new user record")]
        [SwaggerHeader(Constants.ApiKeyHeaderName, "API Key of Client")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Created")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, "Duplicate UserName (for the client)")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Invalid API Key")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Problem with data i.e. UserName is non-unique")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, "Problem accessing database")]
        public async Task<ActionResult<UserDTO>> CreateUserDbRecord(UserWithPasswordDTO userData)
        {
            _logger.LogEntry(Request);
            _logger.LogData(userData);

            // Database not ready
            // if (!_context.Database.CanConnect()) return _logger.LogExit(StatusCode((int)HttpStatusCode.ServiceUnavailable));
            // Data validation
            UserDbRecord? user = null;
            if (!ValidateAndPrepareData(userData, out user)) return _logger.LogExit(BadRequest());

            _context.Users.Add(user!);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return _logger.LogExit(Conflict());
            }

            CreatedAtActionResult createdAtActionResult = CreatedAtAction("CreateUserDbRecord", new { id = user!.Id }, user!);
            return Ok(_logger.LogOk(new UserDTO((createdAtActionResult.Value as UserDbRecord)!)));
        }

        /// <summary>
        /// Deletes a user record from database
        /// </summary>
        /// <param name="id"></param>
        /// <returns>See below for list of responses</returns>
        // DELETE: api/User/5
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Delete the existing user data")]
        [SwaggerHeader(Constants.ApiKeyHeaderName, "API Key of Client")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, "User record deleted")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Invalid API Key")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "User record not found")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, "Problem accessing database")]
        public async Task<IActionResult> DeleteUserDbRecord(Guid id)
        {
            _logger.LogEntry(Request);
     
            var userDbRecord = await _context.Users.FindAsync(id);
            if (userDbRecord == null)
            {
                return _logger.LogExit(NotFound());
            }

            _context.Users.Remove(userDbRecord);
            await _context.SaveChangesAsync();

            return _logger.LogExit(NoContent());
        }

        /// <summary>
        /// Gets the User DTO stored in database from username and plain text password
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("VerifyPassword")]
        [SwaggerOperation(Summary = "Verify if UserName and password match any stored record (and client)")]
        [SwaggerHeader(Constants.ApiKeyHeaderName, "API Key of Client")]
        [SwaggerResponse((int)HttpStatusCode.OK, "OK")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "UserName or password is empty")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, "Invalid password")]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, "Invalid API Key")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "User record not found")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, "Problem accessing database")]
        public async Task<ActionResult<UserDTO>> VerifyUserPassword(VerifyDTO data)
        {
            _logger.LogEntry(Request);
            _logger.LogData(data);          

        
            if (!Guid.TryParse(Request.Headers[Constants.ApiClientIdHederName], out var clientId)) return _logger.LogExit(Unauthorized());
            if (string.IsNullOrWhiteSpace(data.UserName) || string.IsNullOrWhiteSpace(data.Password)) return _logger.LogExit(BadRequest());

            var userDbRecord = await _context.Users.FirstOrDefaultAsync((rec) => rec.UserName == data.UserName && rec.ClientId == clientId);
            if (userDbRecord == null) return _logger.LogExit(NotFound());

            bool result = await VerifyPasswordAsync(data.Password!, userDbRecord.PasswordHash, userDbRecord.Id);
            if (result)
            {
                return Ok(_logger.LogOk(new UserDTO(userDbRecord)));
            }
            else
            {
                return _logger.LogExit(NoContent());
            }
        }

        private bool UserDbRecordExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
