using RequestModelBinder.Attributes;

namespace RequestModelBinderTest.ServiceRequestModels
{
    [Required("All properties must be provided")]
    public class AuthServiceModel : AuthServiceBaseModel
    {
        public string Identifier { get; set; }

        [Optional]
        public int Pin { get; set; }

        public Test? TestEnum { get; set; }

        public ChildModel ChildModelTest { get; set; }
    }

    public enum Test
    {
        First = 0,
        Second = 1,
        Third = 3
    }
}