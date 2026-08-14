"use client";

import Dialog from "@mui/material/Dialog";
import DialogTitle from "@mui/material/DialogTitle";
import DialogContent from "@mui/material/DialogContent";
import DialogActions from "@mui/material/DialogActions";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemIcon from "@mui/material/ListItemIcon";
import ListItemText from "@mui/material/ListItemText";
import Divider from "@mui/material/Divider";
import ErrorOutlineRounded from "@mui/icons-material/ErrorOutlineRounded";
import type { RegistroPreview } from "@/features/importacao/types/registro.types";

type ProblemasRegistroDialogProps = {
  open: boolean;
  registro: RegistroPreview | null;
  onClose: () => void;
};

export default function ProblemasRegistroDialog({
  open,
  registro,
  onClose,
}: ProblemasRegistroDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Problemas encontrados</DialogTitle>

      <DialogContent dividers>
        {registro && (
          <Stack spacing={2}>
            <Stack spacing={0.5}>
              {registro.matricula && (
                <Typography variant="body2" sx={{ color: "text.secondary" }}>
                  Matrícula: {registro.matricula}
                </Typography>
              )}
              {registro.nome && (
                <Typography variant="body2" sx={{ color: "text.secondary" }}>
                  Nome: {registro.nome}
                </Typography>
              )}
            </Stack>

            <List disablePadding>
              {registro.erros.map((erro, indice) => (
                <div key={`${erro.campo}-${indice}`}>
                  {indice > 0 && <Divider component="li" />}
                  <ListItem disableGutters alignItems="flex-start">
                    <ListItemIcon sx={{ minWidth: 36, mt: 0.5 }}>
                      <ErrorOutlineRounded sx={{ fontSize: 20, color: "error.main" }} />
                    </ListItemIcon>
                    <ListItemText
                      primary={
                        <Typography
                          variant="body2"
                          sx={{ fontWeight: 600, color: "text.primary" }}
                        >
                          {erro.campo}
                        </Typography>
                      }
                      secondary={
                        <Typography
                          variant="body2"
                          sx={{ color: "text.secondary" }}
                        >
                          {erro.mensagem}
                        </Typography>
                      }
                    />
                  </ListItem>
                </div>
              ))}
            </List>
          </Stack>
        )}
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose} color="inherit">
          Fechar
        </Button>
      </DialogActions>
    </Dialog>
  );
}