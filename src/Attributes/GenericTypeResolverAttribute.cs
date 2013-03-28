using System;

namespace RequestModelBinder.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class GenericTypeResolverAttribute : Attribute
    {
        public Type[] GenericType { get; set; }

        public GenericTypeResolverAttribute(params Type[] genericType)
        {
            GenericType = genericType;
        }
    }
}