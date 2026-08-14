"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import Box from "@mui/material/Box";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import Alert from "@mui/material/Alert";
import CircularProgress from "@mui/material/CircularProgress";
import CheckCircle from "@mui/icons-material/CheckCircle";
import ItemValidacao from "@/features/importacao/components/ItemValidacao";
import SectionCard from "@/design-system/components/SectionCard";
import type {
  ItemValidacao as ItemValidacaoType,
  StatusValidacao,
  ValidacaoArquivoResponse,
} from "@/features/importacao/types/validacao.types";
import {
  sleep,
  tempoAleatorioValidacao,
} from "@/features/importacao/utils/sleep";
import { validarArquivo } from "@/features/importacao/services/importacaoService";
import { salvarImportacaoCache } from "@/features/importacao/cache/importacaoCache";

type TelaValidacaoProps = {
  arquivo: File;
  hashArquivo: string | null;
  respostaCache: ValidacaoArquivoResponse | null;
  onValidacaoConcluida?: (response: ValidacaoArquivoResponse) => void;
  onValidadasPeloCache?: () => void;
};

const VALIDACOES_INICIAIS: ItemValidacaoType[] = [
  { id: "estrutura", descricao: "Estrutura da planilha", status: "pendente" },
  {
    id: "numeroMinimoRegistros",
    descricao: "Número mínimo de registros",
    status: "pendente",
  },
  {
    id: "colunasObrigatorias",
    descricao: "Colunas obrigatórias",
    status: "pendente",
  },
];

const ORDEM_VALIDACOES = [
  "estrutura",
  "numeroMinimoRegistros",
  "colunasObrigatorias",
] as const;

const MENSAGEM_ERRO_INESPERADO =
  "Falha ao validar o arquivo. Tente novamente.";

function mapearItensDaResposta(
  response: ValidacaoArquivoResponse
): ItemValidacaoType[] {
  return ORDEM_VALIDACOES.map((id) => {
    const validacao = response.validacoes.find((item) => item.id === id);
    const sucesso = validacao?.sucesso ?? false;

    return {
      id,
      descricao: VALIDACOES_INICIAIS.find((item) => item.id === id)?.descricao ?? id,
      status: sucesso ? "sucesso" : "erro",
      mensagemErro: sucesso ? undefined : (validacao?.mensagemErro ?? undefined),
    };
  });
}

export default function TelaValidacao({
  arquivo,
  hashArquivo,
  respostaCache,
  onValidacaoConcluida,
  onValidadasPeloCache,
}: TelaValidacaoProps) {
  const cacheInicial = useMemo(
    () => (respostaCache ? mapearItensDaResposta(respostaCache) : null),
    [respostaCache]
  );

  const [itens, setItens] = useState<ItemValidacaoType[]>(
    cacheInicial ?? VALIDACOES_INICIAIS
  );
  const [validando, setValidando] = useState(false);
  const [validacaoConcluida, setValidacaoConcluida] = useState(
    Boolean(cacheInicial)
  );
  const [houveErro, setHouveErro] = useState(false);
  const [mensagemErroComunicacao, setMensagemErroComunicacao] = useState<
    string | null
  >(null);
  const processandoRef = useRef(false);

  const atualizarStatus = useCallback(
    (id: string, status: StatusValidacao, mensagemErro?: string) => {
      setItens((atual) =>
        atual.map((item) =>
          item.id === id ? { ...item, status, mensagemErro } : item
        )
      );
    },
    []
  );

  const validar = useCallback(async () => {
    if (processandoRef.current) {
      return;
    }
    processandoRef.current = true;
    setValidando(true);
    setValidacaoConcluida(false);
    setHouveErro(false);
    setMensagemErroComunicacao(null);
    setItens(VALIDACOES_INICIAIS);

    try {
      const response = await validarArquivo(arquivo);

      for (const id of ORDEM_VALIDACOES) {
        atualizarStatus(id, "validando");
        await sleep(tempoAleatorioValidacao());

        const validacao = response.validacoes.find(
          (item) => item.id === id
        );

        if (!validacao) {
          atualizarStatus(id, "erro");
          continue;
        }

        atualizarStatus(
          id,
          validacao.sucesso ? "sucesso" : "erro",
          validacao.sucesso ? undefined : validacao.mensagemErro ?? undefined
        );
      }

      setValidando(false);
      setValidacaoConcluida(true);
      setHouveErro(!response.sucesso);

      if (response.sucesso) {
        if (hashArquivo) {
          await salvarImportacaoCache(hashArquivo, arquivo, response);
        }
        onValidacaoConcluida?.(response);
      }
    } catch (erro) {
      setValidando(false);
      setHouveErro(true);
      setMensagemErroComunicacao(
        erro instanceof Error ? erro.message : MENSAGEM_ERRO_INESPERADO
      );
    } finally {
      processandoRef.current = false;
    }
  }, [arquivo, hashArquivo, atualizarStatus, onValidacaoConcluida]);

  const sucessoCompleto = validacaoConcluida && !houveErro;

  const dadosDoCache = Boolean(respostaCache && cacheInicial);

  useEffect(() => {
    if (dadosDoCache) {
      onValidadasPeloCache?.();
    }
  }, [dadosDoCache, onValidadasPeloCache]);

  const textoBotao = validando
    ? "Validando arquivo..."
    : houveErro
      ? "Validar novamente"
      : validacaoConcluida
        ? "Arquivo validado"
        : "Validar arquivo";

  return (
    <SectionCard titulo="Validação do Arquivo">
      {dadosDoCache && (
        <Alert severity="info" sx={{ mb: 2 }}>
          Arquivo previamente validado
        </Alert>
      )}

      <Grid container spacing={2}>
        {itens.map((item) => (
          <Grid size={{ xs: 12, md: 6 }} key={item.id}>
            <ItemValidacao
              descricao={item.descricao}
              status={item.status}
              mensagemErro={item.mensagemErro}
            />
          </Grid>
        ))}
      </Grid>

      {mensagemErroComunicacao && (
        <Alert severity="error" sx={{ mt: 2 }}>
          {mensagemErroComunicacao}
        </Alert>
      )}

      <Box sx={{ mt: 4, display: "flex", justifyContent: "flex-end" }}>
        <Button
          type="button"
          onClick={validar}
          disabled={validando || sucessoCompleto}
          variant={sucessoCompleto ? "outlined" : "contained"}
          color={sucessoCompleto ? "success" : "primary"}
          startIcon={
            validando ? (
              <CircularProgress size={16} color="inherit" />
            ) : sucessoCompleto ? (
              <CheckCircle />
            ) : undefined
          }
          sx={{
            ...(sucessoCompleto && {
              cursor: "not-allowed",
              borderColor: "success.light",
              backgroundColor: "success.light",
              color: "success.dark",
            }),
            ...(validando && {
              backgroundColor: "primary.light",
              color: "#ffffff",
            }),
          }}
        >
          {textoBotao}
        </Button>
      </Box>
    </SectionCard>
  );
}