using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco9;

/// <summary>
/// Registro 9999 - Encerramento do Arquivo Digital. Nivel hierarquico 0, ocorrencia unica por arquivo.
/// Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 303.
/// </summary>
[RegistroSped(Codigo = "9999", Nivel = 0, Bloco = "9")]
public sealed partial class Registro9999 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "9999";

    /// <summary>Quantidade total de linhas do arquivo digital, incluindo este registro.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLin { get; set; }
}
