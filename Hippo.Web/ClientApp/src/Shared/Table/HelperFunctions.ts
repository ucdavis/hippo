import { SortingFn } from "@tanstack/react-table";

export const sortByDate: SortingFn<any> = (rowA, rowB, columnId) => {
  const dateA = new Date(rowA.getValue(columnId)).getTime();
  const dateB = new Date(rowB.getValue(columnId)).getTime();
  return dateA - dateB; // Sort by raw timestamp
};
