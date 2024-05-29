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
  FilterFns,
  TableState,
  RowData,
  ColumnFiltersState,
} from "@tanstack/react-table";
// import { ColumnFilterHeaders, DefaultColumnFilter } from "./Filtering";
import { PaginationItem, PaginationLink } from "reactstrap";
import { DebouncedInput } from "./DebouncedInput";

declare module "@tanstack/react-table" {
  // allows us to define custom properties for our columns
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  interface ColumnMeta<TData extends RowData, TValue> {
    filterVariant?: "text" | "range" | "select";
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
  filterFns?: FilterFns;
}

export const ReactTable = <T extends object>({
  columns,
  data,
  initialState,
  filterFns: filterTypes,
}: Props<T>) => {
  const defaultColumn = React.useMemo(() => ({}) as Partial<Column<T>>, []);

  const [columnFilters, setColumnFilters] = React.useState<ColumnFiltersState>(
    [],
  );

  const table = useReactTable({
    columns,
    data,
    defaultColumn,
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

  return (
    <>
      <table className="table table-bordered table-striped">
        <thead>
          {table.getHeaderGroups().map((headerGroup) => (
            <React.Fragment key={headerGroup.id}>
              <tr className="table-row">
                {headerGroup.headers.map((header) => {
                  const context = header.getContext();
                  const isSorted = context.column.getIsSorted();
                  const sortIndex = context.column.getSortIndex();
                  const isDesc = isSorted && sorting[sortIndex].desc;

                  return (
                    <th
                      colSpan={header.colSpan}
                      key={header.id}
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
                  {headerGroup.headers.map((header) => (
                    <th colSpan={header.colSpan} key={header.id}>
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
          {page.rows.map((row) => (
            <tr className="rt-tr-group" key={row.id}>
              {row.getAllCells().map((cell) => (
                <td key={cell.id}>
                  {flexRender(cell.column.columnDef.cell, cell.getContext())}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
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
                type="number"
                defaultValue={pageIndex + 1}
                onChange={(e) => {
                  const page = e.target.value ? Number(e.target.value) - 1 : 0;
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
      <DebouncedInput
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
      <DebouncedInput
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
      onChange={(e) => column.setFilterValue(e.target.value)}
      value={columnFilterValue?.toString()}
    >
      <option value="">All</option>
      {sortedUniqueValues.map((value) => (
        //dynamically generated select options from faceted values feature
        <option value={value} key={value}>
          {value}
        </option>
      ))}
    </select>
  ) : (
    <>
      {/* Autocomplete suggestions from faceted values feature */}
      <datalist id={column.id + "list"}>
        {sortedUniqueValues.map((value: any) => (
          <option value={value} key={value} />
        ))}
      </datalist>
      <DebouncedInput
        type="text"
        value={(columnFilterValue ?? "") as string}
        onChange={(value) => column.setFilterValue(value)}
        placeholder={`Search... (${column.getFacetedUniqueValues().size})`}
        list={column.id + "list"}
      />
    </>
  );
};
