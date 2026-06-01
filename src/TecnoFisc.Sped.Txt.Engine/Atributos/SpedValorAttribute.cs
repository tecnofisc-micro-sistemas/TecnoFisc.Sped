namespace TecnoFisc.Sped.Txt.Engine.Atributos;

/// <summary>
/// Define o valor textual SPED de um membro de enum quando o código do leiaute não é numérico.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class SpedValorAttribute : Attribute
{
    public SpedValorAttribute(string valor)
    {
        Valor = valor;
    }

    /// <summary>Valor canônico do campo no arquivo SPED.</summary>
    public string Valor { get; }
}
