import React, { useCallback, useContext, useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { OrderModel, PaymentModel, UpdateOrderStatusModel } from "../../types";
import { authenticatedFetch, parseBadRequest } from "../../util/api";
import { ReactTable } from "../../Shared/ReactTable";
import { createColumnHelper } from "@tanstack/react-table";
import OrderForm from "./OrderForm";
import { usePermissions } from "../../Shared/usePermissions";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { usePromiseNotification } from "../../util/Notifications";
import { notEmptyOrFalsey } from "../../util/ValueChecks";
import { ShowFor } from "../../Shared/ShowFor";
import AppContext from "../../Shared/AppContext";
import {
  convertToPacificDate,
  convertToPacificTime,
} from "../../util/DateHelper";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDollarSign } from "@fortawesome/free-solid-svg-icons";
import { HistoryTable } from "./HistoryTable";

export const Details = () => {
  const { cluster, orderId } = useParams();
  const [{ user }] = useContext(AppContext);
  const [order, setOrder] = useState<OrderModel | null>(null);
  const [balanceRemaining, setBalanceRemaining] = useState<number>(0);
  const [balancePending, setBalancePending] = useState<number>(0);
  const { isClusterAdminForCluster } = usePermissions();
  const [isClusterAdmin, setIsClusterAdmin] = useState(null);
  const [notification, setNotification] = usePromiseNotification();
  const [updateStatusModel, setUpdateStatusModel] =
    useState<UpdateOrderStatusModel | null>(null);
  const adminEditableStatuses = ["Processing", "Active"];
  const sponsorEditableStatuses = ["Created"];

  useEffect(() => {
    setIsClusterAdmin(isClusterAdminForCluster());
  }, [isClusterAdmin, isClusterAdminForCluster]);

  const calculateBalanceRemaining = (data: any) => {
    const balanceRemaining = parseFloat(data.balanceRemaining);
    setBalanceRemaining(balanceRemaining);
    const balancePending = data.payments
      .filter(
        (payment) =>
          payment.status !== "Completed" && payment.status !== "Cancelled",
      )
      .reduce((acc, payment) => acc + parseFloat(payment.amount), 0);
    setBalancePending(balancePending);
  };

  useEffect(() => {
    const fetchOrder = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/order/get/${orderId}`,
      );

      if (response.ok) {
        const data = await response.json();

        setOrder(data);
        calculateBalanceRemaining(data);
      } else {
        alert("Error fetching order");
      }
    };

    fetchOrder();
  }, [cluster, orderId]);

  useEffect(() => {
    if (order) {
      // switch statement for data.status
      switch (order.status) {
        case "Created":
          setUpdateStatusModel({
            currentStatus: order.status,
            newStatus: "Submitted",
          });
          break;
        case "Submitted":
          setUpdateStatusModel({
            currentStatus: order.status,
            newStatus: "Processing",
          });
          break;
        case "Processing":
          setUpdateStatusModel({
            currentStatus: order.status,
            newStatus: "Active",
          });
          break;
        default:
          setUpdateStatusModel({
            currentStatus: order.status,
            newStatus: order.status,
          });
      }
    }
  }, [order]);

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
      nextPaymentDate: updatedOrder.nextPaymentDate,

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
    entryAmount: "",
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
              value={editPaymentModel.entryAmount}
              onChange={(e) => {
                const value = e.target.value;
                if (/^\d*\.?\d*$/.test(value) || /^\d*\.$/.test(value)) {
                  // This regex checks for a valid decimal or integer
                  const model: PaymentModel = {
                    ...editPaymentModel,
                    amount: parseFloat(value),
                    entryAmount: value,
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
      setOrder(data);
      setEditPaymentModel({
        id: 0,
        amount: 0,
        status: "",
        createdOn: "",
        entryAmount: "",
      });
      calculateBalanceRemaining(data);
    }
  }, [cluster, orderId, makePaymentConfirmation, setNotification]);

  const [approveOrderConfirmation] =
    useConfirmationDialog<UpdateOrderStatusModel>(
      {
        title: "Update Order Status",
        message: (setReturn) => (
          <>
            <div>Current Status: {updateStatusModel.currentStatus}</div>
            <div>Set Status to: {updateStatusModel.newStatus}</div>
            <hr />
            {updateStatusModel.newStatus === "Submitted" && (
              <div className="merlot-bg">
                This will submit the order to the cluster admins for processing.
              </div>
            )}
            {updateStatusModel.newStatus === "Processing" && (
              <div className="merlot-bg">
                This will indicate that an admin will start working on the
                order.
              </div>
            )}
            {updateStatusModel.newStatus === "Active" && (
              <div className="merlot-bg">
                This will move the order to active and allow manual billing as
                well as scheduled billing.
              </div>
            )}
          </>
        ),
        canConfirm: !notification.pending,
      },
      [order, notification.pending, updateStatusModel],
    );

  const updateStatus = useCallback(async () => {
    const [confirmed] = await approveOrderConfirmation();

    if (!confirmed) {
      return;
    }

    const req = authenticatedFetch(
      `/api/${cluster}/order/changeStatus/${orderId}?expectedStatus=${updateStatusModel.newStatus}`,
      {
        method: "POST",
      },
    );
    setNotification(req, "Updating Status", "Status Updated", async (r) => {
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
      setOrder(data);
    }
  }, [
    cluster,
    orderId,
    approveOrderConfirmation,
    setNotification,
    updateStatusModel,
  ]);

  const [cancelOrderConfirmation] = useConfirmationDialog<null>({
    title: "Cancel Order",
    message: "Are you sure you want to cancel this order?",
    canConfirm: !notification.pending,
  });

  const cancelOrder = async () => {
    const [confirmed] = await cancelOrderConfirmation();

    if (!confirmed) {
      return;
    }

    const req = authenticatedFetch(
      `/api/${cluster}/order/CancelOrder/${orderId}`,
      {
        method: "POST",
      },
    );
    setNotification(req, "Cancelling Order", "Order Cancelled", async (r) => {
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
      setOrder(data); //Maybe redirect?
    }
  };

  const [rejectOrderConfirmation] = useConfirmationDialog<string>({
    title: "Reject Order",
    message: (setReturn) => {
      return (
        <div className="row justify-content-center">
          <div className="col-md-8">
            <div className="form-group">
              <label className="form-label">Reason</label>

              <input
                className="form-control"
                id="rejectOrderReason"
                onChange={(e) => {
                  setReturn(e.target.value);
                }}
              ></input>
            </div>
          </div>
        </div>
      );
    },
    canConfirm: (returnValue) => notEmptyOrFalsey(returnValue),
  });

  const rejectOrder = async () => {
    const [confirmed, reason] = await rejectOrderConfirmation();

    if (!confirmed) {
      return;
    }

    const req = authenticatedFetch(
      `/api/${cluster}/order/reject/${orderId}?reason=${reason}`,
      {
        method: "POST",
      },
    );
    setNotification(req, "Rejecting Order", "Order Rejected", async (r) => {
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
      setOrder(data);
    }
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
          {order.piUser?.id === user.detail.id &&
            order.status === "Created" &&
            order.billings.length <= 0 && (
              <h3 style={{ backgroundColor: "#ffcccc" }}>
                This order needs to have billing information added before it can
                be submitted (Approve).
              </h3>
            )}
          <ShowFor
            roles={["System", "ClusterAdmin"]}
            condition={adminEditableStatuses.includes(order.status)}
          >
            <Link
              className="btn btn-primary"
              to={`/${cluster}/order/edit/${order.id}`}
            >
              Edit Order
            </Link>{" "}
          </ShowFor>
          <ShowFor
            condition={
              order.piUser?.id === user.detail.id &&
              sponsorEditableStatuses.includes(order.status)
            }
          >
            <Link
              className="btn btn-primary"
              to={`/${cluster}/order/edit/${order.id}`}
            >
              Edit Order
            </Link>{" "}
          </ShowFor>
          <ShowFor
            roles={["System", "ClusterAdmin"]}
            condition={["Submitted", "Processing"].includes(order.status)}
          >
            <button className="btn btn-primary" onClick={updateStatus}>
              {" "}
              Approve Order
            </button>{" "}
          </ShowFor>
          {/* If you are the sponsor (PI) and it is in the created status, you can move it to submitted if there is billing info */}
          <ShowFor
            condition={
              order.piUser?.id === user.detail.id &&
              order.status === "Created" &&
              order.billings.length > 0
            }
          >
            <button className="btn btn-primary" onClick={updateStatus}>
              {" "}
              Approve Order
            </button>{" "}
          </ShowFor>
          <ShowFor
            condition={
              order.piUser?.id === user.detail.id &&
              ["Created", "Submitted"].includes(order.status)
            }
          >
            <button className="btn btn-primary" onClick={cancelOrder}>
              {" "}
              Cancel Order
            </button>{" "}
          </ShowFor>
          <ShowFor
            roles={["System", "ClusterAdmin"]}
            condition={["Submitted", "Processing"].includes(order.status)}
          >
            <button className="btn btn-primary" onClick={rejectOrder}>
              {" "}
              Reject Order
            </button>{" "}
          </ShowFor>
          <ShowFor
            condition={
              !["Cancelled", "Rejected", "Completed"].includes(order.status)
            }
          >
            <Link
              className="btn btn-primary"
              to={`/${cluster}/order/updatechartstrings/${order.id}`}
            >
              Update Chart Strings
            </Link>{" "}
          </ShowFor>
          <ShowFor
            condition={
              order.status === "Active" &&
              order.piUser?.id === user.detail.id &&
              balanceRemaining > 0
            }
          >
            <button className="btn btn-primary" onClick={makePayment}>
              {" "}
              Onetime Payment
            </button>
          </ShowFor>
          <OrderForm
            orderProp={order}
            readOnly={true}
            isAdmin={isClusterAdmin}
            cluster={cluster}
            onlyChartStrings={false}
            onSubmit={submitOrder}
          />

          <HistoryTable numberOfRows={5} showLinkToAll={true} />
          <h2>Payments</h2>
          <div className="form-group">
            <label htmlFor="fieldBalanceRemaining">Balance Remaining</label>
            <div className="input-group">
              <div className="input-group-prepend">
                <span className="input-group-text" style={{ height: "38px" }}>
                  <FontAwesomeIcon icon={faDollarSign} />
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
              <label htmlFor="fieldBalancePending">
                Total Pending Payments
              </label>
              <div className="input-group">
                <div className="input-group-prepend">
                  <span className="input-group-text" style={{ height: "38px" }}>
                    <FontAwesomeIcon icon={faDollarSign} />
                  </span>
                </div>
                <input
                  className="form-control"
                  id="fieldBalancePending"
                  value={balancePending.toFixed(2)}
                  readOnly
                />
              </div>
            </div>
          )}
          {order.nextPaymentDate && (
            <>
              <div className="form-group">
                <label htmlFor="fieldNextPaymentDate">Next Payment Date</label>
                <input
                  className="form-control"
                  id="fieldNextPaymentDate"
                  value={convertToPacificDate(order.nextPaymentDate)}
                  readOnly
                />
              </div>
              <div className="form-group">
                <label htmlFor="fieldNextPaymentDate">
                  Next Payment Amount
                </label>
                <div className="input-group">
                  <div className="input-group-prepend">
                    <span
                      className="input-group-text"
                      style={{ height: "38px" }}
                    >
                      <FontAwesomeIcon icon={faDollarSign} />
                    </span>
                  </div>
                  <input
                    className="form-control"
                    id="fieldNextPaymentDate"
                    value={order.nextPaymentAmount}
                    readOnly
                  />
                </div>
              </div>
            </>
          )}
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
