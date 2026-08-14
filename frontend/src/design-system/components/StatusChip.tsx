"use client";

import Chip from "@mui/material/Chip";
import CheckCircle from "@mui/icons-material/CheckCircle";
import Error from "@mui/icons-material/Error";
import type { StatusRegistro } from "@/features/importacao/types/registro.types";

type StatusChipProps = {
  status: StatusRegistro;
  onClick?: () => void;
};

const CONFIG_STATUS: Record<
  StatusRegistro,
  { icone: typeof CheckCircle; label: string; cor: "success" | "error" }
> = {
  valido: {
    icone: CheckCircle,
    label: "Válido",
    cor: "success",
  },
  erro: {
    icone: Error,
    label: "Erro",
    cor: "error",
  },
};

export default function StatusChip({ status, onClick }: StatusChipProps) {
  const { icone: Icone, label, cor } = CONFIG_STATUS[status];

  return (
    <Chip
      icon={<Icone sx={{ fontSize: 14 }} />}
      label={label}
      color={cor}
      variant="outlined"
      size="small"
      onClick={onClick}
      sx={{
        fontWeight: 500,
        cursor: onClick ? "pointer" : "default",
        borderColor: (tema) =>
          cor === "success"
            ? tema.palette.success.light
            : tema.palette.error.light,
        backgroundColor: (tema) =>
          cor === "success" ? tema.palette.success.light : tema.palette.error.light,
        color: (tema) =>
          cor === "success" ? tema.palette.success.dark : tema.palette.error.dark,
        "& .MuiChip-icon": {
          color: "inherit",
        },
      }}
    />
  );
}
