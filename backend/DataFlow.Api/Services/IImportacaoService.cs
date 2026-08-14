using DataFlow.Api.DTOs.Responses;

namespace DataFlow.Api.Services;

public interface IImportacaoService
{
    ValidacaoArquivoResponse Validar(Stream arquivoStream);
    ImportacaoJsonResponse? GerarJson(Stream arquivoStream, string nomeArquivo);
}
