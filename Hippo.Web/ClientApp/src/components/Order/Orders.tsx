import { useEffect, useState } from "react";
import { OrderListModel } from "../../types";
import { useParams } from "react-router-dom";
import { authenticatedFetch } from "../../util/api";

import HipTitle from "../../Shared/Layout/HipTitle";
import HipBody from "../../Shared/Layout/HipBody";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipLoading from "../../Shared/LoadingAndErrors/HipLoading";
import HipErrorBoundary from "../../Shared/LoadingAndErrors/HipErrorBoundary";
import HipClientError from "../../Shared/LoadingAndErrors/HipClientError";
import { OrdersTable } from "./Tables/OrdersTable";

export const Orders = () => {
  const [orders, setOrders] = useState<OrderListModel[]>();
  const { cluster, orderType } = useParams();
  const isAdminOrders = orderType === "adminorders";

  useEffect(() => {
    const fetchOrders = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/order/${orderType}`,
      );

      if (response.ok) {
        const data = await response.json();
        setOrders(data);
      } else {
        alert("Error fetching orders");
      }
    };

    fetchOrders();
  }, [cluster, orderType]);

  // RH TODO: handle loading/error states
  if (orders === undefined) {
    return (
      <HipMainWrapper>
        <HipTitle title="Orders" />
        <HipBody>
          <HipLoading />
        </HipBody>
      </HipMainWrapper>
    );
  } else {
    return (
      <HipMainWrapper>
        <HipTitle
          title={isAdminOrders ? "Orders" : "My Orders"}
          subtitle={isAdminOrders ? "Admin" : null}
        />
        <HipBody>
          <HipErrorBoundary
            fallback={
              <HipClientError
                type="alert"
                thereWasAnErrorLoadingThe="Orders Table"
                contactLink={true}
              />
            }
          >
            <OrdersTable
              orders={orders}
              cluster={cluster}
              isAdminOrders={isAdminOrders}
            />
          </HipErrorBoundary>
        </HipBody>
      </HipMainWrapper>
    );
  }
};
