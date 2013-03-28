namespace RequestModelBinderTest.ServiceRequestModels
{
    public class ChildModel : ISecondGenericClass
    {
        public int Count { get; set; }
        public SecondChildModel SecondChildModelTest { get; set; }
    }
}