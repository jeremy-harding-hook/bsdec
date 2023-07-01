using System;

namespace BsdecCore
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BsdecToplevelSavefileClassAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class BsdecReadMethodAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class BsdecWriteMethodAttribute : Attribute { }
}
