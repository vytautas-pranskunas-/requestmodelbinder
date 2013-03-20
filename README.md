requestmodelbinder
==================

Library to invoke methods by binding request parameters to method parameters. Handles inheritenses and child items

Example:
Request Url:
api/?Method=TestAuthBinder&priority=third&name=vytas&createdon=2012.01.04%2010:15Z&ChildModelTest.count=15&ChildModelTest2.SecondChildModelTest.name=test%20name&identifier=123&testenum=third

```csharp
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
    
public class AuthService
{
    public void TestAuthBinder(AuthServiceModel authServiceModel, string name, Test? priority)
    {
        
    } 
}  
```
