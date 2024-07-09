import React, { useEffect, useMemo, useState } from "react";
import { OrderListModel } from "../../types";
import { Link, useParams } from "react-router-dom";
import { authenticatedFetch } from "../../util/api";

import { ShowFor } from "../../Shared/ShowFor";
import { ReactTable } from "../../Shared/ReactTable";
import { createColumnHelper } from "@tanstack/react-table";

export const Orders = () => {
  const [orders, setOrders] = useState<OrderListModel[]>();
  const { cluster, orderType } = useParams();

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

  columns.push(status);
  if (orderType === "adminorders") {
    columns.push(sponsorName);
  }
  columns.push(name);
  columns.push(units);
  columns.push(quantity);
  columns.push(total);
  columns.push(balanceRemaining);
  columns.push(createdOn);
  columns.push(actions);

  if (orders === undefined) {
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">Loading...</div>
      </div>
    );
  } else {
    return (
      <div>
        <div className="row justify-content-center">
          <div className="col-md-8">
            <ReactTable
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
          </div>
        </div>
      </div>
    );
  }
};
