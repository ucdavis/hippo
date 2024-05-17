import React, { useEffect, useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import {
  HistoryModel,
  OrderBillingModel,
  OrderMetadataModel,
  OrderModel,
} from "../../types";
import { authenticatedFetch } from "../../util/api";
import { Column } from "react-table";
import { ReactTable } from "../../Shared/ReactTable";
import ChartStringValidation from "./ChartStringValidation";

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
        Cell: ({ value }) => <span>{new Date(value).toLocaleString()}</span>,
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

  const metadataColumns = useMemo<Column<OrderMetadataModel>[]>(
    () => [
      {
        Header: "Name",
        accessor: "name",
      },
      {
        Header: "Value",
        accessor: "value",
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
      <div className="row justify-content-center">
        <div className="col-md-8">
          <h1>Order Details</h1>
          <p>Order ID: {order.id}</p>

          <p>Order Name: {order.name}</p>

          <h2>Chart Strings</h2>
          <table className="table table-bordered table-striped">
            <thead>
              <tr>
                <th>Chart String</th>
                <th>Percent</th>
                <th>Chart String Validation</th>
                </tr>
              </thead>
              <tbody>
                {order.billings.map((billing) => (
                  <tr key={billing.id}>
                    <td>{billing.chartString}</td>
                    <td>{billing.percentage}</td>
                    <td><ChartStringValidation chartString={billing.chartString} /></td>
                  </tr>
                ))}
                </tbody>
            </table>

          <h2>Metadata</h2>
          <ReactTable columns={metadataColumns} data={order.metaData} />

          <h2>History</h2>
          <ReactTable
            columns={historyColumns}
            data={order.history}
            initialState={{
              sortBy: [{ id: "Date" }],
            }}
          />
        </div>
      </div>
    </div>
  );
};
