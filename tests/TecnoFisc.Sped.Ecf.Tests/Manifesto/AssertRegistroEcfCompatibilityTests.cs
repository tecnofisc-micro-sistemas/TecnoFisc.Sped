using TecnoFisc.Sped.Core.ValueObjects;

namespace TecnoFisc.Sped.Ecf.Tests.Manifesto;

public sealed class AssertRegistroEcfCompatibilityTests
{
    [Theory]
    [InlineData("0930", "IDENT_CPF_CNPJ", 14, true)]
    [InlineData("X357", "NIF/CNPJ", 0, true)]
    [InlineData("Y600", "CPF_CNPJ", 14, false)]
    public void CampoDocumentoComposto_AceitaStringSemForcarValueObject(
        string codigo,
        string campo,
        int tamanho,
        bool obrigatorio)
    {
        var act = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            codigo,
            campo,
            typeof(string),
            tamanho,
            decimais: 0,
            obrigatorio);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("Y600", "CPF_REP_LEG", false)]
    [InlineData("Y612", "CPF", true)]
    public void CampoCpfNumerico_AceitaCpf(
        string codigo,
        string campo,
        bool obrigatorio)
    {
        var act = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            codigo,
            campo,
            typeof(Cpf),
            tamanho: 11,
            decimais: 0,
            obrigatorio);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("Q100", "DATA")]
    [InlineData("Y730", "DATA")]
    public void CampoDataNumerico_AceitaDateOnly(string codigo, string campo)
    {
        var act = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            codigo,
            campo,
            typeof(DateOnly),
            tamanho: 8,
            decimais: 0,
            obrigatorio: true);

        act.Should().NotThrow();
    }

    [Fact]
    public void CampoCpfExclusivo_RejeitaCnpj()
    {
        var act = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            "Y612",
            "CPF",
            typeof(Cnpj),
            tamanho: 11,
            decimais: 0,
            obrigatorio: true);

        act.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*Y612*CPF*tipo*incompatível*Cnpj*");
    }

    [Fact]
    public void CampoDocumentoComposto_RejeitaValueObjectDeDocumentoUnico()
    {
        var act = () => AssertRegistroEcf.FieldMetadataMatchesManifest(
            "Y600",
            "CPF_CNPJ",
            typeof(Cnpj),
            tamanho: 14,
            decimais: 0,
            obrigatorio: false);

        act.Should().Throw<Xunit.Sdk.XunitException>()
            .WithMessage("*Y600*CPF_CNPJ*tipo*incompatível*Cnpj*");
    }
}
