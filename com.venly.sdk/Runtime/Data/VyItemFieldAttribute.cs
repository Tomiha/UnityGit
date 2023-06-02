using System;

namespace Venly.Data
{
    [Flags]
    public enum eVyItemTrait
    {
        None = 0,
        ReadOnly = 1,
        Updateable = 2,
        LiveOnly = 4,

        LiveReadOnly = ReadOnly | LiveOnly
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class VyItemFieldAttribute : Attribute
    {
        public eVyItemTrait Traits { get; set; }
        public bool IsLiveOnly => Traits.HasFlag(eVyItemTrait.LiveOnly);
        public bool IsReadOnly => Traits.HasFlag(eVyItemTrait.ReadOnly);
        public bool IsUpdateable => Traits.HasFlag(eVyItemTrait.Updateable);

        public VyItemFieldAttribute(eVyItemTrait traits = eVyItemTrait.None)
        {
            Traits = traits;
        }
    }
}