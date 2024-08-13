import React, { useEffect, useState } from "react";
import { OrderModel } from "../../types";
import { useNavigate, useParams } from "react-router-dom";
import { usePermissions } from "../../Shared/usePermissions";
import { usePromiseNotification } from "../../util/Notifications";
import OrderForm from "./OrderForm/OrderForm";
import { authenticatedFetch, parseBadRequest } from "../../util/api";
import { OrderStatus } from "../../types/status";
import StatusBar from "./OrderForm/StatusBar";
import HipTitle from "../../Shared/Layout/HipTitle";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipBody from "../../Shared/Layout/HipBody";
import HipLoading from "../../Shared/LoadingAndErrors/HipLoading";
import HipErrorBoundary from "../../Shared/LoadingAndErrors/HipErrorBoundary";
import HipClientError from "../../Shared/LoadingAndErrors/HipClientError";

const defaultOrder: OrderModel = {
  id: 0,
  PILookup: "",
  category: "",
  name: "",
  productName: "",
  description: "",
  notes: "",
  units: "",
  unitPrice: "",
  installments: 5,
  installmentType: "Yearly",
  quantity: 0,
  adjustment: 0,
  adjustmentReason: "",
  status: OrderStatus.Draft,
  createdOn: "",
  externalReference: "",
  adminNotes: "",
  subTotal: "",
  total: "",
  balanceRemaining: "",
  balancePending: "",
  metaData: [],
  billings: [],
  percentTotal: 0,
  historyCount: 0,
  paymentCount: 0,
};
export const CreateOrder: React.FC = () => {
  const { cluster, productId } = useParams();
  const { isClusterAdminForCluster } = usePermissions();
  const [order, setOrder] = useState<OrderModel>(null);
  const [isClusterAdmin, setIsClusterAdmin] = useState(null);
  const [notification, setNotification] = usePromiseNotification();
  const navigate = useNavigate();

  useEffect(() => {
    setIsClusterAdmin(isClusterAdminForCluster());
  }, [isClusterAdmin, isClusterAdminForCluster]);

  useEffect(() => {
    if (productId) {
      const fetchProductOrder = async () => {
        const response = await authenticatedFetch(
          `/api/${cluster}/order/GetProduct/${productId}`,
        );

        if (response.ok) {
          const data = await response.json();
          setOrder(data);
        } else {
          alert("Error fetching product for order");
        }
      };

      fetchProductOrder();
    } else {
      if (isClusterAdmin === false) {
        navigate(`/${cluster}/product/index`);
      } else {
        setOrder(defaultOrder);
      }
    }
  }, [cluster, isClusterAdmin, navigate, productId]);

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
      billings: updatedOrder.billings,
    };

    const req = authenticatedFetch(`/api/${cluster}/order/Save`, {
      method: "POST",
      body: JSON.stringify(editedOrder),
    });

    setNotification(req, "Saving", "Order Saved", async (r) => {
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

      navigate(`/${cluster}/order/details/${data.id}`);
    }

    setOrder(editedOrder); // should be newOrder once it's pulling from the API
  };

  // RH TODO: handle loading/error states
  if (isClusterAdmin === null) {
    return (
      <HipMainWrapper>
        <HipTitle title="New Order" subtitle="Create" />
        <HipBody>
          <HipLoading />
        </HipBody>
      </HipMainWrapper>
    );
  }

  if (!order) {
    return (
      <HipMainWrapper>
        <HipTitle title="New Order" subtitle="Create" />
        <HipBody>
          <HipLoading />
        </HipBody>
      </HipMainWrapper>
    );
  }

  return (
    <HipMainWrapper>
      <HipTitle title={`New Order`} subtitle="Create" />
      <HipBody>
        <HipErrorBoundary>
          <StatusBar
            status={order.status}
            animated={notification.pending}
            showInProgress={true}
          />
        </HipErrorBoundary>
        <HipErrorBoundary
          fallback={
            <HipClientError
              type="alert"
              thereWasAnErrorLoadingThe="Order Form"
              contactLink={true}
            />
          }
        >
          <OrderForm
            orderProp={order}
            isDetailsPage={false}
            isAdmin={isClusterAdmin}
            cluster={cluster}
            onlyChartStrings={false}
            onSubmit={submitOrder}
          />
        </HipErrorBoundary>
      </HipBody>
    </HipMainWrapper>
  );
};
