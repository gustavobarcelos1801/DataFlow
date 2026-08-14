export type ImportacaoJsonRegistro = {
  matricula: string;
  nome: string;
  curso: string;
  dataNascimento: string;
  email: string | null;
  mensalidade: number | null;
};

export type ImportacaoJsonArquivoOrigem = {
  nome: string;
  sha256: string;
};

export type ImportacaoJson = {
  versao: 1;
  tipoOperacao: "importacao";
  geradoEm: string;
  arquivoOrigem: ImportacaoJsonArquivoOrigem;
  quantidadeRegistros: number;
  registros: ImportacaoJsonRegistro[];
};