using System.Globalization;
using System.Security.Cryptography;
using ClosedXML.Excel;
using DataFlow.Api.DTOs.Responses;

namespace DataFlow.Api.Services;

public class ImportacaoService(ILogger<ImportacaoService> logger) : IImportacaoService
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

    private static readonly string[] CabecalhosObrigatorios =
    [
        "Matrícula*",
        "Nome*",
        "Curso*",
        "Data de Nascimento*"
    ];

    private const int NumeroMinimoRegistros = 10;
    private const int IdadeMinima = 18;
    private const int DigitosMinimosMatricula = 8;

    public ValidacaoArquivoResponse Validar(Stream arquivoStream)
    {
        try
        {
            using var workbook = new XLWorkbook(arquivoStream);
            var worksheet = workbook.Worksheets.First();
            return ValidarWorksheet(worksheet);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Não foi possível ler a estrutura da planilha.");
            return CriarRespostaErroAbertura();
        }
    }

    public ImportacaoJsonResponse? GerarJson(Stream arquivoStream, string nomeArquivo)
    {
        if (!arquivoStream.CanSeek)
        {
            throw new ArgumentException("O stream do arquivo precisa ser buscável (seekable).", nameof(arquivoStream));
        }

        arquivoStream.Position = 0;
        using var memoryStream = new MemoryStream();
        arquivoStream.CopyTo(memoryStream);
        var bytesArquivo = memoryStream.ToArray();

        var sha256 = CalcularSha256(bytesArquivo);

        using var streamValidacao = new MemoryStream(bytesArquivo);
        var validacao = Validar(streamValidacao);

        if (!validacao.Sucesso)
        {
            return null;
        }

        if (validacao.Registros.Any(r => r.Status == "erro"))
        {
            return null;
        }

        return new ImportacaoJsonResponse
        {
            Versao = ImportacaoJsonResponse.VersaoAtual,
            TipoOperacao = ImportacaoJsonResponse.TipoOperacaoImportacao,
            GeradoEm = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            ArquivoOrigem = new ArquivoOrigemResponse
            {
                Nome = nomeArquivo,
                Sha256 = sha256
            },
            QuantidadeRegistros = validacao.Registros.Count,
            Registros = [.. validacao.Registros.Select(ConverterParaRegistroJson)]
        };
    }

    private static string CalcularSha256(byte[] bytesArquivo)
    {
        var hash = SHA256.HashData(bytesArquivo);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static RegistroImportacaoJsonResponse ConverterParaRegistroJson(RegistroPreviewResponse registro)
    {
        var dataNascimento = DateTime.ParseExact(
            registro.DataNascimento!,
            "dd/MM/yyyy",
            CultureInfo.InvariantCulture);

        return new RegistroImportacaoJsonResponse
        {
            Matricula = registro.Matricula!,
            Nome = registro.Nome!,
            Curso = registro.Curso!,
            DataNascimento = dataNascimento.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Email = registro.Email,
            Mensalidade = registro.Mensalidade is null
                ? null
                : decimal.Parse(registro.Mensalidade, CultureInfo.InvariantCulture)
        };
    }

    private static ValidacaoArquivoResponse ValidarWorksheet(IXLWorksheet worksheet)
    {
        var cabecalhos = LerCabecalhos(worksheet);
        var quantidadeRegistros = ContarRegistros(worksheet);

        var validacoes = new List<ValidacaoItemResponse>
        {
            ValidarEstrutura(worksheet, cabecalhos),
            ValidarNumeroMinimoRegistros(quantidadeRegistros),
            ValidarColunasObrigatorias(cabecalhos)
        };

        var sucesso = validacoes.All(v => v.Sucesso);

        return new ValidacaoArquivoResponse
        {
            Sucesso = sucesso,
            Validacoes = validacoes,
            Registros = sucesso ? LerRegistros(worksheet) : []
        };
    }

    private static string[] LerCabecalhos(IXLWorksheet worksheet)
    {
        var cabecalhos = new string[CabecalhosOficiais.Length];

        for (var coluna = 1; coluna <= CabecalhosOficiais.Length; coluna++)
        {
            cabecalhos[coluna - 1] = worksheet.Cell(1, coluna).GetString().Trim();
        }

        return cabecalhos;
    }

    private static int ContarRegistros(IXLWorksheet worksheet)
    {
        var ultimaLinhaUsada = worksheet.LastRowUsed()?.RowNumber() ?? 1;
        var quantidadeRegistros = 0;

        for (var linha = 2; linha <= ultimaLinhaUsada; linha++)
        {
            if (LinhaPossuiConteudo(worksheet, linha))
            {
                quantidadeRegistros++;
            }
        }

        return quantidadeRegistros;
    }

    private static bool LinhaPossuiConteudo(IXLWorksheet worksheet, int linha)
    {
        for (var coluna = 1; coluna <= CabecalhosOficiais.Length; coluna++)
        {
            if (!worksheet.Cell(linha, coluna).IsEmpty())
            {
                return true;
            }
        }

        return false;
    }

    private static ValidacaoItemResponse ValidarEstrutura(IXLWorksheet worksheet, string[] cabecalhos)
    {
        var estruturaValida = cabecalhos.SequenceEqual(CabecalhosOficiais) && !ExisteConteudoAlemDaSextaColuna(worksheet);

        return new ValidacaoItemResponse
        {
            Id = "estrutura",
            Descricao = "Estrutura da planilha",
            Sucesso = estruturaValida,
            MensagemErro = estruturaValida ? null : "A estrutura da planilha não corresponde ao modelo padrão do sistema."
        };
    }

    private static bool ExisteConteudoAlemDaSextaColuna(IXLWorksheet worksheet)
    {
        var ultimaColunaUsada = worksheet.LastColumnUsed();
        if (ultimaColunaUsada is null || ultimaColunaUsada.ColumnNumber() <= CabecalhosOficiais.Length)
        {
            return false;
        }

        for (var coluna = CabecalhosOficiais.Length + 1; coluna <= ultimaColunaUsada.ColumnNumber(); coluna++)
        {
            if (worksheet.Column(coluna).CellsUsed().Any())
            {
                return true;
            }
        }

        return false;
    }

    private static ValidacaoItemResponse ValidarNumeroMinimoRegistros(int quantidadeRegistros)
    {
        var sucesso = quantidadeRegistros >= NumeroMinimoRegistros;

        return new ValidacaoItemResponse
        {
            Id = "numeroMinimoRegistros",
            Descricao = "Número mínimo de registros",
            Sucesso = sucesso,
            MensagemErro = sucesso ? null : $"A planilha possui {quantidadeRegistros} registros. O mínimo permitido é {NumeroMinimoRegistros}."
        };
    }

    private static ValidacaoItemResponse ValidarColunasObrigatorias(string[] cabecalhos)
    {
        var colunasAusentes = CabecalhosObrigatorios.Where(cabecalho => !cabecalhos.Contains(cabecalho)).ToArray();
        var sucesso = colunasAusentes.Length == 0;

        return new ValidacaoItemResponse
        {
            Id = "colunasObrigatorias",
            Descricao = "Colunas obrigatórias",
            Sucesso = sucesso,
            MensagemErro = sucesso ? null : FormatarMensagemColunasAusentes(colunasAusentes)
        };
    }

    private static string FormatarMensagemColunasAusentes(string[] colunasAusentes)
    {
        if (colunasAusentes.Length == 1)
        {
            return $"A coluna obrigatória \"{colunasAusentes[0]}\" não foi encontrada.";
        }

        var nomes = string.Join("\" e \"", colunasAusentes);
        return $"As colunas obrigatórias \"{nomes}\" não foram encontradas.";
    }

    private static IReadOnlyList<RegistroPreviewResponse> LerRegistros(IXLWorksheet worksheet)
    {
        var registros = new List<RegistroPreviewResponse>();
        var ultimaLinhaUsada = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var linha = 2; linha <= ultimaLinhaUsada; linha++)
        {
            if (LinhaPossuiConteudo(worksheet, linha))
            {
                registros.Add(LerRegistro(worksheet, linha));
            }
        }

        return registros;
    }

    private static RegistroPreviewResponse LerRegistro(IXLWorksheet worksheet, int linha)
    {
        var erros = new List<ErroRegistroResponse>();

        ValidarMatricula(worksheet.Cell(linha, 1), erros, out var matricula);
        ValidarNome(worksheet.Cell(linha, 2), erros, out var nome);
        ValidarCurso(worksheet.Cell(linha, 3), erros, out var curso);
        ValidarDataNascimento(worksheet.Cell(linha, 4), erros, out var dataNascimento);
        var email = LerEmail(worksheet.Cell(linha, 5));
        ValidarMensalidade(worksheet.Cell(linha, 6), erros, out var mensalidade);

        return new RegistroPreviewResponse
        {
            Matricula = matricula,
            Nome = nome,
            Curso = curso,
            DataNascimento = dataNascimento,
            Email = email,
            Mensalidade = mensalidade,
            Status = erros.Count == 0 ? "valido" : "erro",
            Erros = erros
        };
    }

    private static void ValidarMatricula(IXLCell cell, List<ErroRegistroResponse> erros, out string? matricula)
    {
        matricula = cell.GetFormattedString().Trim();

        if (string.IsNullOrEmpty(matricula))
        {
            erros.Add(new ErroRegistroResponse { Campo = "Matrícula", Mensagem = "Matrícula não preenchida." });
            return;
        }

        if (matricula.Length < DigitosMinimosMatricula || !matricula.All(char.IsDigit))
        {
            erros.Add(new ErroRegistroResponse { Campo = "Matrícula", Mensagem = "A matrícula deve conter somente números e possuir no mínimo 8 dígitos." });
        }
    }

    private static void ValidarNome(IXLCell cell, List<ErroRegistroResponse> erros, out string? nome)
    {
        nome = cell.GetString().Trim();

        if (string.IsNullOrEmpty(nome))
        {
            erros.Add(new ErroRegistroResponse { Campo = "Nome", Mensagem = "Nome não preenchido." });
        }
    }

    private static void ValidarCurso(IXLCell cell, List<ErroRegistroResponse> erros, out string? curso)
    {
        curso = cell.GetString().Trim();

        if (string.IsNullOrEmpty(curso))
        {
            erros.Add(new ErroRegistroResponse { Campo = "Curso", Mensagem = "Curso não preenchido." });
        }
    }

    private static void ValidarDataNascimento(IXLCell cell, List<ErroRegistroResponse> erros, out string? dataNascimento)
    {
        var data = LerDataNascimento(cell);

        if (data is null)
        {
            dataNascimento = null;

            if (cell.IsEmpty())
            {
                erros.Add(new ErroRegistroResponse { Campo = "Data de Nascimento", Mensagem = "Data de nascimento não preenchida." });
            }
            else
            {
                erros.Add(new ErroRegistroResponse { Campo = "Data de Nascimento", Mensagem = "Data de nascimento inválida." });
            }

            return;
        }

        dataNascimento = data.Value.ToString("dd/MM/yyyy");

        if (CalcularIdade(data.Value) < IdadeMinima)
        {
            erros.Add(new ErroRegistroResponse { Campo = "Data de Nascimento", Mensagem = "O aluno deve possuir 18 anos completos ou mais." });
        }
    }

    private static DateTime? LerDataNascimento(IXLCell cell)
    {
        if (cell.IsEmpty())
        {
            return null;
        }

        if (cell.DataType == XLDataType.DateTime)
        {
            return cell.GetDateTime();
        }

        if (cell.DataType == XLDataType.Number)
        {
            try
            {
                return DateTime.FromOADate(cell.GetDouble());
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        return DateTime.TryParse(cell.GetString(), out var data) ? data : null;
    }

    private static int CalcularIdade(DateTime dataNascimento)
    {
        var hoje = DateTime.Today;
        var idade = hoje.Year - dataNascimento.Year;

        if (dataNascimento.Date > hoje.AddYears(-idade))
        {
            idade--;
        }

        return idade;
    }

    private static string? LerEmail(IXLCell cell)
    {
        if (cell.IsEmpty())
        {
            return null;
        }

        return cell.GetString().Trim();
    }

    private static void ValidarMensalidade(IXLCell cell, List<ErroRegistroResponse> erros, out string? mensalidade)
    {
        if (cell.IsEmpty())
        {
            mensalidade = null;
            return;
        }

        if (!TryLerDecimal(cell, out var valor))
        {
            mensalidade = cell.GetFormattedString();
            erros.Add(new ErroRegistroResponse { Campo = "Mensalidade", Mensagem = "A mensalidade deve ser um valor numérico." });
            return;
        }

        mensalidade = valor.ToString("0.##", CultureInfo.InvariantCulture);

        if (valor <= 0)
        {
            erros.Add(new ErroRegistroResponse { Campo = "Mensalidade", Mensagem = "A mensalidade deve ser maior que zero." });
        }
    }

    private static bool TryLerDecimal(IXLCell cell, out decimal valor)
    {
        if (cell.DataType == XLDataType.Number)
        {
            valor = (decimal)cell.GetDouble();
            return true;
        }

        var texto = cell.GetString().Trim();
        return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out valor)
            || decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
    }

    private static ValidacaoArquivoResponse CriarRespostaErroAbertura()
    {
        var validacoes = new List<ValidacaoItemResponse>
        {
            new()
            {
                Id = "estrutura",
                Descricao = "Estrutura da planilha",
                Sucesso = false,
                MensagemErro = "Não foi possível ler a estrutura da planilha."
            },
            new()
            {
                Id = "numeroMinimoRegistros",
                Descricao = "Número mínimo de registros",
                Sucesso = false
            },
            new()
            {
                Id = "colunasObrigatorias",
                Descricao = "Colunas obrigatórias",
                Sucesso = false
            }
        };

        return new ValidacaoArquivoResponse
        {
            Sucesso = false,
            Validacoes = validacoes,
            Registros = []
        };
    }
}
