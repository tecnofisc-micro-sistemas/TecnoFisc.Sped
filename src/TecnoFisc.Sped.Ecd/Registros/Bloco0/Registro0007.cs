using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.Ecd.Registros.Bloco0;

/// <summary>
/// Registro 0007 — Outras Inscrições Cadastrais da Pessoa Jurídica. Nível hierárquico 2,
/// ocorrência 0:N. Registra inscrições da pessoa jurídica em entidades que legalmente têm
/// direito de acesso ao livro contábil digital. Conforme Manual de Orientação do Leiaute 9
/// da ECD, p. 77.
/// </summary>
/// <remarks>
/// <para>
/// <c>COD_ENT_REF</c> deve conter código válido conforme Tabela de Instituições
/// Responsáveis pela Administração do Cadastro das Entidades publicada pelo Sped:
/// 00 = nenhuma inscrição em outras entidades; 01 = Banco Central do Brasil (Bacen);
/// 02 = Susep; 03 = CVM; 04 = ANTT; 05 = TSE; siglas de UF (AC, AL, … TO) para
/// Secretarias de Fazenda. Regra <c>REGRA_TABELA_INSTITUICOES_CADASTRO</c> —
/// verificação de responsabilidade do consumidor (ARCHITECTURE §2.3).
/// </para>
/// <para>
/// A regra de formação de <c>COD_INSCR</c> depende de <c>COD_ENT_REF</c>: para Bacen
/// (01) aplica <c>REGRA_VALIDA_ID_BACEN</c> (8 dígitos iniciados em "Z"); para Susep
/// (02), <c>REGRA_VALIDA_ID_SUSEP</c>; para CVM (03), <c>REGRA_VALIDA_ID_CVM</c>.
/// Violação gera aviso (não erro) pelo PGE do Sped Contábil.
/// </para>
/// </remarks>
[RegistroSped(Codigo = "0007", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0007 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0007";

    /// <summary>
    /// Código da instituição responsável pela administração do cadastro, conforme tabela
    /// publicada pelo Sped (ex.: <c>"01"</c> = Bacen; <c>"02"</c> = Susep; <c>"SP"</c> =
    /// Secretaria da Fazenda de São Paulo). Campo <c>COD_ENT_REF</c>.
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 0, Obrigatorio = true)]
    public string? CodEntRef { get; set; }

    /// <summary>
    /// Código cadastral da pessoa jurídica na instituição identificada em
    /// <see cref="CodEntRef"/>. Para Bacen: 8 dígitos iniciados em "Z". Campo
    /// <c>COD_INSCR</c>.
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? CodInscr { get; set; }
}
