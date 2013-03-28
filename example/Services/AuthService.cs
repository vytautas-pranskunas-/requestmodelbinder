using RequestModelBinder.Attributes;
using RequestModelBinderTest.ServiceRequestModels;

namespace RequestModelBinderTest.Services
{
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
}