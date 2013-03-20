using RequestModelBinder;

namespace RequestModelBinderTest.ServiceRequestModels
{
    public class ChildModel
    {
        public int Count { get; set; }
        public SecondChildModel SecondChildModelTest { get; set; }
    }
}