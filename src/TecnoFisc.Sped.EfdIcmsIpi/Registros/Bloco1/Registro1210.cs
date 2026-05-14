using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1210 - Utilizacao de Creditos Fiscais - ICMS. Nivel hierarquico 3,
/// ocorrencia varios por Registro 1200. Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 273.
/// </summary>
[RegistroSped(Codigo = "1210", Nivel = 3, Bloco = "1")]
public sealed partial class Registro1210 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1210";

    /// <summary>Tipo de utilizacao do credito, conforme tabela indicada no item 5.5.</summary>
    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public string? TipoUtil { get; set; }

    /// <summary>Numero do documento utilizado na baixa de creditos.</summary>
    [CampoSped(Ordem = 3, Tamanho = 0)]
    public string? NrDoc { get; set; }

    /// <summary>Total de credito utilizado.</summary>
    [CampoSped(Ordem = 4, Tamanho = 0, Decimais = 2, Obrigatorio = true)]
    public decimal VlCredUtil { get; set; }

    /// <summary>Chave do Documento Eletronico.</summary>
    [CampoSped(Ordem = 5, Tamanho = 44)]
    public ChaveAcesso? ChvDoce { get; set; }
}
