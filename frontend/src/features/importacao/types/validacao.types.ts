import type { RegistroPreview } from "@/features/importacao/types/registro.types";

export type StatusValidacao = "pendente" | "validando" | "sucesso" | "erro";

export type ItemValidacao = {
  id: string;
  descricao: string;
  status: StatusValidacao;
  mensagemErro?: string;
};

export type ValidacaoItem = {
  id: string;
  descricao: string;
  sucesso: boolean;
  mensagemErro: string | null;
};

export type ValidacaoArquivoResponse = {
  sucesso: boolean;
  validacoes: ValidacaoItem[];
  registros: RegistroPreview[];
};
