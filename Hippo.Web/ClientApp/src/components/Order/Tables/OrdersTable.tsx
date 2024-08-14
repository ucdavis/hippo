import React from "react";
import { createColumnHelper } from "@tanstack/react-table";
import { Link } from "react-router-dom";
import { HipTable } from "../../../Shared/Table/HipTable";
import { OrderListModel } from "../../../types";
import { convertToPacificDate } from "../../../util/DateHelper";

interface OrdersTableProps {
  orders: OrderListModel[];
  cluster: string;
  isAdminOrders: boolean;
}

export const OrdersTable: React.FC<OrdersTableProps> = ({
  orders,
  cluster,
  isAdminOrders,
}) => {
  const columns = React.useMemo(() => {
    const columnHelper = createColumnHelper<OrderListModel>();
    const nameAndId = columnHelper.accessor("name", {
      header: "Order",
      id: "name",
      filterFn: (row, id, filterValue) => {
        return (
          row.original.id.toString().includes(filterValue) ||
          row.original.name.toLowerCase().includes(filterValue.toLowerCase())
        );
      },
      cell: (value) => (
        <div>
          <Link to={`/${cluster}/order/details/${value.row.original.id}`}>
            {value.row.original.name}
          </Link>
          <br />
          <small className="text-muted">Order #{value.row.original.id}</small>
        </div>
      ),
    });

    const status = columnHelper.accessor("status", {
      header: "Status",
      id: "status",
    });

    const sponsorName = columnHelper.accessor("sponsorName", {
      header: "Sponsor",
      id: "sponsorName",
    });

    const units = columnHelper.accessor("units", {
      header: "Units",
      id: "units",
    });

    const quantity = columnHelper.accessor("quantity", {
      header: "Quantity",
      id: "quantity",
      filterFn: (row, id, filterValue) => {
        return row.original.quantity.toString().includes(filterValue);
      },
    });

    const total = columnHelper.accessor("total", {
      header: "Total",
      id: "total",
    });

    const balanceRemaining = columnHelper.accessor("balanceRemaining", {
      header: "Balance",
      id: "balanceRemaining",
    });

    const createdOn = columnHelper.accessor("createdOn", {
      header: "Created On",
      id: "createdOn",
      cell: (value) => convertToPacificDate(value.row.original.createdOn),
      sortingFn: (rowA, rowB) => {
        const dateA = new Date(rowA.getValue("createdOn"));
        const dateB = new Date(rowB.getValue("createdOn"));
        return dateA.getTime() - dateB.getTime();
      },
    });

    const actions = columnHelper.display({
      header: "Actions",
      cell: (value) => (
        <div>
          <Link
            className="btn btn-primary"
            to={`/${cluster}/order/details/${value.row.original.id}`}
          >
            Details
          </Link>
        </div>
      ),
    });

    let cols = [];

    cols.push(nameAndId);
    cols.push(status);
    if (isAdminOrders) {
      cols.push(sponsorName);
    }
    cols.push(units);
    cols.push(quantity);
    cols.push(total);
    cols.push(balanceRemaining);
    cols.push(createdOn);
    cols.push(actions);
    return cols;
  }, [cluster, isAdminOrders]);

  return (
    <HipTable
      columns={columns}
      data={orders}
      initialState={{
        sorting: [
          {
            id: "createdOn",
            desc: true,
          },
        ],
      }}
    />
  );
};
