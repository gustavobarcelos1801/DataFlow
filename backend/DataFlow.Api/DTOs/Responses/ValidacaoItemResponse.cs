namespace DataFlow.Api.DTOs.Responses;

public class ValidacaoItemResponse
{
    public required string Id { get; init; }
    public required string Descricao { get; init; }
    public bool Sucesso { get; init; }
    public string? MensagemErro { get; init; }
}
