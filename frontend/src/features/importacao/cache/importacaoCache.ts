import type { ValidacaoArquivoResponse } from "@/features/importacao/types/validacao.types";

/**
 * Sempre que regras de validação, estrutura relevante da resposta ou
 * semântica do processo forem alteradas de maneira incompatível,
 * incrementar IMPORTACAO_CACHE_VERSION.
 */
const IMPORTACAO_CACHE_VERSION = 1;

const NOME_BD = "smart-cash-flow";
const NOME_STORE = "importacoes-validadas";
const KEY_PATH = "hash";

type ImportacaoCacheEntry = {
  hash: string;
  version: number;
  nomeArquivo: string;
  tamanhoArquivo: number;
  ultimaModificacaoArquivo: number;
  salvoEm: string;
  response: ValidacaoArquivoResponse;
};

function abrirBanco(): Promise<IDBDatabase> {
  return new Promise((resolve, reject) => {
    const requisicao = indexedDB.open(NOME_BD, 1);

    requisicao.onupgradeneeded = () => {
      const bd = requisicao.result;
      if (!bd.objectStoreNames.contains(NOME_STORE)) {
        bd.createObjectStore(NOME_STORE, { keyPath: KEY_PATH });
      }
    };

    requisicao.onsuccess = () => {
      resolve(requisicao.result);
    };

    requisicao.onerror = () => {
      reject(requisicao.error);
    };
  });
}

export async function calcularHashArquivo(arquivo: File): Promise<string> {
  const buffer = await arquivo.arrayBuffer();
  const digest = await crypto.subtle.digest("SHA-256", buffer);
  return Array.from(new Uint8Array(digest))
    .map((byte) => byte.toString(16).padStart(2, "0"))
    .join("");
}

export async function buscarImportacaoCache(
  hash: string
): Promise<ValidacaoArquivoResponse | null> {
  try {
    const bd = await abrirBanco();
    try {
      return await new Promise((resolve, reject) => {
        const transacao = bd.transaction(NOME_STORE, "readonly");
        const store = transacao.objectStore(NOME_STORE);
        const requisicao = store.get(hash);

        requisicao.onsuccess = () => {
          const registro = requisicao.result as
            | ImportacaoCacheEntry
            | undefined;

          if (!registro || registro.version !== IMPORTACAO_CACHE_VERSION) {
            resolve(null);
            return;
          }

          resolve(registro.response);
        };

        requisicao.onerror = () => {
          reject(requisicao.error);
        };
      });
    } finally {
      bd.close();
    }
  } catch (erro) {
    console.warn("Falha ao buscar cache de importação:", erro);
    return null;
  }
}

export async function salvarImportacaoCache(
  hash: string,
  arquivo: File,
  response: ValidacaoArquivoResponse
): Promise<void> {
  if (!response.sucesso) {
    return;
  }

  const registro: ImportacaoCacheEntry = {
    hash,
    version: IMPORTACAO_CACHE_VERSION,
    nomeArquivo: arquivo.name,
    tamanhoArquivo: arquivo.size,
    ultimaModificacaoArquivo: arquivo.lastModified,
    salvoEm: new Date().toISOString(),
    response,
  };

  try {
    const bd = await abrirBanco();
    try {
      await new Promise<void>((resolve, reject) => {
        const transacao = bd.transaction(NOME_STORE, "readwrite");
        const store = transacao.objectStore(NOME_STORE);
        store.put(registro);

        transacao.oncomplete = () => resolve();
        transacao.onerror = () => reject(transacao.error);
      });
    } finally {
      bd.close();
    }
  } catch (erro) {
    console.warn("Falha ao salvar cache de importação:", erro);
  }
}