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
    
public class AuthService
{
    [GenericTypeResolver(typeof(AuthServiceModel), typeof(ChildModel))]
	public string TestAuthBinder(IFirstGenericClass first, SecondChildModel second, ISecondGenericClass third, string name)
	{
		return "test results";
	}

	public string TestAuthBinder()
	{
		return "Empty";
	}

	public string TestAuthBinder(ChildModel model)
	{
		return "ChildModel";
	}
}  
```

Binder handles 3 types of attributes: **Required**, **Optional** and **GenericTypeResolver**. 
First two are used for telling binder whether parameter must be provided in URL or it can gain default value if missing. 
GenericTypeResolver is used to describe default instances for generic parameters. One of usages can be when the service has different implementations injected by IoC and needs to handle different request objects.
* **Required** attribute is property and class scoped.
* **Optional** attribute is property scoped only.
* **GenericTypeResolver** attribute is method scoped only.

All GetMethod overloads are supported as well as 2 additionals overloads added which allows to filter by providing predicate.

NuGet package also available.
