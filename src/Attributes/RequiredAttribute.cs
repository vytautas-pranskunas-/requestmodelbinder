using System;

namespace RequestModelBinder.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
    public class RequiredAttribute : Attribute
    {
        public string ErrorMessage {get; set;}

        public RequiredAttribute()
        {
        }

        public RequiredAttribute(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}