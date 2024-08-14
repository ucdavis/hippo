import React, { useCallback, useContext, useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { OrderModel, PaymentModel, UpdateOrderStatusModel } from "../../types";
import { authenticatedFetch, parseBadRequest } from "../../util/api";
import OrderForm from "./OrderForm/OrderForm";
import { usePermissions } from "../../Shared/usePermissions";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { usePromiseNotification } from "../../util/Notifications";
import { notEmptyOrFalsey } from "../../util/ValueChecks";
import { ShowFor } from "../../Shared/ShowFor";
import AppContext from "../../Shared/AppContext";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faDollarSign,
  faXmark,
  faCheck,
  faPencil,
  faPlus,
} from "@fortawesome/free-solid-svg-icons";
import { HistoryTable } from "./HistoryTable";
import { PaymentTable } from "./PaymentTable";
import {
  OrderStatus,
  adminCanApproveStatuses,
  adminCanArchiveStatuses,
  adminCanRejectStatuses,
  adminEditableStatuses,
  canUpdateChartStringsStatuses,
  sponsorCanAddPaymentStatuses,
  sponsorCanApproveStatuses,
  sponsorCanCancelStatuses,
  sponsorEditableStatuses,
} from "../../types/status";
import StatusBar from "./OrderForm/StatusBar";
import OrderPaymentDetails from "./OrderForm/OrderPaymentDetails";
import { Alert } from "reactstrap";
import HipTitle from "../../Shared/Layout/HipTitle";
import HipButton from "../../Shared/HipButton";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipBody from "../../Shared/Layout/HipBody";
import HipLoading from "../../Shared/LoadingAndErrors/HipLoading";
import HipErrorBoundary from "../../Shared/LoadingAndErrors/HipErrorBoundary";
import HipClientError from "../../Shared/LoadingAndErrors/HipClientError";

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
  const [hoverAction, setHoverAction] = useState<OrderStatus | null>(null);

  useEffect(() => {
    setIsClusterAdmin(isClusterAdminForCluster());
  }, [isClusterAdmin, isClusterAdminForCluster]);

  const calculateBalanceRemaining = (data: any) => {
    const balanceRemaining = parseFloat(data.balanceRemaining);
    setBalanceRemaining(balanceRemaining);
    // const balancePending = data.payments
    //   .filter(
    //     (payment) =>
    //       payment.status !== "Completed" && payment.status !== "Cancelled",
    //   )
    //   .reduce((acc, payment) => acc + parseFloat(payment.amount), 0);
    setBalancePending(parseFloat(data.balancePending));
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
        case OrderStatus.Draft:
          setUpdateStatusModel({
            currentStatus: order.status,
            newStatus: OrderStatus.Created,
          });
          break;
        case OrderStatus.Created:
          setUpdateStatusModel({
            currentStatus: order.status,
            newStatus: OrderStatus.Submitted,
          });
          break;
        case OrderStatus.Submitted:
          setUpdateStatusModel({
            currentStatus: order.status,
            newStatus: OrderStatus.Processing,
          });
          break;
        case OrderStatus.Processing:
          setUpdateStatusModel({
            currentStatus: order.status,
            newStatus: OrderStatus.Active,
          });
          break;
        case OrderStatus.Completed:
          setUpdateStatusModel({
            currentStatus: order.status,
            newStatus: OrderStatus.Archived,
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
      balancePending: updatedOrder.balancePending,
      billings: updatedOrder.billings,
      piUser: updatedOrder.piUser,
      percentTotal: updatedOrder.percentTotal,
      nextPaymentDate: updatedOrder.nextPaymentDate,
      historyCount: updatedOrder.historyCount,
      paymentCount: updatedOrder.paymentCount,

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
    status: OrderStatus.Draft,
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
        status: OrderStatus.Draft,
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
            {updateStatusModel.newStatus === "Archived" && (
              <div className="merlot-bg">
                This will archive the order and it will no longer be visible in
                the active orders list.
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
          <div className="col-md-12">
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
    return (
      <HipMainWrapper>
        <HipTitle title="Order" subtitle="Details" />
        <HipBody>
          <HipLoading />
        </HipBody>
      </HipMainWrapper>
    );
  }

  return (
    <HipMainWrapper>
      {order.piUser?.id === user.detail.id &&
        sponsorCanApproveStatuses.includes(order.status) &&
        order.billings.length <= 0 && (
          <Alert color="danger">
            This order needs to have billing information added before it can be
            submitted (Approve).
          </Alert>
        )}
      <HipTitle
        title={`Order ${order.id}: ${order.name}`}
        subtitle="Details"
        buttons={
          <>
            <HipErrorBoundary>
              <ShowFor
                condition={
                  order.piUser?.id === user.detail.id &&
                  sponsorCanCancelStatuses.includes(order.status)
                }
              >
                <HipButton
                  color="danger"
                  onClick={cancelOrder}
                  onMouseEnter={() => setHoverAction(OrderStatus.Cancelled)}
                  onMouseLeave={() => setHoverAction(null)}
                >
                  {" "}
                  <FontAwesomeIcon icon={faXmark} />
                  Cancel Order
                </HipButton>{" "}
              </ShowFor>
            </HipErrorBoundary>
            <HipErrorBoundary>
              <ShowFor
                roles={["System", "ClusterAdmin"]}
                condition={adminCanRejectStatuses.includes(order.status)}
              >
                <HipButton
                  color="danger"
                  onClick={rejectOrder}
                  onMouseEnter={() => setHoverAction(OrderStatus.Rejected)}
                  onMouseLeave={() => setHoverAction(null)}
                >
                  {" "}
                  <FontAwesomeIcon icon={faXmark} />
                  Reject Order
                </HipButton>{" "}
              </ShowFor>
            </HipErrorBoundary>
            <HipErrorBoundary>
              <ShowFor
                roles={["System", "ClusterAdmin"]}
                condition={adminEditableStatuses.includes(order.status)}
              >
                <Link
                  className="btn btn-primary"
                  to={`/${cluster}/order/edit/${order.id}`}
                >
                  <FontAwesomeIcon icon={faPencil} />
                  Edit Order
                </Link>{" "}
              </ShowFor>
            </HipErrorBoundary>
            <HipErrorBoundary>
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
                  <FontAwesomeIcon icon={faPencil} />
                  Edit Order
                </Link>{" "}
              </ShowFor>
            </HipErrorBoundary>
            <HipErrorBoundary>
              <ShowFor
                roles={["System", "ClusterAdmin"]}
                condition={adminCanApproveStatuses.includes(order.status)}
              >
                <HipButton
                  className="btn btn-primary"
                  onClick={updateStatus}
                  onMouseEnter={() =>
                    setHoverAction(
                      order.status === OrderStatus.Submitted
                        ? OrderStatus.Processing
                        : OrderStatus.Active,
                    )
                  }
                  onMouseLeave={() => setHoverAction(null)}
                >
                  {" "}
                  <FontAwesomeIcon icon={faCheck} />
                  Approve Order
                </HipButton>{" "}
              </ShowFor>
            </HipErrorBoundary>
            <HipErrorBoundary>
              <ShowFor
                roles={["System", "ClusterAdmin"]}
                condition={
                  adminCanArchiveStatuses.includes(order.status) &&
                  new Date(order.expirationDate) <= new Date()
                }
              >
                <HipButton
                  className="btn btn-primary"
                  onClick={updateStatus}
                  onMouseEnter={() => setHoverAction(OrderStatus.Archived)}
                  onMouseLeave={() => setHoverAction(null)}
                >
                  {" "}
                  <FontAwesomeIcon icon={faCheck} />
                  Archive Order
                </HipButton>{" "}
              </ShowFor>
            </HipErrorBoundary>
            <HipErrorBoundary>
              {/* If you are the sponsor (PI) and it is in the created status, you can move it to submitted if there is billing info */}
              <ShowFor
                condition={canUpdateChartStringsStatuses.includes(order.status)}
              >
                <Link
                  className="btn btn-secondary"
                  to={`/${cluster}/order/updatechartstrings/${order.id}`}
                >
                  <FontAwesomeIcon icon={faDollarSign} />
                  Update Chart Strings
                </Link>{" "}
              </ShowFor>
            </HipErrorBoundary>
            <HipErrorBoundary>
              <ShowFor
                condition={
                  order.piUser?.id === user.detail.id &&
                  sponsorCanApproveStatuses.includes(order.status) &&
                  order.billings.length > 0
                }
              >
                <HipButton
                  className="btn btn-primary"
                  onClick={updateStatus}
                  onMouseEnter={() => setHoverAction(OrderStatus.Submitted)}
                  onMouseLeave={() => setHoverAction(null)}
                >
                  {" "}
                  <FontAwesomeIcon icon={faCheck} />
                  Approve Order
                </HipButton>{" "}
              </ShowFor>
            </HipErrorBoundary>
            <HipErrorBoundary>
              <ShowFor
                condition={
                  sponsorCanAddPaymentStatuses.includes(order.status) &&
                  order.piUser?.id === user.detail.id &&
                  balanceRemaining > 0
                }
              >
                <HipButton className="btn btn-primary" onClick={makePayment}>
                  {" "}
                  <FontAwesomeIcon icon={faPlus} />
                  Onetime Payment
                </HipButton>
              </ShowFor>
            </HipErrorBoundary>
          </>
        }
      />
      <HipBody>
        <HipErrorBoundary>
          <StatusBar status={order.status} showOnHover={hoverAction} />
        </HipErrorBoundary>
        <HipErrorBoundary
          fallback={
            <HipClientError
              thereWasAnErrorLoadingThe="Order Details"
              type="alert"
              contactLink={true}
            />
          }
        >
          <OrderForm
            orderProp={order}
            isDetailsPage={true}
            isAdmin={isClusterAdmin}
            cluster={cluster}
            onlyChartStrings={false}
            onSubmit={submitOrder}
          />
        </HipErrorBoundary>
        <HipErrorBoundary
          fallback={
            <HipClientError
              thereWasAnErrorLoadingThe="Order Payment Details"
              type="alert"
            />
          }
        >
          <OrderPaymentDetails
            balancePending={order.balancePending}
            balanceRemaining={order.balanceRemaining}
            nextPaymentDate={order.nextPaymentDate}
            nextPaymentAmount={order.nextPaymentAmount}
          />
        </HipErrorBoundary>
        <HipErrorBoundary
          fallback={
            <HipClientError
              thereWasAnErrorLoadingThe="Order Payment Table"
              type="alert"
            />
          }
        >
          <PaymentTable
            numberOfRows={5}
            showLinkToAll={true}
            paymentCount={order.paymentCount}
          />
        </HipErrorBoundary>
        <HipErrorBoundary
          fallback={
            <HipClientError
              thereWasAnErrorLoadingThe="Order History"
              type="alert"
            />
          }
        >
          <HistoryTable
            numberOfRows={5}
            showLinkToAll={true}
            historyCount={order.historyCount}
          />
        </HipErrorBoundary>
      </HipBody>
    </HipMainWrapper>
  );
};
