import React, { useCallback, useEffect, useMemo, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { HistoryModel, OrderModel, PaymentModel } from "../../types";
import { authenticatedFetch, parseBadRequest } from "../../util/api";
import { ReactTable } from "../../Shared/ReactTable";
import { createColumnHelper } from "@tanstack/react-table";
import OrderForm from "./OrderForm";
import { usePermissions } from "../../Shared/usePermissions";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { usePromiseNotification } from "../../util/Notifications";

export const Details = () => {
  const { cluster, orderId } = useParams();
  const [order, setOrder] = useState<OrderModel | null>(null);
  const [balanceRemaining, setBalanceRemaining] = useState<number>(0);
  const [balancePending, setBalancePending] = useState<number>(0);
  const { isClusterAdminForCluster } = usePermissions();
  const [isClusterAdmin, setIsClusterAdmin] = useState(null);
  const [notification, setNotification] = usePromiseNotification();

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

  const [editPaymentModel, setEditPaymentModel] = useState<PaymentModel>({
    id: 0,
    amount: 0,
    status: "",
    createdOn: "",
  });

  const [makePaymentConfirmation] = useConfirmationDialog<PaymentModel>(
    {
      title: "Make One Time Payment",
      message: (setReturn) => (
        <>
          <div className="form-group">
            <label htmlFor="fieldAmount">Amount</label>
            <input
              className="form-control"
              id="fieldAmount"
              required
              value={editPaymentModel.amount}
              onChange={(e) => {
                const value = e.target.value;
                if (/^\d*\.?\d*$/.test(value) || /^\d*\.$/.test(value)) {
                  // This regex checks for a valid decimal or integer
                  const model: PaymentModel = {
                    ...editPaymentModel,
                    amount: parseFloat(value),
                  };
                  setEditPaymentModel(model);
                  setReturn(model);
                }
              }}
            />
          </div>
        </>
      ),
      canConfirm: !notification.pending && editPaymentModel.amount > 0,
    },
    [editPaymentModel, notification.pending],
  );

  const makePayment = useCallback(async () => {
    const [confirmed, editPaymentModel] = await makePaymentConfirmation();

    if (!confirmed) {
      return;
    }
    console.log(editPaymentModel);
    debugger;

    const req = authenticatedFetch(
      `/api/${cluster}/order/makepayment/${orderId}?amount=${editPaymentModel.amount}`,
      {
        method: "POST",
      },
    );
    setNotification(req, "Making Payment", "Payment Made", async (r) => {
      if (r.status === 400) {
        const errors = await parseBadRequest(response);
        return errors;
      } else {
        return "An error happened, please try again.";
      }
    });

    const response = await req;

    if (response.ok) {
      const data = await response.json();
      console.log(data);
      setOrder(data);
      setEditPaymentModel({ id: 0, amount: 0, status: "", createdOn: "" });
    }
  }, [cluster, orderId, makePaymentConfirmation, setNotification]);

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
          <Link
            className="btn btn-primary"
            to={`/${cluster}/order/edit/${order.id}`}
          >
            Edit Order
          </Link>{" "}
          <button className="btn btn-primary"> Approve Order</button>{" "}
          <button className="btn btn-primary"> Cancel Order</button>{" "}
          <button className="btn btn-primary"> Update Chart Strings</button>{" "}
          <button className="btn btn-primary" onClick={() => makePayment()}>
            {" "}
            Onetime Payment
          </button>
          <OrderForm
            orderProp={order}
            readOnly={true}
            isAdmin={isClusterAdmin}
            cluster={cluster}
            onSubmit={submitOrder}
          />
          {/* <h2>Chart Strings</h2>
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
          </table> */}
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
