import React from "react";
import { createColumnHelper } from "@tanstack/react-table";

import { HipTable } from "../../../Shared/Table/HipTable";
import { PaymentReportModel } from "../../../types";
import { convertToPacificDate } from "../../../util/DateHelper";

interface PaymentsTableProps {
  payments: PaymentReportModel[];
  cluster: string;
}

export const PaymentsTable: React.FC<PaymentsTableProps> = ({
  payments,
  cluster,
}) => {
  const columns = React.useMemo(() => {
    const columnHelper = createColumnHelper<PaymentReportModel>();
    const id = columnHelper.accessor("id", {
      header: "Payment Id",
      id: "id",
    });
    const orderId = columnHelper.accessor("orderId", {
      header: "Order Id",
      id: "orderId",
    });
    const trackingNumber = columnHelper.accessor("trackingNumber", {
      header: "Tracking Number",
      id: "trackingNumber",
    });
    const createdOn = columnHelper.accessor("createdOn", {
      header: "Payment Created On",
      id: "createdOn",
      cell: (value) => convertToPacificDate(value.row.original.createdOn),
      sortingFn: (rowA, rowB) => {
        const dateA = new Date(rowA.getValue("createdOn"));
        const dateB = new Date(rowB.getValue("createdOn"));
        return dateA.getTime() - dateB.getTime();
      },
    });
    const completedOn = columnHelper.accessor("createdOn", {
      header: "Payment Completed On",
      id: "completedOn",
      cell: (value) => convertToPacificDate(value.row.original.completedOn),
      sortingFn: (rowA, rowB) => {
        const dateA = new Date(rowA.getValue("completedOn"));
        const dateB = new Date(rowB.getValue("completedOn"));
        return dateA.getTime() - dateB.getTime();
      },
    });
    const createdBy = columnHelper.accessor("createdBy", {
      header: "Created By",
      id: "createdBy",
    });
    const orderName = columnHelper.accessor("orderName", {
      header: "Order Name",
      id: "orderName",
    });
    const productName = columnHelper.accessor("productName", {
      header: "Product Name",
      id: "productName",
    });
    const description = columnHelper.accessor("description", {
      header: "Description",
      id: "description",
    });

    const amount = columnHelper.accessor("amount", {
      header: "Amount",
      id: "amount",
      cell: (value) => `$${value.row.original.amount}`,
    });
    const billingInfo = columnHelper.accessor("billingInfo", {
      header: "Billing Info",
      id: "billingInfo",
    });

    const sponsor = columnHelper.accessor("sponsor", {
      header: "Sponsor",
      id: "sponsor",
    });

    const metaDataString = columnHelper.accessor("metaDataString", {
      header: "Meta Data",
      id: "metaDataString",
    });

    let cols = [];

    cols.push(id);
    cols.push(orderId);
    cols.push(trackingNumber);
    cols.push(createdBy);
    cols.push(createdOn);
    cols.push(completedOn);
    cols.push(sponsor);
    cols.push(amount);
    cols.push(orderName);
    cols.push(productName);
    cols.push(description);
    cols.push(billingInfo);
    cols.push(metaDataString);
    return cols;
  }, []);

  return (
    <HipTable
      columns={columns}
      data={payments}
      initialState={{
        sorting: [
          {
            id: "orderId",
            desc: true,
          },
          {
            id: "completedOn",
            desc: true,
          },
        ],
      }}
    />
  );
};
