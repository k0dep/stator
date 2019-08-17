using System;

namespace Stator.Editor
{
    public class MethodParameter
    {
        public Type ParameterType { get; set; }
        public string Name { get; set; }

        public MethodParameter(Type parameterType, string name)
        {
            this.ParameterType = parameterType;
            this.Name = name;
        }

        public override string ToString()
        {
            return $"{ParameterType.GetRightFullName()} {Name}";
        }
    }
}