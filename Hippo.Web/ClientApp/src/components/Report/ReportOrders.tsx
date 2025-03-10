import { useEffect, useState } from "react";
import { OrderListModel } from "../../types";
import { useParams } from "react-router-dom";
import { authenticatedFetch } from "../../util/api";

import HipTitle from "../../Shared/Layout/HipTitle";
import HipBody from "../../Shared/Layout/HipBody";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipErrorBoundary from "../../Shared/LoadingAndErrors/HipErrorBoundary";
import HipClientError from "../../Shared/LoadingAndErrors/HipClientError";
import { OrdersTable } from "../Order/Tables/OrdersTable";
import HipLoadingTable from "../../Shared/LoadingAndErrors/HipLoadingTable";

export const ReportOrders = () => {
  const [orders, setOrders] = useState<OrderListModel[]>();
  const { cluster, reportType } = useParams(); //ExpiringOrders or ArchivedOrders

  useEffect(() => {
    setOrders(undefined);
  }, [reportType]);

  useEffect(() => {
    const fetchOrders = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/report/${reportType}`,
      );

      if (response.ok) {
        const data = await response.json();
        setOrders(data);
      } else {
        alert("Error fetching Orders");
      }
    };

    fetchOrders();
  }, [cluster, reportType]);

  // RH TODO: handle loading/error states
  const Title = (
    <HipTitle
      title={"Orders"}
      subtitle={
        reportType === "ExpiringOrders"
          ? "Expired Orders and Expiring in 31 Days Orders"
          : "Archived Orders"
      }
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
              isAdminOrders={true}
              showTableMessages={true}
            />
          </HipErrorBoundary>
        </HipBody>
      </HipMainWrapper>
    );
  }
};
