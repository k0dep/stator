using System;
using System.Linq;

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

        public static string GetTypeSafeName(this Type type)
        {
            var ns = type.Namespace?.GetHashCode().ToString("X");
            var name = ns + "_" + type.Name.Replace('.', '_').Replace('`', '_').Replace('*', '_');
            if (type.IsGenericType)
            {
                var paramerets = type.GetGenericArguments();
                name += "_" + string.Join("_", paramerets.Select(GetTypeSafeName).ToArray());
            }
            return name;
        }

        public static string GetSingletonFieldName(this Type type)
        {
            return $"i_{type.GetTypeSafeName()}";
        }

        public static string GetResolveNameBind(this Type type)
        {
            return $"F_Resolve_{type.GetTypeSafeName()}";
        }

        public static string GetResolveName(this Type type)
        {
            return $"Resolve_{type.GetTypeSafeName()}";
        }

        public static string GetDependencyName(this Type type)
        {
            return $"dep_{type.GetTypeSafeName()}";
        }

    }
}