using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D420 — Complemento dos Documentos Informados (códigos 13, 14, 15 e 16).
/// Agrupa por município de origem os valores resumidos no registro D400.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.2.2, p. 198.
/// </summary>
/// <remarks>
/// <b>V016 (Guide 3.0.7 item 3):</b> campos <c>VL_BC_ICMS</c> e <c>VL_ICMS</c> passaram de
/// O (Obrigatório) para OC (Obrigatório Condicional).
/// Regra fiscal — pacote read-only não valida; consumidor (PVA, regras próprias) verifica.
/// </remarks>
[RegistroSped(Codigo = "D420", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD420 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D420";

    /// <summary>Código do município de origem do serviço, conforme a tabela IBGE (7 dígitos).</summary>
    [CampoSped(Ordem = 2, Tamanho = 7, Obrigatorio = true)]
    public int? CodMunOrig { get; set; }

    /// <summary>Valor total da prestação de serviço.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlServ { get; set; }

    /// <summary>Valor total da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Valor total do ICMS.</summary>
    [CampoSped(Ordem = 5, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }
}
