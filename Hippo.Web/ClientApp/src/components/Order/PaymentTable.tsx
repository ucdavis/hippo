import React, { useEffect, useState } from "react";
import { PaymentModel } from "../../types";
import { useNavigate, useParams } from "react-router-dom";
import { authenticatedFetch } from "../../util/api";
import { createColumnHelper } from "@tanstack/react-table";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDollarSign } from "@fortawesome/free-solid-svg-icons";
import { convertToPacificTime } from "../../util/DateHelper";
import { ReactTable } from "../../Shared/ReactTable";

interface PaymentTableProps {
  numberOfRows: number;
  showLinkToAll: boolean;
}

export const PaymentTable: React.FC<PaymentTableProps> = ({
  numberOfRows,
  showLinkToAll,
}) => {
  const [payments, setPayments] = useState<PaymentModel[]>([]);
  const { cluster, orderId } = useParams();
  const navigate = useNavigate();

  useEffect(() => {
    const fetchPayments = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/order/GetPayments/${orderId}?max=${numberOfRows}`,
      );

      if (response.ok) {
        const data = await response.json();

        setPayments(data);
      } else {
        alert("Error fetching payments");
      }
    };

    fetchPayments();
  }, [cluster, numberOfRows, orderId]);

  const paymentColumnHelper = createColumnHelper<PaymentModel>();

  const paymentColumns = [
    paymentColumnHelper.accessor("amount", {
      header: "Amount",
      id: "amount",
      cell: (value) => (
        <span>
          <FontAwesomeIcon icon={faDollarSign} />{" "}
          {value.row.original.amount.toFixed(2)}
        </span>
      ),
    }),
    paymentColumnHelper.accessor("status", { header: "Status", id: "status" }),
    paymentColumnHelper.accessor("createdOn", {
      header: "Created On",
      id: "createdOn",
      cell: (value) => (
        <span>{convertToPacificTime(value.row.original.createdOn)}</span>
      ),
    }),
    paymentColumnHelper.accessor("createdBy", {
      header: "Created By",
      id: "createdBy",
      cell: (value) => (
        <>
          {value.row.original.createdBy && (
            <>
              {value.row.original.createdBy.firstName}{" "}
              {value.row.original.createdBy.lastName} (
              {value.row.original.createdBy.email})
            </>
          )}
          {!value.row.original.createdBy && <>System</>}
        </>
      ),
    }),
  ];

  if (!payments) {
    return <div>Loading Payments...</div>;
  }

  if (payments.length === 0) {
    return null;
  }

  return (
    <>
      {!showLinkToAll ? <h2>Payments</h2> : <br />}
      <small>Last {payments.length} Payments</small>{" "}
      {showLinkToAll ? (
        <>
          {payments.length >= numberOfRows && (
            <button
              onClick={() => navigate(`/${cluster}/order/payments/${orderId}`)}
              className="float-right"
            >
              View All
            </button>
          )}
        </>
      ) : (
        <button
          onClick={() => navigate(`/${cluster}/order/details/${orderId}`)}
          className="float-right"
        >
          Back to Order Details
        </button>
      )}
      <ReactTable
        columns={paymentColumns}
        data={payments}
        initialState={{
          sorting: [
            {
              id: "createdOn",
              desc: true,
            },
          ],
        }}
      />
    </>
  );
};
