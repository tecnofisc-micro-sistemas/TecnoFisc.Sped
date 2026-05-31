using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco0;

/// <summary>
/// Registro 0015 — Dados do Contribuinte Substituto ou Responsável pelo ICMS Destino.
/// Nível hierárquico 2, ocorrência várias por arquivo. Conforme Guia Prático EFD-ICMS/IPI
/// V3.0.6, p. 29.
/// </summary>
[RegistroSped(Codigo = "0015", Nivel = 2, Bloco = "0")]
public sealed partial class Registro0015 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0015";

    /// <summary>
    /// Sigla da UF do contribuinte substituído ou do consumidor final não contribuinte
    /// (ICMS Destino EC 87/15).
    /// </summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public string? UfSt { get; set; }

    /// <summary>
    /// Inscrição Estadual do contribuinte substituto na UF do contribuinte substituído
    /// ou do consumidor final não contribuinte (ICMS Destino EC 87/15).
    /// </summary>
    [CampoSped(Ordem = 3, Tamanho = 14, Obrigatorio = true)]
    public InscricaoEstadual IeSt { get; set; }
}
