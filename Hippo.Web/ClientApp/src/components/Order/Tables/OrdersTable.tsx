import React from "react";
import { createColumnHelper } from "@tanstack/react-table";
import { Link } from "react-router-dom";
import { HipTable } from "../../../Shared/Table/HipTable";
import { OrderListModel } from "../../../types";
import { convertToPacificDate } from "../../../util/DateHelper";
import { Progress } from "reactstrap";
import { OrderStatus, statusValue } from "../Statuses/status";
import { sortByDate } from "../../../Shared/Table/HelperFunctions";

interface OrdersTableProps {
  orders: OrderListModel[];
  cluster: string;
  isAdminOrders: boolean;
  showTableMessages?: boolean;
}

export const OrdersTable: React.FC<OrdersTableProps> = ({
  orders,
  cluster,
  isAdminOrders,
  showTableMessages,
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
        let color = "primary";
        if (
          status === OrderStatus.Cancelled ||
          status === OrderStatus.Rejected
        ) {
          color = "danger";
        } else if (status === OrderStatus.Archived) {
          color = "secondary";
        } else if (status === OrderStatus.Active) {
          color = "success";
        }
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
      sortingFn: (rowA, rowB, columnId) => sortByDate(rowA, rowB, columnId),
    });

    const expirationDate = columnHelper.accessor("expirationDate", {
      header: "Expires On",
      id: "expirationDate",
      cell: (value) => convertToPacificDate(value.row.original.expirationDate),
      sortingFn: (rowA, rowB, columnId) => sortByDate(rowA, rowB, columnId),
    });

    const isRecurring = columnHelper.accessor("isRecurring", {
      header: "Recurring",
      id: "isRecurring",
      cell: (value) => (value.row.original.isRecurring ? "Yes" : "No"),
      filterFn: (row, id, filterValue) => {
        return (row.original.isRecurring ? "YES" : "NO").startsWith(
          filterValue?.toUpperCase(),
        );
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

    const messages = columnHelper.accessor("messages", {
      header: "Messages",
      id: "messages",
    });

    const nextPaymentDate = columnHelper.accessor("nextPaymentDate", {
      header: "Next Payment",
      id: "nextPaymentDate",
      cell: (value) => convertToPacificDate(value.row.original.nextPaymentDate),
      sortingFn: (rowA, rowB, columnId) => sortByDate(rowA, rowB, columnId),
    });

    const cols = [];

    cols.push(nameAndId);
    cols.push(status);
    if (isAdminOrders) {
      cols.push(sponsorName);
    }
    cols.push(isRecurring);
    cols.push(expirationDate);
    cols.push(total);
    cols.push(balanceRemaining);
    cols.push(createdOn);
    if (showTableMessages === true) {
      cols.push(nextPaymentDate);
      cols.push(messages);
    }
    cols.push(actions);
    return cols;
  }, [cluster, isAdminOrders, showTableMessages]);

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
