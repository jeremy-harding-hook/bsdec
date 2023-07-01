using System;

namespace BsdecSchemaGen
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BsdecToplevelSaveClassAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class BsdecReadMethodAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class BsdecWriteMethodAttribute : Attribute { }
}
