namespace TecnoFisc.Sped.Core.Atributos;

/// <summary>
/// Marca um registro ou campo SPED como descontinuado a partir de uma versão de leiaute.
/// O parser rejeita registros marcados quando a versão do arquivo (<c>VersaoLeiaute</c> do
/// <c>Registro0000</c>) for maior ou igual a <see cref="EmVersao"/>. Campos individuais
/// são ignorados pelo gerador a partir da mesma versão.
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
