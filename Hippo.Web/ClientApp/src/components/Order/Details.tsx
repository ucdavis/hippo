import React, { useEffect, useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import { HistoryModel, OrderModel } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { Column } from "react-table";
import { ReactTable } from "../../Shared/ReactTable";

export const Details = () => {
  const { cluster, orderId } = useParams();
  const [order, setOrder] = useState<OrderModel | null>(null);

  useEffect(() => {
    const fetchOrder = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/order/get/${orderId}`,
      );

      if (response.ok) {
        const data = await response.json();
        setOrder(data);
      } else {
        alert("Error fetching order");
      }
    };

    fetchOrder();
  }, [cluster, orderId]);

  const historyColumns = useMemo<Column<HistoryModel>[]>(
    () => [
      {
        Header: "Date",
        accessor: "actedDate",
      },
      {
        Header: "Actor",
        accessor: "actedBy",
        Cell: ({ value }) => (
          <span>
            {value.firstName} {value.lastName} ({value.email})
          </span>
        ),
      },
      {
        Header: "Status",
        accessor: "status",
      },
      {
        Header: "Details",
        accessor: "details",
      },
    ],
    [],
  );

  if (!order) {
    return <div>Loading...</div>;
  }

  console.log(order);

  return (
    <div>
      <h1>Order Details</h1>
      <p>Order ID: {order.id}</p>

      <p>Order Name: {order.name}</p>

      <h2>History</h2>
      <ReactTable
        columns={historyColumns}
        data={order.history}
        initialState={{
          sortBy: [{ id: "Date" }],
        }}
      />
    </div>
  );
};
