using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Net;
using TestApi1.ViewModel;
using TestApi1.Authentication;

namespace TestApi1.Logging
{
    public class ApiLogger
    {
        ILogger _logger;
        string _api;
        string _hostname;
        public ApiLogger(ILogger logger, string api)
        {
            _logger = logger;
            _api = api;
            _hostname = System.Environment.MachineName;

        }

        public void LogEntry(
            HttpRequest request,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
        )
        {
            _logger.LogInformation("API {0}/{1}/{2} called on server {3} from {4}({5}) in file {6} line {7}",
                _api,
                request.Method,
                memberName,
                _hostname,
                request.HttpContext.Connection.RemoteIpAddress,
                request.Headers[Constants.ApiClientHederName],
                sourceFilePath,
                sourceLineNumber);
        }

        public StatusCodeResult LogExit(
            StatusCodeResult exitCode,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
        )
        {
            _logger.LogInformation("API {0}/{1} exited with status {2} on server {3}  in file {4} line {5}",
                _api,
                memberName,
                exitCode,
                _hostname,
                sourceFilePath,
                sourceLineNumber);
            return exitCode;
        }

        public ActionResult<T> LogExit<T>(
           ActionResult<T> result,
           [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
           [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
           [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
       )
        {
            _logger.LogInformation("API {0}/{1} exited with status {2} on server {3}  in file {4} line {5}",
                _api,
                memberName,
                ((ObjectResult)((IConvertToActionResult)result).Convert()).StatusCode,
                _hostname,
                sourceFilePath,
                sourceLineNumber);
            return result;
        }

        public T LogOk<T>(
               T result,
               [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
               [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
               [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
            )
        {
            _logger.LogInformation("API {0}/{1} exited with status {2} on server {3}  in file {4} line {5}",
                _api,
                memberName,
                HttpStatusCode.OK,
                _hostname,
                sourceFilePath,
                sourceLineNumber);
            return result;
        }

        public void LogError(
           string message,
           [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
           [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
           [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
       )
        {
            _logger.LogError("API {0}/{1} reported '{2}' on server {3}  in file {4} line {5}",
                _api,
                memberName,
                message,
                _hostname,
                sourceFilePath,
                sourceLineNumber);
        }

        public void LogError(
            string message,
            Exception e,
           [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
           [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
           [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
        )
        {
            _logger.LogError("API {0}/{1} reported '{2}' on server {3}  in file {4} line {5}\n{6}",
                _api,
                memberName,
                message,
                _hostname,
                sourceFilePath,
                sourceLineNumber,
                e.ToString());
        }

        public void LogData(
            UserWithPasswordDTO data,
           [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
           [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
           [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
        ) 
        {
            _logger.LogInformation("API {0}/{1} parsed data on server {2}  in file {3} line {4}\nId={6}\nUserName={7}\nFullName={8}\nEMail={9}\nMobilePhoneNumber={10}\nLanguge={11}\nCulture={12}",
                _api,
                memberName,
                _hostname,
                sourceFilePath,
                sourceLineNumber,
                data.Id,
                data.UserName,
                data.FullName,
                data.EMail,
                data.MobilePhoneNumber,
                data.Language,
                data.Culture
             );
        }

        public void LogData(
           VerifyDTO data,
          [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
          [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
          [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
        )
        {
            _logger.LogInformation("API {0}/{1} parsed data on server {2} in file {3} line {4}\nUserName={5}\nPassword={6}",
                _api,
                memberName,
                _hostname,
                sourceFilePath,
                sourceLineNumber,
                data.UserName,
                "**********"
             );
        }
    }
}

