using System;
using System.Web;
using System.Web.Http;
using RequestModelBinderTest.Services;
using RequestModelBinder;

namespace RequestModelBinderTest.Controllers
{
    public class CoreController : ApiController
    {
        public string Get()
        {
            var requestString = HttpUtility.ParseQueryString(Request.RequestUri.Query);
            var methodName = requestString.Get("Method");

            var authService = new AuthService();

            try
            {
                ModelBinder.InvokeMethod(authService, methodName, requestString);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "success";
        }
    }
}