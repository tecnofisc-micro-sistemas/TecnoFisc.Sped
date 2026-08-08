namespace TecnoFisc.Sped.Core.Atributos;

/// <summary>
/// Marca um registro ou campo SPED como descontinuado a partir de uma versão de leiaute.
/// Atributo **informacional** quanto ao comportamento de leitura/escrita — não muda o que o
/// parser aceita nem o que o escritor materializa numa linha. Em pacotes read-only
/// (ARCHITECTURE §2.5/§4.7) o parser continua reconhecendo registros marcados como
/// descontinuados porque arquivos históricos das versões anteriores ainda precisam ser lidos.
/// Pacotes read+write podem usar a anotação para decidir se geram ou não o registro em arquivos
/// da versão alvo. O valor de <see cref="EmVersao"/> é propagado ao catálogo (
/// <c>MetadadosRegistro.DescontinuadoEm</c>) pelos dois caminhos que constroem catálogo —
/// o gerado em compile-time (<c>RegistroSpedCatalogoGenerator</c>) e o reflexivo
/// (<c>CatalogoBuilder</c>) — que precisam concordar (ver
/// <c>RegistroSpedCatalogoGeneratorVigenciaTests</c>).
/// </summary>
/// <remarks>
/// <para>
/// First-use: criado no sub-stage 8.016.004 para marcar <c>Registro1600</c> como substituído
/// pelo <c>Registro1601</c> a partir de V016 (3.0.7 item 6).
/// </para>
/// <para>
/// Suporte a <see cref="AttributeTargets.Field"/> adicionado no sub-stage 8.017.050 para
/// marcar membros de enum (Tabela 4.1.2 — códigos 04 e 05 descontinuados em V017).
/// </para>
/// </remarks>
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field,
    AllowMultiple = false,
    Inherited = false)]
public sealed class DescontinuadoAttribute : Attribute
{
    /// <summary>
    /// Versão do leiaute a partir da qual o registro ou campo não deve mais ser usado.
    /// Convenção: valor numérico do enum <c>LayoutXxx</c> do módulo
    /// (ex.: <c>(int)LayoutEfdIcmsIpi.V016</c> = 16).
    /// </summary>
    public required int EmVersao { get; init; }
}
