using System;

namespace RequestModelBinder.Attributes
{
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