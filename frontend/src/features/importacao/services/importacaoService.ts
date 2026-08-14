import type { ValidacaoArquivoResponse } from "@/features/importacao/types/validacao.types";
import type { ImportacaoJson } from "@/features/importacao/types/importacaoJson.types";

const CAMPO_ARQUIVO = "arquivo";
const ENDPOINT_VALIDAR = "/api/importacoes/validar";
const ENDPOINT_GERAR_JSON = "/api/importacoes/gerar-json";

const MENSAGEM_ERRO_URL_AUSENTE =
  "A variável de ambiente NEXT_PUBLIC_API_URL não está definida.";

const MENSAGEM_ERRO_RESPOSTA =
  "Falha ao validar o arquivo no servidor. Tente novamente.";

const MENSAGEM_ERRO_GERACAO_JSON =
  "Não foi possível gerar o JSON. O arquivo ou algum registro possui inconsistências.";

function obterUrlBase(): string {
  const urlBase = process.env.NEXT_PUBLIC_API_URL;

  if (!urlBase) {
    throw new Error(MENSAGEM_ERRO_URL_AUSENTE);
  }

  return urlBase;
}

export async function validarArquivo(
  arquivo: File
): Promise<ValidacaoArquivoResponse> {
  const urlBase = obterUrlBase();

  const formData = new FormData();
  formData.append(CAMPO_ARQUIVO, arquivo);

  let resposta: Response;

  try {
    resposta = await fetch(`${urlBase}${ENDPOINT_VALIDAR}`, {
      method: "POST",
      body: formData,
    });
  } catch {
    throw new Error(
      "Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente."
    );
  }

  if (!resposta.ok) {
    throw new Error(MENSAGEM_ERRO_RESPOSTA);
  }

  return (await resposta.json()) as ValidacaoArquivoResponse;
}

export async function gerarJsonImportacao(
  arquivo: File
): Promise<ImportacaoJson> {
  const urlBase = obterUrlBase();

  const formData = new FormData();
  formData.append(CAMPO_ARQUIVO, arquivo);

  let resposta: Response;

  try {
    resposta = await fetch(`${urlBase}${ENDPOINT_GERAR_JSON}`, {
      method: "POST",
      body: formData,
    });
  } catch {
    throw new Error(
      "Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente."
    );
  }

  if (resposta.status === 422) {
    throw new Error(MENSAGEM_ERRO_GERACAO_JSON);
  }

  if (!resposta.ok) {
    throw new Error(
      "Falha ao gerar o JSON no servidor. Tente novamente."
    );
  }

  return (await resposta.json()) as ImportacaoJson;
}
