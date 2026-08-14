"use client";

import { useState } from "react";
import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import Box from "@mui/material/Box";
import Snackbar from "@mui/material/Snackbar";
import Alert from "@mui/material/Alert";

type JsonVisualDialogProps = {
  open: boolean;
  jsonFormatado: string;
  onClose: () => void;
};

export default function JsonVisualDialog({
  open,
  jsonFormatado,
  onClose,
}: JsonVisualDialogProps) {
  const [copiado, setCopiado] = useState(false);
  const [erroCopiar, setErroCopiar] = useState<string | null>(null);

  const aoCopiar = async () => {
    try {
      await navigator.clipboard.writeText(jsonFormatado);
      setCopiado(true);
      setErroCopiar(null);
    } catch {
      setErroCopiar("Não foi possível copiar o JSON.");
    }
  };

  return (
    <>
      <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
        <DialogTitle>JSON da importação</DialogTitle>

        <DialogContent dividers>
          <Box
            component="pre"
            sx={{
              m: 0,
              fontFamily: "monospace",
              fontSize: "0.75rem",
              lineHeight: 1.6,
              whiteSpace: "pre",
              overflow: "auto",
              maxHeight: 480,
              p: 2,
              borderRadius: 2,
              backgroundColor: "grey.50",
              border: "1px solid",
              borderColor: "divider",
              color: "text.primary",
              userSelect: "text",
            }}
          >
            {jsonFormatado}
          </Box>
        </DialogContent>

        <DialogActions>
          <Button onClick={aoCopiar} variant="contained" color="primary">
            Copiar JSON
          </Button>
          <Button onClick={onClose} color="inherit">
            Fechar
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar
        open={copiado}
        autoHideDuration={2500}
        onClose={() => setCopiado(false)}
        anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
      >
        <Alert severity="success" variant="filled" onClose={() => setCopiado(false)}>
          JSON copiado
        </Alert>
      </Snackbar>

      {erroCopiar && (
        <Snackbar
          open
          autoHideDuration={4000}
          onClose={() => setErroCopiar(null)}
          anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
        >
          <Alert severity="error" variant="filled" onClose={() => setErroCopiar(null)}>
            {erroCopiar}
          </Alert>
        </Snackbar>
      )}
    </>
  );
}