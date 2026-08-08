using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Ecf.Versionamento;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>
/// Registro X305 — existiu nos leiautes 8 a 10 e foi removido no leiaute 11.
/// Reconhecido pelo catálogo para que arquivos históricos sejam legíveis, mas sem campos
/// modelados: o conteúdo das colunas sai por <c>ColunasNaoModeladas</c>. Modelar os campos
/// exige extraí-los do manual do leiaute 10 e é evolução planejada, puramente aditiva.
/// Nível hierárquico confirmado no Manual ECF Leiaute 10 (Anexo ADE Cofis nº 59/2023, página 471).
/// </summary>
[RegistroSped(Codigo = "X305", Nivel = 3, Bloco = "X")]
[Descontinuado(EmVersao = (int)LayoutEcf.V011)]
public sealed partial class RegistroX305 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "X305";
}
