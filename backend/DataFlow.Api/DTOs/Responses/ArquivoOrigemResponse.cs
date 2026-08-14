namespace DataFlow.Api.DTOs.Responses;

public class ArquivoOrigemResponse
{
    public required string Nome { get; init; }
    public required string Sha256 { get; init; }
}