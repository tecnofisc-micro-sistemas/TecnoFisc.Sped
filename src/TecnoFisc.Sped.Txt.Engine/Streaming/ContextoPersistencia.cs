namespace TecnoFisc.Sped.Txt.Engine.Streaming;

/// <summary>
/// Identificadores surrogate atribuídos a um registro durante o streaming, prontos para uso
/// como PK/FK em modelos relacionais. Emitido por
/// <see cref="StreamingExtensions.WithContext(System.Collections.Generic.IAsyncEnumerable{TecnoFisc.Sped.Txt.Engine.Abstracoes.RegistroSped}, long, System.Threading.CancellationToken)"/>.
/// </summary>
/// <param name="IdRegistroAtual">
/// ID sequencial atribuído ao registro corrente. Cresce monotônico a partir do valor inicial
/// passado em <c>startAt</c> (default <c>1</c>).
/// </param>
/// <param name="IdPai">
/// ID do registro pai na hierarquia, ou <c>null</c> se o registro corrente é raiz (`0000`,
/// `9990`, `9999`, etc.). O pai sempre foi visto antes na sequência, então seu ID já está
/// resolvido — basta o consumidor amarrar como FK no INSERT.
/// </param>
public readonly record struct ContextoPersistencia(long IdRegistroAtual, long? IdPai);
