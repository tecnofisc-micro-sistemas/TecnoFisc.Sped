using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Txt.Engine.Tests._Sintetico;

/// <summary>
/// Campo 3 só existe a partir da versão 12; campo 4, a partir da 20. Campo 2 existe sempre.
/// <c>DesdeVersao</c> é não-decrescente ao longo da posição (0, 12, 20) — a única forma que
/// <c>CatalogoBuilder</c> aceita (<c>CatalogoBuilder.ValidarVigenciaCrescente</c> rejeita um
/// campo sempre-presente ou de versão anterior depois de um campo versionado). Um
/// arquivo que declara versão anterior mas traz as colunas 3 e/ou 4 fisicamente preenchidas não
/// pode atribuí-las fora de hora nem deslocar a coluna seguinte.
/// </summary>
[RegistroSped(Codigo = "A300", Nivel = 2, Bloco = "A")]
public sealed partial class RegistroVigenciaColunaSintetico : RegistroSped
{
    public override string Codigo => "A300";

    [CampoSped(Ordem = 2, Nome = "ANTES")]
    public string? Antes { get; set; }

    [CampoSped(Ordem = 3, Nome = "NOVO", DesdeVersao = 12)]
    public string? Novo { get; set; }

    [CampoSped(Ordem = 4, Nome = "DEPOIS", DesdeVersao = 20)]
    public string? Depois { get; set; }
}
