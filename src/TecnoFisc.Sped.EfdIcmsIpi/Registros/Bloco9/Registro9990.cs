using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.EfdIcmsIpi.Registros.Bloco9;

/// <summary>
/// Registro 9990 - Encerramento do Bloco 9. Nivel hierarquico 1, ocorrencia unica por arquivo.
/// Conforme Guia Pratico EFD-ICMS/IPI V3.0.6, p. 302.
/// </summary>
[RegistroSped(Codigo = "9990", Nivel = 1, Bloco = "9")]
public sealed partial class Registro9990 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "9990";

    /// <summary>Quantidade total de linhas do Bloco 9, incluindo o registro 9999.</summary>
    [CampoSped(Ordem = 2, Obrigatorio = true)]
    public int QtdLin9 { get; set; }
}
