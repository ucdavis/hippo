import React, { useEffect, useMemo, useState } from "react";
import { OrderListModel } from "../../types";
import { Link, useParams } from "react-router-dom";
import { authenticatedFetch } from "../../util/api";

import { HipTable } from "../../Shared/Table/HipTable";
import { createColumnHelper } from "@tanstack/react-table";
import { convertToPacificDate } from "../../util/DateHelper";
import HipTitle from "../../Shared/Layout/HipTitle";
import HipBody from "../../Shared/Layout/HipBody";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipLoading from "../../Shared/LoadingAndErrors/HipLoading";

export const Orders = () => {
  const [orders, setOrders] = useState<OrderListModel[]>();
  const { cluster, orderType } = useParams();
  const isAdminOrders = orderType === "adminorders";

  useEffect(() => {
    const fetchOrders = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/order/${orderType}`,
      );

      if (response.ok) {
        const data = await response.json();
        setOrders(data);
      } else {
        alert("Error fetching orders");
      }
    };

    fetchOrders();
  }, [cluster, orderType]);

  const columnHelper = createColumnHelper<OrderListModel>();

  const id = columnHelper.accessor("id", {
    header: "ID",
    id: "id",
    filterFn: (row, id, filterValue) => {
      return row.original.id.toString().includes(filterValue);
    },
  });

  const status = columnHelper.accessor("status", {
    header: "Status",
    id: "status",
  });

  const sponsorName = columnHelper.accessor("sponsorName", {
    header: "Sponsor",
    id: "sponsorName",
  });

  const name = columnHelper.accessor("name", {
    header: "Order Name",
    id: "name",
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

  let columns = [];

  columns.push(id);
  columns.push(status);
  if (isAdminOrders) {
    columns.push(sponsorName);
  }
  columns.push(name);
  columns.push(units);
  columns.push(quantity);
  columns.push(total);
  columns.push(balanceRemaining);
  columns.push(createdOn);
  columns.push(actions);

  // RH TODO: handle loading/error states
  if (orders === undefined) {
    return (
      <HipMainWrapper>
        <HipTitle title="Orders" />
        <HipBody>
          <HipLoading />
        </HipBody>
      </HipMainWrapper>
    );
  } else {
    return (
      <HipMainWrapper>
        <HipTitle title={isAdminOrders ? "Admin Orders" : "My Orders"} />
        <HipBody>
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
        </HipBody>
      </HipMainWrapper>
    );
  }
};
