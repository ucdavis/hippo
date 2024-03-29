import * as React from "react";
import {
  useTable,
  useFilters,
  useGlobalFilter,
  useSortBy,
  usePagination,
} from "react-table";
import type { Column, FilterTypes, TableState } from "react-table";
import { ColumnFilterHeaders, DefaultColumnFilter } from "./Filtering";
import { PaginationItem, PaginationLink } from "reactstrap";

export const setDefaultPageSize = (pageSize: number) => {
  localStorage.setItem("HippoDefaultPageSize", pageSize.toString());
};

export const getDefaultPageSize = () => {
  return Number(localStorage.getItem("HippoDefaultPageSize") ?? "0") || 10;
};

interface Props<T extends object> {
  columns: Column<T>[];
  data: T[];
  initialState?: Partial<TableState<T>>;
  filterTypes?: FilterTypes<T>;
}

export const ReactTable = <T extends object>({
  columns,
  data,
  initialState,
  filterTypes,
}: Props<T>) => {
  const defaultColumn = React.useMemo(
    () =>
      ({
        Filter: DefaultColumnFilter,
      } as Partial<Column<T>>),
    []
  );

  const {
    getTableProps,
    getTableBodyProps,
    headerGroups,
    prepareRow,
    // pagination
    page,
    canPreviousPage,
    canNextPage,
    pageOptions,
    pageCount,
    gotoPage,
    nextPage,
    previousPage,
    setPageSize,
    state: { pageIndex, pageSize },
  } = useTable(
    {
      columns,
      data,
      defaultColumn,
      initialState: {
        pageIndex: 0,
        pageSize: getDefaultPageSize(),
        ...initialState,
      },
      filterTypes,
      autoResetSortBy: false,
      autoResetFilters: false,
    },
    useFilters, // useFilters!
    useGlobalFilter, // useGlobalFilter!
    useSortBy,
    usePagination
  );

  return (
    <>
      <table
        className="table table-bordered table-striped"
        {...getTableProps()}
      >
        <thead>
          {headerGroups.map((headerGroup) => (
            <tr className="table-row" {...headerGroup.getHeaderGroupProps()}>
              {headerGroup.headers.map((column) => (
                <th
                  {...column.getHeaderProps(column.getSortByToggleProps())}
                  className={`sort-${
                    column.isSorted
                      ? column.isSortedDesc
                        ? "desc"
                        : "asc"
                      : "none"
                  }`}
                >
                  {column.render("Header")}
                  {/* Render the columns filter UI */}
                  <span>
                    {column.isSorted ? (
                      column.isSortedDesc ? (
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
              ))}
            </tr>
          ))}
          <ColumnFilterHeaders headerGroups={headerGroups} />
        </thead>
        <tbody {...getTableBodyProps()}>
          {page.map((row) => {
            prepareRow(row);
            return (
              <tr className="rt-tr-group" {...row.getRowProps()}>
                {row.cells.map((cell) => {
                  return (
                    <td {...cell.getCellProps()}>{cell.render("Cell")}</td>
                  );
                })}
              </tr>
            );
          })}
        </tbody>
      </table>
      <div className="pagination justify-content-center">
        <PaginationItem
          className="align-self-center"
          onClick={() => gotoPage(0)}
          disabled={!canPreviousPage}
        >
          <PaginationLink first />
        </PaginationItem>
        <PaginationItem
          className="align-self-center"
          onClick={() => previousPage()}
          disabled={!canPreviousPage}
        >
          <PaginationLink previous />
        </PaginationItem>
        <PaginationItem>
          <PaginationLink>
            <span>
              Page{" "}
              <strong>
                {pageIndex + 1} of {pageOptions.length}
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
                  gotoPage(page);
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
                setPageSize(Number(e.target.value));
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
          onClick={() => nextPage()}
          disabled={!canNextPage}
        >
          <PaginationLink next />
        </PaginationItem>
        <PaginationItem
          className="align-self-center"
          onClick={() => gotoPage(pageCount - 1)}
          disabled={!canNextPage}
        >
          <PaginationLink last />
        </PaginationItem>
      </div>
      <br />
    </>
  );
};
