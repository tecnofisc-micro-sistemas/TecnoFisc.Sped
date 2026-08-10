using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoY;

/// <summary>Registro Y650 - participantes do consórcio.</summary>
[RegistroSped(Codigo = "Y650", Nivel = 3, Bloco = "Y")]
public sealed partial class RegistroY650 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "Y650";

    [CampoSped(Ordem = 2, Tamanho = 14, Obrigatorio = true, Nome = "CNPJ")]
    public Cnpj Cnpj { get; set; }

    [CampoSped(Ordem = 3, Tamanho = 19, Decimais = 2, Nome = "VL_PART")]
    public decimal? VlPart { get; set; }
}
