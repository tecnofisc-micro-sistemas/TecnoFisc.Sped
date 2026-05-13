using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D310 — Complemento dos Bilhetes de Passagem Rodoviário (13), Aquaviário (14),
/// Passagem e Nota de Bagagem (15) e Ferroviário (16).
/// Agrupa por município de origem os valores dos documentos fiscais resumidos no registro D300.
/// Nível hierárquico 3, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 185.
/// </summary>
[RegistroSped(Codigo = "D310", Nivel = 3, Bloco = "D")]
public sealed partial class RegistroD310 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D310";

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
