import React, { useEffect, useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import {
  HistoryModel,
  OrderMetadataModel,
  OrderModel,
  PaymentModel,
} from "../../types";
import { authenticatedFetch } from "../../util/api";
import { ReactTable } from "../../Shared/ReactTable";
import { createColumnHelper } from "@tanstack/react-table";
import ChartStringValidation from "./ChartStringValidation";
import OrderForm from "./OrderForm";
import { usePermissions } from "../../Shared/usePermissions";
import { Row } from "reactstrap";

export const Details = () => {
  const { cluster, orderId } = useParams();
  const [order, setOrder] = useState<OrderModel | null>(null);
  const [balanceRemaining, setBalanceRemaining] = useState<number>(0);
  const [balancePending, setBalancePending] = useState<number>(0);
  const { isClusterAdminForCluster } = usePermissions();
  const [isClusterAdmin, setIsClusterAdmin] = useState(null);

  useEffect(() => {
    setIsClusterAdmin(isClusterAdminForCluster());
  }, [isClusterAdmin, isClusterAdminForCluster]);

  useEffect(() => {
    const fetchOrder = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/order/get/${orderId}`,
      );

      if (response.ok) {
        const data = await response.json();
        console.log(data);
        setOrder(data);
        const balanceRemaining = parseFloat(data.balanceRemaining);
        setBalanceRemaining(balanceRemaining);
        const balancePending = data?.payments
          .filter((payment) => payment.status !== "Completed")
          .reduce((acc, payment) => acc + parseFloat(payment.amount), 0);
        setBalancePending(balancePending);
      } else {
        alert("Error fetching order");
      }
    };

    fetchOrder();
  }, [cluster, orderId]);

  const historyColumnHelper = createColumnHelper<HistoryModel>();

  const historyColumns = [
    historyColumnHelper.accessor("actedDate", {
      header: "Date",
      id: "actedDate",
      cell: (value) => (
        <span>{new Date(value.row.original.actedDate).toLocaleString()}</span>
      ),
    }),
    historyColumnHelper.accessor("actedBy", {
      header: "Actor",
      id: "actedBy",
      cell: (value) => (
        <>
          {value.row.original.actedBy && (
            <>
              {value.row.original.actedBy.firstName}{" "}
              {value.row.original.actedBy.lastName} (
              {value.row.original.actedBy.email})
            </>
          )}
          {!value.row.original.actedBy && <>System</>}
        </>
      ),
    }),
    historyColumnHelper.accessor("status", { header: "Status", id: "status" }),
    historyColumnHelper.accessor("details", {
      header: "Details",
      id: "details",
    }),
  ];

  const paymentColumnHelper = createColumnHelper<PaymentModel>();

  const paymentColumns = [
    paymentColumnHelper.accessor("amount", {
      header: "Amount",
      id: "amount",
      cell: (value) => (
        <span>
          <i className="fas fa-dollar-sign" />{" "}
          {value.row.original.amount.toFixed(2)}
        </span>
      ),
    }),
    paymentColumnHelper.accessor("status", { header: "Status", id: "status" }),
    paymentColumnHelper.accessor("createdOn", {
      header: "Created On",
      id: "createdOn",
      cell: (value) => (
        <span>{new Date(value.row.original.createdOn).toLocaleString()}</span>
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

  // async function so the form can manage the loading state
  const submitOrder = async (updatedOrder: OrderModel) => {
    const editedOrder: OrderModel = {
      // uneditable fields
      id: updatedOrder.id,
      status: updatedOrder.status,
      createdOn: updatedOrder.createdOn,
      total: updatedOrder.total,
      subTotal: updatedOrder.subTotal,
      balanceRemaining: updatedOrder.balanceRemaining,
      billings: updatedOrder.billings,
      payments: updatedOrder.payments,
      history: updatedOrder.history,
      piUser: updatedOrder.piUser,
      percentTotal: updatedOrder.percentTotal,

      // editable fields
      PILookup: updatedOrder.PILookup,
      name: updatedOrder.name,
      productName: updatedOrder.productName,
      description: updatedOrder.description,
      category: updatedOrder.category,
      externalReference: updatedOrder.externalReference,
      notes: updatedOrder.notes,
      units: updatedOrder.units,
      unitPrice: updatedOrder.unitPrice,
      quantity: updatedOrder.quantity,
      installments: updatedOrder.installments,
      installmentType: updatedOrder.installmentType,
      adjustment: updatedOrder.adjustment,
      adjustmentReason: updatedOrder.adjustmentReason,
      adminNotes: updatedOrder.adminNotes,
      metaData: updatedOrder.metaData,
      lifeCycle: updatedOrder.lifeCycle,
      expirationDate: updatedOrder.expirationDate,
      installmentDate: updatedOrder.installmentDate,
    };

    // TODO: await API call
    // const newOrder = await new Promise((resolve) => setTimeout(resolve, 1000));

    setOrder(editedOrder); // should be newOrder once it's pulling from the API
  };

  if (!order) {
    return <div>Loading...</div>;
  }

  return (
    <div>
      <div className="row justify-content-center">
        <div className="col-md-8">
          <h1>Order Details: Id {order.id}</h1>
          <h2>
            {order.piUser?.name} ({order.piUser?.email})
          </h2>

          <OrderForm
            orderProp={order}
            readOnly={true}
            isAdmin={isClusterAdmin}
            cluster={cluster}
            onSubmit={submitOrder}
          />

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
                  <td>
                    <ChartStringValidation
                      chartString={billing.chartString}
                      key={billing.chartString}
                    />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          <h2>History</h2>
          <ReactTable
            columns={historyColumns}
            data={order.history}
            initialState={{
              sorting: [
                {
                  id: "actedDate",
                  desc: true,
                },
              ],
            }}
          />
          <h2>Payments</h2>
          <div className="form-group">
            <label htmlFor="fieldBalanceRemaining">Balance Remaining</label>
            <div className="input-group">
              <div className="input-group-prepend">
                <span className="input-group-text" style={{ height: "38px" }}>
                  <i className="fas fa-dollar-sign" />
                </span>
              </div>
              <input
                className="form-control"
                id="fieldBalanceRemaining"
                value={order.balanceRemaining}
                readOnly
              />
            </div>
          </div>
          {balancePending !== 0 && (
            <div className="form-group">
              <label htmlFor="fieldBalancePending">Balance Pending</label>
              <div className="input-group">
                <div className="input-group-prepend">
                  <span className="input-group-text" style={{ height: "38px" }}>
                    <i className="fas fa-dollar-sign" />
                  </span>
                </div>
                <input
                  className="form-control"
                  id="fieldBalancePending"
                  value={(balanceRemaining - balancePending).toFixed(2)}
                  readOnly
                />
              </div>
            </div>
          )}
          <br />
          {order.payments.length !== 0 && (
            <ReactTable
              columns={paymentColumns}
              data={order.payments}
              initialState={{
                sorting: [
                  {
                    id: "createdOn",
                    desc: true,
                  },
                ],
              }}
            />
          )}
        </div>
      </div>
    </div>
  );
};
