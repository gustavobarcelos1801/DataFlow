"use client";

import { DataGrid, type GridColDef, type GridRowsProp } from "@mui/x-data-grid";
import { ptBR } from "@mui/x-data-grid/locales";
import Box from "@mui/material/Box";

type AppDataGridProps = {
  rows: GridRowsProp;
  columns: GridColDef[];
  getRowId: (row: GridRowsProp[number]) => string | number;
};

export default function AppDataGrid({
  rows,
  columns,
  getRowId,
}: AppDataGridProps) {
  return (
    <Box sx={{ width: "100%" }}>
      <DataGrid
        rows={rows}
        columns={columns}
        getRowId={getRowId}
        localeText={ptBR.components.MuiDataGrid.defaultProps.localeText}
        initialState={{
          pagination: {
            paginationModel: { page: 0, pageSize: 5 },
          },
        }}
        pageSizeOptions={[5, 10, 15, 20, 25]}
        disableRowSelectionOnClick
        disableColumnResize
        autoHeight
        sx={{
          border: "1px solid",
          borderColor: "divider",
          borderRadius: 2,
          "& .MuiDataGrid-columnHeaders": {
            backgroundColor: "grey.50",
            fontWeight: 500,
          },
          "& .MuiDataGrid-columnHeaderTitle": {
            fontWeight: 500,
            fontSize: "0.75rem",
            textTransform: "uppercase",
            letterSpacing: "0.05em",
            color: "text.secondary",
          },
          "& .MuiDataGrid-cell": {
            fontSize: "0.875rem",
            whiteSpace: "nowrap",
            overflow: "hidden",
            textOverflow: "ellipsis",
          },
        }}
      />
    </Box>
  );
}