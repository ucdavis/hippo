import React, { useEffect, useState } from "react";
import { OrderModel } from "../../types";
import { useParams } from "react-router-dom";
import { usePermissions } from "../../Shared/usePermissions";
import { usePromiseNotification } from "../../util/Notifications";
import OrderForm from "./OrderForm";

const defaultOrder: OrderModel = {
  id: 0,
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
  status: "N/A",
  createdOn: "",
  externalReference: "",
  adminNotes: "",
  subTotal: "",
  total: "",
  balanceRemaining: "",
  metaData: [],
  billings: [],
  payments: [],
  history: [],
};
const CreateOrder: React.FC = () => {
  const { cluster, productId } = useParams();
  const { isClusterAdminForCluster } = usePermissions();
  const [order, setOrder] = useState<OrderModel>(null);
  const [isClusterAdmin, setIsClusterAdmin] = useState(false);
  const [notification, setNotification] = usePromiseNotification();

  useEffect(() => {
    setIsClusterAdmin(isClusterAdminForCluster());
    if (productId) {
      setOrder((order) => ({ ...order, name: productId }));
    } else {
      if (!isClusterAdmin) {
        setOrder(null);

        alert("Product ID not found"); //TODO: replace with toast

        window.location.href = `/${cluster}/product/index`;
      } else {
        setOrder((order) => ({ ...order, name: "No Product ID" }));
      }
    }
  }, [cluster, isClusterAdmin, isClusterAdminForCluster, productId]);

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

      // editable fields
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
    };

    // TODO: await API call
    // const newOrder = await new Promise((resolve) => setTimeout(resolve, 1000));

    setOrder(editedOrder); // should be newOrder once it's pulling from the API
  };

  if (!order) {
    return <div>Loading... {productId}</div>;
  }

  return (
    <div>
      {order && (
        <div>
          <h2>Create Order</h2>

          <OrderForm
            orderProp={order}
            readOnly={false}
            onSubmit={submitOrder}
          />
        </div>
      )}
    </div>
  );
};

export default CreateOrder;
