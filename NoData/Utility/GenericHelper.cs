using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace NoData.Utility
{

    internal static class GenericHelper
    {
        public static object CreateAndCallMethodOnClass(
            Type classType,
            Type[] classGenerics, 
            object[] ctorArguments, 
            string methodName, 
            Type[] methodArgumentTypes,
            object[] methodArguments)
        {
            var classGenericType = classType.MakeGenericType(classGenerics);
            var subExpandObject = Activator.CreateInstance(classGenericType, ctorArguments);
            var methodInfo = classGenericType.GetMethod(methodName, methodArgumentTypes);
            return methodInfo.Invoke(subExpandObject, methodArguments);
        }

        public static object CreateAndCallMethodOnStaticClass(
            Type classType,
            Type[] methodGenerics, 
            string methodName, 
            Type[] methodArgumentTypes,
            object[] methodArguments)
        {
            var methodInfo = classType.GetMethod(methodName, methodArgumentTypes);
            return methodInfo.MakeGenericMethod(methodGenerics).Invoke(null, methodArguments);
        }

        public static TResult CreateAndCallMethodOnClass<TClass, TResult>(
            Type[] classGenerics,
            object[] ctorArguments,
            string methodName,
            Type[] methodArgumentTypes,
            object[] methodArguments
            )
        => (TResult)CreateAndCallMethodOnClass(typeof(TClass), classGenerics, ctorArguments, methodName, methodArgumentTypes, methodArguments);
        
    }
}
