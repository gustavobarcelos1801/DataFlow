"use client";

import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import Alert from "@mui/material/Alert";

type ConfirmarImportacaoDialogProps = {
  open: boolean;
  onClose: () => void;
  onGerarJsonVisual: () => void;
  onGerarArquivoJson: () => void;
  mensagemErro?: string | null;
};

export default function ConfirmarImportacaoDialog({
  open,
  onClose,
  onGerarJsonVisual,
  onGerarArquivoJson,
  mensagemErro,
}: ConfirmarImportacaoDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Confirmar importação</DialogTitle>

      <DialogContent>
        <Typography variant="body2" sx={{ color: "text.secondary" }}>
          Todos os registros foram validados com sucesso. Escolha como deseja
          gerar as instruções de importação.
        </Typography>

        {mensagemErro && (
          <Alert severity="error" sx={{ mt: 2 }}>
            {mensagemErro}
          </Alert>
        )}
      </DialogContent>

      <DialogActions sx={{ flexDirection: { xs: "column", sm: "row" }, gap: 1 }}>
        <Button onClick={onGerarJsonVisual} variant="contained" color="primary">
          Gerar JSON Visual
        </Button>
        <Button onClick={onGerarArquivoJson} variant="contained" color="primary">
          Gerar Arquivo JSON
        </Button>
        <Button onClick={onClose} color="inherit">
          Cancelar
        </Button>
      </DialogActions>
    </Dialog>
  );
}
