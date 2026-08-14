"use client";

import { useCallback, useRef, useState } from "react";
import type { ChangeEvent, DragEvent } from "react";
import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import Button from "@mui/material/Button";
import Alert from "@mui/material/Alert";
import UploadFile from "@mui/icons-material/UploadFile";
import Info from "@mui/icons-material/Info";
import Download from "@mui/icons-material/Download";
import SectionCard from "@/design-system/components/SectionCard";

type TelaUploadProps = {
  onArquivoValido?: (arquivo: File) => void;
};

const EXTENSAO_VALIDA = ".xlsx";

function ehArquivoXlsx(arquivo: File): boolean {
  return arquivo.name.toLowerCase().endsWith(EXTENSAO_VALIDA);
}

export default function TelaUpload({ onArquivoValido }: TelaUploadProps) {
  const [arquivo, setArquivo] = useState<File | null>(null);
  const [erro, setErro] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const processarArquivo = useCallback(
    (arquivoSelecionado: File) => {
      if (!ehArquivoXlsx(arquivoSelecionado)) {
        setArquivo(null);
        setErro("Selecione um arquivo válido no formato .xlsx.");
        return;
      }

      setArquivo(arquivoSelecionado);
      setErro(null);
      onArquivoValido?.(arquivoSelecionado);
    },
    [onArquivoValido]
  );

  const abrirSeletor = useCallback(() => {
    inputRef.current?.click();
  }, []);

  const aoSelecionarArquivo = useCallback(
    (evento: ChangeEvent<HTMLInputElement>) => {
      const arquivoSelecionado = evento.target.files?.[0];
      if (arquivoSelecionado) {
        processarArquivo(arquivoSelecionado);
      }
      evento.target.value = "";
    },
    [processarArquivo]
  );

  const aoSoltarArquivo = useCallback(
    (evento: DragEvent<HTMLDivElement>) => {
      evento.preventDefault();
      const arquivoSoltado = evento.dataTransfer.files?.[0];
      if (arquivoSoltado) {
        processarArquivo(arquivoSoltado);
      }
    },
    [processarArquivo]
  );

  const aoArrastarSobre = useCallback((evento: DragEvent<HTMLDivElement>) => {
    evento.preventDefault();
  }, []);

  return (
    <SectionCard titulo="Arquivo de Importação">
      <Box
        role="button"
        tabIndex={0}
        onClick={abrirSeletor}
        onKeyDown={(evento) => {
          if (evento.key === "Enter" || evento.key === " ") {
            abrirSeletor();
          }
        }}
        onDragOver={aoArrastarSobre}
        onDrop={aoSoltarArquivo}
        sx={{
          display: "flex",
          height: 64,
          width: "100%",
          cursor: "pointer",
          alignItems: "center",
          justifyContent: "center",
          gap: 1,
          borderRadius: 2,
          border: "2px dashed",
          borderColor: "divider",
          backgroundColor: "grey.50",
          color: "text.secondary",
          transition: "border-color 0.2s, background-color 0.2s",
          "&:hover": {
            borderColor: "grey.400",
            backgroundColor: "grey.100",
          },
        }}
      >
        <UploadFile sx={{ fontSize: 20 }} />
        <Typography variant="body2" component="span">
          {arquivo ? arquivo.name : "Selecionar arquivo .xlsx"}
        </Typography>
      </Box>

      {erro && (
        <Alert severity="error" sx={{ mt: 2 }}>
          {erro}
        </Alert>
      )}

      <input
        ref={inputRef}
        type="file"
        accept=".xlsx"
        style={{ display: "none" }}
        onChange={aoSelecionarArquivo}
      />

      <Box sx={{ mt: 3, display: "flex", alignItems: "center", gap: 1 }}>
        <Info sx={{ fontSize: 16, color: "info.main" }} />
        <Typography variant="body2">
          Utilize o modelo padrão do DataFlow
        </Typography>
      </Box>

      <Button
        component="a"
        href="/modelos/modelo-importacao-dataflow.xlsx"
        download="modelo-importacao-dataflow.xlsx"
        variant="outlined"
        color="inherit"
        startIcon={<Download />}
        sx={{ mt: 2, color: "text.primary", borderColor: "divider" }}
      >
        Baixar modelo de planilha
      </Button>
    </SectionCard>
  );
}