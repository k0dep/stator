using System;

namespace Stator.Editor
{
    public static class TypeExtensions
    {
        public static string GetRightFullName(this Type type)
        {
            var ns = type.Namespace;
            if(string.IsNullOrEmpty(ns))
            {
                return type.GetFriendlyName();
            }
            return ns + "." + type.GetFriendlyName();
        }

        public static string GetFriendlyName(this Type type)
        {
            string friendlyName = type.Name;
            if (type.IsGenericType)
            {
                int iBacktick = friendlyName.IndexOf('`');
                if (iBacktick > 0)
                {
                    friendlyName = friendlyName.Remove(iBacktick);
                }
                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    string typeParamName = GetFriendlyName(typeParameters[i]);
                    friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                }
                friendlyName += ">";
            }

            return friendlyName;
        }
    }
}