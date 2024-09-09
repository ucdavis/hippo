import React, { useEffect, useState } from "react";
import { OrderModel } from "../../types";
import { useNavigate, useParams } from "react-router-dom";
import { usePermissions } from "../../Shared/usePermissions";
import { usePromiseNotification } from "../../util/Notifications";
import OrderForm from "./OrderForm/OrderForm";
import { authenticatedFetch, parseBadRequest } from "../../util/api";
import StatusBar from "./Statuses/StatusBar";
import HipTitle from "../../Shared/Layout/HipTitle";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipBody from "../../Shared/Layout/HipBody";
import HipLoading from "../../Shared/LoadingAndErrors/HipLoading";
import HipErrorBoundary from "../../Shared/LoadingAndErrors/HipErrorBoundary";
import HipClientError from "../../Shared/LoadingAndErrors/HipClientError";

export const EditOrder: React.FC = () => {
  const { cluster, orderId } = useParams();
  const { isClusterAdminForCluster } = usePermissions();
  const [order, setOrder] = useState<OrderModel>(null);
  const [isClusterAdmin, setIsClusterAdmin] = useState(null);
  const [notification, setNotification] = usePromiseNotification();
  const navigate = useNavigate();

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
        setOrder(data);
      } else {
        alert("Error fetching order");
      }
    };

    fetchOrder();
  }, [cluster, orderId]);

  // async function so the form can manage the loading state
  const submitOrder = async (updatedOrder: OrderModel) => {
    const req = authenticatedFetch(`/api/${cluster}/order/Save`, {
      method: "POST",
      body: JSON.stringify(updatedOrder),
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

      setOrder(data);
      navigate(`/${cluster}/order/details/${data.id}`);
    } else {
      setOrder(updatedOrder);
    }
  };

  // RH TODO: handle loading/error states
  if (isClusterAdmin === null) {
    return (
      <HipMainWrapper>
        <HipTitle title="Order" subtitle="Edit" />
        <HipBody>
          <HipLoading />
        </HipBody>
      </HipMainWrapper>
    );
  }

  if (!order) {
    return (
      <HipMainWrapper>
        <HipTitle title="Order" subtitle="Edit" />
        <HipBody>
          <HipLoading />
        </HipBody>
      </HipMainWrapper>
    );
  }

  return (
    <HipMainWrapper>
      <HipTitle title={`Order ${order.id}: ${order.name}`} subtitle="Edit" />
      <HipBody>
        <HipErrorBoundary>
          <StatusBar status={order.status} />
        </HipErrorBoundary>
        <HipErrorBoundary
          fallback={
            <HipClientError
              thereWasAnErrorLoadingThe="Order Form"
              type="alert"
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
