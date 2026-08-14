"use client";

import { useState } from "react";
import Box from "@mui/material/Box";
import Paper from "@mui/material/Paper";
import Typography from "@mui/material/Typography";
import Stack from "@mui/material/Stack";
import Button from "@mui/material/Button";
import Alert from "@mui/material/Alert";
import Tooltip from "@mui/material/Tooltip";
import StatusChip from "@/design-system/components/StatusChip";
import AppDataGrid from "@/design-system/components/AppDataGrid";
import SectionCard from "@/design-system/components/SectionCard";
import ProblemasRegistroDialog from "@/features/importacao/components/ProblemasRegistroDialog";
import CancelarImportacaoDialog from "@/features/importacao/components/CancelarImportacaoDialog";
import ConfirmarImportacaoDialog from "@/features/importacao/components/ConfirmarImportacaoDialog";
import JsonVisualDialog from "@/features/importacao/components/JsonVisualDialog";
import { gerarJsonImportacao } from "@/features/importacao/services/importacaoService";
import type { GridColDef } from "@mui/x-data-grid";
import type { RegistroPreview } from "@/features/importacao/types/registro.types";

type RegistroGridRow = RegistroPreview & {
  id: number;
};

type TelaPreviewProps = {
  registros: RegistroPreview[];
  arquivo: File;
  onCancelarImportacao: () => void;
};

const formatadorMoeda = new Intl.NumberFormat("pt-BR", {
  style: "currency",
  currency: "BRL",
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

function formatarMensalidade(registro: RegistroPreview) {
  const mensalidade = registro.mensalidade;

  if (mensalidade === null) {
    return "";
  }

  const valor = Number(mensalidade);

  if (Number.isNaN(valor)) {
    return mensalidade;
  }

  return formatadorMoeda.format(valor);
}

export default function TelaPreview({
  registros,
  arquivo,
  onCancelarImportacao,
}: TelaPreviewProps) {
  const [registroSelecionado, setRegistroSelecionado] =
    useState<RegistroPreview | null>(null);
  const [dialogCancelarAberto, setDialogCancelarAberto] = useState(false);
  const [dialogConfirmarAberto, setDialogConfirmarAberto] = useState(false);
  const [jsonVisualAberto, setJsonVisualAberto] = useState(false);
  const [jsonFormatado, setJsonFormatado] = useState("");
  const [erroGeracao, setErroGeracao] = useState<string | null>(null);

  const totalRegistros = registros.length;
  const totalValidos = registros.filter(
    (registro) => registro.status === "valido"
  ).length;
  const totalErros = registros.filter(
    (registro) => registro.status === "erro"
  ).length;

  const possuiErros = totalErros > 0;

  const linhas: RegistroGridRow[] = registros.map((registro, index) => ({
    ...registro,
    id: index,
  }));

  const aoClicarConfirmar = () => {
    setDialogConfirmarAberto(true);
  };

  const aoGerarJsonVisual = async () => {
    try {
      const payload = await gerarJsonImportacao(arquivo);
      setJsonFormatado(JSON.stringify(payload, null, 2));
      setDialogConfirmarAberto(false);
      setJsonVisualAberto(true);
      setErroGeracao(null);
    } catch (erro) {
      setErroGeracao(
        erro instanceof Error ? erro.message : "Falha ao gerar o JSON visual."
      );
    }
  };

  const aoGerarArquivoJson = async () => {
    try {
      const payload = await gerarJsonImportacao(arquivo);
      const json = JSON.stringify(payload, null, 2);
      const blob = new Blob([json], { type: "application/json;charset=utf-8" });
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = `${arquivo.name.replace(/\.xlsx$/i, "")}-importacao.json`;
      link.click();
      URL.revokeObjectURL(url);
      setDialogConfirmarAberto(false);
      setErroGeracao(null);
    } catch (erro) {
      setErroGeracao(
        erro instanceof Error ? erro.message : "Falha ao gerar o arquivo JSON."
      );
    }
  };

  const colunas: GridColDef[] = [
    {
      field: "matricula",
      headerName: "Matrícula",
      flex: 0.8,
      minWidth: 125,
      valueGetter: (_v, row) => (row as RegistroPreview).matricula ?? "",
    },
    {
      field: "nome",
      headerName: "Nome",
      flex: 1.3,
      minWidth: 190,
      valueGetter: (_v, row) => (row as RegistroPreview).nome ?? "",
    },
    {
      field: "curso",
      headerName: "Curso",
      flex: 1.4,
      minWidth: 220,
      valueGetter: (_v, row) => (row as RegistroPreview).curso ?? "",
    },
    {
      field: "dataNascimento",
      headerName: "Data de nascimento",
      flex: 1,
      minWidth: 175,
      valueGetter: (_v, row) => (row as RegistroPreview).dataNascimento ?? "",
    },
    {
      field: "email",
      headerName: "Email",
      flex: 1.6,
      minWidth: 250,
      valueGetter: (_v, row) => (row as RegistroPreview).email ?? "",
    },
    {
      field: "mensalidade",
      headerName: "Mensalidade",
      flex: 0.9,
      minWidth: 155,
      align: "right",
      headerAlign: "right",
      valueGetter: (_v, row) => formatarMensalidade(row as RegistroPreview),
    },
    {
      field: "status",
      headerName: "Status",
      flex: 0.8,
      minWidth: 130,
      renderCell: (params) => {
        const registro = params.row as RegistroPreview;
        return (
          <StatusChip
            status={registro.status}
            onClick={
              registro.status === "erro"
                ? () => setRegistroSelecionado(registro)
                : undefined
            }
          />
        );
      },
    },
  ];

  return (
    <SectionCard titulo="Pré-visualização da Importação">
      <Box sx={{ mt: 3, display: "grid", gridTemplateColumns: { xs: "1fr", md: "repeat(3, 1fr)" }, gap: 2 }}>
        <Paper sx={{ p: 2, pl: 2.5, border: "1px solid", borderColor: "divider", borderRadius: "10px", borderLeft: "4px solid", borderLeftColor: "primary.main", backgroundColor: "background.paper" }}>
          <Typography variant="caption" sx={{ display: "block", fontWeight: 500, textTransform: "uppercase", letterSpacing: "0.05em", color: "text.secondary", fontSize: "0.75rem" }}>
            Total de registros
          </Typography>
          <Typography variant="h4" sx={{ mt: 0.5, fontWeight: 600, color: "primary.main" }}>
            {totalRegistros}
          </Typography>
        </Paper>

        <Paper sx={{ p: 2, pl: 2.5, border: "1px solid", borderColor: "divider", borderRadius: "10px", borderLeft: "4px solid", borderLeftColor: "success.main", backgroundColor: "background.paper" }}>
          <Typography variant="caption" sx={{ display: "block", fontWeight: 500, textTransform: "uppercase", letterSpacing: "0.05em", color: "text.secondary", fontSize: "0.75rem" }}>
            Válidos
          </Typography>
          <Typography variant="h4" sx={{ mt: 0.5, fontWeight: 600, color: "success.main" }}>
            {totalValidos}
          </Typography>
        </Paper>

        <Paper sx={{ p: 2, pl: 2.5, border: "1px solid", borderColor: "divider", borderRadius: "10px", borderLeft: "4px solid", borderLeftColor: "error.main", backgroundColor: "background.paper" }}>
          <Typography variant="caption" sx={{ display: "block", fontWeight: 500, textTransform: "uppercase", letterSpacing: "0.05em", color: "text.secondary", fontSize: "0.75rem" }}>
            Com erro
          </Typography>
          <Typography variant="h4" sx={{ mt: 0.5, fontWeight: 600, color: "error.main" }}>
            {totalErros}
          </Typography>
        </Paper>
      </Box>

      <Box sx={{ mt: 4, overflowX: "auto" }}>
        <AppDataGrid
          rows={linhas}
          columns={colunas}
          getRowId={(row) => (row as RegistroGridRow).id}
        />
      </Box>

      {erroGeracao && (
        <Alert severity="error" sx={{ mt: 2 }}>
          {erroGeracao}
        </Alert>
      )}

      <Stack direction="row" spacing={2} sx={{ mt: 4, justifyContent: "flex-end" }}>
        <Button variant="outlined" color="inherit" onClick={() => setDialogCancelarAberto(true)}>
          Cancelar
        </Button>
        <Tooltip
          title={
            possuiErros
              ? "Não é possível concluir com erros na planilha."
              : ""
          }
        >
          <Box
            component="span"
            sx={{
              cursor: possuiErros ? "not-allowed" : "default",
              display: "inline-flex",
            }}
          >
            <Button
              variant="contained"
              color="primary"
              disabled={possuiErros}
              onClick={aoClicarConfirmar}
            >
              Confirmar
            </Button>
          </Box>
        </Tooltip>
      </Stack>

      <ProblemasRegistroDialog
        open={registroSelecionado !== null}
        registro={registroSelecionado}
        onClose={() => setRegistroSelecionado(null)}
      />

      <CancelarImportacaoDialog
        open={dialogCancelarAberto}
        onClose={() => setDialogCancelarAberto(false)}
        onConfirmar={() => {
          setDialogCancelarAberto(false);
          onCancelarImportacao();
        }}
      />

      <ConfirmarImportacaoDialog
        open={dialogConfirmarAberto}
        onClose={() => {
          setErroGeracao(null);
          setDialogConfirmarAberto(false);
        }}
        onGerarJsonVisual={aoGerarJsonVisual}
        onGerarArquivoJson={aoGerarArquivoJson}
        mensagemErro={erroGeracao}
      />

      <JsonVisualDialog
        open={jsonVisualAberto}
        jsonFormatado={jsonFormatado}
        onClose={() => setJsonVisualAberto(false)}
      />
    </SectionCard>
  );
}
