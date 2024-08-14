import React from "react";
import { createColumnHelper } from "@tanstack/react-table";
import { Link } from "react-router-dom";
import { HipTable } from "../../../Shared/Table/HipTable";
import { OrderListModel } from "../../../types";
import { convertToPacificDate } from "../../../util/DateHelper";
import HipProgress from "../../../Shared/HipProgress";
import { Progress } from "reactstrap";
import { OrderStatus } from "../../../types/status";

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

    const statusValue = (status: OrderStatus) => {
      switch (status) {
        case OrderStatus.Created:
          return 1;
        case OrderStatus.Submitted:
          return 2;
        case OrderStatus.Processing:
          return 3;
        case OrderStatus.Cancelled:
          return 2.5;
        case OrderStatus.Active:
          return 4;
        case OrderStatus.Completed:
          return 5;
        case OrderStatus.Rejected:
          return 5;
        default:
          return 0;
      }
    };
    const status = columnHelper.accessor("status", {
      header: "Status",
      id: "status",
      meta: {
        filterVariant: "select",
      },
      sortingFn: (rowA, rowB) => {
        const statusA: OrderStatus = rowA.getValue("status");
        const statusB: OrderStatus = rowB.getValue("status");
        return statusValue(statusA) - statusValue(statusB);
      },
      cell: (value) => {
        const status = value.row.original.status;
        const barValue = statusValue(status);
        const color =
          status === OrderStatus.Cancelled || status === OrderStatus.Rejected
            ? "danger"
            : "primary";
        return (
          <div className="hip-progress table-status">
            <Progress max={5} value={barValue} color={color}></Progress>
            <small className="text-muted">{status}</small>
          </div>
        );
      },
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
      filterFn: (row, id, filterValue) => {
        return row.original.total.toString().startsWith(filterValue);
      },
      cell: (value) => {
        const total = value.row.original.total;
        const balanceRemaining = value.row.original.balanceRemaining;
        const color = balanceRemaining > 0 ? "primary" : "success";
        const barValue = total - balanceRemaining;
        return (
          <div className="hip-progress table-status">
            <Progress max={total} value={barValue} color={color}></Progress>
            <small className="text-muted">${total.toFixed(2)}</small>
          </div>
        );
      },
    });

    const balanceRemaining = columnHelper.accessor("balanceRemaining", {
      header: "Balance",
      id: "balanceRemaining",
      filterFn: (row, id, filterValue) => {
        return row.original.balanceRemaining.toString().startsWith(filterValue);
      },
      cell: (value) => {
        const total = value.row.original.total;
        const balanceRemaining = value.row.original.balanceRemaining;
        const color = balanceRemaining > 0 ? "primary" : "success";
        return (
          <div className="hip-progress table-status">
            <Progress
              max={total}
              value={balanceRemaining}
              color={color}
            ></Progress>
            <small className="text-muted">${balanceRemaining.toFixed(2)}</small>
          </div>
        );
      },
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
