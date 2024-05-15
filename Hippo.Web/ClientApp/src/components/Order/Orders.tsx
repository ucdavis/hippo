import React, { useEffect, useMemo, useState } from "react";
import { OrderListModel } from "../../types";
import { useParams } from "react-router-dom";
import { authenticatedFetch } from "../../util/api";
import { Column } from "react-table";
import { ShowFor } from "../../Shared/ShowFor";
import { ReactTable } from "../../Shared/ReactTable";

export const Orders = () => {
  const [orders, setOrders] = useState<OrderListModel[]>();
  const { cluster } = useParams();

  useEffect(() => {
    const fetchOrders = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/order/myorders`,
      );

      if (response.ok) {
        const data = await response.json();
        setOrders(data);
      } else {
        alert("Error fetching orders");
      }
    };

    fetchOrders();
  }, [cluster]);
  //add columns with useMemo
  const columns = useMemo<Column<OrderListModel>[]>(
    () => [
      {
        Header: "Status",
        accessor: "status",
      },
      {
        Header: "Order Name",
        accessor: "name",
      },
      {
        Header: "Units",
        accessor: "units",
      },
      {
        Header: "Quantity",
        accessor: "quantity",
      },
      {
        Header: "Total",
        accessor: "total",
      },
      {
        Header: "Balance",
        accessor: "balanceRemaining",
      },
      {
        Header: "Created On",
        accessor: "createdOn",
      },
      {
        Header: "Actions",
        sortable: false,
        Cell: ({ row }) => (
          <div>
            <button className="btn btn-primary">Details</button>{" "}
            <button className="btn btn-primary">Edit</button>{" "}
          </div>
        ),
      },
    ],
    [],
  );

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
                sortBy: [{ id: "Name" }],
              }}
            />
          </div>
        </div>
      </div>
    );
  }
};
