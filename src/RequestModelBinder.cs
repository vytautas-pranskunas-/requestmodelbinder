//-----------------------------------------------------------------//
//------------------------Programmed by:---------------------------//
//----------------------Vytautas Pranskunas------------------------//
//--------------------------2013.03.19-----------------------------//
//---------------These lines should not be removed-----------------//
//-----------------------------------------------------------------//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using RequestModelBinder.Attributes;

namespace RequestModelBinder
{
    public static class ModelBinder
    {
        public static object InvokeMethod(object instance, string methodName, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers,
                                          NameValueCollection requestItems = null)
        {
            var method = instance.GetType().GetMethod(methodName, bindingAttr, binder, callConvention, types, modifiers);
            return InvokeMethod(instance, method, requestItems);
        }

        public static object InvokeMethod(object instance, string methodName, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers,
                                          NameValueCollection requestItems = null)
        {
            var method = instance.GetType().GetMethod(methodName, bindingAttr, binder, types, modifiers);
            return InvokeMethod(instance, method, requestItems);
        }

        public static object InvokeMethod(object instance, string methodName, BindingFlags bindingAttr, NameValueCollection requestItems = null)
        {
            var method = instance.GetType().GetMethod(methodName, bindingAttr);
            return InvokeMethod(instance, method, requestItems);
        }

        public static object InvokeMethod(object instance, string methodName, Type[] types, ParameterModifier[] modifiers, NameValueCollection requestItems = null)
        {
            var method = instance.GetType().GetMethod(methodName, types, modifiers);
            return InvokeMethod(instance, method, requestItems);
        }

        public static object InvokeMethod(object instance, string methodName, Type[] types, NameValueCollection requestItems = null)
        {
            var method = instance.GetType().GetMethod(methodName, types);
            return InvokeMethod(instance, method, requestItems);
        }

        public static object InvokeMethod(object instance, string methodName, NameValueCollection requestItems = null, bool callEmpty = false)
        {
            if (callEmpty)
            {
                return InvokeMethod(instance, m => !m.GetParameters().Any());
            }

            var method = instance.GetType().GetMethod(methodName);
            return InvokeMethod(instance, method, requestItems);
        }

        public static object InvokeMethod(object instance, BindingFlags bindingAttr, Func<MethodInfo, bool> func, NameValueCollection requestItems = null)
        {
            var instanceType = instance.GetType();
            var methods = instanceType.GetMethods(bindingAttr);

            if (!methods.Any())
            {
                throw new ArgumentNullException(string.Format("There is no methods in {0}", instanceType.Name));
            }

            var method = methods.FirstOrDefault(func);
            return InvokeMethod(instance, method, requestItems);
        }

        public static object InvokeMethod(object instance, Func<MethodInfo, bool> func, NameValueCollection requestItems = null)
        {
            var instanceType = instance.GetType();
            var methods = instanceType.GetMethods();

            if (!methods.Any())
            {
                throw new ArgumentNullException(string.Format("There is no methods in {0}", instanceType.Name));
            }

            var method = methods.FirstOrDefault(func);
            return InvokeMethod(instance, method, requestItems);
        }

        private static object InvokeMethod(object instance, MethodInfo method, NameValueCollection requestItems)
        {
            if (method == null)
            {
                throw new MethodAccessException("There is no such method");
            }

            var methodParams = method.GetParameters();
            if (!methodParams.Any())
            {
                return method.Invoke(instance, null);
            }

            var requestItemsLowered = new NameValueCollection();
            if (requestItems != null)
            {
                foreach (string requestItem in requestItems)
                {
                    requestItemsLowered[requestItem.ToLower()] = requestItems[requestItem];
                }
            }

            var genericTypeResolvers = GetTypeForGenericParameters(method);
            var currentGenericParameter = 0;
            var parametersList = new List<object>();
            foreach (var parameter in methodParams)
            {
                var parameterType = GetUnderlyingTypeOrSelf(parameter.ParameterType);

                if (parameterType.Namespace == null)
                {
                    continue;
                }

                if (parameterType.Namespace.StartsWith("System") || parameterType.IsEnum)
                {
                    parametersList.Add(ParameterValueMapper(parameter, requestItemsLowered));
                    continue;
                }

                if (parameterType.IsInterface)
                {
                    parameterType = GetGenericParameterType(genericTypeResolvers, currentGenericParameter, parameter);
                    currentGenericParameter++;
                }

                object destination;
                GetObjectParams(parameterType, string.Empty, requestItemsLowered, out destination);
                parametersList.Add(destination);
            }

            return method.Invoke(instance, parametersList.ToArray());
        }

        private static object ParameterValueMapper(ParameterInfo parameterInfo, NameValueCollection requestItems)
        {
            var parameterName = parameterInfo.Name.ToLower();
            var parameterType = GetUnderlyingTypeOrSelf(parameterInfo.ParameterType);

            var requestItemValue = requestItems.Get(parameterName);

            try
            {

                if (parameterType.IsEnum)
                {
                    return Enum.Parse(parameterType, requestItemValue, true);
                }

                return Convert.ChangeType(requestItemValue, parameterType);
            }
            catch
            {
                return null;
            }
        }

        private static void GetObjectParams(Type parameterType, string classPath, NameValueCollection requestItems, out object destination)
        {
            destination = Activator.CreateInstance(parameterType);
            var destinationProperties = destination.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var destinationProperty in destinationProperties)
            {
                var destinationPropertyType = GetUnderlyingTypeOrSelf(destinationProperty.PropertyType);

                if (destinationPropertyType.Namespace == null)
                {
                    continue;
                }

                if (!destinationPropertyType.Namespace.StartsWith("System") && !destinationPropertyType.IsEnum)
                {
                    object innerDestination;
                    GetObjectParams(destinationPropertyType, GetChildPropertyClassName(classPath, destinationProperty.Name), requestItems, out innerDestination);
                    destinationProperty.SetValue(destination, innerDestination, null);
                    continue;
                }

                PropertyValueMapper(destination, destinationProperty, requestItems, classPath);
            }
        }

        private static void PropertyValueMapper(object destination, PropertyInfo destinationProperty, NameValueCollection requestItems, string classPath)
        {
            var destinationPropertyType = GetUnderlyingTypeOrSelf(destinationProperty.PropertyType);

            var propertyNameLower = string.Format("{0}{1}", classPath, destinationProperty.Name.ToLower()).ToLower();

            var requestItemValue = requestItems.Get(propertyNameLower);
            if (string.IsNullOrWhiteSpace(requestItemValue))
            {
                var propertyLevelOptionalAttribute = Attribute.GetCustomAttribute(destinationProperty, typeof(OptionalAttribute));
                var classLevelRequiredAttribute = Attribute.GetCustomAttribute(destination.GetType(), typeof(RequiredAttribute));
                if (classLevelRequiredAttribute != null && propertyLevelOptionalAttribute == null)
                {
                    var definedErrorMessage = ((RequiredAttribute)classLevelRequiredAttribute).ErrorMessage;
                    var errorMessage = string.Format("{0}{1}Property '{2}' is required.", definedErrorMessage, !string.IsNullOrEmpty(definedErrorMessage) ? ". " : string.Empty, GetMissingProperties(destination, requestItems, classPath));
                    throw new NullReferenceException(errorMessage);
                }

                var propertyLevelRequiredAttribute = Attribute.GetCustomAttribute(destinationProperty, typeof(RequiredAttribute));
                if (propertyLevelRequiredAttribute != null && propertyLevelOptionalAttribute == null)
                {
                    var errorMessage =
                        string.IsNullOrWhiteSpace(((RequiredAttribute)propertyLevelRequiredAttribute).ErrorMessage)
                            ? string.Format("Property '{0}' is required", propertyNameLower)
                            : ((RequiredAttribute)propertyLevelRequiredAttribute).ErrorMessage;

                    throw new NullReferenceException(errorMessage);
                }

                //check if constructor did not set value yet
                /* if (GetDefault(destinationProperty.PropertyType).Equals(destinationProperty.GetValue(destination, null)))
                 {
                     //default value
                     destinationProperty.SetValue(destination, null, null);
                 }*/
                return;
            }

            try
            {
                if (destinationPropertyType.IsEnum)
                {
                    destinationProperty.SetValue(destination, Enum.Parse(destinationPropertyType, requestItemValue, true), null);
                }
                else
                {
                    destinationProperty.SetValue(destination, Convert.ChangeType(requestItemValue, destinationPropertyType), null);
                }
            }
            catch
            {
                destinationProperty.SetValue(destination, null, null);
            }
        }

        /*private static object GetDefault(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }*/

        private static string GetMissingProperties(object destination, NameValueCollection requestItems, string classPath)
        {
            var missingProperties = string.Empty;
            var properties = destination.GetType().GetProperties();

            var retVal = properties
                .Where(p => p.PropertyType.Namespace != null && (p.PropertyType.Namespace.StartsWith("System") || p.PropertyType.IsEnum))
                .Where(p => Attribute.GetCustomAttribute(p, typeof(OptionalAttribute)) == null)
                .Select(propertyInfo => string.Format("{0}{1}", classPath, propertyInfo.Name.ToLower()).ToLower())
                .Where(propertyNameLower => string.IsNullOrWhiteSpace(requestItems.Get(propertyNameLower)))
                .Aggregate(missingProperties, (current, propertyNameLower) => current + (propertyNameLower + ", "));

            return retVal.Substring(0, retVal.Length - 2);
        }

        private static Type GetGenericParameterType(Type[] types, int memberNo, ParameterInfo parameter)
        {
            if (!types.Any())
            {
                throw new IndexOutOfRangeException("There is no types defined for interfaces");
            }

            if (types.Length < memberNo + 1)
            {
                throw new IndexOutOfRangeException(string.Format("Theris is no type defined for {0}", parameter.Name));
            }

            return types[memberNo];
        }

        private static string GetChildPropertyClassName(string currentClassPath, string newClassName)
        {
            return string.IsNullOrEmpty(currentClassPath)
                       ? string.Format("{0}.", newClassName)
                       : string.Format("{0}{1}.", currentClassPath, newClassName);
        }

        private static Type GetUnderlyingTypeOrSelf(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        private static Type[] GetTypeForGenericParameters(MethodInfo method)
        {
            var genericTypeResolverAttribute = Attribute.GetCustomAttribute(method, typeof(GenericTypeResolverAttribute));
            return genericTypeResolverAttribute != null ? ((GenericTypeResolverAttribute)genericTypeResolverAttribute).GenericType : null;
        }
    }
}