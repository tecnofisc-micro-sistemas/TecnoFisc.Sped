using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.BlocoD;

/// <summary>
/// Registro D697 — Registro de Informações de Outras UFs, Relativamente aos Serviços
/// "Não-Medidos" de Televisão por Assinatura via Satélite.
/// Totaliza por UF os valores de base de cálculo e ICMS dos documentos emitidos para
/// assinantes da UF informada, conforme documentos dos arquivos do Convênio ICMS nº 115/03.
/// Nível hierárquico 4, ocorrência 1:N.
/// Conforme Guia Prático EFD-ICMS/IPI V3.0.6, p. 205.
/// </summary>
[RegistroSped(Codigo = "D697", Nivel = 4, Bloco = "D")]
public sealed partial class RegistroD697 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "D697";

    /// <summary>Sigla da unidade da federação.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2)]
    public string? Uf { get; set; }

    /// <summary>Valor da base de cálculo do ICMS.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0, Decimais = 2)]
    public decimal? VlBcIcms { get; set; }

    /// <summary>Valor do ICMS.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2)]
    public decimal? VlIcms { get; set; }
}
