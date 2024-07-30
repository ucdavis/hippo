import React, { useEffect, useState } from "react";
import { OrderModel } from "../../types";
import { useNavigate, useParams } from "react-router-dom";
import { usePermissions } from "../../Shared/usePermissions";
import { usePromiseNotification } from "../../util/Notifications";
import OrderForm from "./OrderForm/OrderForm";
import { authenticatedFetch, parseBadRequest } from "../../util/api";

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
    const editedOrder: OrderModel = {
      // uneditable fields
      id: updatedOrder.id,
      status: updatedOrder.status,
      createdOn: updatedOrder.createdOn,
      total: updatedOrder.total,
      subTotal: updatedOrder.subTotal,
      balanceRemaining: updatedOrder.balanceRemaining,
      balancePending: updatedOrder.balancePending,
      piUser: null,
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

  if (isClusterAdmin === null) {
    return <div>Loading...</div>;
  }

  if (!order) {
    return <div>Loading... {orderId}</div>;
  }

  return (
    <div>
      {order && (
        <div>
          <h2>Edit Order</h2>

          <OrderForm
            orderProp={order}
            readOnly={false}
            isAdmin={isClusterAdmin}
            cluster={cluster}
            onlyChartStrings={false}
            onSubmit={submitOrder}
          />
          {notification.pending && <div>Saving...</div>}
        </div>
      )}
    </div>
  );
};
