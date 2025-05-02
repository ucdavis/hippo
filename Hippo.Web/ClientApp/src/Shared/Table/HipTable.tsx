import * as React from "react";
import {
  useReactTable,
  getCoreRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  getFacetedUniqueValues,
  getFacetedMinMaxValues,
  getFacetedRowModel,
  flexRender,
} from "@tanstack/react-table";
import type {
  Column,
  ColumnDef,
  TableState,
  RowData,
  ColumnFiltersState,
  Cell,
  Header,
} from "@tanstack/react-table";
import { Button, PaginationItem, PaginationLink } from "reactstrap";
import innerText from "react-innertext";
import { arrayToCsv, startDownload } from "../../util/ExportHelpers";
import { isStringArray } from "../../util/TypeChecks";
import HipDumbTable from "./HipDumbTable";

declare module "@tanstack/react-table" {
  // allows us to define custom properties for our columns
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  interface ColumnMeta<TData extends RowData, TValue> {
    filterVariant?: "text" | "range" | "select";
    exportFn?: (data: TData) => string;
  }
}

export const setDefaultPageSize = (pageSize: number) => {
  localStorage.setItem("HippoDefaultPageSize", pageSize.toString());
};

export const getDefaultPageSize = () => {
  return Number(localStorage.getItem("HippoDefaultPageSize") ?? "0") || 10;
};

interface Props<T extends object> {
  columns: ColumnDef<T, any>[];
  data: T[];
  initialState?: Partial<TableState>;
  disableExport?: boolean;
  disablePagination?: boolean;
  disableFilter?: boolean;
}

export const HipTable = <T extends object>({
  columns,
  data,
  initialState,
  disableExport = false,
  disablePagination = false,
  disableFilter = false,
}: Props<T>) => {
  const defaultColumn = React.useMemo(() => ({}) as Partial<Column<T>>, []);

  const [columnFilters, setColumnFilters] = React.useState<ColumnFiltersState>(
    [],
  );

  const table = useReactTable({
    columns,
    data,
    defaultColumn,
    enableFilters: !disableFilter,
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getFacetedMinMaxValues: getFacetedMinMaxValues(),
    getFacetedRowModel: getFacetedRowModel(),
    getFacetedUniqueValues: getFacetedUniqueValues(),
    initialState: {
      pagination: {
        pageIndex: 0,
        pageSize: getDefaultPageSize(),
      },
      ...initialState,
    },
    state: {
      columnFilters: columnFilters,
    },
    onColumnFiltersChange: setColumnFilters,
  });

  const {
    pagination: { pageIndex, pageSize },
    sorting,
  } = table.getState();
  const pageCount = table.getPageCount();

  const page = table.getPaginationRowModel();

  const getHeaderExport = (header: Header<T, unknown>) => {
    if (typeof header.column.columnDef.header === "function") {
      const content = header.column.columnDef.header(header.getContext());
      if (typeof content === "string") {
        return content;
      }
      return innerText(content);
    } else {
      return header.column.columnDef.header ?? header.id;
    }
  };

  const getCellExport = (cell: Cell<T, unknown>) => {
    const { exportFn } = cell.column.columnDef.meta ?? {};
    if (exportFn) {
      return exportFn(cell.row.original);
    }
    const value = cell.getValue();
    if (value === null || value === undefined) return "";
    if (value instanceof Date) return value.toLocaleDateString();
    if (typeof value === "number") return value.toLocaleString();
    if (typeof value === "boolean") return value ? "Yes" : "No";
    if (isStringArray(value)) return value.join(", ");
    if (typeof value === "object") return JSON.stringify(value);
    return value.toString();
  };

  const handleExportCsv = (filter: boolean) => {
    const headers = table
      .getHeaderGroups()
      .map((x) => x.headers)
      .flat();
    const headerNames = headers
      .filter((header) => !!header.column.accessorFn) // exclude group/display columns
      .map(getHeaderExport);
    const rowModel = filter
      ? table.getFilteredRowModel()
      : table.getPreFilteredRowModel();
    const rowData = rowModel.rows.map((row) =>
      row
        .getAllCells()
        .filter((cell) => !!cell.column.accessorFn) // exclude group/display columns
        .map(getCellExport),
    );

    const csvString = arrayToCsv([headerNames, ...rowData]);

    const blob = new Blob([csvString], { type: "text/csv;charset=utf-8;" });
    startDownload(blob, "exported-data.csv");
  };

  const columnFilterApplied = table
    .getAllColumns()
    .some((c) => c.getIsFiltered());

  return (
    <>
      {!disableExport && (
        <div className="data-table-prolog float-end">
          {columnFilterApplied && (
            <>
              <Button color="link" onClick={() => handleExportCsv(true)}>
                Export Filtered CSV
              </Button>{" "}
              {" | "}
            </>
          )}
          <Button color="link" onClick={() => handleExportCsv(false)}>
            Export CSV
          </Button>
        </div>
      )}
      <HipDumbTable>
        <thead>
          {table.getHeaderGroups().map((headerGroup, index) => (
            <React.Fragment key={headerGroup.id}>
              <tr className="table-row" key={headerGroup.id + "_" + index}>
                {headerGroup.headers.map((header, index) => {
                  const context = header.getContext();
                  const isSorted = context.column.getIsSorted();
                  const sortIndex = context.column.getSortIndex();
                  const isDesc = isSorted && sorting[sortIndex].desc;

                  return (
                    <th
                      colSpan={header.colSpan}
                      key={header.id + "_" + index}
                      className={`sort-${
                        isSorted ? (isDesc ? "desc" : "asc") : "none"
                      }`}
                      style={
                        header.column.getCanSort() ? { cursor: "pointer" } : {}
                      }
                      onClick={header.column.getToggleSortingHandler()}
                    >
                      {flexRender(header.column.columnDef.header, context)}
                      {/* Render the columns filter UI */}
                      <span>
                        {isSorted ? (
                          isDesc ? (
                            <>
                              {" "}
                              <i className="fas fa-long-arrow-alt-down" />
                            </>
                          ) : (
                            <>
                              {" "}
                              <i className="fas fa-long-arrow-alt-up" />
                            </>
                          )
                        ) : (
                          ""
                        )}
                      </span>
                    </th>
                  );
                })}
              </tr>
              {headerGroup.headers.some((header) =>
                header.column.getCanFilter(),
              ) && (
                <tr>
                  {headerGroup.headers.map((header, index) => (
                    <th colSpan={header.colSpan} key={header.id + '_' + index}>
                      {header.column.getCanFilter() && (
                        <div>
                          <Filter column={header.column} />
                        </div>
                      )}
                    </th>
                  ))}
                </tr>
              )}
            </React.Fragment>
          ))}
        </thead>
        <tbody>
          {page.rows.map((row, index) => (
            <tr className="rt-tr-group" key={row.id + "_" + index}>
              {row.getAllCells().map((cell, index) => (
                <td key={cell.id + "_" + index} className="rt-td">
                  {flexRender(cell.column.columnDef.cell, cell.getContext())}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </HipDumbTable>
      {!disablePagination && (
        <div className="pagination justify-content-center">
          <PaginationItem
            className="align-self-center"
            onClick={table.firstPage}
            disabled={table.getCanPreviousPage()}
          >
            <PaginationLink first />
          </PaginationItem>
          <PaginationItem
            className="align-self-center"
            onClick={table.previousPage}
            disabled={table.getCanPreviousPage()}
          >
            <PaginationLink previous />
          </PaginationItem>
          <PaginationItem>
            <PaginationLink>
              <span>
                Page{" "}
                <strong>
                  {pageIndex + 1} of {pageCount}
                </strong>{" "}
              </span>
              <span>
                | Go to page:{" "}
                <input
                  className="form-control d-inline"
                  name="pageIndex"
                  type="number"
                  defaultValue={pageIndex + 1}
                  onChange={(e) => {
                    const page = e.target.value
                      ? Number(e.target.value) - 1
                      : 0;
                    table.setPageIndex(page);
                  }}
                  style={{ width: "100px" }}
                />
              </span>{" "}
            </PaginationLink>
          </PaginationItem>
          <PaginationItem>
            <PaginationLink>
              <select
                className="form-control"
                name="pageSize"
                value={pageSize}
                onChange={(e) => {
                  setDefaultPageSize(Number(e.target.value));
                  table.setPageSize(Number(e.target.value));
                }}
              >
                {[10, 25, 50, 100].map((pageSize) => (
                  <option key={pageSize} value={pageSize}>
                    Show {pageSize}
                  </option>
                ))}
              </select>{" "}
            </PaginationLink>
          </PaginationItem>
          <PaginationItem
            className="align-self-center"
            onClick={table.nextPage}
            disabled={table.getCanNextPage()}
          >
            <PaginationLink next />
          </PaginationItem>
          <PaginationItem
            className="align-self-center"
            onClick={table.lastPage}
            disabled={table.getCanNextPage()}
          >
            <PaginationLink last />
          </PaginationItem>
        </div>
      )}
      <br />
    </>
  );
};

const Filter = ({ column }: { column: Column<any, unknown> }) => {
  const { filterVariant } = column.columnDef.meta ?? {};

  const columnFilterValue = column.getFilterValue();

  const sortedUniqueValues = React.useMemo(
    () =>
      filterVariant === "range"
        ? []
        : Array.from(column.getFacetedUniqueValues().keys())
            .sort()
            .slice(0, 5000),
    [column, filterVariant],
  );

  return filterVariant === "range" ? (
    <div>
      <input
        className="form-control"
        id={column.id}
        type="number"
        min={Number(column.getFacetedMinMaxValues()?.[0] ?? "")}
        max={Number(column.getFacetedMinMaxValues()?.[1] ?? "")}
        value={(columnFilterValue as [number, number])?.[0] ?? ""}
        onChange={(value) =>
          column.setFilterValue((old: [number, number]) => [value, old?.[1]])
        }
        placeholder={
          "Min" + column.getFacetedMinMaxValues()?.[0] !== undefined
            ? "(" + column.getFacetedMinMaxValues()?.[0] + ")"
            : ""
        }
      />
      <input
        className="form-control"
        id={column.id}
        type="number"
        min={Number(column.getFacetedMinMaxValues()?.[0] ?? "")}
        max={Number(column.getFacetedMinMaxValues()?.[1] ?? "")}
        value={(columnFilterValue as [number, number])?.[1] ?? ""}
        onChange={(value) =>
          column.setFilterValue((old: [number, number]) => [old?.[0], value])
        }
        placeholder={
          "Man" + column.getFacetedMinMaxValues()?.[1] !== undefined
            ? "(" + column.getFacetedMinMaxValues()?.[1] + ")"
            : ""
        }
      />
    </div>
  ) : filterVariant === "select" ? (
    <select
      className="form-select"
      id={column.id}
      onChange={(e) => column.setFilterValue(e.target.value)}
      value={columnFilterValue?.toString()}
    >
      <option value="">All</option>
      {sortedUniqueValues.map((value, index) => (
        //dynamically generated select options from faceted values feature
        <option value={value} key={value + "_" + index}>
          {value}
        </option>
      ))}
    </select>
  ) : (
    <>
      <input
        className="form-control"
        id={column.id}
        type="text"
        value={(columnFilterValue ?? "") as string}
        onChange={(e) => column.setFilterValue(e.target.value)}
        placeholder={"Search..."}
        autoComplete="none"
      />
    </>
  );
};
