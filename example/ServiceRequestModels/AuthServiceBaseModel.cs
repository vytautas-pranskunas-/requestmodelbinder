using System;

namespace RequestModelBinderTest.ServiceRequestModels
{
    public class AuthServiceBaseModel
    {
        private string identifier;
        public DateTime CreatedOn { get; set; }
        public ChildModel ChildModelTest2 { get; set; }
    }
}