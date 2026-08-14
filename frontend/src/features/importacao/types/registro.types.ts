export type StatusRegistro = "valido" | "erro";

export type ErroRegistro = {
  campo: string;
  mensagem: string;
};

export type RegistroPreview = {
  matricula: string | null;
  nome: string | null;
  curso: string | null;
  dataNascimento: string | null;
  email: string | null;
  mensalidade: string | null;
  status: StatusRegistro;
  erros: ErroRegistro[];
};
