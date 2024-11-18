using Ardalis.SmartEnum;

namespace Moder.Core.Services.GameResources.Base;

public abstract partial class ResourcesService<TType, TContent, TParseResult>
{
    protected sealed class WatcherFilter : SmartEnum<WatcherFilter, byte>
    {
        public static readonly WatcherFilter AllFiles = new("*.*", 0);
        public static readonly WatcherFilter Text = new("*.txt", 1);
        public static readonly WatcherFilter LocalizationFiles = new("*.yml", 2);

        private WatcherFilter(string name, byte value) : base(name, value)
        {
        }
    }
}
