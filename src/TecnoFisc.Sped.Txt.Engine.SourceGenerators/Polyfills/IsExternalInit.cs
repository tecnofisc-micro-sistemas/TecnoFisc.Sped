// Polyfill para records/init em netstandard2.0. Roslyn requer netstandard2.0 nos source
// generators, mas C# 9+ continua disponível enquanto provermos este tipo manualmente.
namespace System.Runtime.CompilerServices;

internal static class IsExternalInit
{
}
