using System.Security.Cryptography;
using System.Text.Json;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging.Abstractions;
using DataFlow.Api.Services;

namespace DataFlow.Tests;

public class ImportacaoJsonServiceTests
{
    private static readonly string[] CabecalhosOficiais =
    [
        "Matrícula*",
        "Nome*",
        "Curso*",
        "Data de Nascimento*",
        "Email",
        "Mensalidade"
    ];

    private readonly ImportacaoService _service = new(NullLogger<ImportacaoService>.Instance);

    [Fact]
    public void GerarJson_ArquivoTotalmenteValido_DeveGerarJsonCompleto()
    {
        using var stream = CriarWorkbookComRegistros(50);

        var resultado = _service.GerarJson(stream, "alunos.xlsx");

        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.Versao);
        Assert.Equal("importacao", resultado.TipoOperacao);
        Assert.Equal("alunos.xlsx", resultado.ArquivoOrigem.Nome);
        Assert.Equal(50, resultado.QuantidadeRegistros);
        Assert.Equal(50, resultado.Registros.Count);
    }

    [Fact]
    public void GerarJson_Tela2Falha_DeveRetornarNull()
    {
        using var stream = CriarWorkbookComRegistros(9);

        var resultado = _service.GerarJson(stream, "alunos.xlsx");

        Assert.Null(resultado);
    }

    [Fact]
    public void GerarJson_RegistroInvalido_DeveRetornarNull()
    {
        using var stream = CriarWorkbookComRegistroInvalido();

        var resultado = _service.GerarJson(stream, "alunos.xlsx");

        Assert.Null(resultado);
    }

    [Fact]
    public void GerarJson_Matricula_DevePreservarComoString()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "00123456",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: 890m);

        var resultado = _service.GerarJson(stream, "alunos.xlsx");

        Assert.NotNull(resultado);
        var registro = resultado.Registros[0];
        Assert.Equal("00123456", registro.Matricula);
        Assert.IsType<string>(registro.Matricula);
    }

    [Fact]
    public void GerarJson_DataNascimento_DeveSairEmYyyyMmDd()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "00123456",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2001, 2, 14),
            email: "ana@exemplo.com",
            mensalidade: 890m);

        var resultado = _service.GerarJson(stream, "alunos.xlsx");

        Assert.NotNull(resultado);
        var registro = resultado.Registros[0];
        Assert.Equal("2001-02-14", registro.DataNascimento);
    }

    [Fact]
    public void GerarJson_EmailVazio_DeveSairNull()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "00123456",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: null,
            mensalidade: 890m);

        var resultado = _service.GerarJson(stream, "alunos.xlsx");

        Assert.NotNull(resultado);
        var registro = resultado.Registros[0];
        Assert.Null(registro.Email);
    }

    [Fact]
    public void GerarJson_MensalidadePreenchida_DeveSairNumero()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "00123456",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: 890m);

        var resultado = _service.GerarJson(stream, "alunos.xlsx");

        Assert.NotNull(resultado);
        var registro = resultado.Registros[0];
        Assert.NotNull(registro.Mensalidade);
        Assert.Equal(890m, registro.Mensalidade.Value);
    }

    [Fact]
    public void GerarJson_MensalidadeVazia_DeveSairNull()
    {
        using var stream = CriarWorkbookComRegistroEspecifico(
            matricula: "00123456",
            nome: "Aluno Teste",
            curso: "Curso A",
            dataNascimento: new DateTime(2000, 1, 1),
            email: "aluno@email.com",
            mensalidade: null);

        var resultado = _service.GerarJson(stream, "alunos.xlsx");

        Assert.NotNull(resultado);
        var registro = resultado.Registros[0];
        Assert.Null(registro.Mensalidade);
    }

    [Fact]
    public void GerarJson_QuantidadeRegistros_DeveCorresponderAoArquivo()
    {
        using var stream = CriarWorkbookComRegistros(50);

        var resultado = _service.GerarJson(stream, "alunos.xlsx");

        Assert.NotNull(resultado);
        Assert.Equal(50, resultado.QuantidadeRegistros);
        Assert.Equal(resultado.Registros.Count, resultado.QuantidadeRegistros);
    }

    [Fact]
    public void GerarJson_Sha256_DeveSerHexadecimalCom64Caracteres()
    {
        using var stream = CriarWorkbookComRegistros(50);

        var resultado = _service.GerarJson(stream, "alunos.xlsx");

        Assert.NotNull(resultado);
        Assert.Equal(64, resultado.ArquivoOrigem.Sha256.Length);
        Assert.IsType<string>(resultado.ArquivoOrigem.Sha256);
        Assert.True(resultado.ArquivoOrigem.Sha256.All(Uri.IsHexDigit));
    }

    [Fact]
    public void GerarJson_Sha256_DeveCorresponderAosBytesDoXlsx()
    {
        using var stream = CriarWorkbookComRegistros(50);
        var bytesOriginais = ((MemoryStream)stream).ToArray();
        var sha256Esperado = Convert.ToHexString(SHA256.HashData(bytesOriginais)).ToLowerInvariant();

        stream.Position = 0;
        var resultado = _service.GerarJson(stream, "alunos.xlsx");

        Assert.NotNull(resultado);
        Assert.Equal(sha256Esperado, resultado.ArquivoOrigem.Sha256);
    }

    [Fact]
    public void GerarJson_JsonFinal_NaoDeveConterStatusNemErros()
    {
        using var stream = CriarWorkbookComRegistros(50);

        var resultado = _service.GerarJson(stream, "alunos.xlsx");

        Assert.NotNull(resultado);
        var json = JsonSerializer.Serialize(resultado);
        Assert.DoesNotContain("\"status\"", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"erros\"", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GerarJson_GeradoEm_DeveSerUtcIso8601()
    {
        using var stream = CriarWorkbookComRegistros(50);

        var resultado = _service.GerarJson(stream, "alunos.xlsx");

        Assert.NotNull(resultado);
        var data = DateTimeOffset.Parse(resultado.GeradoEm);
        Assert.Equal(TimeSpan.Zero, data.Offset);
    }

    private static MemoryStream CriarWorkbookComRegistros(int quantidadeRegistros)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Planilha1");

        for (var coluna = 0; coluna < CabecalhosOficiais.Length; coluna++)
        {
            worksheet.Cell(1, coluna + 1).Value = CabecalhosOficiais[coluna];
        }

        for (var linha = 0; linha < quantidadeRegistros; linha++)
        {
            worksheet.Cell(linha + 2, 1).Value = $"2026{linha:0000}";
            worksheet.Cell(linha + 2, 2).Value = $"Aluno {linha + 1}";
            worksheet.Cell(linha + 2, 3).Value = "Curso A";
            worksheet.Cell(linha + 2, 4).Value = new DateTime(2000, 1, 1).AddDays(linha);
            worksheet.Cell(linha + 2, 5).Value = $"aluno{linha + 1}@email.com";
            worksheet.Cell(linha + 2, 6).Value = 890m;
        }

        return SalvarWorkbookEmMemoria(workbook);
    }

    private static MemoryStream CriarWorkbookComRegistroInvalido()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Planilha1");

        for (var coluna = 0; coluna < CabecalhosOficiais.Length; coluna++)
        {
            worksheet.Cell(1, coluna + 1).Value = CabecalhosOficiais[coluna];
        }

        for (var linha = 0; linha < 10; linha++)
        {
            worksheet.Cell(linha + 2, 1).Value = $"2026{linha:0000}";
            worksheet.Cell(linha + 2, 2).Value = $"Aluno {linha + 1}";
            worksheet.Cell(linha + 2, 3).Value = "Curso A";
            worksheet.Cell(linha + 2, 4).Value = new DateTime(2000, 1, 1).AddDays(linha);
            worksheet.Cell(linha + 2, 5).Value = $"aluno{linha + 1}@email.com";
            worksheet.Cell(linha + 2, 6).Value = 890m;
        }

        worksheet.Cell(2, 1).Value = "123";

        return SalvarWorkbookEmMemoria(workbook);
    }

    private static MemoryStream CriarWorkbookComRegistroEspecifico(
        string matricula,
        string nome,
        string curso,
        DateTime dataNascimento,
        string? email,
        decimal? mensalidade)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Planilha1");

        for (var coluna = 0; coluna < CabecalhosOficiais.Length; coluna++)
        {
            worksheet.Cell(1, coluna + 1).Value = CabecalhosOficiais[coluna];
        }

        worksheet.Cell(2, 1).Value = matricula;
        worksheet.Cell(2, 2).Value = nome;
        worksheet.Cell(2, 3).Value = curso;
        worksheet.Cell(2, 4).Value = dataNascimento;
        if (email is not null) worksheet.Cell(2, 5).Value = email;
        if (mensalidade is not null) worksheet.Cell(2, 6).Value = mensalidade.Value;

        for (var linha = 1; linha < 10; linha++)
        {
            var linhaAtual = linha + 2;
            worksheet.Cell(linhaAtual, 1).Value = $"2026{linha:0000}";
            worksheet.Cell(linhaAtual, 2).Value = $"Aluno Suplente {linha}";
            worksheet.Cell(linhaAtual, 3).Value = "Curso A";
            worksheet.Cell(linhaAtual, 4).Value = new DateTime(2000, 1, 1).AddDays(linha);
            worksheet.Cell(linhaAtual, 5).Value = $"suplente{linha}@email.com";
            worksheet.Cell(linhaAtual, 6).Value = 890m;
        }

        return SalvarWorkbookEmMemoria(workbook);
    }

    private static MemoryStream SalvarWorkbookEmMemoria(XLWorkbook workbook)
    {
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}