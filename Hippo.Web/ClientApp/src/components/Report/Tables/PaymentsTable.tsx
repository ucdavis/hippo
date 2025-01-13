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

    const category = columnHelper.accessor("category", {
      header: "Category",
      id: "category",
    });
    const units = columnHelper.accessor("units", {
      header: "Units",
      id: "units",
    });
    const unitPrice = columnHelper.accessor("unitPrice", {
      header: "Unit Price",
      id: "unitPrice",
      cell: (value) => `$${value.row.original.unitPrice}`,
    });
    const installments = columnHelper.accessor("installments", {
      header: "Installments",
      id: "installments",
    });
    const installmentType = columnHelper.accessor("installmentType", {
      header: "Installment Type",
      id: "installmentType",
    });
    const isRecurring = columnHelper.accessor("isRecurring", {
      header: "Is Recurring",
      id: "isRecurring",
    });
    const externalReference = columnHelper.accessor("externalReference", {
      header: "External Reference",
      id: "externalReference",
    });
    const quantity = columnHelper.accessor("quantity", {
      header: "Quantity",
      id: "quantity",
    });
    const total = columnHelper.accessor("total", {
      header: "Total",
      id: "total",
      cell: (value) => `$${value.row.original.total}`,
    });
    const balanceRemaining = columnHelper.accessor("balanceRemaining", {
      header: "Balance Remaining",
      id: "balanceRemaining",
      cell: (value) => `$${value.row.original.balanceRemaining}`,
    });
    const notes = columnHelper.accessor("notes", {
      header: "Notes",
      id: "notes",
    });
    const adminNotes = columnHelper.accessor("adminNotes", {
      header: "Admin Notes",
      id: "adminNotes",
    });
    const orderStatus = columnHelper.accessor("orderStatus", {
      header: "Order Status",
      id: "orderStatus",
    });
    const installmentDate = columnHelper.accessor("installmentDate", {
      header: "Installment Date",
      id: "installmentDate",
      cell: (value) => convertToPacificDate(value.row.original.installmentDate),
      sortingFn: (rowA, rowB) => {
        const dateA = new Date(rowA.getValue("installmentDate"));
        const dateB = new Date(rowB.getValue("installmentDate"));
        return dateA.getTime() - dateB.getTime();
      },
    });
    const expirationDate = columnHelper.accessor("expirationDate", {
      header: "Expiration Date",
      id: "expirationDate",
      cell: (value) => convertToPacificDate(value.row.original.expirationDate),
      sortingFn: (rowA, rowB) => {
        const dateA = new Date(rowA.getValue("expirationDate"));
        const dateB = new Date(rowB.getValue("expirationDate"));
        return dateA.getTime() - dateB.getTime();
      },
    });
    const nextPaymentDate = columnHelper.accessor("nextPaymentDate", {
      header: "Next Payment Date",
      id: "nextPaymentDate",
      cell: (value) => convertToPacificDate(value.row.original.nextPaymentDate),
      sortingFn: (rowA, rowB) => {
        const dateA = new Date(rowA.getValue("nextPaymentDate"));
        const dateB = new Date(rowB.getValue("nextPaymentDate"));
        return dateA.getTime() - dateB.getTime();
      },
    });
    const orderCreatedOn = columnHelper.accessor("orderCreatedOn", {
      header: "Order Created On",
      id: "orderCreatedOn",
      cell: (value) => convertToPacificDate(value.row.original.orderCreatedOn),
      sortingFn: (rowA, rowB) => {
        const dateA = new Date(rowA.getValue("orderCreatedOn"));
        const dateB = new Date(rowB.getValue("orderCreatedOn"));
        return dateA.getTime() - dateB.getTime();
      },
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
    cols.push(category);
    cols.push(units);
    cols.push(unitPrice);
    cols.push(installments);
    cols.push(installmentType);
    cols.push(isRecurring);
    cols.push(externalReference);
    cols.push(quantity);
    cols.push(total);
    cols.push(balanceRemaining);
    cols.push(orderStatus);
    cols.push(installmentDate);
    cols.push(expirationDate);
    cols.push(nextPaymentDate);
    cols.push(orderCreatedOn);
    cols.push(notes);
    cols.push(adminNotes);
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
