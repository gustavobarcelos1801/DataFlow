using Microsoft.AspNetCore.Mvc;
using DataFlow.Api.DTOs.Responses;
using DataFlow.Api.Services;

namespace DataFlow.Api.Controllers;

[ApiController]
[Route("api/importacoes")]
public class ImportacaoController(IImportacaoService importacaoService) : ControllerBase
{
    [HttpPost("validar")]
    public ActionResult<ValidacaoArquivoResponse> Validar(IFormFile? arquivo)
    {
        if (arquivo is null || arquivo.Length == 0)
        {
            return BadRequest(new { mensagem = "Nenhum arquivo foi enviado." });
        }

        if (!Path.GetExtension(arquivo.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { mensagem = "Selecione um arquivo válido no formato .xlsx." });
        }

        using var stream = arquivo.OpenReadStream();
        var resultado = importacaoService.Validar(stream);

        return Ok(resultado);
    }

    [HttpPost("gerar-json")]
    public ActionResult<ImportacaoJsonResponse> GerarJson(IFormFile? arquivo)
    {
        if (arquivo is null || arquivo.Length == 0)
        {
            return BadRequest(new { mensagem = "Nenhum arquivo foi enviado." });
        }

        if (!Path.GetExtension(arquivo.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { mensagem = "Selecione um arquivo válido no formato .xlsx." });
        }

        using var stream = arquivo.OpenReadStream();
        var resultado = importacaoService.GerarJson(stream, arquivo.FileName);

        if (resultado is null)
        {
            return UnprocessableEntity(new { mensagem = "Não foi possível gerar o JSON. O arquivo ou algum registro possui inconsistências." });
        }

        return Ok(resultado);
    }
}
