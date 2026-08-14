"use client";

import Paper from "@mui/material/Paper";
import Typography from "@mui/material/Typography";
import Box from "@mui/material/Box";

type SectionCardProps = {
  titulo: string;
  children: React.ReactNode;
};

export default function SectionCard({ titulo, children }: SectionCardProps) {
  return (
    <Paper
      component="section"
      elevation={0}
      sx={{
        width: "100%",
        p: 4,
        border: "1px solid",
        borderColor: "divider",
      }}
    >
      <Typography component="h1" variant="h4">
        {titulo}
      </Typography>
      <Box sx={{ mt: 3 }}>{children}</Box>
    </Paper>
  );
}