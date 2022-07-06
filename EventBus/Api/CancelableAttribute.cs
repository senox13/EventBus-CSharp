using System;

namespace EventBus.Api{
    /// <summary>
    /// Attribute which can be added to an <see cref="Event"/> subclass to
    /// indicate that it can be canceled.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public sealed class CancelableAttribute : Attribute{}
}
