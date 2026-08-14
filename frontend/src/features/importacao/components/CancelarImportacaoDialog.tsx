"use client";

import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";

type CancelarImportacaoDialogProps = {
  open: boolean;
  onClose: () => void;
  onConfirmar: () => void;
};

export default function CancelarImportacaoDialog({
  open,
  onClose,
  onConfirmar,
}: CancelarImportacaoDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Cancelar importação</DialogTitle>

      <DialogContent>
        <Typography variant="body2" sx={{ color: "text.secondary" }}>
          Deseja realmente cancelar esta importação? Os dados exibidos nesta
          operação serão descartados.
        </Typography>
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose} color="inherit">
          Voltar
        </Button>
        <Button onClick={onConfirmar} color="error" variant="outlined">
          Cancelar importação
        </Button>
      </DialogActions>
    </Dialog>
  );
}