using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco1;

/// <summary>
/// Registro 1390 - Controle de Producao de Usina. Nivel hierarquico 2, ocorrencia varios
/// por arquivo. Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 279.
/// </summary>
[RegistroSped(Codigo = "1390", Nivel = 2, Bloco = "1")]
public sealed partial class Registro1390 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "1390";

    /// <summary>Codigo do produto conforme tabela 5.8 da Secretaria de Fazenda da UF.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true)]
    public int CodProd { get; set; }
}
