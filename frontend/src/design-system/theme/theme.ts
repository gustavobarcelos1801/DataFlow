import { createTheme } from "@mui/material/styles";

const theme = createTheme({
  palette: {
    mode: "light",
    primary: {
      main: "#06b6d4",
      light: "#22d3ee",
      dark: "#0891b2",
      contrastText: "#ffffff",
    },
    success: {
      main: "#10b981",
      light: "#d1fae5",
      dark: "#047857",
      contrastText: "#ffffff",
    },
    error: {
      main: "#ef4444",
      light: "#fee2e2",
      dark: "#b91c1c",
      contrastText: "#ffffff",
    },
    warning: {
      main: "#f97316",
      light: "#ffedd5",
      dark: "#c2410c",
      contrastText: "#ffffff",
    },
    info: {
      main: "#06b6d4",
      light: "#cffafe",
      dark: "#0e7490",
      contrastText: "#ffffff",
    },
    background: {
      default: "#f5f6f7",
      paper: "#ffffff",
    },
    text: {
      primary: "#18181b",
      secondary: "#52525b",
      disabled: "#a1a1aa",
    },
    divider: "#e4e4e7",
  },
  shape: {
    borderRadius: 12,
  },
  spacing: 8,
  typography: {
    fontFamily: "Arial, Helvetica, sans-serif",
    h1: {
      fontSize: "22px",
      fontWeight: 500,
      color: "#18181b",
      lineHeight: 1.3,
    },
    h4: {
      fontSize: "22px",
      fontWeight: 500,
      color: "#18181b",
      lineHeight: 1.3,
    },
    body1: {
      fontSize: "0.875rem",
      color: "#52525b",
    },
    body2: {
      fontSize: "0.875rem",
      color: "#18181b",
    },
    caption: {
      fontSize: "0.75rem",
      color: "#52525b",
    },
  },
  components: {
    MuiCssBaseline: {
      styleOverrides: {
        body: {
          backgroundColor: "#f5f6f7",
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          borderRadius: 16,
          boxShadow:
            "0px 1px 2px 0px rgba(0, 0, 0, 0.05), 0px 1px 3px 0px rgba(0, 0, 0, 0.05)",
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: "none",
          borderRadius: 8,
          minHeight: 44,
          paddingLeft: 24,
          paddingRight: 24,
          fontWeight: 500,
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          borderRadius: 999,
          fontWeight: 500,
        },
      },
    },
    MuiAlert: {
      styleOverrides: {
        root: {
          borderRadius: 10,
        },
      },
    },
  },
});

export default theme;