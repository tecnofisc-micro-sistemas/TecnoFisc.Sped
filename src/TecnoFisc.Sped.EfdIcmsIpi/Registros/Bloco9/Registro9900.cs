using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco9;

/// <summary>
/// Registro 9900 - Registros do Arquivo. Nivel hierarquico 2, ocorrencia varios por arquivo.
/// Totaliza cada codigo de registro presente no arquivo, inclusive os posteriores ao 9900.
/// Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 302.
/// </summary>
[RegistroSped(Codigo = "9900", Nivel = 2, Bloco = "9")]
public sealed partial class Registro9900 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "9900";

    /// <summary>Codigo do registro que sera totalizado no campo QTD_REG_BLC.</summary>
    [CampoSped(Ordem = 2, Tamanho = 4, Obrigatorio = true)]
    public string? RegBlc { get; set; }

    /// <summary>Total de registros do tipo informado no campo REG_BLC.</summary>
    [CampoSped(Ordem = 3, Obrigatorio = true)]
    public int QtdRegBlc { get; set; }
}
