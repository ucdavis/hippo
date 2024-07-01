import React, { useEffect, useMemo, useState } from "react";
import { OrderListModel } from "../../types";
import { Link, useParams } from "react-router-dom";
import { authenticatedFetch } from "../../util/api";

import { ShowFor } from "../../Shared/ShowFor";
import { ReactTable } from "../../Shared/ReactTable";
import { createColumnHelper } from "@tanstack/react-table";

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

  const columnHelper = createColumnHelper<OrderListModel>();

  const columns = [
    columnHelper.accessor("status", {
      header: "Status",
      id: "status",
    }),
    columnHelper.accessor("name", {
      header: "Order Name",
      id: "name",
    }),
    columnHelper.accessor("units", {
      header: "Units",
      id: "units",
    }),
    columnHelper.accessor("quantity", {
      header: "Quantity",
      id: "quantity",
    }),
    columnHelper.accessor("total", {
      header: "Total",
      id: "total",
    }),
    columnHelper.accessor("balanceRemaining", {
      header: "Balance",
      id: "balanceRemaining",
    }),
    columnHelper.accessor("createdOn", {
      header: "Created On",
      id: "createdOn",
    }),
    columnHelper.display({
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
    }),
  ];

  //add columns with useMemo
  // const columns = useMemo<Column<OrderListModel>[]>(
  //   () => [
  //     {
  //       Header: "Status",
  //       accessor: "status",
  //     },
  //     {
  //       Header: "Order Name",
  //       accessor: "name",
  //     },
  //     {
  //       Header: "Units",
  //       accessor: "units",
  //     },
  //     {
  //       Header: "Quantity",
  //       accessor: "quantity",
  //     },
  //     {
  //       Header: "Total",
  //       accessor: "total",
  //     },
  //     {
  //       Header: "Balance",
  //       accessor: "balanceRemaining",
  //     },
  //     {
  //       Header: "Created On",
  //       accessor: "createdOn",
  //     },
  //     {
  //       Header: "Actions",
  //       sortable: false,
  //       Cell: ({ row }) => (
  //         <div>
  //           <Link
  //             className="btn btn-primary"
  //             to={`/${cluster}/order/details/${row.original.id}`}
  //           >
  //             Details
  //           </Link>{" "}
  //           <button className="btn btn-primary">Edit</button>{" "}
  //         </div>
  //       ),
  //     },
  //   ],
  //   [cluster],
  // );

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
