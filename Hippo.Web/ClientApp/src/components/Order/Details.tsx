import React, { useCallback, useContext, useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { OrderModel, PaymentModel } from "../../types";
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
} from "@fortawesome/free-solid-svg-icons";
import { HistoryTable } from "./HistoryTable";
import { PaymentTable } from "./PaymentTable";
import {
  OrderStatus,
  UpdateOrderStatusModel,
  adminCanApproveStatuses,
  adminCanArchiveStatuses,
  adminCanRejectStatuses,
  adminEditableStatuses,
  canUpdateChartStringsStatuses,
  sponsorCanAddPaymentStatuses,
  sponsorCanApproveStatuses,
  sponsorCanCancelStatuses,
  sponsorEditableStatuses,
} from "./Statuses/status";
import StatusBar from "./Statuses/StatusBar";
import OrderPaymentDetails from "./OrderForm/OrderPaymentDetails";
import HipTitle from "../../Shared/Layout/HipTitle";
import HipButton from "../../Shared/HipComponents/HipButton";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipBody from "../../Shared/Layout/HipBody";
import HipLoading from "../../Shared/LoadingAndErrors/HipLoading";
import HipErrorBoundary from "../../Shared/LoadingAndErrors/HipErrorBoundary";
import HipClientError from "../../Shared/LoadingAndErrors/HipClientError";
import { getNextStatus } from "./Statuses/status";
import HipAlert from "../../Shared/HipComponents/HipAlert";
import StatusDialog from "./Statuses/StatusDialog";
import { HipFormGroup } from "../../Shared/Form/HipFormGroup";

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
      const nextStatus: UpdateOrderStatusModel = getNextStatus({
        status: order.status,
        isRecurring: order.isRecurring,
      });
      setUpdateStatusModel(nextStatus);
    }
  }, [order]);

  // never actually called on this page, as details is edit only
  const submitOrder = async (updatedOrder: OrderModel) => {
    setOrder(updatedOrder); // should be newOrder once it's pulling from the API
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
          <StatusDialog
            newStatus={updateStatusModel.newStatus}
            currentStatus={updateStatusModel.currentStatus}
            isAdmin={isClusterAdmin}
          />
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

  const [cancelOrderConfirmation] = useConfirmationDialog<null>(
    {
      title: "Cancel Order",
      message: (setReturn) => (
        <StatusDialog
          newStatus={OrderStatus.Cancelled}
          currentStatus={order.status}
          isAdmin={isClusterAdmin}
          newStatusDanger={true}
          hideDescription={true}
        >
          <>
            {order.wasRateAdjusted && (
              <>
                <hr />
                <h1 className="hip-text-danger-dark">WARNING!</h1>
                <p>
                  Cancelling an existing recurring order that has had its Unit
                  Price adjusted will result in interruption of your service.
                  Please contact the admin(s) before Confirming this action.
                </p>
                <p>The Cancel button on this dialog will cancel this action.</p>
              </>
            )}
          </>
        </StatusDialog>
      ),
      canConfirm: !notification.pending,
    },
    [order],
  );

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

  const [changeRateConfirmation] = useConfirmationDialog<number>(
    {
      title: "Change Rate",
      message: (setReturn) => {
        return (
          <StatusDialog
            newStatus={OrderStatus.Created}
            currentStatus={order.status}
            isAdmin={isClusterAdmin}
            hideDescription={true}
            newStatusDanger={false}
          >
            <HipFormGroup size="lg">
              <h3>
                This will set the recurring order back to created with a new
                unit price. The PI will need to approve the order. Once the
                order is approved, processed, and activated, it will begin
                billing at the new unit price after the current billing cycle.
              </h3>
              <h3>Billing will not resume until this happens.</h3>
              <br />
              <h3>
                Current Unit Price{" "}
                <span className={"hip-text-primary"}>${order.unitPrice}</span>
              </h3>
              <h4 className="form-label">New Unit Price</h4>
              <input
                className="form-control"
                id="newUnitPrice"
                type="number"
                min="0.01"
                step="0.01"
                inputMode="decimal"
                pattern="^\d*\.?\d*$"
                placeholder="Enter new unit price"
                onChange={(e) => {
                  const value = e.target.value;
                  const num = parseFloat(value);
                  // Allow only numbers with up to 2 decimal places
                  if (
                    (/^\d*\.?\d{0,2}$/.test(value) || value === "") &&
                    (!value || !isNaN(num))
                  ) {
                    setReturn(num);
                  }
                }}
              />
            </HipFormGroup>
          </StatusDialog>
        );
      },
      canConfirm: (returnValue) =>
        typeof returnValue === "number" && returnValue >= 0.01,
    },
    [order],
  );

  const changeRate = async () => {
    const [confirmed, newUnitPrice] = await changeRateConfirmation();

    if (!confirmed) {
      return;
    }

    const req = authenticatedFetch(
      `/api/${cluster}/order/changeRecurringRate/${orderId}?newRate=${newUnitPrice}`,
      {
        method: "POST",
      },
    );
    setNotification(
      req,
      "Changing Rate",
      "Rate Changed, set to Created",
      async (r) => {
        if (r.status === 400) {
          const errors = await parseBadRequest(response);
          return errors;
        } else {
          return "An error happened, please try again.";
        }
      },
    );

    const response = await req;

    if (response.ok) {
      const data = await response.json();
      setOrder(data);
      //just reget the whole order to update fields
      window.location.reload();
    }
  };

  const [rejectOrderConfirmation] = useConfirmationDialog<string>(
    {
      title: "Reject Order",
      message: (setReturn) => {
        return (
          <StatusDialog
            newStatus={OrderStatus.Rejected}
            currentStatus={order.status}
            isAdmin={isClusterAdmin}
            hideDescription={true}
            newStatusDanger={true}
          >
            <HipFormGroup size="lg">
              {order.wasRateAdjusted && (
                <>
                  <hr />
                  <h1 className="hip-text-danger-dark">WARNING!</h1>
                  <p>
                    Rejecting an existing recurring order that has had it's Unit
                    Price adjusted will result in billing stopping. You probably
                    don't want to do this.
                  </p>
                  <hr />
                </>
              )}

              <br />
              <h4 className="form-label">Reason</h4>
              <input
                className="form-control"
                id="rejectOrderReason"
                onChange={(e) => {
                  setReturn(e.target.value);
                }}
              ></input>
            </HipFormGroup>
          </StatusDialog>
        );
      },
      canConfirm: (returnValue) => notEmptyOrFalsey(returnValue),
    },
    [order],
  );

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
        <HipTitle title={`Order ${orderId ?? ""}`} subtitle="Details" />
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
          <HipAlert color="danger">
            This order needs to have billing information added before it can be
            submitted (Approve).
          </HipAlert>
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
                  sponsorEditableStatuses.includes(order.status) &&
                  !order.wasRateAdjusted
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
                  order.isRecurring && order.status === OrderStatus.Active
                }
              >
                <HipButton
                  className="btn btn-primary"
                  onClick={updateStatus}
                  onMouseEnter={() => setHoverAction(OrderStatus.Closed)}
                  onMouseLeave={() => setHoverAction(null)}
                >
                  {" "}
                  <FontAwesomeIcon icon={faCheck} />
                  Close Recurring Order
                </HipButton>{" "}
              </ShowFor>
            </HipErrorBoundary>
            <HipErrorBoundary>
              <ShowFor
                roles={["System", "ClusterAdmin", "FinancialAdmin"]}
                condition={
                  order.isRecurring && order.status === OrderStatus.Active
                }
              >
                {/*TODO: fix what it does */}
                <HipButton
                  className="btn btn-primary"
                  onClick={changeRate}
                  onMouseEnter={() => setHoverAction(OrderStatus.Created)}
                  onMouseLeave={() => setHoverAction(null)}
                >
                  {" "}
                  <FontAwesomeIcon icon={faCheck} />
                  Change Rate
                </HipButton>{" "}
              </ShowFor>
            </HipErrorBoundary>
            <HipErrorBoundary>
              <ShowFor
                roles={["System", "ClusterAdmin"]}
                condition={
                  adminCanArchiveStatuses.includes(order.status) &&
                  (order.isRecurring ||
                    new Date(order.expirationDate) <= new Date())
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
                  <FontAwesomeIcon icon={faPencil} />
                  Update Billing Info
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
                  balanceRemaining > 0 &&
                  !order.isRecurring
                }
              >
                <HipButton className="btn btn-secondary" onClick={makePayment}>
                  {" "}
                  <FontAwesomeIcon icon={faDollarSign} />
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
            isRecurring={order.isRecurring}
            totalPaid={order.totalPaid}
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
