import React, { useEffect, useState } from "react";
import { PaymentModel } from "../../types";
import { useNavigate, useParams } from "react-router-dom";
import { authenticatedFetch } from "../../util/api";
import { createColumnHelper } from "@tanstack/react-table";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDollarSign } from "@fortawesome/free-solid-svg-icons";
import { convertToPacificTime } from "../../util/DateHelper";
import { HipTable } from "../../Shared/Table/HipTable";
import HipButton from "../../Shared/HipButton";

interface PaymentTableProps {
  numberOfRows: number;
  showLinkToAll: boolean;
  paymentCount: number;
}

export const PaymentTable: React.FC<PaymentTableProps> = ({
  numberOfRows,
  showLinkToAll,
  paymentCount,
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
  }, [cluster, numberOfRows, orderId, paymentCount]);

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
            <HipButton
              onClick={() => navigate(`/${cluster}/order/payments/${orderId}`)}
              color="link"
            >
              View All
            </HipButton>
          )}
        </>
      ) : (
        <HipButton
          onClick={() => navigate(`/${cluster}/order/details/${orderId}`)}
          className="float-right"
          color="link"
        >
          Back to Order Details
        </HipButton>
      )}
      <HipTable
        columns={paymentColumns}
        data={payments}
        disablePagination={numberOfRows <= 10} // pagination defaults to showing 10 rows per page, if we are guaranteed to show less than that, don't paginate
        disableFilter={numberOfRows <= 10} // same as above
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
