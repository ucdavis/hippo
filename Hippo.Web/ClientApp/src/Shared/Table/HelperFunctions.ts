import { SortingFn } from "@tanstack/react-table";

export const sortByDate: SortingFn<any> = (rowA, rowB, columnId) => {
  const dateA = rowA.getValue(columnId) ? new Date(rowA.getValue(columnId)).getTime() : 0;
  const dateB = rowB.getValue(columnId) ? new Date(rowB.getValue(columnId)).getTime() : 0;
  return dateA - dateB; // Sort by raw timestamp
};
