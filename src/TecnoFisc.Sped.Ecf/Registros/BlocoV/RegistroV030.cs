using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoV;

/// <summary>Registro V030 - mês do período da declaração DEREX.</summary>
[RegistroSped(Codigo = "V030", Nivel = 3, Bloco = "V")]
public sealed partial class RegistroV030 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "V030";

    /// <summary>Mês com dois caracteres, preservado sem validar o período do arquivo.</summary>
    [CampoSped(Ordem = 2, Tamanho = 2, Obrigatorio = true, Nome = "MES")]
    public string? Mes { get; set; }
}
