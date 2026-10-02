using System.Buffers.Binary;
using System.Text;
using TecnoFisc.Sped.Core.ValueObjects;
using TecnoFisc.Sped.NFeNFCe.Tests.Streaming;

namespace TecnoFisc.Sped.NFeNFCe.Tests;

public sealed class ReviewRegressionTests
{
    private const string Key = "31210711222333000181550050007156531436322400";
    private static string Event => $"<evento><infEvento><chNFe>{Key}</chNFe><tpEvento>210210</tpEvento><nSeqEvento>1</nSeqEvento></infEvento></evento>";

    [Fact]
    public async Task ConflictingEndRecordsCannotBypassZipBudgets()
    {
        using var original = ZipStreamingTests.Archive(("a.xml", ReadLimitsTests.Fixture), ("b.xml", ReadLimitsTests.Fixture), ("c.xml", ReadLimitsTests.Fixture));
        byte[] bytes = original.ToArray();
        int end = bytes.Length - 22;
        byte[] forged = new byte[bytes.Length + 23];
        bytes.CopyTo(forged, 0);
        Array.Clear(forged, end + 8, 12);
        BinaryPrimitives.WriteUInt16LittleEndian(forged.AsSpan(end + 20), 23);
        bytes.AsSpan(end, 22).CopyTo(forged.AsSpan(end + 22));
        using var stream = new MemoryStream(forged);
        await Assert.ThrowsAsync<InvalidDataException>(async () =>
        {
            await foreach (var result in new ParserNFe(new() { MaxZipEntries = 1, MaxZipCentralDirectoryBytes = 1 }).ReadZipAsync(stream, "forged", TestContext.Current.CancellationToken))
                _ = result;
        });
    }

    [Theory]
    [InlineData("35261000074274500608550010000200003000000016")]
    [InlineData("35261000074274500608558900000000011000000019")]
    public void CpfDiscriminatorTakesPrecedenceOverCoincidentCnpjChecksum(string value)
    {
        var key = ChaveAcesso.Create(value);
        key.CpfEmitente.Should().Be(Cpf.Create("74274500608"));
        Assert.Throws<FormatException>(() => key.CnpjEmitente);
    }

    [Fact]
    public void IncompleteEventIdentitiesDoNotCollapseDistinctSummaries()
    {
        var first = new EventoGenerico { ChaveAcesso = ChaveAcesso.Create(Key), TpEvento = 610600 };
        var second = first with { Retorno = new() { NProt = "222" } };
        new HashSet<Evento> { first, second, first }.Should().HaveCount(2);
    }

    [Fact]
    public void CompleteEventIdentityIgnoresOptionalRepresentationId()
    {
        var first = new EventoGenerico { ChaveAcesso = ChaveAcesso.Create(Key), TpEvento = 210210, NSeqEvento = 1, Id = "ID" };
        new HashSet<Evento> { first, first with { Id = null } }.Should().ContainSingle();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task UnknownWrapperChildrenFollowStrictAndDiagnosticPolicy(bool insideEvent, bool foreign)
    {
        string unknown = foreign ? "<future xmlns=\"urn:future\"><evento/></future>" : "<future><evento/></future>";
        string xml = insideEvent ? Event.Replace("<infEvento>", unknown + "<infEvento>", StringComparison.Ordinal) : "<NFeLog>" + unknown + Event + "</NFeLog>";
        using var strict = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        await Assert.ThrowsAsync<FormatException>(() => new ParserNFe(new() { Strict = true }).ReadEventoAsync(strict, TestContext.Current.CancellationToken));
        int ignored = 0;
        using var tolerant = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var result = await new ParserNFe(new() { OnIgnoredElement = _ => ignored++ }).ReadEventoAsync(tolerant, TestContext.Current.CancellationToken);
        result.TpEvento.Should().Be(210210);
        ignored.Should().Be(1);
    }

    [Fact]
    public async Task KnownSoapEnvelopeAndEventSignatureRemainAcceptedInStrictMode()
    {
        string signed = Event.Replace("</evento>", "<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo/></Signature></evento>", StringComparison.Ordinal);
        string xml = "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body><nfeConsultaNFeLogResult><retConsNFeLog><NFeLog>"
            + signed + "</NFeLog></retConsNFeLog></nfeConsultaNFeLogResult></soap:Body></soap:Envelope>";
        int ignored = 0;
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var result = await new ParserNFe(new() { Strict = true, OnIgnoredElement = _ => ignored++ }).ReadEventoAsync(input, TestContext.Current.CancellationToken);
        result.TpEvento.Should().Be(210210);
        ignored.Should().Be(0);
    }

    [Fact]
    public async Task ZipWithOrdinaryCommentRemainsAccepted()
    {
        using var original = ZipStreamingTests.Archive(("a.xml", ReadLimitsTests.Fixture));
        byte[] bytes = original.ToArray();
        byte[] commented = new byte[bytes.Length + 4];
        bytes.CopyTo(commented, 0);
        BinaryPrimitives.WriteUInt16LittleEndian(commented.AsSpan(bytes.Length - 2), 4);
        "test"u8.CopyTo(commented.AsSpan(bytes.Length));
        using var stream = new MemoryStream(commented);
        int count = 0;
        await foreach (var result in new ParserNFe().ReadZipAsync(stream, "commented", TestContext.Current.CancellationToken))
        {
            result.IsSuccess.Should().BeTrue();
            count++;
        }
        count.Should().Be(1);
    }
}
