import { useEffect, useState } from "react";
import { OrderListModel } from "../../types";
import { useParams } from "react-router-dom";
import { authenticatedFetch } from "../../util/api";

import HipTitle from "../../Shared/Layout/HipTitle";
import HipBody from "../../Shared/Layout/HipBody";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipErrorBoundary from "../../Shared/LoadingAndErrors/HipErrorBoundary";
import HipClientError from "../../Shared/LoadingAndErrors/HipClientError";
import { OrdersTable } from "./Tables/OrdersTable";
import HipLoadingTable from "../../Shared/LoadingAndErrors/HipLoadingTable";

export const Orders = () => {
  const [orders, setOrders] = useState<OrderListModel[]>();
  const { cluster, orderType } = useParams();
  const isAdminOrders = orderType === "adminorders";

  useEffect(() => {
    setOrders(undefined);
  }, [orderType]);

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
  const Title = (
    <HipTitle
      title={"Orders"}
      subtitle={isAdminOrders ? "Admin" : "Personal"}
    />
  );
  if (orders === undefined) {
    return (
      <HipMainWrapper>
        {Title}
        <HipBody>
          <HipLoadingTable />
        </HipBody>
      </HipMainWrapper>
    );
  } else {
    return (
      <HipMainWrapper>
        {Title}
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
