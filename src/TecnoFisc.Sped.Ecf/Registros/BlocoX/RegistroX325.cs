using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Ecf.Versionamento;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoX;

/// <summary>
/// Registro X325 — existiu nos leiautes 8 a 10 e foi removido no leiaute 11.
/// O registro é reconhecido pelo catálogo para que um arquivo histórico seja legível, mas seus
/// campos não são modelados: o conteúdo das colunas não vira propriedade tipada — chega ao
/// consumidor em bruto em <see cref="RegistroSped.ColunasNaoModeladas"/>, com a posição e o
/// motivo <c>AlemDoModelo</c>. Modelar os campos exige extraí-los do manual do leiaute correspondente
/// e é evolução planejada, puramente aditiva. Escrita não suportada: um <c>EscritorSpedTxt</c>
/// lança <see cref="InvalidOperationException"/> ao tentar gravar este registro, em vez de emitir
/// uma linha só com o código.
/// Nível hierárquico confirmado no Manual ECF Leiaute 10 (Anexo ADE Cofis nº 59/2023, página 490).
/// </summary>
[RegistroSped(Codigo = "X325", Nivel = 3, Bloco = "X")]
[Descontinuado(EmVersao = (int)LayoutEcf.V011)]
public sealed partial class RegistroX325 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "X325";
}
