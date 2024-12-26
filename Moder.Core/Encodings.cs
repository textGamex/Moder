using System.Text;

namespace Moder.Core;

public static class Encodings
{
    public static readonly UTF8Encoding Utf8NotBom = new(false);
}