using System;
using System.Reflection;
using System.Web;
using System.Web.Http;
using RequestModelBinderTest.Services;
using RequestModelBinder;
using System.Linq;

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
                //call by name
                var result = ModelBinder.InvokeMethod(authService, methodName, requestString);

                //call by filter
                Func<MethodInfo, bool> func = m => m.GetParameters().Any(c => c.ParameterType.Name.Equals("ChildModel"));
                var result2 = ModelBinder.InvokeMethod(authService, func, requestString);

                //call empty
                var result3 = ModelBinder.InvokeMethod(authService, methodName, callEmpty:true);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "success";
        }
    }
}