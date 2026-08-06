using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoC;

/// <summary>Registro C350 - data dos saldos de resultado antes do encerramento.</summary>
[RegistroSped(Codigo = "C350", Nivel = 3, Bloco = "C")]
public sealed partial class RegistroC350 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C350";

    [CampoSped(Ordem = 2, Tamanho = 8, Formato = "ddMMyyyy", Obrigatorio = true)]
    public DateOnly DtRes { get; set; }
}
