"use client";

import Box from "@mui/material/Box";
import Typography from "@mui/material/Typography";
import CircularProgress from "@mui/material/CircularProgress";
import CheckCircle from "@mui/icons-material/CheckCircle";
import Cancel from "@mui/icons-material/Cancel";
import RadioButtonUnchecked from "@mui/icons-material/RadioButtonUnchecked";
import type { StatusValidacao } from "@/features/importacao/types/validacao.types";

type ItemValidacaoProps = {
  descricao: string;
  status: StatusValidacao;
  mensagemErro?: string;
};

function IconeStatus({ status }: { status: StatusValidacao }) {
  switch (status) {
    case "pendente":
      return <RadioButtonUnchecked sx={{ fontSize: 20, color: "grey.400" }} />;
    case "validando":
      return <CircularProgress size={20} sx={{ color: "info.main" }} />;
    case "sucesso":
      return <CheckCircle sx={{ fontSize: 20, color: "success.main" }} />;
    case "erro":
      return <Cancel sx={{ fontSize: 20, color: "error.main" }} />;
  }
}

export default function ItemValidacao({
  descricao,
  status,
  mensagemErro,
}: ItemValidacaoProps) {
  return (
    <Box
      sx={{
        display: "flex",
        minHeight: 56,
        alignItems: "flex-start",
        gap: 1.5,
        borderRadius: 2,
        backgroundColor: "grey.50",
        px: 2,
        py: 1.5,
      }}
    >
      <Box sx={{ mt: 0.5, flexShrink: 0, display: "flex" }}>
        <IconeStatus status={status} />
      </Box>
      <Box sx={{ minWidth: 0 }}>
        <Typography variant="body2" component="span" sx={{ fontWeight: 500, color: "text.primary", display: "block" }}>
          {descricao}
        </Typography>
        {status === "erro" && mensagemErro && (
          <Typography
            variant="caption"
            sx={{ mt: 0.5, display: "block", lineHeight: 1.5, color: "error.main" }}
          >
            {mensagemErro}
          </Typography>
        )}
      </Box>
    </Box>
  );
}