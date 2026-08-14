"use client";

import { useCallback, useRef, useState } from "react";
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import CircularProgress from "@mui/material/CircularProgress";
import Stack from "@mui/material/Stack";
import TelaPreview from "@/features/importacao/components/TelaPreview";
import TelaUpload from "@/features/importacao/components/TelaUpload";
import TelaValidacao from "@/features/importacao/components/TelaValidacao";
import type { RegistroPreview } from "@/features/importacao/types/registro.types";
import type { ValidacaoArquivoResponse } from "@/features/importacao/types/validacao.types";
import {
  buscarImportacaoCache,
  calcularHashArquivo,
} from "@/features/importacao/cache/importacaoCache";

export default function Home() {
  const [arquivo, setArquivo] = useState<File | null>(null);
  const [hashArquivo, setHashArquivo] = useState<string | null>(null);
  const [chaveUpload, setChaveUpload] = useState(0);
  const [respostaCache, setRespostaCache] =
    useState<ValidacaoArquivoResponse | null>(null);
  const [registros, setRegistros] = useState<RegistroPreview[] | null>(null);
  const [verificandoArquivo, setVerificandoArquivo] = useState(false);
  const operacaoIdRef = useRef(0);

  const aoReceberArquivo = useCallback(async (arquivoSelecionado: File) => {
    const idOperacao = ++operacaoIdRef.current;

    setArquivo(null);
    setHashArquivo(null);
    setRespostaCache(null);
    setRegistros(null);

    setArquivo(arquivoSelecionado);
    setVerificandoArquivo(true);

    try {
      const hash = await calcularHashArquivo(arquivoSelecionado);

      if (operacaoIdRef.current !== idOperacao) {
        return;
      }

      setHashArquivo(hash);

      const cache = await buscarImportacaoCache(hash);

      if (operacaoIdRef.current !== idOperacao) {
        return;
      }

      if (cache) {
        setRespostaCache(cache);
        setRegistros(cache.registros);
      }
    } catch (erro) {
      console.warn("Falha ao verificar cache de importação:", erro);
    } finally {
      if (operacaoIdRef.current === idOperacao) {
        setVerificandoArquivo(false);
      }
    }
  }, []);

  const aoValidacaoConcluida = useCallback(
    (response: ValidacaoArquivoResponse) => {
      setRegistros(response.registros);
    },
    []
  );

  const aoValidadasPeloCache = useCallback(() => {
    // A resposta do cache já define os registros em `aoReceberArquivo`.
  }, []);

  const handleCancelarImportacao = useCallback(() => {
    operacaoIdRef.current += 1;
    setArquivo(null);
    setHashArquivo(null);
    setRespostaCache(null);
    setRegistros(null);
    setVerificandoArquivo(false);
    setChaveUpload((chave) => chave + 1);
  }, []);

  return (
    <Box
      component="main"
      sx={{
        width: "100%",
        display: "flex",
        flexDirection: "column",
        alignItems: "flex-start",
        justifyContent: "flex-start",
        p: { xs: 1.5, sm: 4 },
        gap: 4,
      }}
    >
      <TelaUpload key={chaveUpload} onArquivoValido={aoReceberArquivo} />

      {verificandoArquivo && (
        <Stack
          component="section"
          direction="row"
          spacing={1.5}
          sx={{
            width: "100%",
            alignItems: "center",
            p: 4,
            border: "1px solid",
            borderColor: "divider",
            borderRadius: 3,
            backgroundColor: "background.paper",
          }}
        >
          <CircularProgress size={20} />
          <Typography variant="body2" sx={{ color: "text.secondary" }}>
            Verificando arquivo...
          </Typography>
        </Stack>
      )}

      {arquivo && !verificandoArquivo && (
        <TelaValidacao
          key={`${arquivo.name}-${arquivo.lastModified}`}
          arquivo={arquivo}
          hashArquivo={hashArquivo}
          respostaCache={respostaCache}
          onValidacaoConcluida={aoValidacaoConcluida}
          onValidadasPeloCache={aoValidadasPeloCache}
        />
      )}

      {arquivo && registros && !verificandoArquivo && (
        <TelaPreview
          registros={registros}
          arquivo={arquivo}
          onCancelarImportacao={handleCancelarImportacao}
        />
      )}
    </Box>
  );
}