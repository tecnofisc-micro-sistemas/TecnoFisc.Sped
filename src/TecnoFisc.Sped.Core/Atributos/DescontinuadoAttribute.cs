namespace TecnoFisc.Sped.Core.Atributos;

/// <summary>
/// Marca um registro ou campo SPED como descontinuado a partir de uma versão de leiaute.
/// Atributo **informacional** — não altera o comportamento do parser nem do gerador. Em
/// pacotes read-only (ARCHITECTURE §2.5/§4.7) o parser continua reconhecendo registros
/// marcados como descontinuados porque arquivos históricos das versões anteriores ainda
/// precisam ser lidos. Pacotes read+write podem usar a anotação para decidir se geram ou
/// não o registro em arquivos da versão alvo.
/// </summary>
/// <remarks>
/// First-use: criado no sub-stage 8.016.004 para marcar <c>Registro1600</c> como substituído
/// pelo <c>Registro1601</c> a partir de V016 (3.0.7 item 6).
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class DescontinuadoAttribute : Attribute
{
    /// <summary>
    /// Versão do leiaute a partir da qual o registro ou campo não deve mais ser usado.
    /// Convenção: valor numérico do enum <c>LayoutXxx</c> do módulo
    /// (ex.: <c>(int)LayoutEfdIcmsIpi.V016</c> = 16).
    /// </summary>
    public required int EmVersao { get; init; }
}
