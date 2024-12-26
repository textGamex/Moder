#if WINDOWS
using System.Runtime.Versioning;

namespace Moder.Core.Infrastructure.FileSort;

[SupportedOSPlatform("windows")]
public sealed class WindowsFileSortComparer : IFileSortComparer
{
    // CSharp 实现 https://www.codeproject.com/Articles/11016/Numeric-String-Sort-in-C?msg=1183262#xx1183262xx
    public int Compare(string? x, string? y)
    {
        if (string.IsNullOrEmpty(x))
        {
            if (string.IsNullOrEmpty(y))
            {
                return 0;
            }

            return -1;
        }
        if (string.IsNullOrEmpty(y))
        {
            return 1;
        }

        return Vanara.PInvoke.ShlwApi.StrCmpLogicalW(x, y);
    }
}
#endif
