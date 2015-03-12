namespace Converters.ComInterfaces
{
    using Converters.ComInterfaces.MetadataEnums;

    public static class Extentions
    {
        public static bool IsNotEmpty(this int token, CorTokenType tokenBase)
        {
            return token != (int) tokenBase;
        }

        public static bool Is(this int token, CorTokenType tokenBase)
        {
            return (token & 0xff000000) == (int)tokenBase;
        }
    }
}
